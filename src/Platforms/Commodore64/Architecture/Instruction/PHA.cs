namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Push Accumulator Instruction
/// </summary>
/// <param name="cpu"></param>
class PHA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public int Cycles => 3;

    public const int OP_CODE = 0x48 | AddressMode.Implied;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        CPU.StackPush(CPU.A.Value);
    }
}
