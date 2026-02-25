using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class PHP_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            PHP.OP_CODE,
            // this is skipped by the break instruction            
            0xEA
        ])
        {
            P = StatusFlag.Zero
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(StatusFlag.Zero, emulator.Cpu.P.Value);

        // stack - status register
        Assert.AreEqual(
            StatusFlag.Zero, 
            emulator.PeekRam(MOS6510Cpu.STACK_START_ADDRESS));
    }
}
