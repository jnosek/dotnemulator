using System;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CMP_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new TestEmulator([
            CMP.BASE_OP_CODE | AccumulatorInstruction.Immediate,
            0b0000_0110,
            0xEA
        ])
        {
            Accumulator = 0b0000_0111
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new TestEmulator([
            CMP.BASE_OP_CODE | AccumulatorInstruction.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b0000_1000) ])
        {
            Accumulator = 0b0000_0111
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void ZeroPageX()
    {
        // arrange
        var emulator = new TestEmulator([
            CMP.BASE_OP_CODE | AccumulatorInstruction.ZeroPageX,
            0x03,
            0xEA
        ],
        [ (0x06, 0b0000_0111) ])
        {
            Accumulator = 0b0000_0111,
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0111, emulator.Accumulator);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new TestEmulator([
            CMP.BASE_OP_CODE | AccumulatorInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b1111_1111) ])
        {
            Accumulator = 0b1111_1111
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1111_1111, emulator.Accumulator);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new TestEmulator([
            CMP.BASE_OP_CODE | AccumulatorInstruction.AbsoluteX,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC003, 0b0000_0001) ])
        {
            Accumulator = 0b1000_0000,
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0000, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void AbsoluteY()
    {
        // arrange
        var emulator = new TestEmulator([
            CMP.BASE_OP_CODE | AccumulatorInstruction.AbsoluteY,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC006, 0b0000_0110) ])
        {
            Accumulator = 0b0000_0001,
            Y = 0x06
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void IndexedIndirect()
    {
        // arrange
        var emulator = new TestEmulator([
            CMP.BASE_OP_CODE | AccumulatorInstruction.Indexed_Indirect,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x06, 0x06),
            (0x07, 0xC0),
            (0xC006, 0b0000_0101)])
        {
            Accumulator = 0b0000_0110,
            X = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0110, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange
        var emulator = new TestEmulator(
        // rom
        [
            CMP.BASE_OP_CODE | AccumulatorInstruction.Indirect_Indexed,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x02, 0x06),
            (0x03, 0xC0),
            (0xC00A, 0b0000_0110)])
        {
            Accumulator = 0b0000_0101,
            Y = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0101, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }
}
