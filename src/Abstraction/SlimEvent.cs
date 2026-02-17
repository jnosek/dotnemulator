public class SlimEvent
{
    private readonly List<Action> _actions = new List<Action>();

    public SlimEvent()
    {
    }

    public void Subscribe(Action action)
    {
        // track the number of subscribers to optimize invocation
        _actions.Add(action);

        // if only one subscriber, invoke directly, otherwise use MultiInvoke to call all subscribers
        if(_actions.Count == 1)
            Invoke = action;
        else
            Invoke = MultiInvoke;
    }


    public Action Invoke { get; private set; } = () => { };

    /// <summary>
    /// If there are multiple subscribers, this method will be used to invoke all of them. 
    /// If there is only one subscriber, the Invoke property will point directly to that subscriber for optimal performance.
    /// </summary>
    private void MultiInvoke()
    {
        for(int i = 0; i < _actions.Count; i++)
        {
            _actions[i]();
        }
    }
}