namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Branch if Overflow Set Instruction
/// </summary>
/// <param name="cpu"></param>
class BVS(MOS6510Cpu cpu) : AddressInstruction(cpu, OP_CODE, AddressMode.Relative)
{
    public const int OP_CODE = 0x70;

    public const int Cycles = 2;

    public override void Execute(int instruction, int address)
    {
        if(CPU.P.OverflowFlag)
        {
            CPU.PC.Value = address;
        }
    }
}