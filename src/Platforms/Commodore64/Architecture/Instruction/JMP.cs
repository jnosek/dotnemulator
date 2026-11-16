namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Jump Instruction
/// </summary>
class JMP : AddressInstruction
{
    // Address Modes

    // 0x0C
    private const int Absolute = 0x0C;
    private const int Indirect = 0x0C;

    // op codes
    public const int ABSOLUTE_OP_CODE = 0x4C;
    public const int INDIRECT_OP_CODE = 0x6C;

    private JMP(MOS6510Cpu cpu, int opCode, int addressMode) : 
        base(cpu, opCode, addressMode)
    {
        DecodeOperand = opCode switch {
            ABSOLUTE_OP_CODE => GetAbsoluteOperand,
            INDIRECT_OP_CODE => GetIndirectOperand,
             _ => throw new InvalidOperationException($"Unsupported address mode: {addressMode}")
        };
    }

    /// <summary>
    /// Build the jump instructions for its two possible address modes
    /// </summary>
    /// <remarks>
    /// JMP is an exception in the instruction set, as its two addressable modes
    /// have different base opcodes
    /// <remarks>
    /// <param name="cpu"></param>
    /// <returns></returns>
    public static JMP[] Build(MOS6510Cpu cpu) => [
        new JMP(cpu, ABSOLUTE_OP_CODE, Absolute),
        new JMP(cpu, INDIRECT_OP_CODE, Indirect)
    ];

    public static readonly int[] Cycles = [3, 5];



    protected override Func<int> DecodeOperand { get; }

    public override void Execute(int instruction, int address)
    {

        CPU.PC.Value = address;
    }
}
