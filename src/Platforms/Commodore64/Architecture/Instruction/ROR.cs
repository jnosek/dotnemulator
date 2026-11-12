namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// ROR (Rotate Right) instruction. Shifts all bits of the value at the specified memory location right by one position.
/// The bit that was in the carry flag is rotated into bit 7, and the bit that was in bit 0 is rotated into the carry flag.
/// </summary>
class ROR : XAddressInstruction
{
    public const int BASE_OP_CODE = 0x62;

    public static readonly int[] AddressModes =
    [
        Absolute,
        AbsoluteX,
        ZeroPage,
        ZeroPageX
    ];

    private ROR(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode)
    {
    }

    public readonly int[] Cycles = [6, 7, 5, 6];

    public override void Execute(int instruction, int address)
    {
        var value = CPU.Read(address);

        var result = RotateRight(CPU, value);
        
        CPU.Write(address, result);
    }

    public static int RotateRight(MOS6510Cpu cpu, int value)
    {
        // grab new carry before shit
        var carry = (value & 0x01) != 0;

        // shift and bring in current carry
        value >>= 1;
        var result = (value & 0xFF) | (cpu.P.CarryFlag ? 0x80 : 0x00);

        // set new carry and check flags
        cpu.P.CarryFlag = carry;
        cpu.P.ZeroFlag = result == 0;
        cpu.P.NegativeFlag = (result & 0x80) != 0;

        return result;
    }
}
