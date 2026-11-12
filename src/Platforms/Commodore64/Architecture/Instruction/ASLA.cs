namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// ASLA (Arithmetic Shift Left Accumulator) instruction
/// Implied addressing mode, operates directly on the A register
/// </summary>
/// <param name="cpu"></param>
class ASLA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x0A;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        var value = CPU.A.Value;
        var result = ASL.ShiftLeft(CPU, value);
        CPU.A.Value = result;
    }
}

