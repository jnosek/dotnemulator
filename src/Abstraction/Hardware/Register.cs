namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// Maximum of 32bit register
/// </summary>
public class Register
{
    public int Size { get; }
    public int Mask { get;}

    private int _data;

    public Register(int size = 1)
    {
        if(size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than 0.");
        
        if(size > 32)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be less than or equal to 32.");

        Size = size;
        Mask = (1 << size) - 1;
    }

    public void Write(int value)
    {
        _data = value & Mask;
    }

    public int Read()
    {
        return _data & Mask;
    }

    public bool ReadBit(int bit)
    {
        return (_data & (1 << bit)) != 0;
    }
}