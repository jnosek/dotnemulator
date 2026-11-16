namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A single-bit signal line, such as an interrupt request or a read/write select.
/// </summary>
/// <remarks>
/// <para>
/// <b>Polarity:</b> <c>true</c> means the signal is <b>asserted</b> (active), whatever the physical voltage.
/// For an active-low pin, such as the 6510 IRQ or the C64 cartridge GAME line, <c>true</c> means the pin is pulled
/// low. Document what "asserted" means on each wire. A line that several devices can assert should be an
/// <see cref="OpenCollectorWire"/> instead.
/// </para>
/// <para>
/// Subscribers receive the new level when <see cref="Strobe"/> changes it, so every notification is an edge
/// (released to asserted, or asserted to released).
/// </para>
/// </remarks>
public class Wire : SlimEventSource<bool>
{
    /// <summary>
    /// The current level of the wire: <c>true</c> when asserted.
    /// </summary>
    public bool Value { get; private set; }

    public Wire(bool value = false)
    {
        Value = value;
    }

    /// <summary>
    /// Sets the level of the wire without notifying subscribers.
    /// </summary>
    /// <remarks>
    /// Use this for signals that other components only <b>sample</b> while handling a cycle started elsewhere,
    /// for example a read/write select that memory checks when the address bus is strobed. Nothing reacts
    /// to the change itself, so there is no handler call on the hot path.
    /// </remarks>
    /// <param name="value">The new level.</param>
    /// <seealso cref="Strobe"/>
    public void Drive(bool value)
    {
        Value = value;
    }

    /// <summary>
    /// Sets the level of the wire and, if the level changed, notifies subscribers with the new level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this for signals that other components must <b>react</b> to as soon as they change, such as an
    /// edge-triggered interrupt line. Subscribers run synchronously, in the order they subscribed, before this
    /// method returns.
    /// </para>
    /// <para>
    /// Subscribers are only notified when the level actually changes, so each notification is an edge.
    /// Strobing the level the wire already has does nothing. A level set with <see cref="Drive"/> counts as the
    /// current level, so a later <see cref="Strobe"/> to the same level does not notify.
    /// </para>
    /// </remarks>
    /// <param name="value">The new level.</param>
    /// <seealso cref="Drive"/>
    public void Strobe(bool value)
    {
        if(Value == value)
            return;

        Value = value;
        Notify(value);
    }
}
