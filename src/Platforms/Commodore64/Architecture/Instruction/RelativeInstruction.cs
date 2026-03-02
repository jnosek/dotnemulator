using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

abstract class RelativeInstruction : IInstruction
{
    protected readonly MOS6510Cpu CPU;

    public int OpCode { get; }

    protected RelativeInstruction(MOS6510Cpu cpu, int opCode)
    {
        CPU = cpu;
        OpCode = opCode;
    }

    public void Execute(int instruction)
    {
        var operand = GetRelativeOperand();
        Execute(instruction, operand);
    }

    public abstract void Execute(int instruction, int address);

    private int GetRelativeOperand()
    {
        var operand = CPU.ReadNextByte();

        return (CPU.PC.Value + operand) & 0xFFFF;
    }
}
