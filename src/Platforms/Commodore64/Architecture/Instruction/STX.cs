using System;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// STX (Store X Register) instruction. 
/// Stores the value of the X register into memory at the specified address.
/// </summary>
class STX : YAddressInstruction
{
     public const int BASE_OP_CODE = 0x82;

    public static readonly int[] AddressModes = [
        Absolute,
        ZeroPage,
        ZeroPageY
    ];

    public readonly int[] Cycles = [4, 3, 4];

    private STX(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        var value = CPU.X.Value;
        CPU.Write(address, value);
    }
}
