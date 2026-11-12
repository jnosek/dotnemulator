namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Return from Interrupt Instruction
/// </summary>
class RTI(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public int Cycles => 6;

    public const int OP_CODE = 0x40;

    public override int OpCode => OP_CODE;


    public override void Execute(int instruction)
    {
        // restore process flags
        CPU.P.Value = CPU.StackPop();

        // restore program counter
        var address = CPU.StackPop();
        address |= CPU.StackPop() << 8;

        CPU.PC.Value = address;
    }
}
