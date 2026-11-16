namespace Dotnemulator.Abstraction;

/// <summary>
/// Something other components can subscribe to, to be told when it changes.
/// </summary>
public interface IEventSource
{
    /// <summary>
    /// Registers <paramref name="eventSink"/> to be called on each notification.
    /// </summary>
    /// <param name="eventSink">The handler to call.</param>
    /// <returns>A subscription; dispose it to stop receiving notifications.</returns>
    IDisposable Subscribe(Action eventSink);
}

/// <summary>
/// Something other components can subscribe to, to be told the new <typeparamref name="T"/> when it changes.
/// </summary>
/// <typeparam name="T">The value passed to subscribers.</typeparam>
public interface IEventSource<T>
{
    /// <summary>
    /// Registers <paramref name="eventSink"/> to be called on each notification.
    /// </summary>
    /// <param name="eventSink">The handler to call.</param>
    /// <returns>A subscription; dispose it to stop receiving notifications.</returns>
    IDisposable Subscribe(Action<T> eventSink);
}
