using Dotnemulator.Platforms.Commodore64;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test;

namespace Commodore64.Test.Instructions;


[TestClass]
public sealed class STA_Test
{

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new TestEmulator([
            STA.BASE_OP_CODE | AddressMode.ZeroPage,
            0x03,
            0xEA
        ]);

        emulator.Accumulator = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0x03));

    }
}
