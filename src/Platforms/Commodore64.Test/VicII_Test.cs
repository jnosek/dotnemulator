using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class VicII_Test
{
    private const int ALL_RAM = 0b0000_0000;
    private const int IO_BANKED_IN = 0b0000_0111;

    private static Emulator Build(int portPins = IO_BANKED_IN)
    {
        var emulator = new EmulatorBuilder()
            .WithTestKernel([])
            .Build();

        emulator.PortBus.Strobe(portPins);

        return emulator;
    }

    private static int Read(Emulator emulator, int address)
    {
        emulator.ReadWrite.Drive(ReadWriteMode.Read);
        emulator.AddressBus.Strobe(address);
        return emulator.DataBus.Read();
    }

    private static void Write(Emulator emulator, int address, int value)
    {
        emulator.ReadWrite.Drive(ReadWriteMode.Write);
        emulator.DataBus.Drive(value);
        emulator.AddressBus.Strobe(address);
    }

    private static void Clock(VicII vic, int cycles)
    {
        for(int i = 0; i < cycles; i++)
            vic.Clock();
    }

    [TestMethod]
    public void RasterAdvancesOneLineEvery63Cycles()
    {
        var vic = Build().Vic;

        Clock(vic, VicII.CYCLES_PER_LINE - 1);
        Assert.AreEqual(0, vic.RasterLine);

        vic.Clock();
        Assert.AreEqual(1, vic.RasterLine);
    }

    [TestMethod]
    public void RasterWrapsAfterLine311()
    {
        var vic = Build().Vic;

        Clock(vic, VicII.CYCLES_PER_LINE * (VicII.LINES_PER_FRAME - 1));
        Assert.AreEqual(311, vic.RasterLine);

        Clock(vic, VicII.CYCLES_PER_LINE);
        Assert.AreEqual(0, vic.RasterLine);
    }

    [TestMethod]
    public void EveryBusCycleClocksTheRaster()
    {
        var emulator = Build();

        for(int i = 0; i < VicII.CYCLES_PER_LINE; i++)
            Read(emulator, 0x0002);

        Assert.AreEqual(1, emulator.Vic.RasterLine);
    }

    [TestMethod]
    public void ReadRasterLine()
    {
        var emulator = Build();
        Clock(emulator.Vic, VicII.CYCLES_PER_LINE * 0x137);

        Assert.AreEqual(0x37, Read(emulator, 0xD012));
        Assert.AreEqual(0x80, Read(emulator, 0xD011) & 0x80, "bit 7 of $D011 is bit 8 of the raster line");
    }

    [TestMethod]
    public void RasterBit8ClearOnLowerLines()
    {
        var emulator = Build();
        Clock(emulator.Vic, VicII.CYCLES_PER_LINE * 0x37);

        Assert.AreEqual(0x37, Read(emulator, 0xD012));
        Assert.AreEqual(0x00, Read(emulator, 0xD011) & 0x80);
    }

    [TestMethod]
    public void WriteRasterSetsCompareNotCounter()
    {
        var emulator = Build();

        Write(emulator, 0xD012, 0x37);
        Write(emulator, 0xD011, 0x9B);

        Assert.AreEqual(0x137, emulator.Vic.RasterCompare);
        Assert.AreEqual(0, emulator.Vic.RasterLine);
        Assert.AreEqual(0x00, Read(emulator, 0xD012));
        Assert.AreEqual(0x1B, Read(emulator, 0xD011), "bits 0-6 are stored, bit 7 reads the raster line");
    }

    [TestMethod]
    public void OtherRegistersStoreValues()
    {
        var emulator = Build();

        Write(emulator, 0xD020, 0x0E);

        Assert.AreEqual(0x0E, Read(emulator, 0xD020));
    }

    [TestMethod]
    public void RegistersRepeatEvery64Bytes()
    {
        var emulator = Build();

        Write(emulator, 0xD020, 0x0E);

        Assert.AreEqual(0x0E, Read(emulator, 0xD060));
        Assert.AreEqual(0x0E, Read(emulator, 0xD3E0));
    }

    [TestMethod]
    public void WriteDoesNotReachRamUnderneath()
    {
        var emulator = Build();

        Write(emulator, 0xD020, 0x0E);

        Assert.AreEqual(0x00, emulator.MemoryMap.PeekRam(0xD020));
    }

    [TestMethod]
    public void IgnoredWhenIoNotBankedIn()
    {
        var emulator = Build(ALL_RAM);

        Write(emulator, 0xD012, 0x42);

        Assert.AreEqual(0x42, emulator.MemoryMap.PeekRam(0xD012));
        Assert.AreEqual(0x42, Read(emulator, 0xD012));
        Assert.AreEqual(0, emulator.Vic.RasterCompare);
    }

    [TestMethod]
    public void RestOfIoAreaStillStored()
    {
        var emulator = Build();

        Write(emulator, 0xDC00, 0x42);

        Assert.AreEqual(0x42, Read(emulator, 0xDC00));
    }
}
