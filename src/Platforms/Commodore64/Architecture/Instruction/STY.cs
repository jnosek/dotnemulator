namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class STY : AddressInstruction
{
    public const int BASE_OP_CODE = 0x80;

    public static readonly int[] AddressModes = [
        AddressMode.Absolute,
        AddressMode.ZeroPage,
        AddressMode.ZeroPageX
    ];

    public static readonly int[] Cycles = [4, 3, 4];

    private STY(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        var y = CPU.Y.Value;

        CPU.Write(address, y);
    }
}
