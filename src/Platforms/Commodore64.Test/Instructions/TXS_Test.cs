using System;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class TXS_Test
{
    [TestMethod]
    public void Implied()
    {
        // arrange
        var emulator = new TestEmulator([
            TXS.OP_CODE,
            0xEA
        ])
        {
            X = 0xFA
        };

        Assert.AreEqual(0xFF, emulator.Cpu.SP);

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0xFA, emulator.Cpu.SP);
    }
}
