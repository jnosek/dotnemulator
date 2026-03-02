namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Jump Instruction
/// </summary>
class JMP : AddressInstruction
{
    private JMP(MOS6510Cpu cpu, int opCode, int addressMode) : 
        base(cpu, opCode, addressMode)
    {
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
        new JMP(cpu, ABSOLUTE_OP_CODE, AddressMode.Explicit_Absolute),
        new JMP(cpu, INDIRECT_OP_CODE, AddressMode.Indirect)
    ];

    public static readonly int[] Cycles = [3, 5];

    public const int ABSOLUTE_OP_CODE = 0x4C;
    public const int INDIRECT_OP_CODE = 0x6C;

   public override void Execute(int instruction, int address)
    {

        CPU.PC.Value = address;
    }
}
