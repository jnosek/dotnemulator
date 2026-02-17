namespace Dotnemulator.Abstraction.Hardware;

public class Wire : SlimEventSource<bool>
{
    public bool Value { get; private set; }

    public Wire(bool value = false)
    {
        Value = value;
    }

    public void Assert(bool value)
    {
        Value = value;
        Update(value);
    }
}