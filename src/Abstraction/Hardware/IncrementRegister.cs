namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A register that increments its value on each read. 
/// This can be useful for certain hardware components that require a counter or timer functionality.
/// </summary>
public class IncrementRegister : Register
{
    public IncrementRegister(int size = 1) : base(size)
    {
    }   

    public override int Read()
    {
        var val = base.Read();
        Write(val + 1);
        return val;
    }
}
