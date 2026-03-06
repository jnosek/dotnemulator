namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Rotate Right Accumulator (RORA) instruction. Shifts all bits of the A register right by one position. 
/// The bit that was in the carry flag is rotated into bit 7, and the bit that was in bit 0 is rotated into the carry flag.
/// </summary>
/// <param name="cpu"></param>
class RORA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
     public const int OP_CODE = 0x6A;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        var value = CPU.A.Value;
        var result = ROR.RotateRight(CPU, value);
        CPU.A.Value = result;
    }
}
