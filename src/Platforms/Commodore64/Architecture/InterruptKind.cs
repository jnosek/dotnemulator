namespace Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Which interrupt, if any, a <see cref="MOS6510Cpu.Step"/> serviced.
/// </summary>
internal enum InterruptKind
{
    /// <summary>
    /// The step executed an instruction.
    /// </summary>
    None,

    /// <summary>
    /// The step ran the interrupt sequence for an IRQ.
    /// </summary>
    Irq,

    /// <summary>
    /// The step ran the interrupt sequence for an NMI.
    /// </summary>
    Nmi
}
