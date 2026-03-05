using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CPX_Test
{
    [TestMethod]
    public void Immediate_WithNegative()
    {
        // arrange
        var emulator = new TestEmulator([
            CPX.BASE_OP_CODE | XAddressInstruction.Immediate,
            0b0100_0010,
            0xEA
        ])
        {
            X = 0b0000_0010
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.X);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void ZeroPage_WithZero()
    {
        // arrange
        var emulator = new TestEmulator([
            CPX.BASE_OP_CODE | XAddressInstruction.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b0000_0001) ])
        {
            X = 0b0000_0001
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.X);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void Absolute_WithCarry()
    {
        // arrange
        var emulator = new TestEmulator([
            CPX.BASE_OP_CODE | XAddressInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b0000_0010) ])
        {
            X = 0b0000_1000
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_1000, emulator.X);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }
}
