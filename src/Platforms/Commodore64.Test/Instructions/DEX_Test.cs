using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class DEX_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                DEX.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0x03;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x02, emulator.Cpu.X.Value);
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
                DEX.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0x01;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x00, emulator.Cpu.X.Value);
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
                DEX.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0x83;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0x82, emulator.Cpu.X.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Implied_ZeroToNegativeWrapAround()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                DEX.OP_CODE,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0x00;

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0xFF, emulator.Cpu.X.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
