namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class JSR(MOS6510Cpu cpu) : 
    AddressModeInstruction(cpu, BASE_CODE, AddressMode.Absolute)
{
    public const int BASE_CODE = 0x20;
    public const int OP_CODE = BASE_CODE | AddressMode.Absolute;

    public override void Execute(int instruction)
    {
        var operand = DecodeOperand();
        
        // push return address to stack
        var returnAddress = CPU.PC.Value;
        CPU.StackPush(returnAddress >> 8); // high byte
        CPU.StackPush(returnAddress & 0xFF); // low byte

        // set PC to target address
        CPU.PC.Value = operand;
    }
}
