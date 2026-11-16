namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Decrease Y Instruction
/// </summary>
class DEY(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x88;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        CPU.Y.Decrement();

        // set status flags
        CPU.P.NegativeFlag = (CPU.Y.Value & 0x80) != 0;
        CPU.P.ZeroFlag = CPU.Y.Value == 0;
    }
}
