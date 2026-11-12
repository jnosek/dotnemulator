using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public sealed class STA_Test
{

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STA.BASE_OP_CODE | AccumulatorInstruction.ZeroPage,
                0x03,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0x03));
    }

    [TestMethod]
    public void ZeroPageX()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STA.BASE_OP_CODE | AccumulatorInstruction.ZeroPageX,
                0x03,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1010_1010;
        emulator.Cpu.X.Value = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0x06));
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STA.BASE_OP_CODE | AccumulatorInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0xC000));
    }

    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STA.BASE_OP_CODE | AccumulatorInstruction.AbsoluteX,
                0x00,
                0xC0,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1010_1010;
        emulator.Cpu.X.Value = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0xC003));
    }

    [TestMethod]
    public void AbsoluteY()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STA.BASE_OP_CODE | AccumulatorInstruction.AbsoluteY,
                0x00,
                0xC0,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1010_1010;
        emulator.Cpu.Y.Value = 0x06;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0xC006));
    }

    [TestMethod]
    public void IndexedIndirect()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STA.BASE_OP_CODE | AccumulatorInstruction.Indexed_Indirect,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x06, 0x06),
                (0x07, 0xC0)
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1010_1010;
        emulator.Cpu.X.Value = 0x04;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0xC006));
    }

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STA.BASE_OP_CODE | AccumulatorInstruction.Indirect_Indexed,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x02, 0x06),
                (0x03, 0xC0)
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1010_1010;
        emulator.Cpu.Y.Value = 0x04;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0xC00A));
    }
}
