using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TYA_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                TYA.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.Y.Value = 0x03;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x03, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_Zero()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                TYA.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.A.Value = 0x05;
        emulator.Cpu.Y.Value = 0x00;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x00, emulator.Cpu.A.Value);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_Negative()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                TYA.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.Y.Value = 0x83;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x83, emulator.Cpu.A.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
