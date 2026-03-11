namespace Dotnemulator.Platforms.Commodore64;

using System.Diagnostics;
using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

public class Emulator
{
    internal Bus AddressBus { get; } = new Bus(16);
    internal Bus DataBus { get; } = new Bus(8);
    internal Bus PortBus { get; } = new Bus(8);

    internal MOS6510Cpu Cpu { get; }
    internal MemoryPla Pla { get; }
    internal MemoryMap MemoryMap { get; }

    /// <summary>
    /// Initializes a new instance of the Emulator class.
    /// <param name="memoryMapFactory">Optional factory function to create a custom MemoryMap. If not provided, a default MemoryMap will be used.</param>
    /// </summary>
    internal Emulator(Func<Emulator, MemoryMap>? memoryMapFactory = null)
    {
        Cpu = new MOS6510Cpu(AddressBus, DataBus, PortBus);
        Pla = new MemoryPla(AddressBus, PortBus);
        MemoryMap = memoryMapFactory?.Invoke(this) ?? 
            new MemoryMap(AddressBus, DataBus, Pla.MemoryBankBus);
    }

    public void Start()
    {
        PrintRomHashes();

        Cpu.Reset();
    }

    public void Stop()
    {
        
    }

    [Conditional("DEBUG")]
    private void PrintRomHashes()
    {
        var romHashes = MemoryMap.GetRomHashes();
        Debug.WriteLine($"BASIC ROM SHA256:  {romHashes[0]}");
        Debug.WriteLine($"CHAR ROM SHA256:   {romHashes[1]}");
        Debug.WriteLine($"KERNEL ROM SHA256: {romHashes[2]}");
    }
}