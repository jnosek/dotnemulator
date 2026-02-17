namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;

class MOS6510Cpu
{
    /// <summary>
    /// Program Counter
    /// </summary>
    readonly Register PC = new Register(16);

    /// <summary>
    /// Status Register
    /// </summary>
    readonly Register P = new Register(8);

    /// <summary>
    /// Accumulator
    /// </summary>
    readonly Register A = new Register(8);
    
    /// <summary>
    /// Index Register X
    /// </summary>
    readonly Register X = new Register(8);

    /// <summary>
    /// Index Register Y
    /// </summary>
    readonly Register Y = new Register(8);   

    // used to control direction of Port Bus
    readonly Register Address0 = new Register(8);

    // used to read/write values on port bus
    readonly Register Address1 = new Register(8);
    
    /// <summary>
    /// Address Bus
    /// </summary>
    readonly Bus AddressBus;

    /// <summary>
    /// Data Bus
    /// </summary>
    readonly Bus DataBus;

    /// <summary>
    /// Port Bus
    /// </summary>
    readonly Bus PortBus;
    
    public MOS6510Cpu(Bus addressBus, Bus dataBus, Bus portBus)
    {
        AddressBus = addressBus;
        DataBus = dataBus;
        PortBus = portBus;
    }

    /// <summary>
    /// Load PC from memory location 0xFFFC/0xFFFD
    /// </summary>
    public void Reset()
    {
        AddressBus.Assert(0xFFFC);
        int value = DataBus.Value << 8;

        AddressBus.Assert(0xFFFD);
        value += DataBus.Value;
        PC.Write(value);

        RunLoop();
    }

    /// <summary> 
    /// Executes the instruction fetch-decode-execute loop. This method will run indefinitely until the emulator is stopped. 
    /// </summary>
    private void RunLoop()
    {
    }

    /// <summary>
    /// Writes a value to the data bus. If the address bus is set to 0 or 1, it writes to the port bus instead.
    /// </summary>
    /// <remarks>
    /// The MOS6510 has a unique feature where internally the first two addresses (0 and 1) are mapped to special registers that control the port bus. 
    /// This method takes that into account when writing values.
    /// </remarks>
    /// <param name="value"></param>
    private void Write(byte value)
    {
        // if address bus is greater than 1, write to data bus
        if(AddressBus.Value > 1)
            DataBus.Assert(value);
        // if address bus is 0, write to port bus control register
        else if(AddressBus.Value == 0)
            Address0.Write(value);
        // else, address bus is 1, write to port bus data register, and the bus itself
        else
        {
            Address1.Write(value);
            PortBus.Assert(value);
        }
    }
}