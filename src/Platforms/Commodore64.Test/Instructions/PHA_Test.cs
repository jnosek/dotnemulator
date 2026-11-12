using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class PHA_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                PHA.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0b0101_0101;

        // act
        emulator.Start();

        // assert
        // stack register
        Assert.AreEqual((MOS6510Cpu.STACK_START_ADDRESS & 0xFF) - 1, emulator.Cpu.SP);

        // stack contents
        Assert.AreEqual(
            0b0101_0101,
            emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS));

        // accumulator
        Assert.AreEqual(0b0101_0101, emulator.Cpu.A.Value);
    }
}
