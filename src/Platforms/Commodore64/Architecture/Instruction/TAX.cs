namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Transfer Accumulator to X Instruction
/// </summary>
/// <param name="cpu"></param>
class TAX(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xAA;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        var a = CPU.A.Value;

        CPU.X.Value = a;

        // set status flags
        CPU.P.NegativeFlag = (a & 0x80) != 0;
        CPU.P.ZeroFlag = a == 0;
    }
}
