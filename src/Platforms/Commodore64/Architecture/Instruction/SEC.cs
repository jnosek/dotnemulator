namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Set Carry Instruction
/// </summary>
/// <param name="cpu"></param>
public class SEC(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x38;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        CPU.P.CarryFlag = true;
    }
}
