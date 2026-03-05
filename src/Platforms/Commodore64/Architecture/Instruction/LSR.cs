namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Logical Shift Right (LSR) instruction. Shifts all bits of the value at the specified memory address right by one position.
/// A zero is rotated into bit 7, and the bit that was in bit 0 is rotated into the carry flag.
/// </summary>
class LSR : XAddressInstruction
{
    private LSR(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode)
    {
    }

    public const int BASE_OP_CODE = 0x42;

    public static readonly int[] AddressModes =
    [
        Absolute,
        AbsoluteX,
        ZeroPage,
        ZeroPageX
    ];

    public readonly int[] Cycles = [6, 7, 5, 6];

    public override void Execute(int instruction, int address)
    {
        var value = CPU.Read(address);

        var result = ShiftRight(CPU, value);
        
        CPU.Write(address, result);
    }

    public static int ShiftRight(MOS6510Cpu cpu, int value)
    {
        var result = (value & 0xFF) >> 1;

        cpu.P.CarryFlag = (value & 0x01) != 0;
        cpu.P.ZeroFlag = result == 0;
        cpu.P.NegativeFlag = false;

        return result;
    }
}
