using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Diagnostics;

namespace Dotnemulator.Platforms.Commodore64;

/// <summary>
/// Runs the CPU with debugging aids, for tests and tools.
/// </summary>
/// <remarks>
/// <para>
/// Each aid is an <see cref="IExecutionObserver"/> (or, for memory watches, a <see cref="MemoryWatch"/> probe)
/// in <c>Diagnostics/</c>. The properties here are a convenient way to set them up: each run creates the
/// matching observers, in this order, with any extra <see cref="Observers"/> after the step limit:
/// </para>
/// <list type="number">
/// <item><see cref="StepLimitObserver"/> (<see cref="MaxSteps"/>)</item>
/// <item><see cref="Observers"/></item>
/// <item><see cref="BreakpointObserver"/> (<see cref="BreakpointCallbacks"/>)</item>
/// <item><see cref="StopOnOpcodeObserver"/> for <c>NOP</c> (<see cref="IsStopOnNopEnabled"/>)</item>
/// <item><see cref="JumpLoopObserver"/> (<see cref="IsJumpLoopDetectionEnabled"/>)</item>
/// <item><see cref="InstructionTraceObserver"/>: the last 100 steps, for the exceptions' instruction logs</item>
/// <item><see cref="StopAtAddressObserver"/> (<see cref="EndAddress"/>)</item>
/// </list>
/// </remarks>
public class DebugExecutionStrategy : IExecutionStrategy
{
    const int INSTRUCTION_LOG_WINDOW_SIZE = InstructionTraceObserver.DEFAULT_CAPACITY;

    public bool IsStopOnNopEnabled { get; set; } = false;

    public bool IsJumpLoopDetectionEnabled { get; set; } = false;

    public int? StartAddress { get; set; }

    /// <summary>
    /// If execution reaches this address, it will stop once the instruction there has run.
    /// </summary>
    /// <remarks>
    /// This can be useful for debugging or testing specific sections of code.
    /// </remarks>
    public int? EndAddress { get; set; }

    /// <summary>
    /// If this many steps run without another stop condition, a <see cref="StepLimitException"/> is thrown.
    /// </summary>
    /// <remarks>
    /// A guard against programs that hang in a loop jump-loop detection cannot see (more than one instruction long).
    /// <c>null</c> (the default) means no limit.
    /// </remarks>
    public long? MaxSteps { get; set; }

    public Dictionary<int, Action<MOS6510Cpu>> BreakpointCallbacks { get; init; } = new();

    /// <summary>
    /// Called with the data bus value for every bus cycle, read or write, at the given address.
    /// </summary>
    public Dictionary<int, Action<int>> MemoryWatchCallbacks { get; init; } = new();

    /// <summary>
    /// Called with the value written for every write cycle at the given address. Reads are ignored.
    /// </summary>
    /// <remarks>
    /// Use this to capture output a program sends to a port (for example the functional test's report channel),
    /// where a read of the same address must not be mistaken for output.
    /// </remarks>
    public Dictionary<int, Action<int>> MemoryWriteWatchCallbacks { get; init; } = new();

    /// <summary>
    /// Extra observers, called after the step limit and before the breakpoints.
    /// </summary>
    internal List<IExecutionObserver> Observers { get; } = new();

    IList<IExecutionObserver> IExecutionStrategy.Observers => Observers;

    public void Execute(MOS6510Cpu cpu)
    {
        cpu.Initialize(StartAddress);

        // removed when Execute ends, so running again does not stack up memory-watch handlers
        using var memoryWatch = CreateMemoryWatch(cpu);

        var observers = new ExecutionObservers(CreateObservers());

        while(true)
        {
            if(observers.BeforeStep(cpu))
                break;

            var result = cpu.Step();

            if(observers.AfterStep(cpu, result))
                break;
        }
    }

    /// <summary>
    /// The observers for one run. Created fresh each time, so step counts and the trace start empty.
    /// </summary>
    private IEnumerable<IExecutionObserver> CreateObservers()
    {
        var trace = new InstructionTraceObserver(INSTRUCTION_LOG_WINDOW_SIZE);

        if(MaxSteps is long maxSteps)
            yield return new StepLimitObserver(maxSteps, trace);

        foreach(var observer in Observers)
            yield return observer;

        if(BreakpointCallbacks.Count > 0)
            yield return new BreakpointObserver(BreakpointCallbacks);

        if(IsStopOnNopEnabled)
            yield return new StopOnOpcodeObserver(NOP.OP_CODE);

        // compares with the trace's latest entry, so it must run before the trace records this step
        if(IsJumpLoopDetectionEnabled)
            yield return new JumpLoopObserver(trace);

        yield return trace;

        if(EndAddress is int endAddress)
            yield return new StopAtAddressObserver(endAddress);
    }

    private MemoryWatch? CreateMemoryWatch(MOS6510Cpu cpu)
    {
        if(MemoryWatchCallbacks.Count == 0 && MemoryWriteWatchCallbacks.Count == 0)
            return null;

        // created last, so it runs after the memory map: the data bus holds the value written or just read
        var watch = new MemoryWatch(cpu.AddressBus, cpu.DataBus, cpu.ReadWrite);

        foreach(var (address, callback) in MemoryWatchCallbacks)
            watch.OnAccess(address, callback);

        foreach(var (address, callback) in MemoryWriteWatchCallbacks)
            watch.OnWrite(address, callback);

        return watch;
    }
}
