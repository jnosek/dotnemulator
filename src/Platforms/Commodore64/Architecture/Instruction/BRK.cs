using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

class BRK(MOS6510Cpu cpu) : IInstruction
{
    private readonly MOS6510Cpu _cpu = cpu;

    public int Cycles => 7;

    public const int OP_CODE = 0x00 | AddressMode.Implied;

    public int OpCode => OP_CODE;

    public void Execute(int instruction)
    {
        _cpu.P.BreakCommandFlag = true;

        // increment by an additional 1
        // and read next Instruction Address
        // for returning to after IRQ routine
        _cpu.PC.Increment();
        var pc = _cpu.PC.Read();

        // push PC and status register to stack
        // since the stack point address grows down
        // we write the high byte first, then the low byte
         _cpu.Write(
            _cpu.SP.Decrement(), 
            pc >> 8);
        
        _cpu.Write(
            _cpu.SP.Decrement(),
            pc & 0xFF);
       

        // store status flag register
        _cpu.Write(
            _cpu.SP.Decrement(),
            _cpu.P.Read());

        // read IRQ/BRK vector
        var low  = _cpu.Read(MOS6510Cpu.IRQ_VECTOR);
        var high = _cpu.Read(MOS6510Cpu.IRQ_VECTOR + 1);
        _cpu.PC.Write((high << 8) | low);
        
        // then set I flag
        _cpu.P.InterruptDisableFlag = true;       
    }
}
