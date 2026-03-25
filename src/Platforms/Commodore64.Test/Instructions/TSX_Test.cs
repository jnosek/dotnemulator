using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TSX_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                TSX.OP_CODE,
                0xEA
            ])
            .Build();

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0xFF, emulator.Cpu.X.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
