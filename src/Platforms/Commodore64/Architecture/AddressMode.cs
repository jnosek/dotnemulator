namespace Dotnemulator.Platforms.Commodore64.Architecture;

public static class AddressMode
{
    // These address modes and intrinsic to the structure of the opcode

    // 0x00
    public const int Indexed_Indirect = 0b000 << 2;

    // 0x04
    public const int ZeroPage = 0b001 << 2;

    // 0x08
    public const int Immediate = 0b010 << 2;

    // 0x0C
    public const int Absolute = 0b011 << 2;

    // 0x10
    public const int Indirect_Indexed = 0b100 << 2;

    // 0x14
    public const int ZeroPageX = 0b101 << 2;

    // 0x18
    public const int AbsoluteY = 0b110 << 2;

    // 0x1C
    public const int AbsoluteX = 0b111 << 2;

    // these address modes are explicit and not represented as part of the opcode
    // they are specified by instruction that needs them

    /// <summary>
    /// Implied should not bused with the AddressModeInstruction class
    /// This is added for syntax completeness
    /// </summary>
    public const int Implied = 0x00_00;
    public const int Relative = 0x01_10;

    public const int Explicit_Indexed_Indirect = 0x1000 | (0b000 << 2);
    public const int Explicit_ZeroPage = 0x1000 | (0b001 << 2);
    public const int Explicit_Immediate = 0x1000 | (0b010 << 2);
    public const int Explicit_Absolute = 0x1000 | (0b011 << 2);
    public const int Explicit_Indirect_Indexed = 0x1000 | (0b100 << 2);
    public const int Explicit_ZeroPageX = 0x1000 | (0b101 << 2);
    public const int Explicit_AbsoluteY = 0x1000 | (0b110 << 2);
    public const int Explicit_AbsoluteX = 0x1000 | (0b111 << 2);

    public const int Undefined = 0xFF00;
}
