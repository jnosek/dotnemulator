namespace Dotnemulator.Platforms.Commodore64.Architecture;

public static class AddressMode
{
    public const int Implied = 0x00;
    public const int Relative = 0x10;

    public const int Indexed_Indirect = 0b000 << 2;
    public const int ZeroPage = 0b001 << 2;
    public const int Immediate = 0b010 << 2;
    public const int Absolute = 0b011 << 2;
    public const int Indirect_Indexed = 0b100 << 2;
    public const int ZeroPageX = 0b101 << 2;
    public const int AbsoluteY = 0b110 << 2;
    public const int AbsoluteX = 0b111 << 2;
    
    public const int Undefined = 0xFF;
}
