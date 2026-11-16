using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// The 6502/6510 processor status register, P.
/// </summary>
/// <remarks>
/// Two bits are fixed: the Unused bit always reads as 1, and the Break (B) bit is always 0 in the register
/// itself. B only exists in the copy of P pushed to the stack by <c>BRK</c> and <c>PHP</c>, so a handler can tell a
/// software interrupt from a hardware one. Both rules are applied in <see cref="Coerce"/>, so they hold for every
/// write: <see cref="Register.Value"/>, <see cref="Register.SetFlag"/> and the increment/decrement methods.
/// </remarks>
internal class StatusRegister : Register
{
    public StatusRegister() : base(8)
    {
        // stores 0 through Coerce, which sets the Unused bit
        Value = 0;
    }

    /// <summary>
    /// Forces the Unused bit on and the Break bit off, then masks to 8 bits.
    /// </summary>
    protected override int Coerce(int value)
    {
        // the unused flag is always set to 1
        value |= StatusFlag.Unused;

        // the break command flag is always clear in the register
        // (it is only set in the copy pushed to the stack by a software instruction)
        value &= ~StatusFlag.BreakCommand;

        return base.Coerce(value);
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
