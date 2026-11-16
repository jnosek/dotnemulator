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
        Mask = BitWidth.GetMask(size);
        Size = size;
    }

    // Data Operations

    /// <summary>
    /// Place a value on the bus without triggering an update event
    /// </summary>
    /// <remarks>
    /// This is useful for requesting reads from the bus without trigger write notifications
    /// value is property masked before stored
    /// </remarks>
    /// <param name="value"></param>
    public void Drive(int value)
    {
        _data = value & Mask;
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