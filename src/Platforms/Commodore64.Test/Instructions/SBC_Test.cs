using Dotnemulator.Platforms.Commodore64;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class SBC_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange: 7 - 3 - 0(borrow) = 4
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.Immediate,
                0x03,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0x07;
        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x04, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void ZeroPage()
    {
        // arrange: 5 - 10 - 0(borrow) = -5 = 0xFB, borrow occurs
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.ZeroPage,
                0x10,
                0xEA
            ])
            .WithRam([ (0x10, 0x0A) ])
            .Build();

        emulator.Cpu.A.Value = 0x05;
        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0xFB, emulator.Cpu.A.Value);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void ZeroPageX()
    {
        // arrange: 8 - 8 - 0(borrow) = 0, zero result
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.ZeroPageX,
                0x04,
                0xEA
            ])
            .WithRam([ (0x07, 0x08) ])
            .Build();

        emulator.Cpu.A.Value = 0x08;
        emulator.Cpu.X.Value = 0x03;
        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x00, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange: 16 - 1 - 1(borrow) = 14 = 0x0E, carry-in is clear
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([ (0xC000, 0x01) ])
            .Build();

        emulator.Cpu.A.Value = 0x10;
        emulator.Cpu.P.CarryFlag = false;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x0E, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void AbsoluteX()
    {
        // arrange: 0x50(+80) - 0xB0(-80 signed) - 0(borrow) = -96 = 0xA0, signed overflow
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.AbsoluteX,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([ (0xC003, 0xB0) ])
            .Build();

        emulator.Cpu.A.Value = 0x50;
        emulator.Cpu.X.Value = 0x03;
        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0xA0, emulator.Cpu.A.Value);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsTrue(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void AbsoluteY()
    {
        // arrange: 0xD0(-48 signed) - 0x70(+112) - 0(borrow) = 0x60(+96), signed overflow
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.AbsoluteY,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([ (0xC006, 0x70) ])
            .Build();

        emulator.Cpu.A.Value = 0xD0;
        emulator.Cpu.Y.Value = 0x06;
        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x60, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsTrue(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void IndexedIndirect()
    {
        // arrange: 0x20 - 0x05 - 1(borrow) = 0x1A = 26, carry-in clear
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.Indexed_Indirect,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x06, 0x50),
                (0x07, 0xC0),
                (0xC050, 0x05)
            ])
            .Build();

        emulator.Cpu.A.Value = 0x20;
        emulator.Cpu.X.Value = 0x04;
        emulator.Cpu.P.CarryFlag = false;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x1A, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange: 0x02 - 0x03 - 0(borrow) = -1 = 0xFF, borrow occurs
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SBC.BASE_OP_CODE | AccumulatorInstruction.Indirect_Indexed,
                0x02,
                0xEA
            ])
            .WithRam([
                (0x02, 0x06),
                (0x03, 0xC0),
                (0xC00A, 0x03)
            ])
            .Build();

        emulator.Cpu.A.Value = 0x02;
        emulator.Cpu.Y.Value = 0x04;
        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0xFF, emulator.Cpu.A.Value);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }
}
