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

    internal MOS6510Cpu Cpu { get; }
    internal MemoryPla Pla { get; }
    internal MemoryMap MemoryMap { get; }

    internal IExecutionStrategy ExecutionStrategy { get; set; }

    /// <summary>
    /// Initializes a new instance of the Emulator class.
    /// <param name="memoryMapFactory">Optional factory function to create a custom MemoryMap. If not provided, a default MemoryMap will be used.</param>
    /// </summary>
    /// <remarks>
    /// Construction order matters: components that subscribe to <see cref="AddressBus"/> are notified in the
    /// order they subscribed. The <see cref="MemoryPla"/> must be created before <see cref="MemoryMap"/> (and any
    /// future I/O chip) so the bank selection is on <see cref="MemoryPla.MemoryBankBus"/> before they handle the cycle.
    /// </remarks>
    internal Emulator(Func<Emulator, MemoryMap>? memoryMapFactory = null)
    {
        Cpu = new MOS6510Cpu(AddressBus, DataBus, PortBus, ReadWrite);
        Pla = new MemoryPla(AddressBus, PortBus);
        MemoryMap = memoryMapFactory?.Invoke(this) ?? 
            new MemoryMap(AddressBus, DataBus, Pla.MemoryBankBus, ReadWrite);

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