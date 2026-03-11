using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class PLA_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                PLA.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .Build();

        emulator.Cpu.StackPush(0x11);

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x11, emulator.Cpu.A.Value);
        Assert.AreEqual(0, emulator.Cpu.P.Value);

        // stack reset
        Assert.AreEqual(MOS6510Cpu.STACK_START_ADDRESS & 0xFF, emulator.Cpu.SP);
    }

    [TestMethod]
    public void Implied_NegativeValue()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                PLA.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .Build();

        emulator.Cpu.StackPush(0x81);

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x81, emulator.Cpu.A.Value);
        Assert.AreEqual(StatusFlag.Negative, emulator.Cpu.P.Value);

        // stack reset
        Assert.AreEqual(MOS6510Cpu.STACK_START_ADDRESS & 0xFF, emulator.Cpu.SP);
    }

    [TestMethod]
    public void Implied_ZeroValue()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                PLA.OP_CODE,
                // this is skipped by the break instruction            
                0xEA
            ])
            .Build();

        emulator.Cpu.StackPush(0x0);

        // act
        emulator.Cpu.Test();

        // assert
        Assert.AreEqual(0x0, emulator.Cpu.A.Value);
        Assert.AreEqual(StatusFlag.Zero, emulator.Cpu.P.Value);

        // stack reset
        Assert.AreEqual(MOS6510Cpu.STACK_START_ADDRESS & 0xFF, emulator.Cpu.SP);
    }
}
