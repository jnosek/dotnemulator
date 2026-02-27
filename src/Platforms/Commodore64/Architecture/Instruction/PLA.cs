namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Pull Accumulator Instruction
/// </summary>
class PLA(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0x68;

    public const int Cycles = 4;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        var value = CPU.StackPop();

        CPU.A.Value = value;

        // set flags
        CPU.P.ZeroFlag = value == 0;
        CPU.P.NegativeFlag = (value & 0x80) != 0;
    }
}
