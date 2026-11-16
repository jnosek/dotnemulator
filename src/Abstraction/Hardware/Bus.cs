using System.Diagnostics;

namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A multi-bit (1 to 32 bits) set of signal lines shared between components, such as an address or data bus.
/// </summary>
/// <remarks>
/// <para>
/// The component that starts a bus cycle uses <see cref="Strobe"/>; components responding to it use
/// <see cref="Drive"/>. A single-bit control signal should normally be a <see cref="Wire"/> instead.
/// </para>
/// <para>
/// <b>Subscriber order matters.</b> Subscribers are notified synchronously, in the order they subscribed. When
/// one subscriber produces a value that another subscriber of the same bus samples during the same cycle, the
/// producer must subscribe first, and the composition root that wires the components together is responsible
/// for creating them in an order that guarantees this.
/// </para>
/// <para>
/// Prefer avoiding such dependencies. Stateless decode logic (for example an address decoder that selects a
/// memory bank) should be queried directly by the component handling the cycle rather than subscribing to the
/// same bus, so the dependency is visible in a constructor instead of hidden in subscription order.
/// </para>
/// </remarks>
public class Bus : SlimEventSource
{
    /// <summary>
    /// Width of the bus in bits.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Mask with the low <see cref="Size"/> bits set; every value placed on the bus is ANDed with it.
    /// </summary>
    public int Mask { get; }

    private int _data;

    /// <param name="size">Width of the bus in bits, from 1 to 32.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is outside 1 to 32.</exception>
    public Bus(int size)
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
    /// Reads a single line of the bus.
    /// </summary>
    /// <remarks>
    /// The bit index is only checked in DEBUG builds, to keep this allocation- and branch-free on the hot path.
    /// </remarks>
    /// <param name="bit">Zero-based bit index, from 0 to <see cref="Size"/> - 1.</param>
    /// <returns><c>true</c> if the bit is set.</returns>
    public bool ReadBit(int bit)
    {
        Debug.Assert(bit >= 0 && bit < Size, $"Bit {bit} is outside the {Size}-bit bus.");

        return (_data & (1 << bit)) != 0;
    }
}