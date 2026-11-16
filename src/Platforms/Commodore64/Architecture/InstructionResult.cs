namespace Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// The result of one <see cref="MOS6510Cpu.Step"/>: an instruction, or an interrupt sequence.
/// </summary>
/// <param name="Address">
/// Address of the instruction that was executed or, for an interrupt sequence, the return address pushed to the
/// stack. In that case the instruction at this address has not run yet.
/// </param>
/// <param name="Instruction">
/// The opcode that was executed. For an interrupt sequence this is <c>$00</c> (<c>BRK</c>): the real 6502 forces
/// <c>BRK</c> into its instruction register instead of fetching an opcode, and runs it with the interrupt's vector.
/// </param>
/// <param name="Operand">Operand of the instruction (currently always 0).</param>
/// <param name="Interrupt">
/// <see cref="InterruptKind.None"/> for an executed instruction, otherwise which interrupt was serviced.
/// Check this before treating <paramref name="Address"/> as an instruction that ran.
/// </param>
internal record struct InstructionResult(int Address, int Instruction, int Operand, InterruptKind Interrupt = InterruptKind.None);
