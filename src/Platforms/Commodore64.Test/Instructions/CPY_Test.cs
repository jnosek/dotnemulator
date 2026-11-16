using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CPY_Test
{
    [TestMethod]
    public void Immediate_WithNegative()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CPY.BASE_OP_CODE | XAddressInstruction.Immediate,
                0b0100_0010,
                0xEA
            ])
            .Build();

        emulator.Cpu.Y.Value = 0b0000_0010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.Cpu.Y.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void ZeroPage_WithZero()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CPY.BASE_OP_CODE | XAddressInstruction.ZeroPage,
                0x03,
                0xEA
            ])
            .WithRam([(0x3, 0b0000_0001)])
            .Build();

        emulator.Cpu.Y.Value = 0b0000_0001;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Cpu.Y.Value);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }

    [TestMethod]
    public void Absolute_WithCarry()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                CPY.BASE_OP_CODE | XAddressInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC000, 0b0000_0010)])
            .Build();

        emulator.Cpu.Y.Value = 0b0000_1000;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_1000, emulator.Cpu.Y.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
    }
}
