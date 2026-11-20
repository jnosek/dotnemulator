namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Throws a <see cref="JumpLoopException"/> when the same instruction at the same address runs twice in a row,
/// such as a <c>JMP *</c> trap or a branch to itself.
/// </summary>
/// <remarks>
/// <para>
/// Compares each step with the latest entry in <paramref name="trace"/>, so it must be attached before the trace:
/// the trace has not recorded this step yet when the comparison runs.
/// </para>
/// <para>
/// Loops of more than one instruction are not detected: they are also how ordinary code waits and counts (the
/// KERNAL's RAM test is one), and telling a hang apart would need the machine state to be compared as well. Use a
/// <see cref="StepLimitObserver"/> for those.
/// </para>
/// </remarks>
internal sealed class JumpLoopObserver(InstructionTraceObserver trace) : IExecutionObserver
{
    public ObserverAction AfterStep(MOS6510Cpu cpu, InstructionResult result)
    {
        if(trace.Count > 0)
        {
            var last = trace.Latest;
            if(result.Instruction == last.Instruction && result.Address == last.Address)
            {
                throw new JumpLoopException($"Detected potential jump loop at address {result.Address:X4} with instruction {result.Instruction:X2}")
                {
                    InstructionLog = trace.GetLog()
                };
            }
        }

        return ObserverAction.Continue;
    }
}
