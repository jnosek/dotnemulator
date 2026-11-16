namespace Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// The result of one <see cref="MOS6510Cpu.Step"/>: an instruction, or an interrupt sequence.
/// </summary>
/// <param name="Address">
/// Address of the instruction that was executed or, for an interrupt sequence, the return address pushed to the stack.
/// </param>
/// <param name="Instruction">
/// The opcode that was executed, or <see cref="IRQ_SEQUENCE"/> / <see cref="NMI_SEQUENCE"/> for an interrupt sequence.
/// </param>
/// <param name="Operand">Operand of the instruction (currently always 0).</param>
internal record struct InstructionResult(int Address, int Instruction, int Operand)
{
    /// <summary>
    /// <see cref="Instruction"/> value for a step that serviced an IRQ instead of executing an instruction.
    /// </summary>
    /// <remarks>Negative so it can never collide with a real opcode (0x00-0xFF).</remarks>
    public const int IRQ_SEQUENCE = -1;

    /// <summary>
    /// <see cref="Instruction"/> value for a step that serviced an NMI instead of executing an instruction.
    /// </summary>
    /// <remarks>Negative so it can never collide with a real opcode (0x00-0xFF).</remarks>
    public const int NMI_SEQUENCE = -2;

    /// <summary>
    /// <c>true</c> when this step serviced an interrupt rather than executing an instruction.
    /// </summary>
    public readonly bool IsInterrupt => Instruction < 0;
}
