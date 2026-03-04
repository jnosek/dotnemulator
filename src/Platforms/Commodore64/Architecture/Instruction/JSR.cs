namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Jump to Subroutine (JSR) instruction. 
/// It pushes the return address to stack and sets the program counter to target address.
/// </summary>
/// <param name="cpu"></param>
class JSR : AddressInstruction
{
    public const int OP_CODE = 0x20;

    protected override Func<int> DecodeOperand { get; }

    public JSR(MOS6510Cpu cpu) : 
        base(cpu, OP_CODE, 0x00)
    {
        // JSR only supports absolute addressing mode, at a different address mode code 
        // than the rest of the instruction set
        DecodeOperand = GetAbsoluteOperand;
    }

    public override void Execute(int instruction, int address)
    {
        // push return address to stack
        var returnAddress = CPU.PC.Value;
        CPU.StackPush(returnAddress >> 8); // high byte
        CPU.StackPush(returnAddress & 0xFF); // low byte

        // set PC to target address
        CPU.PC.Value = address;
    }
}
