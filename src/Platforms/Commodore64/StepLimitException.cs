using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

/// <summary>
/// Thrown by <see cref="DebugExecutionStrategy"/> when <see cref="DebugExecutionStrategy.MaxSteps"/> steps have run
/// without reaching a stop condition, so a hung program fails instead of running forever.
/// </summary>
internal class StepLimitException : Exception
{
    public StepLimitException(string message) : base(message)
    {
    }

    public required List<InstructionResult> InstructionLog { get; init; }
}
