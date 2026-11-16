namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A single-bit signal line, such as an interrupt request or a read/write select.
/// </summary>
/// <remarks>
/// Subscribers receive the new level when the wire is strobed.
/// </remarks>
public class Wire : SlimEventSource<bool>
{
    /// <summary>
    /// The current level of the wire.
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
    /// Sets the level of the wire and then notifies subscribers with the new level.
    /// </summary>
    /// <remarks>
    /// Use this for signals that other components must <b>react</b> to as soon as they change.
    /// Subscribers run synchronously, in the order they subscribed, before this method returns.
    /// Subscribers are notified on every call, even when the level is unchanged.
    /// </remarks>
    /// <param name="value">The new level.</param>
    /// <seealso cref="Drive"/>
    public void Strobe(bool value)
    {
        Value = value;
        Update(value);
    }
}
