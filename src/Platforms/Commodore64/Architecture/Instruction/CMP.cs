namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Compare the accumulator with a memory value.
/// </summary>
/// <param name="cpu"></param>
/// <param name="addressMode"></param>
class CMP(MOS6510Cpu cpu, int addressMode) : 
    AddressInstruction(cpu, BASE_OP_CODE, addressMode)
{
    public const int BASE_OP_CODE = 0xC1;

    public static readonly int[] AddressModes = [
        AddressMode.Immediate,
        AddressMode.Absolute,
        AddressMode.AbsoluteX,
        AddressMode.AbsoluteY,
        AddressMode.ZeroPage,
        AddressMode.ZeroPageX,
        AddressMode.Indexed_Indirect,
        AddressMode.Indirect_Indexed,
    ];

    public readonly int[] Cycles = [4, 5, 5, 3, 4, 6, 6];

    public override void Execute(int instruction, int address)
    {
        
        // get value at address
        var accumulator = CPU.A.Value;
        var value = CPU.Read(address);

        var result = accumulator - value;

        // set flags
        CPU.P.NegativeFlag = (result & 0x80) != 0;
        CPU.P.ZeroFlag = result == 0;
        CPU.P.CarryFlag = result >= 0;
    }
}
