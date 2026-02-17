using System.ComponentModel;
using System.Security;

/// <summary>
/// Maximum of 32bit bus
/// </summary>
public class Bus
{
    public int Size { get; }
    public int Mask { get; }

    public SlimEvent OnValueChanged { get; } = new SlimEvent();

    private int _data;

    public Bus(int size = 1)
    {
        if(size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than 0.");
        
        if(size > 32)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be less than or equal to 32.");

        Size = size;
        Mask = (1 << size) - 1;
    }

    public void Assert(int value)
    {
        _data = value & Mask;
        OnValueChanged.Invoke();
    }

    public int Value => _data;
    
    public bool BitValue(int bit)
    {
        return (_data & (1 << bit)) != 0;
    }
}