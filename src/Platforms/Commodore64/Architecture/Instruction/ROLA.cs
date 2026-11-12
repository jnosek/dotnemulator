namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Rotate Left Accumulator (ROLA) instruction. Shifts all bits of the A register left by one position. 
/// The bit that was in the carry flag is rotated into bit 0, and the bit that was in bit 7 is rotated into the carry flag.
/// </summary>
/// <param name="cpu"></param>
class ROLA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x2A;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        var value = CPU.A.Value;
        var result = ROL.RotateLeft(CPU, value);
        CPU.A.Value = result;
    }
}
