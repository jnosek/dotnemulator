namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class NOP(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xEA;

    public const int Cycles = 2;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
       // NOP does nothing
    }
}
