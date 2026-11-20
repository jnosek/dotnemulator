namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Throws a <see cref="StepLimitException"/> before the step after <paramref name="limit"/> steps have run, so a
/// hung program fails instead of running forever.
/// </summary>
/// <param name="limit">Steps allowed.</param>
/// <param name="trace">Optional trace whose log goes into the exception.</param>
internal sealed class StepLimitObserver(long limit, InstructionTraceObserver? trace = null) : IExecutionObserver
{
    public long Limit { get; } = limit;

    /// <summary>
    /// Steps run so far.
    /// </summary>
    public long Steps { get; private set; }

    public ObserverAction BeforeStep(MOS6510Cpu cpu)
    {
        if(Steps++ == Limit)
        {
            throw new StepLimitException($"Step limit of {Limit} reached at address {cpu.PC.Value:X4}")
            {
                InstructionLog = trace?.GetLog() ?? new()
            };
        }

        return ObserverAction.Continue;
    }
}
