using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class INY_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            INY.OP_CODE,
            0xEA
        ])
        {
            Y = 0x03
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x04, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_Zero()
    {
        // arrange
        var emulator = new TestEmulator([
            INY.OP_CODE,
            0xEA
        ])
        {
            Y = 0xFF
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
            INY.OP_CODE,
            0xEA
        ])
        {
            Y = 0x83
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x84, emulator.Y);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
