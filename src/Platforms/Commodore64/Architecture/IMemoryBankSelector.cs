namespace Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Combinational address decoder that selects which memory bank responds to an address.
/// </summary>
/// <remarks>
/// <para>
/// Implementations model stateless decode logic, such as the C64 memory PLA. Their output depends only on their
/// current inputs, so on real hardware it has settled before memory samples it. Components handling a bus cycle
/// call <see cref="SelectBank"/> directly instead of waiting for the decoder to react to the same address strobe,
/// which removes any dependency on subscription order.
/// </para>
/// <para>
/// Implementations must be allocation-free: this is called on every memory access.
/// </para>
/// </remarks>
internal interface IMemoryBankSelector
{
    /// <summary>
    /// Returns the bank selected for <paramref name="address"/> under the current configuration.
    /// </summary>
    /// <param name="address">16-bit CPU address.</param>
    /// <returns>One of the <see cref="MemoryPla"/> bank constants, e.g. <see cref="MemoryPla.RAM"/>.</returns>
    int SelectBank(int address);
}
