namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Store Accumulator Instruction
/// </summary>
class STA(MOS6510Cpu cpu, int addressMode) : 
    AddressModeInstruction(cpu, BASE_OP_CODE, addressMode)
{
    public const int BASE_OP_CODE = 0x81;

    public static readonly int[] AddressModes = [
        AddressMode.Absolute,
        AddressMode.AbsoluteX,
        AddressMode.AbsoluteY,
        AddressMode.ZeroPage,
        AddressMode.ZeroPageX,
        AddressMode.Indexed_Indirect,
        AddressMode.Indirect_Indexed,
    ];

    public readonly int[] Cycles = [4, 5, 5, 3, 4, 6, 6];

    public override void Execute(int instruction)
    {
        var address = DecodeOperand();
        CPU.Write(address, CPU.A.Read());
    }
}
