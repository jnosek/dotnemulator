using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CLI_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CLI.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.P.Value = StatusFlag.InterruptDisable;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0, emulator.Cpu.P.Value);
    }
}
