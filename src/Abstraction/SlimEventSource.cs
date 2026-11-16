namespace Dotnemulator.Abstraction;

/// <summary>
/// Minimal, allocation-free publish/subscribe for hardware signals that carry no value, such as a bus strobe.
/// </summary>
/// <remarks>
/// <para>
/// Built for the emulator's hot path. <see cref="Notify"/> calls a single delegate: with one subscriber it is that
/// subscriber itself, with none it is a shared no-op, and with several it is a loop over an array of subscribers.
/// </para>
/// <para>
/// Subscribers are called synchronously, in the order they subscribed. Subscribing and disposing a subscription
/// are setup-time operations: they allocate and copy the subscriber array. A subscriber added or removed while a
/// notification is in progress does not affect that notification.
/// </para>
/// </remarks>
public class SlimEventSource : IEventSource
{
    private static readonly Action NoSubscribers = static () => { };

    private Action[] _eventSinks = [];

    /// <summary>
    /// What <see cref="Notify"/> calls: the no-op, the only subscriber, or <see cref="_notifyAll"/>.
    /// </summary>
    private Action _notify = NoSubscribers;

    private readonly Action _notifyAll;

    public SlimEventSource()
    {
        _notifyAll = NotifyAll;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(Action eventSink)
    {
        ArgumentNullException.ThrowIfNull(eventSink);

        _eventSinks = [.. _eventSinks, eventSink];
        UpdateNotifyTarget();

        return new Subscription(this, eventSink);
    }

    /// <summary>
    /// Calls every subscriber, in the order they subscribed.
    /// </summary>
    /// <remarks>
    /// Protected so that only the signal itself (for example <see cref="Hardware.Bus.Strobe"/>) decides when
    /// subscribers are told about a change.
    /// </remarks>
    protected void Notify() => _notify();

    private void Unsubscribe(Action eventSink)
    {
        // match by reference, so disposing one subscription never removes a different one with an equal delegate
        var index = Array.FindIndex(_eventSinks, sink => ReferenceEquals(sink, eventSink));
        if(index < 0)
            return;

        var remaining = new Action[_eventSinks.Length - 1];
        Array.Copy(_eventSinks, 0, remaining, 0, index);
        Array.Copy(_eventSinks, index + 1, remaining, index, remaining.Length - index);

        _eventSinks = remaining;
        UpdateNotifyTarget();
    }

    private void UpdateNotifyTarget()
    {
        _notify = _eventSinks.Length switch
        {
            0 => NoSubscribers,
            1 => _eventSinks[0],
            _ => _notifyAll
        };
    }

    private void NotifyAll()
    {
        // read the array once: a subscriber added or removed during this loop replaces the field, not this array
        var eventSinks = _eventSinks;

        for(int i = 0; i < eventSinks.Length; i++)
        {
            eventSinks[i]();
        }
    }

    private sealed class Subscription(SlimEventSource source, Action eventSink) : IDisposable
    {
        private SlimEventSource? _source = source;

        /// <summary>
        /// Removes the subscriber. Disposing more than once does nothing.
        /// </summary>
        public void Dispose()
        {
            _source?.Unsubscribe(eventSink);
            _source = null;
        }
    }
}
