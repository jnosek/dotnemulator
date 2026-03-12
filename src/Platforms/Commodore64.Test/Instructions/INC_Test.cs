using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class INC_Test
{
    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                INC.BASE_OP_CODE | XAddressInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC000, 0b0000_0010)])
            .Build();

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0011, emulator.MemoryMap.PeekRam(0xC000));

        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void AbsoluteX_WithZeroWrapAround()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                INC.BASE_OP_CODE | XAddressInstruction.AbsoluteX,
                0x00,
                0xC0,
                0xEA
            ])
            .WithRam([(0xC003, 0xFF)])
            .Build();

        emulator.Cpu.X.Value = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0x00, emulator.MemoryMap.PeekRam(0xC003));

        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                INC.BASE_OP_CODE | XAddressInstruction.ZeroPage,
                0x03,
                0xEA
            ])
            .WithRam([(0x0003, 0x06)])
            .Build();

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0x07, emulator.MemoryMap.PeekRam(0x0003));

        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
    }

    [TestMethod]
    public void ZeroPageX_WithNegative()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                INC.BASE_OP_CODE | XAddressInstruction.ZeroPageX,
                0x03,
                0xEA
            ])
            .WithRam([(0x0006, 0b1100_0000)])
            .Build();

        emulator.Cpu.X.Value = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1100_0001, emulator.MemoryMap.PeekRam(0x0006));

        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
    }
}
