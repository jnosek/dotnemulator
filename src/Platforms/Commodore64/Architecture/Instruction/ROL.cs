namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Rotate Left (ROL) instruction. Rotates all bits of the specified memory
/// address left by one position. The bit that was in the carry flag is rotated 
/// into bit 0, and the bit that was in bit 7 is rotated into the carry flag.
/// </summary>
class ROL : XAddressInstruction
{
    public const int BASE_OP_CODE = 0x22;

    public static readonly int[] AddressModes =
    [
        Absolute,
        AbsoluteX,
        ZeroPage,
        ZeroPageX
    ];

    private ROL(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode)
    {
    }

    public readonly int[] Cycles = [6, 7, 5, 6];

    public override void Execute(int instruction, int address)
    {
        var value = CPU.Read(address);

        var result = RotateLeft(CPU, value);
        
        CPU.Write(address, result);
    }

    public static int RotateLeft(MOS6510Cpu cpu, int value)
    {
        // rotate left
        value <<= 1;

        // truncate value and add current carry to bit 0
        var result = (value & 0xFF) | (cpu.P.CarryFlag ? 0x01 : 0x00);

        // set new carry and check flags
        cpu.P.CarryFlag = (value & 0x1_00) != 0;
        cpu.P.ZeroFlag = result == 0;
        cpu.P.NegativeFlag = (result & 0x80) != 0;

        return result;
    }
}
