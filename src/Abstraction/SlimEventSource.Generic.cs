namespace Dotnemulator.Abstraction;

/// <summary>
/// Minimal, allocation-free publish/subscribe for hardware signals that carry a value, such as a wire's level.
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
/// <typeparam name="T">The value passed to subscribers.</typeparam>
public class SlimEventSource<T> : IEventSource<T>
{
    private static readonly Action<T> NoSubscribers = static _ => { };

    private Action<T>[] _eventSinks = [];

    /// <summary>
    /// What <see cref="Notify"/> calls: the no-op, the only subscriber, or <see cref="_notifyAll"/>.
    /// </summary>
    private Action<T> _notify = NoSubscribers;

    private readonly Action<T> _notifyAll;

    public SlimEventSource()
    {
        _notifyAll = NotifyAll;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(Action<T> eventSink)
    {
        ArgumentNullException.ThrowIfNull(eventSink);

        _eventSinks = [.. _eventSinks, eventSink];
        UpdateNotifyTarget();

        return new Subscription(this, eventSink);
    }

    /// <summary>
    /// Calls every subscriber with <paramref name="value"/>, in the order they subscribed.
    /// </summary>
    /// <remarks>
    /// Protected so that only the signal itself (for example <see cref="Hardware.Wire.Strobe"/>) decides when
    /// subscribers are told about a change.
    /// </remarks>
    /// <param name="value">The value to pass to each subscriber.</param>
    protected void Notify(T value) => _notify(value);

    private void Unsubscribe(Action<T> eventSink)
    {
        // match by reference, so disposing one subscription never removes a different one with an equal delegate
        var index = Array.FindIndex(_eventSinks, sink => ReferenceEquals(sink, eventSink));
        if(index < 0)
            return;

        var remaining = new Action<T>[_eventSinks.Length - 1];
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

    private void NotifyAll(T value)
    {
        // read the array once: a subscriber added or removed during this loop replaces the field, not this array
        var eventSinks = _eventSinks;

        for(int i = 0; i < eventSinks.Length; i++)
        {
            eventSinks[i](value);
        }
    }

    private sealed class Subscription(SlimEventSource<T> source, Action<T> eventSink) : IDisposable
    {
        private SlimEventSource<T>? _source = source;

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
