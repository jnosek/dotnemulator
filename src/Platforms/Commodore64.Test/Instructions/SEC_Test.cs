using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class SEC_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                SEC.OP_CODE,
                0xEA
            ])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.Carry, emulator.Cpu.P.Value);
    }
}
