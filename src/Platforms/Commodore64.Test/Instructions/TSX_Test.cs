using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TSX_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            TSX.OP_CODE,
            0xEA
        ]);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0xFF, emulator.X);
    }
}
