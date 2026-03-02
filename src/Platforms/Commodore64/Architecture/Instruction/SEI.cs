namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

public class SEI(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x78;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        CPU.P.InterruptDisableFlag = true;
    }
}

