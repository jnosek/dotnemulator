namespace Dotnemulator.Platforms.Commodore64.Display;

using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Read-only text view of the C64's 40x25 screen RAM.
/// </summary>
/// <remarks>
/// <para>
/// The VIC-II shows screen memory as character codes ("screen codes", not PETSCII). The KERNAL puts the screen at
/// <c>$0400-$07E7</c>, so without a video front end the text on screen can be read straight out of RAM.
/// </para>
/// <para>
/// Reads go through <see cref="MemoryMap.PeekRam"/>, so no bus cycles are started and the CPU is not disturbed.
/// The screen address is fixed at <c>$0400</c>; <c>$D018</c> and the CIA 2 VIC bank are not followed yet.
/// Codes are decoded with the uppercase/graphics character set.
/// </para>
/// </remarks>
internal class TextScreen
{
    public const int COLUMNS = 40;
    public const int ROWS = 25;
    public const int SCREEN_ADDRESS = 0x0400;

    /// <summary>
    /// Shown for the graphics characters (screen codes 64-127), which have no text equivalent yet.
    /// </summary>
    public const char GRAPHICS_CHARACTER = '▒';

    private readonly MemoryMap _memoryMap;

    public TextScreen(MemoryMap memoryMap)
    {
        _memoryMap = memoryMap;
    }

    /// <summary>
    /// The screen code at <paramref name="row"/>, <paramref name="column"/>.
    /// </summary>
    public int ReadCode(int row, int column) => _memoryMap.PeekRam(SCREEN_ADDRESS + row * COLUMNS + column);

    /// <summary>
    /// The screen as 25 strings of 40 characters, reverse video shown as the plain character.
    /// </summary>
    public string[] GetRows()
    {
        var rows = new string[ROWS];
        Span<char> line = stackalloc char[COLUMNS];

        for(int row = 0; row < ROWS; row++)
        {
            for(int column = 0; column < COLUMNS; column++)
                line[column] = Decode(ReadCode(row, column));

            rows[row] = new string(line);
        }

        return rows;
    }

    /// <summary>
    /// <c>true</c> when bit 7 of <paramref name="screenCode"/> is set: the character is shown in reverse video.
    /// </summary>
    public static bool IsReverse(int screenCode) => (screenCode & 0x80) != 0;

    /// <summary>
    /// Converts a screen code to the character it shows in the uppercase/graphics set, ignoring reverse video.
    /// </summary>
    public static char Decode(int screenCode)
    {
        var code = screenCode & 0x7F;

        return code switch
        {
            0 => '@',
            <= 26 => (char)('A' + code - 1),
            27 => '[',
            28 => '£',
            29 => ']',
            30 => '↑',
            31 => '←',
            // 32-63 match ASCII: space, punctuation and digits
            < 64 => (char)code,
            _ => GRAPHICS_CHARACTER
        };
    }
}
