using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class ORA_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.Immediate,
            0b0000_0010,
            0xEA
        ])
        {
            Accumulator = 0b0000_0101
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ImmediateWithZeroValue()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.Immediate,
            0x00,
            0xEA
        ])
        {
            Accumulator = 0b0000_0000
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0000, emulator.Accumulator);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b0000_0010) ])
        {
            Accumulator = 0b0000_0101
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPageX()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.ZeroPageX,
            0x03,
            0xEA
        ],
        [ (0x06, 0b0000_0010) ])
        {
            Accumulator = 0b0000_0101,
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b0000_0010) ])
        {
            Accumulator = 0b0000_0101
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    // add a negative flag check
    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.AbsoluteX,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC003, 0b1000_0010) ])
        {
            Accumulator = 0b0000_0101,
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void AbsoluteY()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.AbsoluteY,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC006, 0b0000_0010) ])
        {
            Accumulator = 0b0000_0101,
            Y = 0x06
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void IndexedIndirect()
    {
        // arrange
        var emulator = new TestEmulator([
            ORA.BASE_OP_CODE | AccumulatorInstruction.Indexed_Indirect,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x06, 0x06),
            (0x07, 0xC0),
            (0xC006, 0b0000_0010)])
        {
            Accumulator = 0b0000_0101,
            X = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange
        var emulator = new TestEmulator(
        // rom
        [
            ORA.BASE_OP_CODE | AccumulatorInstruction.Indirect_Indexed,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x02, 0x06),
            (0x03, 0xC0),
            (0xC00A, 0b0000_0010)])
        {
            Accumulator = 0b0000_0101,
            Y = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }
}
