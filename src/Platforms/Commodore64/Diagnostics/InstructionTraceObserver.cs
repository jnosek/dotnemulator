namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Keeps the results of the last few steps, for error reports.
/// </summary>
/// <remarks>
/// A fixed-size ring buffer, so recording a step does not allocate. Only <see cref="GetLog"/> allocates, and it is
/// meant for when something has gone wrong.
/// </remarks>
internal sealed class InstructionTraceObserver : IExecutionObserver
{
    public const int DEFAULT_CAPACITY = 100;

    private readonly InstructionResult[] _entries;
    private int _next;

    public InstructionTraceObserver(int capacity = DEFAULT_CAPACITY)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0);
        _entries = new InstructionResult[capacity];
    }

    /// <summary>
    /// Number of steps held, up to the capacity.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// The most recent step recorded. Only valid when <see cref="Count"/> is above zero.
    /// </summary>
    public InstructionResult Latest => _entries[(_next + _entries.Length - 1) % _entries.Length];

    public ObserverAction AfterStep(MOS6510Cpu cpu, InstructionResult result)
    {
        _entries[_next] = result;
        _next = (_next + 1) % _entries.Length;

        if(Count < _entries.Length)
            Count++;

        return ObserverAction.Continue;
    }

    /// <summary>
    /// The steps held, newest first.
    /// </summary>
    public List<InstructionResult> GetLog()
    {
        var log = new List<InstructionResult>(Count);

        for(int i = 1; i <= Count; i++)
            log.Add(_entries[(_next + _entries.Length - i) % _entries.Length]);

        return log;
    }
}
