using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class RTI_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                RTI.OP_CODE,
            ])
            .WithRam([
                (0xC000, 0xEA)
            ])
            .Build();

        emulator.Cpu.StackPush(0xC0);
        emulator.Cpu.StackPush(0x00);
        emulator.Cpu.StackPush(StatusFlag.Overflow);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(StatusFlag.Overflow, emulator.Cpu.P.Value);
        Assert.AreEqual(MOS6510Cpu.STACK_START_ADDRESS & 0xFF, emulator.Cpu.SP);
        Assert.AreEqual(0xC001, emulator.Cpu.PC.Value);
    }
}
