using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Test.Mocks;

namespace Dotnemulator.Platforms.Commodore64.Test.Instructions;

[TestClass]
public class ADC_Test
{
    [TestMethod]
    public void Immediate()
    {
        // arrange
        var emulator = new TestEmulator([
            ADC.BASE_OP_CODE | AddressMode.Immediate,
            0b0000_0100,
            0xEA
        ])
        {
            Accumulator = 0b0000_0101
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_1001, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void ZeroPage_WithCarry()
    {
        // arrange
        var emulator = new TestEmulator([
            ADC.BASE_OP_CODE | AddressMode.ZeroPage,
            0x03,
            0xEA
        ],
        [ (0x3, 0b1111_1111) ])
        {
            Accumulator = 0b0000_0010
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void ZeroPageX_WithOverflow()
    {
        // arrange
        var emulator = new TestEmulator([
            ADC.BASE_OP_CODE | AddressMode.Immediate,
            0b1111_1111,
            0xEA
        ])
        {
            Accumulator = 0b1000_0000
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0111_1111, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsTrue(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void Absolute_BDC()
    {
        // arrange
        var emulator = new TestEmulator([
            ADC.BASE_OP_CODE | AddressMode.Absolute,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC000, 0b0001_0010) ])
        {
            Accumulator = 0b0011_0001
        };

        emulator.Cpu.P.DecimalModeFlag = true;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0100_0011, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void AbsoluteX_BDC_WithCarry()
    {
        // arrange
        var emulator = new TestEmulator([
            ADC.BASE_OP_CODE | AddressMode.AbsoluteX,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC003, 0b1001_0000) ])
        {
            X = 0x03,
            Accumulator = 0b0001_0001
        };

        emulator.Cpu.P.DecimalModeFlag = true;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0001, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void AbsoluteY_WithCarryIn()
    {
        // arrange
        var emulator = new TestEmulator([
            ADC.BASE_OP_CODE | AddressMode.AbsoluteY,
            0x00,
            0xC0,
            0xEA
        ],
        [ (0xC003, 0b1001_0001) ])
        {
            Y = 0x03,
            Accumulator = 0b0000_0001
        };

        emulator.Cpu.P.CarryFlag = true;

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b1001_0011, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsTrue(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void IndexedIndirect_ZeroCheck()
    {
        // arrange
        var emulator = new TestEmulator([
            ADC.BASE_OP_CODE | AddressMode.Indexed_Indirect,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x06, 0x06),
            (0x07, 0xC0),
            (0xC006, 0b1111_1111)])
        {
            Accumulator = 0b0000_0001,
            X = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_0000, emulator.Accumulator);
        Assert.IsTrue(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsTrue(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }

    [TestMethod]
    public void IndirectIndexed()
    {
        // arrange
        var emulator = new TestEmulator(
        // rom
        [
            ADC.BASE_OP_CODE | AddressMode.Indirect_Indexed,
            0x02,
            0xEA
        ],
        // ram
        [
            (0x02, 0x06),
            (0x03, 0xC0),
            (0xC00A, 0b0000_0011)])
        {
            Accumulator = 0b0000_0101,
            Y = 0x04
        };

        // act
        emulator.Start();

        // assert
        Assert.AreEqual(0b0000_1000, emulator.Accumulator);
        Assert.IsFalse(emulator.Cpu.P.ZeroFlag);
        Assert.IsFalse(emulator.Cpu.P.NegativeFlag);
        Assert.IsFalse(emulator.Cpu.P.CarryFlag);
        Assert.IsFalse(emulator.Cpu.P.OverflowFlag);
    }
}
