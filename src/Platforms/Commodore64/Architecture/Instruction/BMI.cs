namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Branch if Minus instruction
/// </summary>
class BMI : RelativeAddressInstruction
{
    public BMI(MOS6510Cpu cpu) : base(cpu) { }

    public const int OP_CODE = 0x20 | AddressMode.Relative;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        var address = GetRelativeOperand();

        if(CPU.P.NegativeFlag)
        {
            CPU.PC.Value = address;
        }
    }
}
