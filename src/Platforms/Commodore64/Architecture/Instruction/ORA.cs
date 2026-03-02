namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Logical OR on Accumulator
/// </summary>
/// <param name="cpu"></param>
/// <param name="addressMode"></param>
class ORA : AddressInstruction
{
    public const int BASE_OP_CODE = 0x01;

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

    private ORA(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        // get value at address
        var value = CPU.Read(address);

        // perform OR operation and write back to accumulator
        CPU.A.Value = CPU.A.Value | value;

        // set flags
        CPU.P.NegativeFlag = (CPU.A.Value & 0x80) != 0;
        CPU.P.ZeroFlag = CPU.A.Value == 0;
    }
}
