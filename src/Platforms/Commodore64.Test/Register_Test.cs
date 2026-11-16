using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class Register_Test
{
    [TestMethod]
    public void Value_IsMaskedToSize()
    {
        var register = new Register(8) { Value = 0x1FF };

        Assert.AreEqual(0xFF, register.Value);
    }

    [TestMethod]
    public void Increment_WrapsAndReturnsNewValue()
    {
        var register = new Register(8) { Value = 0xFF };

        Assert.AreEqual(0x00, register.Increment());
        Assert.AreEqual(0x00, register.Value);
    }

    [TestMethod]
    public void Decrement_WrapsAndReturnsNewValue()
    {
        var register = new Register(8) { Value = 0x00 };

        Assert.AreEqual(0xFF, register.Decrement());
        Assert.AreEqual(0xFF, register.Value);
    }

    [TestMethod]
    public void Advance_ReturnsOldValue_ThenIncrements()
    {
        var register = new Register(16) { Value = 0xFFFF };

        Assert.AreEqual(0xFFFF, register.Advance());
        Assert.AreEqual(0x0000, register.Value);
    }

    [TestMethod]
    public void SetFlag_SetsAndClears_GetFlag_Reads()
    {
        var register = new Register(8);

        register.SetFlag(0b0000_0100, true);
        Assert.IsTrue(register.GetFlag(0b0000_0100));
        Assert.AreEqual(0b0000_0100, register.Value);

        register.SetFlag(0b0000_0100, false);
        Assert.IsFalse(register.GetFlag(0b0000_0100));
        Assert.AreEqual(0, register.Value);
    }

    // --- StatusRegister: fixed bits hold for every kind of write ---

    [TestMethod]
    public void Status_StartsWithOnlyUnusedSet()
    {
        var p = new StatusRegister();

        Assert.AreEqual(StatusFlag.Unused, p.Value);
    }

    [TestMethod]
    public void Status_Value_ForcesUnusedOn_BreakOff()
    {
        var p = new StatusRegister { Value = 0x00 };
        Assert.AreEqual(StatusFlag.Unused, p.Value);

        p.Value = 0xFF;
        Assert.AreEqual(0xFF & ~StatusFlag.BreakCommand, p.Value);
    }

    [TestMethod]
    public void Status_SetFlag_CannotSetBreak()
    {
        var p = new StatusRegister();

        p.SetFlag(StatusFlag.BreakCommand, true);

        Assert.IsFalse(p.BreakCommandFlag);
        Assert.AreEqual(StatusFlag.Unused, p.Value);
    }

    [TestMethod]
    public void Status_SetFlag_CannotClearUnused()
    {
        var p = new StatusRegister();

        p.SetFlag(StatusFlag.Unused, false);

        Assert.IsTrue(p.GetFlag(StatusFlag.Unused));
    }

    [TestMethod]
    public void Status_IncrementAndDecrement_KeepFixedBits()
    {
        var p = new StatusRegister { Value = 0b1110_1111 };   // B would be set by +1

        p.Increment();
        Assert.IsFalse(p.BreakCommandFlag);
        Assert.IsTrue(p.GetFlag(StatusFlag.Unused));

        p.Value = StatusFlag.Unused;                         // Unused would be cleared by -1
        p.Decrement();
        Assert.IsTrue(p.GetFlag(StatusFlag.Unused));
        Assert.IsFalse(p.BreakCommandFlag);
    }

    [TestMethod]
    public void Status_FlagProperties_RoundTrip()
    {
        var p = new StatusRegister
        {
            CarryFlag = true,
            ZeroFlag = true,
            InterruptDisableFlag = true,
            DecimalModeFlag = true,
            OverflowFlag = true,
            NegativeFlag = true
        };

        Assert.AreEqual(0xFF & ~StatusFlag.BreakCommand, p.Value);

        p.CarryFlag = false;
        Assert.IsFalse(p.CarryFlag);
        Assert.IsTrue(p.ZeroFlag);
    }
}
