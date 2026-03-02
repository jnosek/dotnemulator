namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Break instruction
/// </summary>
/// <param name="cpu"></param>
class BRK(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    private readonly MOS6510Cpu _cpu = cpu;

    public int Cycles => 7;

    public const int OP_CODE = 0x00;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        _cpu.P.BreakCommandFlag = true;

        // increment by an additional 1
        // and read next Instruction Address
        // for returning to after IRQ routine
        _cpu.PC.Increment();
        var pc = _cpu.PC.Advance();

        // push PC and status register to stack
        // since the stack point address grows down
        // we write the high byte first, then the low byte
        _cpu.StackPush(pc >> 8);
        _cpu.StackPush(pc & 0xFF);

        // store status flag register
        _cpu.StackPush(_cpu.P.Value);

        // read IRQ/BRK vector

        // get low byte
        var address = _cpu.Read(MOS6510Cpu.IRQ_VECTOR);
        // get high byte
        address |= _cpu.Read(MOS6510Cpu.IRQ_VECTOR + 1) << 8;

        _cpu.PC.Value = address;
        
        // then set I flag
        _cpu.P.InterruptDisableFlag = true;       
    }
}
