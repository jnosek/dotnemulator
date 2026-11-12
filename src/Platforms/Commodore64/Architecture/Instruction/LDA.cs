namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Load Accumulator Instruction
/// </summary>
/// <param name="cpu"></param>
/// <param name="addressMode"></param>
class LDA : AccumulatorInstruction
{
    public const int BASE_OP_CODE = 0xA1;

    public static readonly int[] AddressModes = Default_Modes;

    public readonly int[] Cycles = [4, 5, 5, 3, 4, 6, 6];

    private LDA(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        // get value at address
        var value = CPU.Read(address);
        CPU.A.Value = value;

        // set flags
        CPU.P.NegativeFlag = (CPU.A.Value & 0x80) != 0;
        CPU.P.ZeroFlag = CPU.A.Value == 0;
    }
}
