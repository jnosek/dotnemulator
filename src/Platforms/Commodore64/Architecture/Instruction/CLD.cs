namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Clear Decimal Mode (CLD) instruction
/// </summary>
/// <param name="cpu"></param>
public class CLD(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xD8;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        CPU.P.DecimalModeFlag = false;
    }
}
