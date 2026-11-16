namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Branch if Not Equal (BNE) instruction
/// </summary>
class BNE(MOS6510Cpu cpu) : RelativeInstruction(cpu, OP_CODE)
{
    public const int OP_CODE = 0xD0;    

    public override void Execute(int instruction, int address)
    {
        if(!CPU.P.ZeroFlag)
        {
            CPU.PC.Value = address;
        }
    }
}
