using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64.Architecture;

internal class StatusRegister : Register
{

    public StatusRegister() : base(8)
    {
        // the unused flag is always set to 1
        SetFlag(StatusFlag.Unused, true);
    }

    override public int Value
    {
        get => base.Value;
        set
        {
            // ensure that the unused flag is always set to 1
            value |= StatusFlag.Unused;

            // ensure that the break command flag cleared
            // (it is only marked as set when the flags are pushed to the stack by a software instruction)
            value &= ~StatusFlag.BreakCommand;

            base.Value = value;
        }
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

    /// <summary>
    /// Used to indicate that a BRK instruction has been executed, or that an interrupt is being processed
    /// </summary>
    /// <remarks>
    /// In actuality it is used to indicate that the processor flags were added to the stack by a software instruction,
    /// and not an interrupt. The PHP instruction also sets this flag.
    /// </remarks>
    public bool BreakCommandFlag
    {
        get => GetFlag(StatusFlag.BreakCommand);
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
