using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Push Processor Flags to Stack Instruction
/// </summary>
/// <param name="cpu"></param>
public class PHP(MOS6510Cpu cpu) : IInstruction
{
    private readonly MOS6510Cpu _cpu = cpu;

    public int Cycles => 3;

    public const int OP_CODE = 0x08 | AddressMode.Implied;

    public int OpCode => OP_CODE;

    public void Execute(int instruction)
    {
        _cpu.StackPush(_cpu.P.Value);
    }
}
