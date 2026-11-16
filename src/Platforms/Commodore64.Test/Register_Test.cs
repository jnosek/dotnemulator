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

    // --- StatusRegister ---

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

    [TestMethod]
    public void Status_Value_IsMaskedTo8Bits()
    {
        var p = new StatusRegister { Value = 0x1C3 };

        Assert.AreEqual(0xC3 | StatusFlag.Unused, p.Value);
    }

    [TestMethod]
    public void Status_ClearingEveryFlag_LeavesOnlyUnused()
    {
        var p = new StatusRegister { Value = 0xFF };

        p.CarryFlag = false;
        p.ZeroFlag = false;
        p.InterruptDisableFlag = false;
        p.DecimalModeFlag = false;
        p.OverflowFlag = false;
        p.NegativeFlag = false;

        Assert.AreEqual(StatusFlag.Unused, p.Value);
        Assert.IsFalse(p.BreakCommandFlag);
    }

    [TestMethod]
    public void Status_IsNotARegister()
    {
        // P only supports whole-value writes and named flags; inheriting Register would bring back
        // SetFlag/Increment/Decrement/Advance, which could change the fixed Unused and Break bits
        Assert.IsFalse(typeof(Register).IsAssignableFrom(typeof(StatusRegister)));
    }
}
