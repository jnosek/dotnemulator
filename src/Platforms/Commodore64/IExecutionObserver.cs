using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

/// <summary>
/// Watches execution one instruction at a time, for a limited stretch such as the boot sequence.
/// </summary>
/// <remarks>
/// An <see cref="IExecutionStrategy"/> calls <see cref="BeforeStep"/> before each <see cref="MOS6510Cpu.Step"/>
/// while the observer is attached. Once it returns <c>false</c> the strategy detaches it and is never called
/// again, so watching costs nothing for the rest of the run.
/// </remarks>
internal interface IExecutionObserver
{
    /// <summary>
    /// Called before each step, with PC at the instruction (or interrupt) about to run.
    /// </summary>
    /// <returns><c>true</c> to keep watching, <c>false</c> to detach.</returns>
    bool BeforeStep(MOS6510Cpu cpu);
}
