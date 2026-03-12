using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CMP_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.Immediate,
                0b0000_0110,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0111;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.ZeroPage,
                0x03,
                0xEA
            ])
            .WithRam([(0x3, 0b0000_1000)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0111;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void ZeroPageX()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.ZeroPageX,
                0x03,
                0xEA
            ])
            .WithRam([(0x06, 0b0000_0111)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0111;
        emulator.Cpu.X.Value = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Cpu.A.Value);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC000, 0b1111_1111)])
            .Build();

        emulator.Cpu.A.Value = 0b1111_1111;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1111_1111, emulator.Cpu.A.Value);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.AbsoluteX,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC003, 0b0000_0001)])
            .Build();

        emulator.Cpu.A.Value = 0b1000_0000;
        emulator.Cpu.X.Value = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0000, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void AbsoluteY()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.AbsoluteY,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC006, 0b0000_0110)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0001;
        emulator.Cpu.Y.Value = 0x06;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void IndexedIndirect()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.Indexed_Indirect,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x06, 0x06),
                (0x07, 0xC0),
                (0xC006, 0b0000_0101)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0110;
        emulator.Cpu.X.Value = 0x04;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0110, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CMP.BASE_OP_CODE | AccumulatorInstruction.Indirect_Indexed,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x02, 0x06),
                (0x03, 0xC0),
                (0xC00A, 0b0000_0110)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0101;
        emulator.Cpu.Y.Value = 0x04;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0101, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }
}
