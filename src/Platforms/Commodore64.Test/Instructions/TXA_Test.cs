using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TXA_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            TXA.OP_CODE,
            0xEA
        ])
        {
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x03, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_Zero()
    {
        // arrange
        var emulator = new TestEmulator([
            TXA.OP_CODE,
            0xEA
        ])
        {
            Accumulator = 0x05,
            X = 0x00
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x00, emulator.Accumulator);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_Negative()
    {
        // arrange
        var emulator = new TestEmulator([
            TXA.OP_CODE,
            0xEA
        ])
        {
            X = 0x83
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x83, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
