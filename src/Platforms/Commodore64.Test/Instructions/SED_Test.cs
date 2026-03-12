using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class SED_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                SED.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.P.Value = 0;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.DecimalMode, emulator.Cpu.P.Value);
    }
}
