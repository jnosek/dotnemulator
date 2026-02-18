namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Abstraction.Operations;

class MOS6510Cpu
{
    /// <summary>
    /// Program Counter
    /// </summary>
    readonly Register PC = new Register(16);

    /// <summary>
    /// Status Register
    /// </summary>
    internal readonly Register P = new Register(8);

    /// <summary>
    /// Accumulator
    /// </summary>
    internal readonly Register A = new Register(8);
    
    /// <summary>
    /// Index Register X
    /// </summary>
    internal readonly Register X = new Register(8);

    /// <summary>
    /// Index Register Y
    /// </summary>
    internal readonly Register Y = new Register(8);   

    // used to control direction of Port Bus
    internal readonly Register Address0 = new Register(8);

    // used to read/write values on port bus
    internal readonly Register Address1 = new Register(8);
    
    /// <summary>
    /// Address Bus
    /// </summary>
    internal readonly Bus AddressBus;

    internal readonly InstructionSet instructionSet;

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

        instructionSet = new MOS6510InstructionSet(this);
    }

    /// <summary>
    /// Load PC from memory location 0xFFFC/0xFFFD
    /// </summary>
    public void Reset()
    {
        AddressBus.Drive(0xFFFC);
        int value = DataBus.Read() << 8;

        AddressBus.Drive(0xFFFD);
        value += DataBus.Read();
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
    internal void Write(int value)
    {
        var address = AddressBus.Read();

        // if address bus is greater than 1, write to data bus
        if(address > 1)
        {
            DataBus.Drive(value);
            DataBus.Trigger();
        }
        // if address bus is 0, write to port bus control register
        else if(address == 0)
        {
            Address0.Write(value);
        }
        // else, address bus is 1, write to port bus data register, and the bus itself
        else
        {
            Address1.Write(value);
            PortBus.Drive(value);
            PortBus.Trigger();
        }
    }

    internal int Read()
    {
        // if address bus is greater than 1, read from data bus
        var address = AddressBus.Read();

        // if address bus is greater than 1, read from data bus
        if(address > 1)
        {
            return DataBus.Read();
        }
        // if address bus is 0, read from port bus control register
        else if(address == 0)
        {
            return Address0.Read();
        }
        // else, address bus is 1, read from port bus data register
        else
        {
            return Address1.Read();
        }
    }
}