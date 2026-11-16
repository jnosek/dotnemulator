using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Base class for relative instructions, which have a single byte operand 
/// that is added to the program counter if the branch is taken
/// </summary>
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

        // cast operand to signed byte and add to program counter
        // to get proper positive and negative offsets,
        // then mask to 16 bits to get the target address
        return (CPU.PC.Value + (sbyte)operand) & 0xFFFF;
    }
}
