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

    public Emulator() : this(true)
    {
    }

    protected Emulator(
        bool loadRoms, 
        Func<Bus, Bus, Bus, MemoryMap>? memoryMapFactory = null)
    {
        Cpu = new MOS6510Cpu(AddressBus, DataBus, PortBus);
        Pla = new MemoryPla(AddressBus, PortBus);
        MemoryMap = memoryMapFactory?.Invoke(AddressBus, DataBus, Pla.MemoryBankBus) ?? 
            new MemoryMap(AddressBus, DataBus, Pla.MemoryBankBus);

        if(loadRoms)
        {
            // load roms
            using var basicRomStream = GetType().Assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.basic.901226-01.bin") ?? 
                throw new InvalidOperationException("Failed to load BASIC ROM");
            using var charRomStream = GetType().Assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.char.901225-01.bin") ?? 
                throw new InvalidOperationException("Failed to load CHAR ROM");
            using var kernelRomStream = GetType().Assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.kernel.901227-03.bin") ?? 
                throw new InvalidOperationException("Failed to load KERNEL ROM");

            MemoryMap.LoadRoms(basicRomStream, charRomStream, kernelRomStream);
        }
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