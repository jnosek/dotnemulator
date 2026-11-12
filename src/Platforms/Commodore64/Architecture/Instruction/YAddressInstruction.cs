namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Base class for instructions that function on addresses 
/// with a Y register offset, such as Zero Page,Y or Absolute,Y
/// </summary>
abstract class YAddressInstruction : AddressInstruction
{
     // Address Modes

    // 0x00
    public const int Immediate = 0b000 << 2;

    // 0x04
    public const int ZeroPage = 0b001 << 2;
    
    // 0x0C
    public const int Absolute = 0b011 << 2;

    // 0x14
    public const int ZeroPageY = 0b101 << 2;

    // 0x1C
    public const int AbsoluteY = 0b111 << 2;

    protected static readonly int[] Default_Modes = 
    [
        Immediate,
        Absolute,
        AbsoluteY,
        ZeroPage,
        ZeroPageY
    ];

    protected YAddressInstruction(MOS6510Cpu cpu, int baseCode, int addressMode) : base(cpu, baseCode, addressMode)
    {
        DecodeOperand = addressMode switch 
        {
            // intrinsic address modes
            // these modes are include in the coding of opcodes
            Immediate => GetImmediateOperand,
            ZeroPage => GetZeroPageOperand,
            ZeroPageY => GetZeroPageYOperand,
            Absolute => GetAbsoluteOperand,
            AbsoluteY => GetAbsoluteYOperand,
            _ => throw new InvalidOperationException($"Unsupported address mode: {addressMode}")
        };
    }

    protected override Func<int> DecodeOperand { get; }
}
