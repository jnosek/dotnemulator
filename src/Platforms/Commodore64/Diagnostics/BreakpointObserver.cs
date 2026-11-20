namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Calls an action when PC reaches an address, before the instruction there runs.
/// </summary>
/// <remarks>
/// A one-shot breakpoint removes itself after its first hit. The observer detaches once no breakpoints are left.
/// </remarks>
internal sealed class BreakpointObserver : IExecutionObserver
{
    private readonly Dictionary<int, (Action<MOS6510Cpu> Action, bool OneShot)> _breakpoints = new();

    public BreakpointObserver()
    {
    }

    /// <param name="breakpoints">Permanent breakpoints: address to action.</param>
    public BreakpointObserver(IEnumerable<KeyValuePair<int, Action<MOS6510Cpu>>> breakpoints)
    {
        foreach(var (address, action) in breakpoints)
            Add(address, action);
    }

    /// <summary>
    /// Number of breakpoints still set.
    /// </summary>
    public int Count => _breakpoints.Count;

    /// <summary>
    /// Sets a breakpoint at <paramref name="address"/>, replacing any already there.
    /// </summary>
    /// <param name="oneShot"><c>true</c> to remove the breakpoint after its first hit.</param>
    public BreakpointObserver Add(int address, Action<MOS6510Cpu> action, bool oneShot = false)
    {
        _breakpoints[address] = (action, oneShot);
        return this;
    }

    public ObserverAction BeforeStep(MOS6510Cpu cpu)
    {
        if(_breakpoints.Count == 0)
            return ObserverAction.Detach;

        var address = cpu.PC.Value;
        if(!_breakpoints.TryGetValue(address, out var breakpoint))
            return ObserverAction.Continue;

        if(breakpoint.OneShot)
            _breakpoints.Remove(address);

        breakpoint.Action(cpu);

        return _breakpoints.Count == 0 ? ObserverAction.Detach : ObserverAction.Continue;
    }
}
