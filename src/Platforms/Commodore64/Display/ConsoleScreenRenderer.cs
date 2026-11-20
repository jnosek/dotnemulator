namespace Dotnemulator.Platforms.Commodore64.Display;

using System.Text;

/// <summary>
/// Draws the <see cref="TextScreen"/> in the console as a pseudo-screen, redrawn in place on a timer.
/// </summary>
/// <remarks>
/// <para>
/// After each frame the cursor is left on the line below it. Each refresh moves the cursor up by the frame's height
/// (ANSI <c>ESC[nA</c>) and overwrites the previous frame, so the console shows a live 40x25 screen. Relative
/// movement keeps working after the console has scrolled, and needs no cursor-position query to the terminal. The frame is built in one buffer and written with one call to avoid flicker. Reverse-video
/// characters are drawn with the ANSI reverse attribute.
/// </para>
/// <para>
/// The timer runs on a thread-pool thread while the CPU runs on the caller's thread. The renderer only reads RAM,
/// so the worst case is a frame torn by a scroll in progress, which the next refresh corrects.
/// </para>
/// <para>
/// With <see cref="ClearOnStart"/> off, the console is not cleared: the first frame is written below whatever is
/// already there (for example the boot log), scrolling if needed, and later frames overwrite it in place. Nothing
/// else may write to the console while the renderer runs, and the window must be at least as tall as the frame.
/// </para>
/// <para>
/// When output is redirected the cursor cannot be moved, so nothing is drawn while running and one frame is
/// written when the renderer is disposed.
/// </para>
/// </remarks>
internal sealed class ConsoleScreenRenderer : IDisposable
{
    private const string REVERSE_ON = "\u001b[7m";
    private const string REVERSE_OFF = "\u001b[27m";

    /// <summary>
    /// Lines in one frame: top border, 25 rows, bottom border and the status line.
    /// </summary>
    private const int FRAME_LINES = TextScreen.ROWS + 3;

    private readonly TextScreen _screen;
    private readonly Func<string> _status;
    private readonly TimeSpan _interval;
    private readonly StringBuilder _frame = new();
    private readonly object _drawLock = new();
    private readonly bool _canMoveCursor;

    private Timer? _timer;
    private bool _started;
    private long _frameCount;
    private bool _disposed;

    /// <param name="screen">The screen to draw.</param>
    /// <param name="status">Text for the status line under the frame, e.g. the CPU's PC.</param>
    /// <param name="interval">Time between refreshes; 100 ms when not given.</param>
    public ConsoleScreenRenderer(TextScreen screen, Func<string> status, TimeSpan? interval = null)
    {
        _screen = screen;
        _status = status;
        _interval = interval ?? TimeSpan.FromMilliseconds(100);
        _canMoveCursor = !Console.IsOutputRedirected;
    }

    /// <summary>
    /// <c>true</c> (the default) clears the console when <see cref="Start"/> is called and draws at the top;
    /// <c>false</c> draws below the existing output.
    /// </summary>
    public bool ClearOnStart { get; init; } = true;

    public void Start()
    {
        Console.OutputEncoding = Encoding.UTF8;

        if(!_canMoveCursor)
            return;

        lock(_drawLock)
        {
            if(_disposed || _started)
                return;

            Console.CursorVisible = false;

            if(ClearOnStart)
                Console.Clear();

            // written in sequence, so the console scrolls to make room; later frames move up over it
            BuildFrame();
            Console.Write(_frame);

            _started = true;
            _timer = new Timer(_ => Draw(), null, _interval, _interval);
        }
    }

    /// <summary>
    /// Draws one frame at the top of the console.
    /// </summary>
    public void Draw()
    {
        lock(_drawLock)
        {
            if(_disposed || !_started)
                return;

            Redraw();
        }
    }

    public void Dispose()
    {
        lock(_drawLock)
        {
            if(_disposed)
                return;

            _timer?.Dispose();

            // final frame, so the console is left showing the last state of the screen
            if(_started)
            {
                Redraw();
                Console.CursorVisible = true;
            }
            else if(!_canMoveCursor)
            {
                BuildFrame();
                Console.Write(_frame);
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Overwrites the previous frame. The cursor is on the line below it, and is left there again.
    /// </summary>
    private void Redraw()
    {
        BuildFrame();
        Console.Write($"\u001b[{FRAME_LINES}A\r");
        Console.Write(_frame);
    }

    private void BuildFrame()
    {
        _frame.Clear();
        _frame.Append('┌').Append('─', TextScreen.COLUMNS).Append('┐').AppendLine();

        for(int row = 0; row < TextScreen.ROWS; row++)
        {
            _frame.Append('│');

            var reverse = false;
            for(int column = 0; column < TextScreen.COLUMNS; column++)
            {
                var code = _screen.ReadCode(row, column);

                if(TextScreen.IsReverse(code) != reverse)
                {
                    reverse = !reverse;
                    _frame.Append(reverse ? REVERSE_ON : REVERSE_OFF);
                }

                _frame.Append(TextScreen.Decode(code));
            }

            if(reverse)
                _frame.Append(REVERSE_OFF);

            _frame.Append('│').AppendLine();
        }

        _frame.Append('└').Append('─', TextScreen.COLUMNS).Append('┘').AppendLine();

        // padded so a shorter status line fully overwrites a longer one
        var status = $"{_status()}  frame {++_frameCount}";
        _frame.Append(status.PadRight(TextScreen.COLUMNS + 2)).AppendLine();
    }
}
