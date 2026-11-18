namespace Dotnemulator.Platforms.Commodore64.Display;

using System.Text;

/// <summary>
/// Draws the <see cref="TextScreen"/> in the console as a pseudo-screen, redrawn in place on a timer.
/// </summary>
/// <remarks>
/// <para>
/// Each refresh moves the cursor to the top-left corner and overwrites the previous frame, so the console shows a
/// live 40x25 screen. The frame is built in one buffer and written with one call to avoid flicker. Reverse-video
/// characters are drawn with the ANSI reverse attribute.
/// </para>
/// <para>
/// The timer runs on a thread-pool thread while the CPU runs on the caller's thread. The renderer only reads RAM,
/// so the worst case is a frame torn by a scroll in progress, which the next refresh corrects.
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

    private readonly TextScreen _screen;
    private readonly Func<string> _status;
    private readonly TimeSpan _interval;
    private readonly StringBuilder _frame = new();
    private readonly object _drawLock = new();
    private readonly bool _canMoveCursor;

    private Timer? _timer;
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

    public void Start()
    {
        Console.OutputEncoding = Encoding.UTF8;

        if(!_canMoveCursor)
            return;

        Console.Clear();
        Console.CursorVisible = false;
        _timer = new Timer(_ => Draw(), null, TimeSpan.Zero, _interval);
    }

    /// <summary>
    /// Draws one frame at the top of the console.
    /// </summary>
    public void Draw()
    {
        lock(_drawLock)
        {
            if(_disposed)
                return;

            BuildFrame();

            if(_canMoveCursor)
                Console.SetCursorPosition(0, 0);

            Console.Write(_frame);
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
            BuildFrame();
            if(_canMoveCursor)
            {
                Console.SetCursorPosition(0, 0);
                Console.Write(_frame);
                Console.CursorVisible = true;
            }
            else
            {
                Console.Write(_frame);
            }

            _disposed = true;
        }
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
