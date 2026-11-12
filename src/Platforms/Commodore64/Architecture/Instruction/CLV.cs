namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Clear Overflow Flag (CLV) instruction
/// </summary>
/// <param name="cpu"></param>
class CLV(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xB8;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        CPU.P.OverflowFlag = false;
    }
}
