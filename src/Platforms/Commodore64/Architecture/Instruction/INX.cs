namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Increment X Register (INX) instruction
/// </summary>
/// <param name="cpu"></param>
class INX(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xE8;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        var value = CPU.X.Increment();

        // set status flags
        CPU.P.NegativeFlag = (value & 0x80) != 0;
        CPU.P.ZeroFlag = value == 0;
    }
}
