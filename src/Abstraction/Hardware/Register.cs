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
        Mask = BitWidth.GetMask(size);
        Size = size;
    }

    // TODO: replace with Value property
    public virtual int Value
    {
        get => _data;
        set => _data = value & Mask;
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

    public int Increment()
    {
        _data = (_data + 1) & Mask;
        return _data;
    }

    public int Advance()
    {
        var value = _data;

        Increment();

        return value;
    }

    public int Decrement()
    {
        _data = (_data - 1) & Mask;
        return _data;
    }
}