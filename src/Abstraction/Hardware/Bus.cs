namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// Maximum of 32bit bus
/// </summary>
public class Bus : SlimEventSource
{
    public int Size { get; }
    public int Mask { get; }

    private int _data;

    public Bus(int size = 1)
    {
        Mask = BitWidth.GetMask(size);
        Size = size;
    }

    // Data Operations

    /// <summary>
    /// Places a value on the bus without notifying subscribers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this when a component is <b>responding</b> to a bus cycle that another component started, for example
    /// memory putting the byte it read onto the data bus, or the PLA putting the selected bank onto the bank bus.
    /// The component that started the cycle (or one that runs after it) samples the value with <see cref="Read"/>.
    /// Triggering here would start a new cycle in the middle of the current one, e.g. a memory read on the data bus
    /// would be seen as a write and stored straight back.
    /// </para>
    /// <para>
    /// It is also used to set up a value ahead of a cycle that is started on a different bus.
    /// </para>
    /// <para>
    /// The value is masked to <see cref="Size"/> bits before it is stored.
    /// </para>
    /// </remarks>
    /// <param name="value">The value to place on the bus.</param>
    /// <seealso cref="Strobe"/>
    public void Drive(int value)
    {
        _data = value & Mask;
    }

    /// <summary>
    /// Places a value on the bus and then notifies subscribers, starting a bus cycle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this when a component is <b>initiating</b> a bus cycle, for example the CPU putting an address on the
    /// address bus so memory and the PLA respond. It is equivalent to <see cref="Drive"/> followed by
    /// <see cref="SlimEventSource.Trigger"/>.
    /// </para>
    /// <para>
    /// Subscribers run synchronously, in the order they subscribed, before this method returns. Any value a
    /// responder drives back (such as a byte read from memory) is therefore available to the caller as soon as
    /// <see cref="Strobe"/> returns.
    /// </para>
    /// <para>
    /// The value is masked to <see cref="Size"/> bits before it is stored.
    /// </para>
    /// </remarks>
    /// <param name="value">The value to place on the bus.</param>
    /// <seealso cref="Drive"/>
    public void Strobe(int value)
    {
        _data = value & Mask;
        Trigger();
    }

    /// <summary>
    /// Read the current value on the bus.
    /// </summary>
    /// <returns></returns>
    public int Read()
    {
        return _data;
    }

    /// <summary>
    /// Read a specific bit from the bus.
    /// </summary> 
    /// <param name="bit"></param>
    public bool ReadBit(int bit)
    {
        return (_data & (1 << bit)) != 0;
    }
}