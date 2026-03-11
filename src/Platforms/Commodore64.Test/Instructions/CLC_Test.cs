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
            .WithTestKernel([
                CLC.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.P.Value = StatusFlag.Carry;

        // act
        emulator.Cpu.Test();

        // assert
        // Current P
        Assert.AreEqual(0, emulator.Cpu.P.Value);
    }
}
