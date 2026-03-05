using System;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public sealed class STY_Test
{

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new TestEmulator([
            STY.BASE_OP_CODE | XAddressInstruction.ZeroPage,
            0x03,
            0xEA
        ])
        {
            Y = 0b1010_1010
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0x03));
    }

    [TestMethod]
    public void ZeroPageX()
    {
        // arrange
        var emulator = new TestEmulator([
            STY.BASE_OP_CODE | XAddressInstruction.ZeroPageX,
            0x03,
            0xEA
        ])
        {
            Y = 0b1010_1010,
            X = 0x03
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0x06));
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new TestEmulator([
            STY.BASE_OP_CODE | XAddressInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ])
        {
            Y = 0b1010_1010
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0xC000));
    }
}