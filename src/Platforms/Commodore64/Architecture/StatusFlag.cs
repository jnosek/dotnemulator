namespace Dotnemulator.Platforms.Commodore64.Architecture;

static class StatusFlag
{
    public const int Carry = 0b0000_0001;
    public const int Zero = 0b0000_0010;
    public const int InterruptDisable = 0b0000_0100;
    public const int DecimalMode = 0b0000_1000;
    public const int BreakCommand = 0b0001_0000;
    public const int Unused = 0b0010_0000;
    public const int Overflow = 0b0100_0000;
    public const int Negative = 0b1000_0000;
}