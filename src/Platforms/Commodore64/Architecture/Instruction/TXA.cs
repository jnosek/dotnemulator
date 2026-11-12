namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// TXA (Transfer X to Accumulator) instruction.
/// Transfers the value of the X register to the A register.
/// The zero and negative flags are set based on the value transferred.
/// </summary>
class TXA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x8A;

    public override int OpCode => OP_CODE;

    public int Cycles => 2;

    public override void Execute(int instruction)
    {
        var value = CPU.X.Value;
        CPU.A.Value = value;

        CPU.P.ZeroFlag = value == 0;
        CPU.P.NegativeFlag = (value & 0x80) != 0;
    }
}
