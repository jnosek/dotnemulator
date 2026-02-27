
namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Clear Interrupt Flag Instruction
/// </summary>
class CLI(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x58 | AddressMode.Implied;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        CPU.P.InterruptDisableFlag = false;
    }
}
