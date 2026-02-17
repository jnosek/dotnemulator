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
        // TODO: wire up MemoryMap
    }

    public void Start()
    {
        Cpu.Reset();


    }

    public void Stop()
    {
        
    }
}