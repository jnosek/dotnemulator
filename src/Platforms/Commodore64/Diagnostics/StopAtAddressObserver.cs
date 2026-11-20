namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Stops the run once the instruction at an address has run.
/// </summary>
/// <remarks>
/// An interrupt sequence reports the interrupted address, but the instruction there has not run yet, so it does
/// not count.
/// </remarks>
internal sealed class StopAtAddressObserver(int address) : IExecutionObserver
{
    public int Address { get; } = address;

    public ObserverAction AfterStep(MOS6510Cpu cpu, InstructionResult result) =>
        result.Interrupt == InterruptKind.None && result.Address == Address
            ? ObserverAction.Stop
            : ObserverAction.Continue;
}
