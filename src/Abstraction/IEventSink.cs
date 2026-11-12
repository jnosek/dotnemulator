public interface IEventSource
{
    void Subscribe(Action eventSink);
}

public interface IEventSource<T>
{
    void Subscribe(Action<T> eventSink);
}
