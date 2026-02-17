namespace Dotnemulator.Platforms.Commodore64;

using System.Diagnostics;
using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

public class Emulator
{
    internal Bus AddressBus { get; }
    internal Bus DataBus { get; }
    internal Bus PortBus { get; }

    internal MOS6510Cpu Cpu { get; }
    internal MemoryPla Pla { get; }
    internal MemoryMap MemoryMap { get; }

    public Emulator()
    {
        AddressBus = new Bus(16);
        DataBus = new Bus(8);
        PortBus = new Bus(8);

        Cpu = new MOS6510Cpu(AddressBus, DataBus, PortBus);
        Pla = new MemoryPla(AddressBus, PortBus);
        MemoryMap = new MemoryMap(AddressBus, DataBus, PortBus);

        // load roms
        using var basicRomStream = GetType().Assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.basic.901226-01.bin") ?? 
            throw new InvalidOperationException("Failed to load BASIC ROM");
        using var charRomStream = GetType().Assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.char.901225-01.bin") ?? 
            throw new InvalidOperationException("Failed to load CHAR ROM");
        using var kernelRomStream = GetType().Assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.kernel.901227-03.bin") ?? 
            throw new InvalidOperationException("Failed to load KERNEL ROM");

        MemoryMap.LoadRoms(basicRomStream, charRomStream, kernelRomStream);
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