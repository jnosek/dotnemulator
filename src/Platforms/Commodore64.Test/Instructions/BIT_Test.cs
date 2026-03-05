using System;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class BIT_Test
{
    [TestMethod]
    public void ZeroPage_WithNegative()
    {
        // arrange
        var emulator = new TestEmulator([
            BIT.BASE_OP_CODE | XAddressInstruction.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b1000_0000) ])
        {
            Accumulator = 0b1000_0001
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0001, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void ZeroPage_WithOverflow()
    {
        // arrange
        var emulator = new TestEmulator([
            BIT.BASE_OP_CODE | XAddressInstruction.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b0100_0000) ])
        {
            Accumulator = 0b0100_0001
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0100_0001, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void Absolute_WithZero()
    {
        // arrange
        var emulator = new TestEmulator([
            BIT.BASE_OP_CODE | XAddressInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b0000_0010) ])
        {
            Accumulator = 0b0000_0001
        };;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Accumulator);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void Absolute_WithNegativeAndOverflow()
    {
        // arrange
        var emulator = new TestEmulator([
            BIT.BASE_OP_CODE | XAddressInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b1100_0000) ])
        {
            Accumulator = 0b0100_0001
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0100_0001, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.OverflowFlag);
    }
}
