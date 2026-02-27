namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Branch if Positive (BPL) instruction.
/// </summary>
class BPL(MOS6510Cpu cpu) : AddressInstruction(cpu, OP_CODE, AddressMode.Relative)
{
    public const int OP_CODE = 0x10;

    public override void Execute(int instruction, int address)
    {

        if(!CPU.P.NegativeFlag)
        {
            CPU.PC.Value = address;
        }
    }
}
