using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Base class for implied instructions, which have no operands
/// </summary>
/// <param name="cpu"></param>
public abstract class ImpliedInstruction(MOS6510Cpu cpu) : IInstruction
{
    protected readonly MOS6510Cpu CPU = cpu;

    public abstract int OpCode { get; }

    public abstract void Execute(int instruction);
}
