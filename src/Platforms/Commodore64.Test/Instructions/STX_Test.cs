using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class STX_Test
{
    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STX.BASE_OP_CODE | YAddressInstruction.ZeroPage,
                0x03,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0x03));
    }

    [TestMethod]
    public void ZeroPageY()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STX.BASE_OP_CODE | YAddressInstruction.ZeroPageY,
                0x03,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0b1010_1010;
        emulator.Cpu.Y.Value = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0x06));
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                STX.BASE_OP_CODE | YAddressInstruction.Absolute,
                0x00,
                0xC0,
                0xEA
            ])
            .Build();

        emulator.Cpu.X.Value = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0xC000));
    }
}
