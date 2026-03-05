namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Increase Y Instruction
/// </summary>
/// <param name="cpu"></param>
class INY(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xC8;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        CPU.Y.Increment();

        // set status flags
        CPU.P.NegativeFlag = (CPU.Y.Value & 0x80) != 0;
        CPU.P.ZeroFlag = CPU.Y.Value == 0;
    }
}