using Dotnemulator.Platforms.Commodore64;
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
            .WithTestKernel([
                PHP.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .Build();

        emulator.Cpu.P.Value = StatusFlag.Zero;

        // act
        emulator.Cpu.Test();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.Zero, emulator.Cpu.P.Value);

        // stack - status register
        Assert.AreEqual(
            StatusFlag.Zero, 
            emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS));
    }
}
