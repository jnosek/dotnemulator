namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// The kind of bus cycle signalled on a <see cref="ReadWriteWire"/>.
/// </summary>
public enum ReadWriteMode
{
    /// <summary>
    /// The initiator is reading: the responding component drives the data bus.
    /// </summary>
    Read,

    /// <summary>
    /// The initiator is writing: the data bus already holds the value and the responding component stores it.
    /// </summary>
    Write
}
