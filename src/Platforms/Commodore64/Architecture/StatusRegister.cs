namespace Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// The 6502/6510 processor status register, P.
/// </summary>
/// <remarks>
/// <para>
/// On the real chip P is a set of individual flag bits, not a general-purpose register: it is never incremented or
/// decremented, only read or written whole (by <c>PHP</c>/<c>PLP</c>, <c>BRK</c>/<c>RTI</c> and the interrupt
/// sequence) or changed one flag at a time. So this class does <b>not</b> derive from
/// <see cref="Abstraction.Hardware.Register"/>; it exposes only <see cref="Value"/> and the named flags.
/// </para>
/// <para>
/// Two bits are fixed: the Unused bit always reads as 1, and the Break (B) bit is always 0 in the register itself.
/// B only exists in the copy of P pushed to the stack by <c>BRK</c> and <c>PHP</c>, so a handler can tell a software
/// interrupt from a hardware one. Every way of writing P goes through <see cref="Value"/> or a flag property, and
/// none of them can change those two bits.
/// </para>
/// </remarks>
internal class StatusRegister
{
    private int _value = StatusFlag.Unused;

    /// <summary>
    /// All eight bits of P.
    /// </summary>
    /// <remarks>
    /// Setting it masks to 8 bits, forces the Unused bit on and the Break bit off.
    /// </remarks>
    public int Value
    {
        get => _value;
        set
        {
            // ensure that the unused flag is always set to 1
            value |= StatusFlag.Unused;

            // ensure that the break command flag cleared
            // (it is only marked as set when the flags are pushed to the stack by a software instruction)
            value &= ~StatusFlag.BreakCommand;

            _value = value & 0xFF;
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
    /// and not an interrupt. The PHP instruction also sets this flag. In the register itself it is always <c>false</c>.
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

    private bool GetFlag(int flag) => (_value & flag) != 0;

    /// <remarks>
    /// Private, and only called with the flags that have a settable property above, so the Unused and Break bits
    /// can never be changed through it.
    /// </remarks>
    private void SetFlag(int flag, bool value)
    {
        if(value)
            _value |= flag;
        else
            _value &= ~flag;
    }
}
