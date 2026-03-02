namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class TYA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x98;

    public const int Cycles = 2;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        var y = CPU.Y.Value;

        CPU.A.Value = y;

        // set status flags
        CPU.P.NegativeFlag = (CPU.A.Value & 0x80) != 0;
        CPU.P.ZeroFlag = CPU.A.Value == 0;
    }
}
