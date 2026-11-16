namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Decrease Instruction
/// </summary>
class DEC : XAddressInstruction
{
    public const int BASE_OP_CODE = 0xC2;

    public static readonly int[] AddressModes = [
        Absolute,
        AbsoluteX,
        ZeroPage,
        ZeroPageX
    ];

    public static readonly int[] Cycles = [6, 7, 5, 6];

    private DEC(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        var value = CPU.Read(address);

        var result = (value - 1) & 0xFF;

        CPU.Write(address, result);

        // set flags
        CPU.P.NegativeFlag = (0x80 & result) != 0;
        CPU.P.ZeroFlag = result == 0;
    }
}
