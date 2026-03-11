using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class ADC_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.Immediate,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC003, 0b1001_0000)])
            .Build();

        emulator.Cpu.X.Value = 0x03;
        emulator.Cpu.A.Value = 0b0001_0001;
        emulator.Cpu.P.DecimalModeFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_1001, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void ZeroPage_WithCarry()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.ZeroPage,
                0x03,
                0xEA
            ])
            .WithRam([(0x3, 0b1111_1111)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0010;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void ZeroPageX_WithOverflow()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.Immediate,
                0b1111_1111,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b1000_0000;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0111_1111, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsTrue(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void Absolute_BDC()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC000, 0b0001_0010)])
            .Build();

        emulator.Cpu.A.Value = 0b0011_0001;
        emulator.Cpu.P.DecimalModeFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0100_0011, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void AbsoluteX_BDC_WithCarry()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.AbsoluteX,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC003, 0b1001_0000)])
            .Build();

        emulator.Cpu.X.Value = 0x03;
        emulator.Cpu.A.Value = 0b0001_0001;
        emulator.Cpu.P.DecimalModeFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void AbsoluteY_WithCarryIn()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.AbsoluteY,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC003, 0b1001_0001)])
            .Build();

        emulator.Cpu.Y.Value = 0x03;
        emulator.Cpu.A.Value = 0b0000_0001;
        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b1001_0011, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void IndexedIndirect_ZeroCheck()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.Indexed_Indirect,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x06, 0x06),
                (0x07, 0xC0),
                (0xC006, 0b1111_1111)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0001;
        emulator.Cpu.X.Value = 0x04;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_0000, emulator.Cpu.A.Value);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                ADC.BASE_OP_CODE | AccumulatorInstruction.Indirect_Indexed,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x02, 0x06),
                (0x03, 0xC0),
                (0xC00A, 0b0000_0011)])
            .Build();

        emulator.Cpu.A.Value = 0b0000_0101;
        emulator.Cpu.Y.Value = 0x04;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_1000, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }
}
