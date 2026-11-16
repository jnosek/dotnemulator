using System.Diagnostics;

namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// A single-bit signal line shared by several devices, any one of which can assert it (a wired-OR, such as an
/// open-collector interrupt line).
/// </summary>
/// <remarks>
/// <para>
/// Each device that drives the line calls <see cref="RegisterSource"/> once, while the machine is being wired
/// together, and then uses its own <see cref="Source"/> to <see cref="Source.Assert"/> or <see cref="Source.Release"/>
/// the line. The line is asserted (<see cref="Value"/> is <c>true</c>) while <b>any</b> source holds it, so one device
/// releasing the line does not cancel another device's request.
/// </para>
/// <para>
/// <c>true</c> means asserted, whatever the physical voltage. On the C64 the IRQ and NMI lines are active-low
/// open-collector lines: a device asserts them by pulling the pin low, which is <c>true</c> here.
/// </para>
/// <para>
/// Subscribers are notified only when the combined level changes, so every notification is an edge. A consequence,
/// matching the real hardware: while one source holds the line, another source asserting it produces no new edge.
/// On the C64 this is why RESTORE cannot trigger an NMI while CIA 2 is holding the NMI line.
/// </para>
/// <para>
/// This type deliberately does not derive from <see cref="Wire"/>: there is no way to set the level directly,
/// so the combined level can never get out of step with its sources.
/// </para>
/// </remarks>
public class OpenCollectorWire : SlimEventSource<bool>
{
    /// <summary>
    /// Maximum number of sources a line supports (one bit each in an <see cref="int"/>).
    /// </summary>
    public const int MaxSources = 32;

    /// <summary>
    /// One bit per source; a set bit means that source is asserting the line.
    /// </summary>
    private int _assertedSources;

    private int _sourceCount;

    /// <summary>
    /// <c>true</c> while at least one source is asserting the line.
    /// </summary>
    public bool Value => _assertedSources != 0;

    /// <summary>
    /// Adds a device that can drive this line, initially not asserting it.
    /// </summary>
    /// <remarks>
    /// Call this while wiring the machine together, not on the hot path: it allocates.
    /// </remarks>
    /// <returns>The handle the device uses to assert and release the line.</returns>
    /// <exception cref="InvalidOperationException">The line already has <see cref="MaxSources"/> sources.</exception>
    public Source RegisterSource()
    {
        if(_sourceCount >= MaxSources)
            throw new InvalidOperationException($"An {nameof(OpenCollectorWire)} supports at most {MaxSources} sources.");

        return new Source(this, 1 << _sourceCount++);
    }

    private void Set(int sourceBit, bool asserted)
    {
        var wasAsserted = _assertedSources != 0;

        if(asserted)
            _assertedSources |= sourceBit;
        else
            _assertedSources &= ~sourceBit;

        var isAsserted = _assertedSources != 0;

        if(isAsserted != wasAsserted)
            Notify(isAsserted);
    }

    /// <summary>
    /// One device's connection to an <see cref="OpenCollectorWire"/>.
    /// </summary>
    public sealed class Source
    {
        private readonly OpenCollectorWire _wire;
        private readonly int _bit;

        internal Source(OpenCollectorWire wire, int bit)
        {
            Debug.Assert(bit != 0);

            _wire = wire;
            _bit = bit;
        }

        /// <summary>
        /// <c>true</c> while this source is asserting the line.
        /// </summary>
        public bool IsAsserted => (_wire._assertedSources & _bit) != 0;

        /// <summary>
        /// Starts asserting the line.
        /// </summary>
        /// <remarks>
        /// If no other source was asserting it, the line goes from released to asserted and subscribers are
        /// notified before this method returns. Calling it again while already asserting does nothing.
        /// </remarks>
        public void Assert() => _wire.Set(_bit, true);

        /// <summary>
        /// Stops asserting the line.
        /// </summary>
        /// <remarks>
        /// The line is only released, and subscribers notified, if no other source is still asserting it.
        /// Calling it while not asserting does nothing.
        /// </remarks>
        public void Release() => _wire.Set(_bit, false);
    }
}
