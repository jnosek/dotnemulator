using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class MemoryMap_Test
{
    private static Emulator BuildEmulator()
    {
        var emulator = new EmulatorBuilder()
            .WithTestKernel([])
            .Build();

        // BASIC, KERNAL and I/O banked in
        emulator.PortBus.Strobe(0b0000_0111);

        return emulator;
    }

    [TestMethod]
    public void WriteToRam()
    {
        // arrange
        var emulator = BuildEmulator();

        // act
        emulator.ReadWrite.Drive(ReadWriteMode.Write);
        emulator.DataBus.Drive(0x42);
        emulator.AddressBus.Strobe(0x03);

        // assert
        Assert.AreEqual(0x42, emulator.MemoryMap.PeekRam(0x03));
    }

    [TestMethod]
    public void ReadFromRam()
    {
        // arrange
        var emulator = BuildEmulator();
        emulator.MemoryMap.PokeRam(0x03, 0x42);

        // act
        emulator.ReadWrite.Drive(ReadWriteMode.Read);
        emulator.AddressBus.Strobe(0x03);

        // assert
        Assert.AreEqual(0x42, emulator.DataBus.Read());
    }

    [TestMethod]
    public void WriteCycle_DoesNotReadMemory()
    {
        // arrange
        var emulator = BuildEmulator();
        emulator.MemoryMap.PokeRam(0x03, 0x11);

        // act
        emulator.ReadWrite.Drive(ReadWriteMode.Write);
        emulator.DataBus.Drive(0x42);
        emulator.AddressBus.Strobe(0x03);

        // assert: a read during the write cycle would have driven the old value (0x11) onto the data bus
        Assert.AreEqual(0x42, emulator.DataBus.Read());
    }

    [TestMethod]
    public void WriteToRomArea_GoesToRamUnderneath()
    {
        // arrange
        var emulator = BuildEmulator();

        // act
        emulator.ReadWrite.Drive(ReadWriteMode.Write);
        emulator.DataBus.Drive(0x42);
        emulator.AddressBus.Strobe(0xE000);

        // assert
        Assert.AreEqual(0x42, emulator.MemoryMap.PeekRam(0xE000));
    }

    [TestMethod]
    public void CpuWrite_SetsWriteMode_CpuRead_SetsReadMode()
    {
        // arrange
        var emulator = BuildEmulator();

        // act / assert
        emulator.Cpu.Write(0x0300, 0x42);
        Assert.AreEqual(ReadWriteMode.Write, emulator.ReadWrite.Mode);

        Assert.AreEqual(0x42, emulator.Cpu.Read(0x0300));
        Assert.AreEqual(ReadWriteMode.Read, emulator.ReadWrite.Mode);
    }

    [TestMethod]
    public void MemoryWatch_SeesValueBeingWritten()
    {
        // arrange
        var seen = new List<int>();
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDA.BASE_OP_CODE | AccumulatorInstruction.Immediate, 0x42,
                STA.BASE_OP_CODE | AccumulatorInstruction.Absolute, 0x00, 0x03,
                0xEA
            ])
            .WithRam([(0x0300, 0x11)])
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                IsStopOnNopEnabled = true,
                MemoryWatchCallbacks = new() { [0x0300] = seen.Add }
            })
            .Build();

        // act
        emulator.Start();

        // assert
        CollectionAssert.AreEqual(new[] { 0x42 }, seen);
    }
}
