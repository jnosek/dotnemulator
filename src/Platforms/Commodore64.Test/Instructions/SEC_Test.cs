using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class SEC_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            SEC.OP_CODE,
            0xEA
        ]);

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.Carry, emulator.Cpu.P.Value);
    }
}
