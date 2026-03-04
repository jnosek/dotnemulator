namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Base class for control instructions (branching and jumping)
/// and the applicable memory addressing modes
/// </summary>
abstract class ControlInstruction : AddressInstruction
{
    // Control Address Modes

    // 0x00
    public const int Immediate = 0b000 << 2;

    // 0x04
    public const int ZeroPage = 0b001 << 2;
    
    // 0x0C
    public const int Absolute = 0b011 << 2;

    // 0x14
    public const int ZeroPageX = 0b101 << 2;

    // 0x1C
    public const int AbsoluteX = 0b111 << 2;

    protected static readonly int[] Default_Modes = 
    [
        Immediate,
        Absolute,
        AbsoluteX,
        ZeroPage,
        ZeroPageX
    ];

    protected ControlInstruction(MOS6510Cpu cpu, int baseCode, int addressMode) : base(cpu, baseCode, addressMode)
    {
        DecodeOperand = addressMode switch 
        {
            // intrinsic address modes
            // these modes are include in the coding of opcodes
            Immediate => GetImmediateOperand,
            ZeroPage => GetZeroPageOperand,
            ZeroPageX => GetZeroPageXOperand,
            Absolute => GetAbsoluteOperand,
            AbsoluteX => GetAbsoluteXOperand,
            _ => throw new InvalidOperationException($"Unsupported address mode: {addressMode}")
        };
    }

    protected override Func<int> DecodeOperand { get; }
}
