namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Transfer Stack to X Register Instruction
/// </summary>
class TSX(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xBA;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        CPU.X.Value = CPU.SP;
    }
}

