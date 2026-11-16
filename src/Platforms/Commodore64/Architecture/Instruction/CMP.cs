namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Compare the accumulator with a memory value.
/// </summary>
/// <param name="cpu"></param>
/// <param name="addressMode"></param>
class CMP : AccumulatorInstruction
{
    public const int BASE_OP_CODE = 0xC1;

    public static readonly int[] AddressModes = Default_Modes;

    public readonly int[] Cycles = [4, 5, 5, 3, 4, 6, 6];

    private CMP(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

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
