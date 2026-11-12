namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Decrease X Register Instruction
/// </summary>
/// <param name="cpu"></param>
class DEX(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xCA;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        CPU.X.Decrement();

        // set flags
        CPU.P.NegativeFlag = (CPU.X.Value & 0x80) != 0;
        CPU.P.ZeroFlag = CPU.X.Value == 0;
    }
}
