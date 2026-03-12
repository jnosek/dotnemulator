using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class RTS_Test
{
    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                RTS.OP_CODE,
                0x00,
                0x00,
                0xEA,
            ])
            .Build();

        // routine return address
        var kernelReturn = 0xE002;
        emulator.Cpu.StackPush(kernelReturn >> 8);
        emulator.Cpu.StackPush(kernelReturn & 0xFF);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0xE004, emulator.Cpu.PC.Value);

        // stack pointer should be reset
        Assert.AreEqual(0xFF, emulator.Cpu.SP);
    }
}
