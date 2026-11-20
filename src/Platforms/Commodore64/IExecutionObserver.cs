using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

/// <summary>
/// What an <see cref="IExecutionObserver"/> wants the execution strategy to do next.
/// </summary>
internal enum ObserverAction
{
    /// <summary>Keep running and keep calling this observer.</summary>
    Continue,

    /// <summary>Keep running, but never call this observer again.</summary>
    Detach,

    /// <summary>End the run. From <see cref="IExecutionObserver.BeforeStep"/> the step does not run.</summary>
    Stop
}

/// <summary>
/// Watches execution one instruction at a time, for example to stop at an address or log the boot sequence.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="IExecutionStrategy"/> calls every attached observer, in the order they were added, before and
/// after each <see cref="MOS6510Cpu.Step"/>. An observer that returns <see cref="ObserverAction.Detach"/> is
/// removed; once none are left the runtime strategy runs its plain loop, so watching costs nothing for the rest of
/// the run. The first observer to return <see cref="ObserverAction.Stop"/> ends the run, and the observers after it
/// are not called for that step. Failures (a jump loop, a step limit) are reported by throwing.
/// </para>
/// <para>
/// Both methods are called on every step while attached, so implementations must not allocate.
/// </para>
/// </remarks>
internal interface IExecutionObserver
{
    /// <summary>
    /// Called before each step, with PC at the instruction (or interrupt) about to run.
    /// </summary>
    ObserverAction BeforeStep(MOS6510Cpu cpu) => ObserverAction.Continue;

    /// <summary>
    /// Called after each step with its result. For an interrupt sequence, <see cref="InstructionResult.Address"/>
    /// is the return address, whose instruction has not run yet.
    /// </summary>
    ObserverAction AfterStep(MOS6510Cpu cpu, InstructionResult result) => ObserverAction.Continue;
}
