namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Return from Subroutine Instruction
/// </summary>
class RTS(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int Cycles = 6;

    public const int OP_CODE = 0x60;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        // restore program counter
        var address = CPU.StackPop();
        address |= CPU.StackPop() << 8;

        // increment address by 1 to resume after
        // the end of the original JSR instruction
        CPU.PC.Value = address + 1;
    }
}
