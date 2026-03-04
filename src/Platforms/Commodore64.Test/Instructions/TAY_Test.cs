using System;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TAY_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            TAY.OP_CODE,
            0xEA
        ])
        {
            Accumulator = 0x03
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x03, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_Zero()
    {
        // arrange
        var emulator = new TestEmulator([
            TAY.OP_CODE,
            0xEA
        ])
        {
            Accumulator = 0x00,
            Y = 0x05
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x00, emulator.Y);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_Negative()
    {
        // arrange
        var emulator = new TestEmulator([
            TAY.OP_CODE,
            0xEA
        ])
        {
            Accumulator = 0x83
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x83, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
