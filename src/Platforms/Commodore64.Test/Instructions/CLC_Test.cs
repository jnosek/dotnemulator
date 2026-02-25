using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class CLC_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            CLC.OP_CODE,
            0xEA
        ])
        {
            P = StatusFlag.Carry
        };

        // act
        emulator.Start();

        // assert
        // Current P
        Assert.AreEqual(0, emulator.Cpu.P.Value);
    }
}
