using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class JSR_Test
{
    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                JSR.OP_CODE,
                0x00,
                0xC0,])
            .WithRam([(0xC000, 0xEA)])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0xC001, emulator.Cpu.PC.Value);

        // stack pointer should be decremented by 2 (2 byte address pushed to stack)
        Assert.AreEqual(0xFD, emulator.Cpu.SP);
        // high byte of return address should be on stack
        Assert.AreEqual(0xE0, emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS));
        // low byte of return address should be on stack
        Assert.AreEqual(0x03, emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_START_ADDRESS - 1));
    }
}
