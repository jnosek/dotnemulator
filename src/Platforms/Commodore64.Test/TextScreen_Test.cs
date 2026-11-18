using Dotnemulator.Platforms.Commodore64.Display;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class TextScreen_Test
{
    [TestMethod]
    [DataRow(0x00, '@')]
    [DataRow(0x01, 'A')]
    [DataRow(0x1A, 'Z')]
    [DataRow(0x1B, '[')]
    [DataRow(0x1C, '£')]
    [DataRow(0x1D, ']')]
    [DataRow(0x1E, '↑')]
    [DataRow(0x1F, '←')]
    [DataRow(0x20, ' ')]
    [DataRow(0x2A, '*')]
    [DataRow(0x2E, '.')]
    [DataRow(0x30, '0')]
    [DataRow(0x39, '9')]
    [DataRow(0x3F, '?')]
    [DataRow(0x40, TextScreen.GRAPHICS_CHARACTER)]
    [DataRow(0x7F, TextScreen.GRAPHICS_CHARACTER)]
    public void Decode(int screenCode, char expected)
    {
        Assert.AreEqual(expected, TextScreen.Decode(screenCode));
    }

    [TestMethod]
    public void DecodeIgnoresReverseVideo()
    {
        Assert.AreEqual('A', TextScreen.Decode(0x81));
        Assert.AreEqual(' ', TextScreen.Decode(0xA0));
    }

    [TestMethod]
    [DataRow(0x01, false)]
    [DataRow(0x7F, false)]
    [DataRow(0x80, true)]
    [DataRow(0xA0, true)]
    public void IsReverse(int screenCode, bool expected)
    {
        Assert.AreEqual(expected, TextScreen.IsReverse(screenCode));
    }

    [TestMethod]
    public void GetRowsReadsScreenRam()
    {
        // "HI" at the top left, "OK" at the end of the bottom row
        var emulator = new EmulatorBuilder()
            .WithTestKernel([])
            .WithRam([
                (0x0400, 0x08), (0x0401, 0x09),
                (0x07E6, 0x0F), (0x07E7, 0x0B),
                (0x07E8, 0x01) // just past the screen
            ])
            .Build();

        var rows = new TextScreen(emulator.MemoryMap).GetRows();

        Assert.HasCount(TextScreen.ROWS, rows);
        Assert.IsTrue(rows.All(row => row.Length == TextScreen.COLUMNS));
        Assert.AreEqual("HI", rows[0][..2]);
        Assert.AreEqual("OK", rows[24][^2..]);
    }
}
