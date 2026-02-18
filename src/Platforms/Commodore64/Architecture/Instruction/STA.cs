using System;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Store Accumulator Instruction
/// </summary>
class STA(MOS6510Cpu cpu, AddressMode addressMode) : 
    AddressModeInstruction(cpu, 0x81, addressMode)
{
    public static readonly AddressMode[] AddressModes = [
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
        var address = DecodeAddress(instruction);
        CPU.AddressBus.Drive(address);
        CPU.AddressBus.Trigger();
        CPU.Write(CPU.A.Read());
    }
}
