namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A read/write select line, such as the R/W pin of a 6502-family CPU, carrying a <see cref="ReadWriteMode"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is a single-bit signal like <see cref="Wire"/>, but its state is the <see cref="ReadWriteMode"/> itself
/// rather than a <see cref="bool"/> level, so call sites always say <see cref="ReadWriteMode.Read"/> or
/// <see cref="ReadWriteMode.Write"/>. On the 6510 the pin is high for a read and low for a write.
/// </para>
/// <para>
/// Subscribers receive the new <see cref="ReadWriteMode"/> when <see cref="Strobe"/> changes it, matching
/// <see cref="Wire.Strobe"/>.
/// </para>
/// </remarks>
public class ReadWriteWire : SlimEventSource<ReadWriteMode>
{
    /// <param name="mode">Initial mode; defaults to <see cref="ReadWriteMode.Read"/>, the idle state of a 6502-family CPU.</param>
    public ReadWriteWire(ReadWriteMode mode = ReadWriteMode.Read)
    {
        Mode = mode;
    }

    /// <summary>
    /// The current mode of the line.
    /// </summary>
    public ReadWriteMode Mode { get; private set; }

    /// <summary>
    /// <c>true</c> when the line signals a read cycle.
    /// </summary>
    public bool IsRead => Mode == ReadWriteMode.Read;

    /// <summary>
    /// <c>true</c> when the line signals a write cycle.
    /// </summary>
    public bool IsWrite => Mode == ReadWriteMode.Write;

    /// <summary>
    /// Sets the mode without notifying subscribers.
    /// </summary>
    /// <remarks>
    /// This is the normal way to use the line: the initiator drives the mode before it strobes the address bus,
    /// and the components handling that cycle sample <see cref="Mode"/>. Nothing reacts to the change itself,
    /// so there is no handler call on the hot path.
    /// </remarks>
    /// <param name="mode">The new mode.</param>
    /// <seealso cref="Strobe"/>
    public void Drive(ReadWriteMode mode)
    {
        Mode = mode;
    }

    /// <summary>
    /// Sets the mode and, if the mode changed, notifies subscribers with the new mode.
    /// </summary>
    /// <remarks>
    /// Use this only when another component must react as soon as the mode changes.
    /// Subscribers run synchronously, in the order they subscribed, before this method returns.
    /// Strobing the mode the line already has does nothing, matching <see cref="Wire.Strobe"/>.
    /// </remarks>
    /// <param name="mode">The new mode.</param>
    /// <seealso cref="Drive"/>
    public void Strobe(ReadWriteMode mode)
    {
        if(Mode == mode)
            return;

        Mode = mode;
        Update(mode);
    }
}
