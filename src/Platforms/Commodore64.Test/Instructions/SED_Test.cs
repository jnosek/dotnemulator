using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class SED_Test
{
[TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            SED.OP_CODE,
            0xEA
        ])
        {
            P = 0
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.DecimalMode, emulator.Cpu.P.Value);
    }
}
