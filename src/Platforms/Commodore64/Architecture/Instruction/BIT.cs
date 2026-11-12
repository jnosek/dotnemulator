namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// BIT Test Instruction
/// </summary>
/// <remarks>
/// The BIT instruction sets the Status Flags as follows:
/// It sets the Zero Flag if the result of the AND operation of the Accumulator and memory location is zero
/// The Negative Flag if bit 7 of the memory location is set
/// The Overflow Flag if bit 6 of the memory location is set.
/// The value of the accumulator is not changed by this instruction
/// </remarks>
/// <param name="cpu"></param>
/// <param name="addressMode"></param>
class BIT : XAddressInstruction
{
    public const int BASE_OP_CODE = 0x20;
    public static readonly int[] AddressModes = [
        AccumulatorInstruction.Absolute,
        AccumulatorInstruction.ZeroPage];

    private BIT(MOS6510Cpu cpu, int addressMode) : base(cpu, BASE_OP_CODE, addressMode) { }

    public override void Execute(int instruction, int address)
    {
        var value = CPU.Read(address);

        // set flags
        CPU.P.OverflowFlag = (value & 0x40) != 0;
        CPU.P.NegativeFlag = (value & 0x80) != 0;
        CPU.P.ZeroFlag = (value & CPU.A.Value) == 0;
    }
}
