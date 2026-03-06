using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class LDX_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new TestEmulator([
            LDX.BASE_OP_CODE | YAddressInstruction.Immediate,
            0b0000_0010,
            0xEA
        ]);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.X);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPage_WithZero()
    {
        // arrange
        var emulator = new TestEmulator([
            LDX.BASE_OP_CODE | YAddressInstruction.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b0000_0000) ]);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0000, emulator.X);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPageX_WithNegative()
    {
        // arrange
        var emulator = new TestEmulator([
            LDX.BASE_OP_CODE | YAddressInstruction.ZeroPageY,
            0x03,
            0xEA
        ],
        [ (0x06, 0b1000_0010) ])
        {
            Y = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0010, emulator.X);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new TestEmulator([
            LDX.BASE_OP_CODE | YAddressInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b0000_0010) ]);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.X);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    // add a negative flag check
    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new TestEmulator([
            LDX.BASE_OP_CODE | YAddressInstruction.AbsoluteY,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC003, 0b1000_0010) ])
        {
            Y = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1000_0010, emulator.X);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
