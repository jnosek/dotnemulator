using Dotnemulator.Platforms.Commodore64;
using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test;

namespace Commodore64.Test.Instructions;


[TestClass]
public sealed class STA_Test
{

    [TestMethod]
    public void ZeroPage()
    {
        // arrange
        var emulator = new TestEmulator([
            STA.BASE_OP_CODE | AddressMode.ZeroPage,
            0x03,
            0xEA
        ]);

        emulator.Accumulator = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0x03));
    }

    [TestMethod]
    public void ZeroPageX()
    {
        // arrange
        var emulator = new TestEmulator([
            STA.BASE_OP_CODE | AddressMode.ZeroPageX,
            0x03,
            0xEA
        ]);

        emulator.Accumulator = 0b1010_1010;
        emulator.X = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0x06));
    }

    [TestMethod]
    public void Absolute()
    {
        // arrange
        var emulator = new TestEmulator([
            STA.BASE_OP_CODE | AddressMode.Absolute,
            0x00,
            0xC0,
            0xEA
        ]);

        emulator.Accumulator = 0b1010_1010;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0xC000));
    }

    [TestMethod]
    public void AbsoluteX()
    {
        // arrange
        var emulator = new TestEmulator([
            STA.BASE_OP_CODE | AddressMode.AbsoluteX,
            0x00,
            0xC0,
            0xEA
        ]);

        emulator.Accumulator = 0b1010_1010;
        emulator.X = 0x03;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0xC003));
    }

    [TestMethod]
    public void AbsoluteY()
    {
        // arrange
        var emulator = new TestEmulator([
            STA.BASE_OP_CODE | AddressMode.AbsoluteY,
            0x00,
            0xC0,
            0xEA
        ]);

        emulator.Accumulator = 0b1010_1010;
        emulator.Y = 0x06;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0xC006));
    }

    [TestMethod]
    public void IndexedIndirect()
    {
        // arrange
        var emulator = new TestEmulator([
            STA.BASE_OP_CODE | AddressMode.Indexed_Indirect,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x06, 0x06),
            (0x07, 0xC0)])
        {
            Accumulator = 0b1010_1010,
            X = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0xC006));
    }   

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange
        var emulator = new TestEmulator(
        // rom
        [
            STA.BASE_OP_CODE | AddressMode.Indirect_Indexed,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x02, 0x06),
            (0x03, 0xC0)])
        {
            Accumulator = 0b1010_1010,
            Y = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1010_1010, emulator.PeekRam(0xC00A));
    }
}
