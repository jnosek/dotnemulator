public class Wire
{
    public bool Value { get; private set; }

    public SlimEvent OnValueChanged { get; } = new SlimEvent();

    public Wire(bool value = false)
    {
        Value = value;
    }

    public void Assert(bool value)
    {
        Value = value;
        OnValueChanged.Invoke();
    }
}