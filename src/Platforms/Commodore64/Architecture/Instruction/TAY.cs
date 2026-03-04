using System;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Transfer Accumulator to Y Instruction
/// </summary>
/// <param name="cpu"></param>
class TAY(MOS6510Cpu cpu) : ImpliedInstruction(cpu)
{
    public const int OP_CODE = 0xA8;

    public override int OpCode => OP_CODE;

    public override void Execute(int instruction)
    {
        var a = CPU.A.Value;

        CPU.Y.Value = a;

        // set status flags
        CPU.P.NegativeFlag = (a & 0x80) != 0;
        CPU.P.ZeroFlag = a == 0;
    }
}
