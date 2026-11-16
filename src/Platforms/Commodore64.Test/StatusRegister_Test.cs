using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class StatusRegister_Test
{
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
