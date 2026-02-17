namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// Maximum of 32bit bus
/// </summary>
public class Bus : SlimEventSource
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

    // Data Operations

    /// <summary>
    /// Place a value on the bus without triggering an update event
    /// </summary>
    /// <remarks>
    /// This is useful for requesting reads from the bus without trigger write notifications
    /// <param name="value"></param>
    public void Place(int value)
    {
        _data = value & Mask;
    }

    /// <summary>
    /// Write a value to the bus and trigger an update event. This is used for normal bus writes where connected components need to be notified of the change.
    /// The value will be masked to fit within the bus size.
    /// </summary>
    /// <param name="value"></param>
    public void Write(int value)
    {
        _data = value & Mask;
        Update();
    }

    /// <summary>
    /// Read the current value on the bus.
    /// </summary>
    /// <returns></returns>
    public int Read()
    {
        return _data;
    }

    /// <summary>
    /// Read a specific bit from the bus.
    /// </summary> 
    /// <param name="bit"></param>
    public bool ReadBit(int bit)
    {
        return (_data & (1 << bit)) != 0;
    }
}