namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Clear Carry Instruction
/// </summary>
class CLC(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x18;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        CPU.P.CarryFlag = false;
    }
}
