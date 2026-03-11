using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class SEI_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SEI.OP_CODE,
                0xEA
            ])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.InterruptDisable, emulator.Cpu.P.Value);
    }
}
