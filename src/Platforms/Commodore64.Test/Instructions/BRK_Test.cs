using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class BRK_Test
{

    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                BRK.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .WithRam([(0x0100, 0xEA)])
            .Build();

        // act
        emulator.Start();

        // assert
        // Current PC
        Assert.AreEqual(0x0101, emulator.Cpu.PC.Advance());

        // stack - status register
        Assert.AreEqual(
            StatusFlag.BreakCommand | StatusFlag.Unused,
            emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS - 2));

        // stack - return PC
        Assert.AreEqual(0x02, emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS - 1));
        Assert.AreEqual(0xE0, emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS - 0));

        // status register

        // the break flag is not actually set in the status register
        Assert.IsFalse(emulator.Cpu.P.BreakCommandFlag);
        Assert.IsTrue(emulator.Cpu.P.InterruptDisableFlag);
    }
}
