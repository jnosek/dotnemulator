using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class ROR_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            RORA.OP_CODE,
            0xEA
        ])
        {
            Accumulator = 0x02,
        };
        emulator.Cpu.P.CarryFlag = false;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0x01, emulator.Accumulator);

        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Absolute_WithCarry()
    {
        // arrange
        var emulator = new TestEmulator([
            ROR.BASE_OP_CODE | XAddressInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b1000_0001) ]);
        emulator.Cpu.P.CarryFlag = true;

         // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1100_0000, emulator.PeekRam(0xC000));

        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void AbsoluteX_WithZero()
    {
        // arrange
        var emulator = new TestEmulator([
            ROR.BASE_OP_CODE | XAddressInstruction.AbsoluteX,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC003, 0b0000_0000) ])
        {
            X = 0x03
        };

         // act
        emulator.Start();

        // assert
        Assert.AreEqual(0x00, emulator.PeekRam(0xC003));

        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPage_WithCarryAndZero()
    {
        // arrange
        var emulator = new TestEmulator([
            ROR.BASE_OP_CODE | XAddressInstruction.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x0003, 0b0000_0001) ]);

         // act
        emulator.Start();

        // assert
        Assert.AreEqual(0x00, emulator.PeekRam(0x0003));

        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPageX_WithNegative()
    {
        // arrange
        var emulator = new TestEmulator([
            ROR.BASE_OP_CODE | XAddressInstruction.ZeroPageX,
            0x03,
            0xEA
        ],
        [ (0x0006, 0b0100_0000) ])
        {
            X = 0x03
        };
        emulator.Cpu.P.CarryFlag = true;

         // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_0000, emulator.PeekRam(0x0006));

        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
