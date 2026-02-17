class MOS6510Cpu
{
    /// <summary>
    /// Program Counter
    /// </summary>
    Register PC = new Register(16);

    /// <summary>
    /// Status Register
    /// </summary>
    Register P = new Register(8);

    /// <summary>
    /// Accumulator
    /// </summary>
    Register A = new Register(8);
    
    /// <summary>
    /// Index Register X
    /// </summary>
    Register X = new Register(8);

    /// <summary>
    /// Index Register Y
    /// </summary>
    Register Y = new Register(8);   

    // used to control direction of Port Bus
    Register Address0 = new Register(8);

    // used to read/write values on port bus
    Register Address1 = new Register(8);
    
    /// <summary>
    /// Address Bus
    /// </summary>
    Bus AddressBus = new Bus(16);

    /// <summary>
    /// Data Bus
    /// </summary>
    Bus DataBus = new Bus(8);

    /// <summary>
    /// Port Bus
    /// </summary>
    Bus PortBus = new Bus(8);
    
    /// <summary>
    /// Load PC from memory location FFFC/FFFD
    /// </summary>
    public void Reset()
    {
        AddressBus.Write(0xFFFC);
        int value = DataBus.Read() << 8;

        AddressBus.Write(0xFFFD);
        value += DataBus.Read();
        PC.Write(value);

        RunLoop();
    }

    private void RunLoop()
    {
        
    }
}