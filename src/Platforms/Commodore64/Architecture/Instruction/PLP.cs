namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Pull Processor Flags Instruction
/// </summary>
/// <param name="cpu"></param>
class PLP(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public int Cycles => 4;

    public const int OP_CODE = 0x28 | AddressMode.Implied;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        CPU.P.Value = CPU.StackPop();
    }
}
