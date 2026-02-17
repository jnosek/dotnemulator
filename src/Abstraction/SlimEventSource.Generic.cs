public class SlimEventSource<T> : IEventSource<T>
{
    private readonly List<Action<T>> _eventSinks = new List<Action<T>>();

    public SlimEventSource()
    {
    }

    public void Subscribe(Action<T> eventSink)
    {
        // track the number of subscribers to optimize invocation
        _eventSinks.Add(eventSink);

        // if only one event sink, invoke directly, otherwise use MultiUpdate to call all event sinks
        if(_eventSinks.Count == 1)
            Update = eventSink;
        else
            Update = MultiUpdate;
    }

    /// <summary>
    /// This is the method that will be called to invoke the event sinks. 
    /// It will point directly to the event sink if there is only one, or to MultiUpdate if there are multiple event sinks.
    /// </summary>
    internal Action<T> Update { get; private set; } = _ => { };

    /// <summary>
    /// If there are multiple subscribers, this method will be used to invoke all of them. 
    /// If there is only one subscriber, the Update property will point directly to that subscriber for optimal performance.
    /// </summary>
    private void MultiUpdate(T arg)
    {
        for(int i = 0; i < _eventSinks.Count; i++)
        {
            _eventSinks[i](arg);
        }
    }

    
}
