using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class NOP_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            NOP.OP_CODE
        ]);

        // act
        emulator.Start();

        // assert
    }
}
