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

    public virtual void Write(int value)
    {
        _data = value & Mask;
    }

    public virtual void SetFlag(int flag, bool value)
    {
        if(value)
            _data |= flag & Mask;
        else
            _data &= ~(flag & Mask);
    }

    public virtual bool GetFlag(int flag)
    {
        return (_data & flag) != 0;
    }

    public virtual int Read()
    {
        return _data & Mask;
    }

    public int Increment()
    {
        _data = (_data + 1) & Mask;
        return _data;
    }

    public int Decrement()
    {
        _data = (_data - 1) & Mask;
        return _data;
    }
}