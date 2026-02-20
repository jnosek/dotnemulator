using System;
using Dotnemulator.Platforms.Commodore64;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Mono.Cecil.Cil;

namespace Commodore64.Test.Instructions;

using Instruction = Dotnemulator.Platforms.Commodore64.Architecture.Instruction.STA;

[TestClass]
public sealed class STA
{

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new TestEmulator([
            Instruction.BASE_OP_CODE | AddressMode.ZeroPage,
            0x03,
            0xEA
        ]);

        emulator.Accumulator = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.MemoryMap.PeekRam(0x03));

    }
}
