namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Load Y Instruction
/// </summary>
class LDY : ControlInstruction
{
    public const int BASE_OP_CODE = 0xA0;

    public static readonly int[] AddressModes = [
        Immediate,
        Absolute,
        AbsoluteX,
        ZeroPage,
        ZeroPageX
    ];

    public readonly int[] Cycles = [2, 4, 4, 3, 4];

    private LDY(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        var value = CPU.Read(address);

        CPU.Y.Value = value;

        CPU.P.NegativeFlag = (value & 0x80) != 0;
        CPU.P.ZeroFlag = value == 0;
    }
}
