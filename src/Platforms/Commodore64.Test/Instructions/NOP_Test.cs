using Dotnemulator.Platforms.Commodore64;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class NOP_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                NOP.OP_CODE
            ])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
    }
}
