using System;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Logical OR on Accumulator
/// </summary>
/// <param name="cpu"></param>
/// <param name="addressMode"></param>
class ORA(MOS6510Cpu cpu, AddressMode addressMode) : 
    AddressModeInstruction(cpu, 0x81, addressMode)
{
    public static readonly AddressMode[] AddressModes = [
        AddressMode.Immediate,
        AddressMode.Absolute,
        AddressMode.AbsoluteX,
        AddressMode.AbsoluteY,
        AddressMode.ZeroPage,
        AddressMode.ZeroPageX,
        AddressMode.Indexed_Indirect,
        AddressMode.Indirect_Indexed,
    ];

    public readonly int[] Cycles = [4, 5, 5, 3, 4, 6, 6];

    public override void Execute(int instruction)
    {
        var address = DecodeOperand();
        
        // get value at address
        CPU.AddressBus.Drive(address);
        CPU.AddressBus.Trigger();
        var value = CPU.Read();

        // perform OR operation and write back to accumulator
        CPU.A.Write(CPU.A.Read() | value);

        // set flags
        CPU.P.SetFlag(StatusFlag.Negative, (CPU.A.Read() & 0x80) != 0);
        CPU.P.SetFlag(StatusFlag.Zero, CPU.A.Read() == 0);
    }
}
