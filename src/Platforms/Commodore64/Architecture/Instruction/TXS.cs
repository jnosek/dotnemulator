namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class TXS : ImpliedInstruction
{
    public const int OP_CODE = 0x9A;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public TXS(MOS6510Cpu cpu) : base(cpu) { }

    public override void Execute(int instruction)
    {
        CPU.SP = CPU.X.Value;
    }
}
