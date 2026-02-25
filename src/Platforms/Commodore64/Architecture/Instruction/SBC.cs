using System;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Subtract with Carry instruction. Computes A = A - M - (1 - C).
/// </summary>
class SBC(MOS6510Cpu cpu, int addressMode) :
    AddressModeInstruction(cpu, BASE_OP_CODE, addressMode)
{
    public const int BASE_OP_CODE = 0xE1;

    public static readonly int[] AddressModes = [
        AddressMode.Immediate,
        AddressMode.Absolute,
        AddressMode.AbsoluteX,
        AddressMode.AbsoluteY,
        AddressMode.ZeroPage,
        AddressMode.ZeroPageX,
        AddressMode.Indexed_Indirect,
        AddressMode.Indirect_Indexed,
    ];

    public readonly int[] Cycles = [2, 4, 4, 4, 3, 4, 6, 5];

    public override void Execute(int instruction)
    {
        var address = DecodeOperand();

        var accumulator = CPU.A.Value;
        var value = CPU.Read(address);
        int borrow = CPU.P.CarryFlag ? 0 : 1;

        if (CPU.P.DecimalModeFlag)
        {
            // low nibble
            int low = (accumulator & 0x0F) - (value & 0x0F) - borrow;
            if (low < 0)
            {
                low = (low - 0x06) & 0x0F;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }

            // high nibble
            int high = (accumulator & 0xF0) - (value & 0xF0) - (borrow << 4);
            if (high < 0)
            {
                high = (high - 0x60) & 0xF0;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }

            CPU.A.Value = low | high;
            CPU.P.CarryFlag = borrow == 0;
        }
        else
        {
            var result = accumulator - value - borrow;
            CPU.A.Value = result;

            CPU.P.NegativeFlag = (CPU.A.Value & 0x80) != 0;
            CPU.P.ZeroFlag = CPU.A.Value == 0;
            CPU.P.CarryFlag = result >= 0;
            CPU.P.OverflowFlag = ((accumulator ^ value) & (accumulator ^ result) & 0x80) != 0;
        }
    }
}
