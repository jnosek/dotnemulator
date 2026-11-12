using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class PHP_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                PHP.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .Build();

        emulator.Cpu.P.ZeroFlag = true;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(
            StatusFlag.Zero | StatusFlag.Unused,
            emulator.Cpu.P.Value);

        // stack - status register
        Assert.AreEqual(
            StatusFlag.Zero | StatusFlag.BreakCommand | StatusFlag.Unused,
            emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS));
    }
}
