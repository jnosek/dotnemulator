namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A read/write select line, such as the R/W pin of a 6502-family CPU, expressed as a <see cref="ReadWriteMode"/>.
/// </summary>
/// <remarks>
/// <para>
/// The underlying <see cref="Wire.Value"/> follows the usual active-low write convention (R/W̅):
/// <c>true</c> (high) for <see cref="ReadWriteMode.Read"/> and <c>false</c> (low) for <see cref="ReadWriteMode.Write"/>.
/// Subscribers inherited from <see cref="Wire"/> therefore receive <c>true</c> for a read and <c>false</c> for a write.
/// </para>
/// <para>
/// Prefer <see cref="Mode"/>, <see cref="Drive(ReadWriteMode)"/> and <see cref="Strobe(ReadWriteMode)"/> over the
/// <see cref="bool"/> members so the direction is explicit at the call site.
/// </para>
/// </remarks>
public class ReadWriteWire : Wire
{
    /// <param name="mode">Initial mode; defaults to <see cref="ReadWriteMode.Read"/>, the idle state of a 6502-family CPU.</param>
    public ReadWriteWire(ReadWriteMode mode = ReadWriteMode.Read)
        : base(ToLevel(mode))
    {
    }

    /// <summary>
    /// The current mode of the line.
    /// </summary>
    public ReadWriteMode Mode => Value ? ReadWriteMode.Read : ReadWriteMode.Write;

    /// <summary>
    /// <c>true</c> when the line signals a read cycle.
    /// </summary>
    public bool IsRead => Value;

    /// <summary>
    /// <c>true</c> when the line signals a write cycle.
    /// </summary>
    public bool IsWrite => !Value;

    /// <summary>
    /// Sets the mode without notifying subscribers.
    /// </summary>
    /// <remarks>
    /// This is the normal way to use the line: the initiator drives the mode before it strobes the address bus,
    /// and the components handling that cycle sample <see cref="Mode"/>.
    /// </remarks>
    /// <param name="mode">The new mode.</param>
    /// <seealso cref="Wire.Drive(bool)"/>
    public void Drive(ReadWriteMode mode)
    {
        Drive(ToLevel(mode));
    }

    /// <summary>
    /// Sets the mode and then notifies subscribers with the new level (<c>true</c> for read, <c>false</c> for write).
    /// </summary>
    /// <param name="mode">The new mode.</param>
    /// <seealso cref="Wire.Strobe(bool)"/>
    public void Strobe(ReadWriteMode mode)
    {
        Strobe(ToLevel(mode));
    }

    private static bool ToLevel(ReadWriteMode mode) => mode == ReadWriteMode.Read;
}
