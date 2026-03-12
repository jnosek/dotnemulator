using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class JMP_Test
{
    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                JMP.ABSOLUTE_OP_CODE,
                0x00,
                0xC0,])
            .WithRam([(0xC000, 0xEA)])
            .Build();

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0xC001, emulator.Cpu.PC.Value);
    }

    [TestMethod]
    public void Indirect()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                JMP.INDIRECT_OP_CODE,
                0x00,
                0xC0,])
            .WithRam([
                (0xC000, 0x00),
                (0xC001, 0xC1),
                 // NOP at target address to end test
                (0xC100, 0xEA) ])
            .Build();

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0xC101, emulator.Cpu.PC.Value);
    }
}
