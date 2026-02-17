namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// Maximum of 32bit bus
/// </summary>
public class Bus : SlimEventSource<int>
{
    public int Size { get; }
    public int Mask { get; }

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
        Update(_data);
    }

    public int Value => _data;
    
    public bool BitValue(int bit)
    {
        return (_data & (1 << bit)) != 0;
    }
}