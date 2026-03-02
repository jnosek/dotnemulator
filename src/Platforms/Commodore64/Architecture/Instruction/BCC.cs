namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class BCC(MOS6510Cpu cpu) : RelativeInstruction(cpu, OP_CODE)
{
    public const int OP_CODE = 0x90;
    public override void Execute(int instruction, int address)
    {
        if(!CPU.P.CarryFlag)
        {
            CPU.PC.Value = address;
        }
    }
}
