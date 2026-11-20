namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A probe on the address bus that calls back on bus cycles at chosen addresses, like a logic analyser clipped
/// onto the board.
/// </summary>
/// <remarks>
/// <para>
/// It watches bus cycles, not instructions, so it is not an <see cref="IExecutionObserver"/>: it subscribes to
/// <c>AddressBus</c> when created and unsubscribes when disposed. It works with any execution strategy.
/// </para>
/// <para>
/// Create it after the emulator is built. It then subscribes after <c>MemoryMap</c> and <c>VicII</c>, so when it
/// runs the data bus holds the value written (write cycle) or the value just read (read cycle).
/// </para>
/// </remarks>
internal sealed class MemoryWatch : IDisposable
{
    private readonly Bus _addressBus;
    private readonly Bus _dataBus;
    private readonly ReadWriteWire _readWrite;
    private readonly Dictionary<int, Action<int>> _accessCallbacks = new();
    private readonly Dictionary<int, Action<int>> _writeCallbacks = new();
    private readonly IDisposable _subscription;

    public MemoryWatch(Bus addressBus, Bus dataBus, ReadWriteWire readWrite)
    {
        _addressBus = addressBus;
        _dataBus = dataBus;
        _readWrite = readWrite;

        _subscription = _addressBus.Subscribe(BusCycleHandler);
    }

    public MemoryWatch(Emulator emulator)
        : this(emulator.AddressBus, emulator.DataBus, emulator.ReadWrite)
    {
    }

    /// <summary>
    /// Calls <paramref name="callback"/> with the data bus value on every cycle, read or write, at
    /// <paramref name="address"/>.
    /// </summary>
    public MemoryWatch OnAccess(int address, Action<int> callback)
    {
        _accessCallbacks[address] = _accessCallbacks.TryGetValue(address, out var existing) ? existing + callback : callback;
        return this;
    }

    /// <summary>
    /// Calls <paramref name="callback"/> with the value written on every write cycle at <paramref name="address"/>.
    /// Reads are ignored, so a program reading back a port is not mistaken for output.
    /// </summary>
    public MemoryWatch OnWrite(int address, Action<int> callback)
    {
        _writeCallbacks[address] = _writeCallbacks.TryGetValue(address, out var existing) ? existing + callback : callback;
        return this;
    }

    public void Dispose() => _subscription.Dispose();

    private void BusCycleHandler()
    {
        var address = _addressBus.Read();

        if(_accessCallbacks.TryGetValue(address, out var callback))
            callback(_dataBus.Read());

        if(_readWrite.IsWrite && _writeCallbacks.TryGetValue(address, out var writeCallback))
            writeCallback(_dataBus.Read());
    }
}
