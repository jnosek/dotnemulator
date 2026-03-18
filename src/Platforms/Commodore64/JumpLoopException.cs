using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

internal class JumpLoopException : Exception
{
    public JumpLoopException(string message) : base(message)
    {
    }

    public required List<InstructionResult> InstructionLog { get; init; }
}
    