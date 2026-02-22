using System;
using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64.Architecture;

internal class StatusRegister : Register
{

    public StatusRegister() : base(8)
    {
    }

    public bool CarryFlag
    {
        get => GetFlag(StatusFlag.Carry);
        set => SetFlag(StatusFlag.Carry, value);
    }

    public bool ZeroFlag
    {
        get => GetFlag(StatusFlag.Zero);
        set => SetFlag(StatusFlag.Zero, value);
    }

    public bool InterruptDisableFlag
    {
        get => GetFlag(StatusFlag.InterruptDisable);
        set => SetFlag(StatusFlag.InterruptDisable, value);
    }

    public bool DecimalModeFlag
    {
        get => GetFlag(StatusFlag.DecimalMode);
        set => SetFlag(StatusFlag.DecimalMode, value);
    }

    public bool BreakCommandFlag
    {
        get => GetFlag(StatusFlag.BreakCommand);
        set => SetFlag(StatusFlag.BreakCommand, value);
    }

    public bool OverflowFlag
    {
        get => GetFlag(StatusFlag.Overflow);
        set => SetFlag(StatusFlag.Overflow, value);
    }

    public bool NegativeFlag
    {
        get => GetFlag(StatusFlag.Negative);
        set => SetFlag(StatusFlag.Negative, value);
    }
}
