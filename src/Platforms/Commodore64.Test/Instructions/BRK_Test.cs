using System;
using System.Diagnostics.Contracts;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class BRK_Test
{

    [TestMethod]
    public void Break()
    {
        // arrange
        var emulator = new TestEmulator([
            BRK.OP_CODE,
            // this is skipped by the break instruction            
            0xEA
        ],
        // place NOP at IRQ Handler to end test
        [ (0x0100, 0xEA)]);

        // act
        emulator.Start();

        // assert
        // Current PC
        Assert.AreEqual(0x0101, emulator.Cpu.PC.Read());

        // stack - status register
        Assert.AreEqual(
            StatusFlag.BreakCommand, 
            emulator.Cpu.StackPop());

        // stack - return PC
        Assert.AreEqual(0x02, emulator.Cpu.StackPop());
        Assert.AreEqual(0xE0 , emulator.Cpu.StackPop());

        // status register
        Assert.IsTrue(emulator.Cpu.P.BreakCommandFlag);
        Assert.IsTrue(emulator.Cpu.P.InterruptDisableFlag);
    }
}
