namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Compare X Register (CPX) instruction
/// </summary>
class CPX: XAddressInstruction
{
    public const int BASE_OP_CODE = 0xE0;

    public static readonly int[] AddressModes = [
        Immediate,
        ZeroPage,
        Absolute
    ];

    public static readonly int[] Cycles = [2, 3, 4];

    private CPX(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        var x = CPU.X.Value;
        var value = CPU.Read(address);

        var result = x - value;

        CPU.P.NegativeFlag = (0x80 & result) != 0;
        CPU.P.ZeroFlag = result == 0;
        CPU.P.CarryFlag = result >= 0;
    }
}
