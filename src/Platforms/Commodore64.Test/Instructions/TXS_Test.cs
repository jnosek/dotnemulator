using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TXS_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                TXS.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0xFA;

        Assert.AreEqual(0xFF, emulator.Cpu.SP);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0xFA, emulator.Cpu.SP);
    }
}
