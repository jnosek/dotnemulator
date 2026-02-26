namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Jump to Subroutine (JSR) instruction. 
/// It pushes the return address to stack and sets the program counter to target address.
/// </summary>
/// <param name="cpu"></param>
class JSR(MOS6510Cpu cpu) : 
    AddressModeInstruction(cpu, OP_CODE)
{
    public const int OP_CODE = 0x20;

    public override void Execute(int instruction)
    {
        var operand = GetAbsoluteOperand();
        
        // push return address to stack
        var returnAddress = CPU.PC.Value;
        CPU.StackPush(returnAddress >> 8); // high byte
        CPU.StackPush(returnAddress & 0xFF); // low byte

        // set PC to target address
        CPU.PC.Value = operand;
    }
}
