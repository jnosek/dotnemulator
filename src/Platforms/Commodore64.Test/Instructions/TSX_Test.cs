using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TSX_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                TSX.OP_CODE,
                0xEA
            ])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0xFF, emulator.Cpu.X.Value);
    }
}
