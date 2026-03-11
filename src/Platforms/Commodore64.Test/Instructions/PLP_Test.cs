using Dotnemulator.Platforms.Commodore64;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class PLP_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                PLP.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .Build();

        emulator.Cpu.P.Value = StatusFlag.InterruptDisable;
        emulator.Cpu.StackPush(StatusFlag.Negative);

        // act
        emulator.Cpu.Test();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.Negative, emulator.Cpu.P.Value);

        // stack reset
        Assert.AreEqual(MOS6510Cpu.STACK_START_ADDRESS & 0xFF, emulator.Cpu.SP);
    }
}
