using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class BCC_Test
{
    [TestMethod]
    public void Relative_Branch()
    {
        // arrange
        var emulator = new TestEmulator([
            BCC.OP_CODE,
            0x03,
            0x00,
            0x00,
            0x00,
            0xEA
        ])
        {
            P = ~StatusFlag.Carry
        };

        // act
        emulator.Start();

        // assert
        // kernel start + anticipated offset + ending NOP instruction
        Assert.AreEqual(0xE000 + 5 + 1, emulator.Cpu.PC.Value);
    }

    [TestMethod]
    public void Relative_NoBranch()
    {
        // arrange
        var emulator = new TestEmulator([
            BCC.OP_CODE,
            0x03,
            0xEA,
            0x00,
            0x00,
            0x00
        ])
        {
            P = StatusFlag.Carry
        };

        // act
        emulator.Start();

        // assert
        // kernel start + anticipated offset + ending NOP instruction
        Assert.AreEqual(0xE000 + 2 + 1, emulator.Cpu.PC.Value);
    }
}
