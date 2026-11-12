namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class SED(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xF8;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        CPU.P.DecimalModeFlag = true;
    }
}