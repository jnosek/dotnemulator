namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// LDX (Load X Register) instruction
/// </summary>
class LDX : YAddressInstruction
{
     public const int BASE_OP_CODE = 0xA2;

    public static readonly int[] AddressModes = Default_Modes;

    public readonly int[] Cycles = [2, 4, 4, 3, 4];

    private LDX(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        var value = CPU.Read(address);

        CPU.X.Value = value;

        CPU.P.NegativeFlag = (value & 0x80) != 0;
        CPU.P.ZeroFlag = value == 0;
    }
}
