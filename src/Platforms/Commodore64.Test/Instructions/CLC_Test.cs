using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CLC_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CLC.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.P.Value = StatusFlag.Carry;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.Unused, emulator.Cpu.P.Value);
    }
}
