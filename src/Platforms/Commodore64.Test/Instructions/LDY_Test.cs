using System;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class LDY_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new TestEmulator([
            LDY.BASE_OP_CODE | AddressMode.Immediate,
            0b0000_0010,
            0xEA
        ]);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPage_WithZero()
    {
        // arrange
        var emulator = new TestEmulator([
            LDY.BASE_OP_CODE | AddressMode.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b0000_0000) ]);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0000, emulator.Y);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPageX_WithNegative()
    {
        // arrange
        var emulator = new TestEmulator([
            LDY.BASE_OP_CODE | AddressMode.ZeroPageX,
            0x03,
            0xEA
        ],
        [ (0x06, 0b1000_0010) ])
        {
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0010, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new TestEmulator([
            LDY.BASE_OP_CODE | AddressMode.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b0000_0010) ]);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    // add a negative flag check
    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new TestEmulator([
            LDY.BASE_OP_CODE | AddressMode.AbsoluteX,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC003, 0b1000_0010) ])
        {
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0010, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
