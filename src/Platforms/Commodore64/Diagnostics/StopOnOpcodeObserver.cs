namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Stops the run once an instruction with the given opcode has run, for example <c>NOP</c> (<c>$EA</c>) to end a
/// unit-test program.
/// </summary>
/// <remarks>
/// Interrupt sequences do not count, although they report <c>$00</c> (<c>BRK</c>) as their instruction.
/// </remarks>
internal sealed class StopOnOpcodeObserver(int opCode) : IExecutionObserver
{
    public int OpCode { get; } = opCode;

    public ObserverAction AfterStep(MOS6510Cpu cpu, InstructionResult result) =>
        result.Interrupt == InterruptKind.None && result.Instruction == OpCode
            ? ObserverAction.Stop
            : ObserverAction.Continue;
}
