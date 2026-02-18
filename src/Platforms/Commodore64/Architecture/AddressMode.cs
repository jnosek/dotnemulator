namespace Dotnemulator.Platforms.Commodore64.Architecture;

public enum AddressMode
{
    Indexed_Indirect = 0b000,
    ZeroPage = 0b001,
    Immediate = 0b010,
    Absolute = 0b011,
    
    Indirect_Indexed = 0b100,
    ZeroPageX = 0b101,
    AbsoluteY = 0b110,
    AbsoluteX = 0b111
}
