using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class STX_Test
{
    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new TestEmulator([
            STX.BASE_OP_CODE | YAddressInstruction.ZeroPage,
            0x03,
            0xEA
        ])
        {
            X = 0b1010_1010
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0x03));
    }

    [TestMethod]
    public void ZeroPageY()
    {
        // arrange
        var emulator = new TestEmulator([
            STX.BASE_OP_CODE | YAddressInstruction.ZeroPageY,
            0x03,
            0xEA
        ])
        {
            X = 0b1010_1010,
            Y = 0x03
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
            STX.BASE_OP_CODE | YAddressInstruction.Absolute,
            0x00,
            0xC0,
            0xEA
        ])
        {
            X = 0b1010_1010
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0xC000));
    }
}
