using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class LDX_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDX.BASE_OP_CODE | YAddressInstruction.Immediate,
                0b0000_0010,
                0xEA
            ])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.Cpu.X.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPage_WithZero()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDX.BASE_OP_CODE | YAddressInstruction.ZeroPage,
                0x03,
                0xEA
            ])
            .WithRam([(0x3, 0b0000_0000)])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_0000, emulator.Cpu.X.Value);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPageX_WithNegative()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDX.BASE_OP_CODE | YAddressInstruction.ZeroPageY,
                0x03,
                0xEA
            ])
            .WithRam([(0x06, 0b1000_0010)])
            .Build();

        emulator.Cpu.Y.Value = 0x03;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b1000_0010, emulator.Cpu.X.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDX.BASE_OP_CODE | YAddressInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC000, 0b0000_0010)])
            .Build();

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b0000_0010, emulator.Cpu.X.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    // add a negative flag check
    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDX.BASE_OP_CODE | YAddressInstruction.AbsoluteY,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC003, 0b1000_0010)])
            .Build();

        emulator.Cpu.Y.Value = 0x03;

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0b1000_0010, emulator.Cpu.X.Value);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
