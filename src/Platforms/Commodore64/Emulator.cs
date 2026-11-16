namespace Dotnemulator.Platforms.Commodore64;

using System.Diagnostics;
using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

public class Emulator
{
    internal Bus AddressBus { get; } = new Bus(16);
    internal Bus DataBus { get; } = new Bus(8);
    internal Bus PortBus { get; } = new Bus(8);

    /// <summary>
    /// The 6510 R/W line, signalling whether the current bus cycle is a read or a write.
    /// </summary>
    /// <remarks>
    /// Starts as <see cref="ReadWriteMode.Read"/>, matching the CPU after reset.
    /// </remarks>
    internal ReadWriteWire ReadWrite { get; } = new ReadWriteWire(ReadWriteMode.Read);

    /// <summary>
    /// The shared IRQ line. Devices that can raise an IRQ (VIC-II, CIA 1, cartridges) each register a source on it.
    /// </summary>
    internal OpenCollectorWire InterruptRequest { get; } = new OpenCollectorWire();

    /// <summary>
    /// The shared NMI line. Devices that can raise an NMI (CIA 2, the RESTORE key, cartridges) each register a source on it.
    /// </summary>
    internal OpenCollectorWire NonMaskableInterrupt { get; } = new OpenCollectorWire();

    internal MOS6510Cpu Cpu { get; }
    internal MemoryPla Pla { get; }
    internal MemoryMap MemoryMap { get; }

    internal IExecutionStrategy ExecutionStrategy { get; set; }

    /// <summary>
    /// Initializes a new instance of the Emulator class.
    /// <param name="memoryMapFactory">Optional factory function to create a custom MemoryMap. If not provided, a default MemoryMap will be used.</param>
    /// </summary>
    internal Emulator(Func<Emulator, MemoryMap>? memoryMapFactory = null)
    {
        Cpu = new MOS6510Cpu(AddressBus, DataBus, PortBus, ReadWrite, InterruptRequest, NonMaskableInterrupt);
        Pla = new MemoryPla(PortBus);
        MemoryMap = memoryMapFactory?.Invoke(this) ?? 
            new MemoryMap(AddressBus, DataBus, ReadWrite, Pla);

        ExecutionStrategy = new RuntimeExecutionStrategy();
    }

    public void Start()
    {
        //PrintRomHashes();

        ExecutionStrategy.Execute(Cpu);
    }

    public void Stop()
    {
        
    }

    public void Reset()
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