using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CLD_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CLD.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.P.Value = StatusFlag.DecimalMode;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0, emulator.Cpu.P.Value);
    }
}
