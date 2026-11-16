namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// LSRA (Logical Shift Right Accumulator) instruction. 
/// Shifts all bits of the A register right by one position. 
/// A zero is rotated into bit 7, and the bit that was in bit 0 is rotated into the carry flag.
/// </summary>
/// <param name="cpu"></param>
class LSRA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x4A;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        var value = CPU.A.Value;
        var result = LSR.ShiftRight(CPU, value);
        CPU.A.Value = result;
    }
}
