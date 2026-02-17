public class SlimEventSource : IEventSource
{
    private readonly List<Action> _eventSinks = new List<Action>();

    public SlimEventSource()
    {
    }

    public void Subscribe(Action eventSink)
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
    internal Action Update { get; private set; } = () => { };

    /// <summary>
    /// If there are multiple subscribers, this method will be used to invoke all of them. 
    /// If there is only one subscriber, the Update property will point directly to that subscriber for optimal performance.
    /// </summary>
    private void MultiUpdate()
    {
        for(int i = 0; i < _eventSinks.Count; i++)
        {
            _eventSinks[i]();
        }
    }
}