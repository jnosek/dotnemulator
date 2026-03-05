namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Arithmetic Shift Left (ASL) instruction. Shifts all bits of the 
/// specified memory address left by one position.
/// </summary>
class ASL : XAddressInstruction
{
    private ASL(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode)
    {
    }

    public const int BASE_OP_CODE = 0x02;

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

        var result = ShiftLeft(CPU, value);
        
        CPU.Write(address, result);
    }

    public static int ShiftLeft(MOS6510Cpu cpu, int value)
    {
        value <<= 1;
        var result = value & 0xFF;

        cpu.P.CarryFlag = (value & 0x1_00) != 0;
        cpu.P.ZeroFlag = result == 0;
        cpu.P.NegativeFlag = (result & 0x80) != 0;

        return result;
    }
}
