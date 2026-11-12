namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Branch if Overflow Clear Instruction
/// </summary>
class BVC(MOS6510Cpu cpu) : RelativeInstruction(cpu, OP_CODE)
{
    public const int OP_CODE = 0x50;

    public override void Execute(int instruction, int address)
    {
        if(!CPU.P.OverflowFlag)
        {
            CPU.PC.Value = address;
        }
    }
}
