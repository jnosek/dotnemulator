using System.Text;

namespace Commodore64.FunctionalTest;

/// <summary>
/// Collects the characters the functional test ROM writes to its report output port and prints them as lines
/// to the .NET console.
/// </summary>
/// <remarks>
/// <c>report.i65</c> ends lines with LF followed by CR, so LF completes a line and CR is ignored.
/// </remarks>
internal sealed class FunctionalTestConsole
{
    private const int LINE_FEED = 0x0A;
    private const int CARRIAGE_RETURN = 0x0D;

    private readonly StringBuilder _text = new();
    private readonly StringBuilder _line = new();

    /// <summary>
    /// Everything written so far, including a line that has not been ended yet.
    /// </summary>
    public string Text => _text.ToString() + _line.ToString();

    /// <summary>
    /// Receives one byte written to the output port.
    /// </summary>
    public void Write(int value)
    {
        switch(value)
        {
            case LINE_FEED:
                FlushLine();
                break;
            case CARRIAGE_RETURN:
                break;
            default:
                _line.Append((char)value);
                break;
        }
    }

    /// <summary>
    /// Prints a line that has not been ended yet, for example the last prompt before the test stops.
    /// </summary>
    public void Flush()
    {
        if(_line.Length > 0)
            FlushLine();
    }

    private void FlushLine()
    {
        var line = _line.ToString();
        _line.Clear();
        _text.AppendLine(line);

        Console.WriteLine(line);
    }
}
