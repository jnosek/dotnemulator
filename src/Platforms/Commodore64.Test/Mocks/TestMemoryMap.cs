using System;
using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Commodore64.Test.Mocks;

public class TestMemoryMap : MemoryMap
{
    public TestMemoryMap(
        Bus addressBus,
        Bus dataBus,
        Bus memoryBankBus
    ) : base(addressBus, dataBus, memoryBankBus)
    {
        
    }

    /// <summary>
    /// Reads a value from the RAM at the specified address without affecting the CPU state.
    /// For testing purposes only
    /// </summary>
    /// <param name="address">The address in RAM to read from.</param>
    /// <returns>The value stored at the specified RAM address.</returns>
    public int PeekRam(int address)
    {
        return _ram.Read(address);
    }

    /// <summary>
    /// Set a value in the ram at the specified address without affecting the CPU State.
    /// For testing purposes only
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public void PokeRam(int address, int value)
    {
        _ram.Write(address, value);
    }
}
