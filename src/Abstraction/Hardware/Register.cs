using System.Diagnostics;

namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// Fixed-width (1 to 32 bits) storage inside a chip, such as a CPU's accumulator or program counter.
/// </summary>
/// <remarks>
/// <para>
/// Every value is masked to <see cref="Size"/> bits, so arithmetic wraps the way the hardware does
/// (for example an 8-bit register incremented from <c>$FF</c> becomes <c>$00</c>).
/// </para>
/// <para>
/// <b>Subclassing:</b> <see cref="Value"/>, <see cref="SetFlag"/> and <see cref="GetFlag"/> are virtual.
/// <see cref="Increment"/>, <see cref="Decrement"/> and <see cref="Advance"/> are not, and write the stored value
/// directly, so they do not go through an overridden <see cref="Value"/> setter.
/// </para>
/// </remarks>
public class Register
{
    /// <summary>
    /// Width of the register in bits.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Mask with the low <see cref="Size"/> bits set.
    /// </summary>
    public int Mask { get; }

    private int _data;

    /// <param name="size">Width of the register in bits, from 1 to 32.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is outside 1 to 32.</exception>
    public Register(int size)
    {
        Mask = BitWidth.GetMask(size);
        Size = size;
    }

    /// <summary>
    /// The current contents of the register.
    /// </summary>
    /// <remarks>
    /// Setting it masks the value to <see cref="Size"/> bits.
    /// </remarks>
    public virtual int Value
    {
        get => _data;
        set => _data = value & Mask;
    }

    /// <summary>
    /// Sets or clears the bits in <paramref name="flag"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="flag"/> must fit within <see cref="Size"/> bits; this is only checked in DEBUG builds.
    /// </remarks>
    /// <param name="flag">The bit or bits to change, e.g. <c>0b0000_0001</c>.</param>
    /// <param name="value"><c>true</c> to set the bits, <c>false</c> to clear them.</param>
    public virtual void SetFlag(int flag, bool value)
    {
        Debug.Assert((flag & ~Mask) == 0, $"Flag {flag:X} does not fit in a {Size}-bit register.");

        if(value)
            _data |= flag;
        else
            _data &= ~flag;
    }

    /// <summary>
    /// Returns whether any of the bits in <paramref name="flag"/> are set.
    /// </summary>
    /// <remarks>
    /// <paramref name="flag"/> must fit within <see cref="Size"/> bits; this is only checked in DEBUG builds.
    /// </remarks>
    /// <param name="flag">The bit or bits to test.</param>
    /// <returns><c>true</c> if any of the bits are set.</returns>
    public virtual bool GetFlag(int flag)
    {
        Debug.Assert((flag & ~Mask) == 0, $"Flag {flag:X} does not fit in a {Size}-bit register.");

        return (_data & flag) != 0;
    }

    /// <summary>
    /// Adds one to the register, wrapping at <see cref="Size"/> bits.
    /// </summary>
    /// <returns>The <b>new</b> value, after incrementing.</returns>
    /// <seealso cref="Advance"/>
    public int Increment()
    {
        _data = (_data + 1) & Mask;
        return _data;
    }

    /// <summary>
    /// Adds one to the register, wrapping at <see cref="Size"/> bits, and returns the value it had before.
    /// </summary>
    /// <remarks>
    /// Used to step through memory, e.g. fetching the byte at the program counter and moving past it.
    /// </remarks>
    /// <returns>The <b>old</b> value, before incrementing.</returns>
    /// <seealso cref="Increment"/>
    public int Advance()
    {
        var value = _data;

        Increment();

        return value;
    }

    /// <summary>
    /// Subtracts one from the register, wrapping at <see cref="Size"/> bits.
    /// </summary>
    /// <returns>The <b>new</b> value, after decrementing.</returns>
    public int Decrement()
    {
        _data = (_data - 1) & Mask;
        return _data;
    }
}
