namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Push Processor Flags to Stack Instruction
/// </summary>
/// <param name="cpu"></param>
class PHP(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public int Cycles => 3;

    public const int OP_CODE = 0x08;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        CPU.StackPush(CPU.P.Value);
    }
}
