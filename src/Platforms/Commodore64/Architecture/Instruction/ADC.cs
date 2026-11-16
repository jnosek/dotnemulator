namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// Add with Carry Instruction
/// </summary>
class ADC : AccumulatorInstruction
{
    public const int BASE_OP_CODE = 0x61;

    public static readonly int[] AddressModes = Default_Modes;

    public readonly int[] Cycles = [4, 5, 5, 3, 4, 6, 6];

    private ADC(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        // get value at address
        var value = CPU.Read(address);
        var accumulator = CPU.A.Value;

        if(CPU.P.DecimalModeFlag)
        {
            // perform BCD Add Operation and write back to accumulator
            int carry = CPU.P.CarryFlag ? 1 : 0;

            // low nibble
            int low = (accumulator & 0x0F) + (value & 0x0F) + carry;
            if (low > 0x09)
            {
                low = (low + 0x06) & 0x0F;
                carry = 1;
            }
            else
            {
                carry = 0;
            }

            // high nibble
            int high = (accumulator & 0xF0) + (value & 0xF0) + (carry << 4);
            if (high > 0x90)
            {
                high = (high + 0x60) & 0xF0;
                carry = 1;
            }
            else
            {
                carry = 0;
            }

            // combine nibbles
            CPU.A.Value = low | high;
            CPU.P.CarryFlag = carry != 0;
        }
        else
        {
            // perform ADD operation and write back to accumulator
            var result = accumulator + value + (CPU.P.CarryFlag ? 1 : 0);
            CPU.A.Value = result;

            // set flags
            CPU.P.NegativeFlag = (CPU.A.Value & 0x80) != 0;
            CPU.P.ZeroFlag = CPU.A.Value == 0;
            CPU.P.CarryFlag = result > 0xFF;
            CPU.P.OverflowFlag = 
                // check the sign bit
                (0x80 &
                // and if both numbers share the same sign
                ~(accumulator ^ value) & 
                // and if the result has a different sign
                (accumulator ^ result)) != 0;
        }
    }
}
