namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Abstraction.Operations;

public class MOS6510Cpu
{
    public const int STACK_START_ADDRESS = 0x01FF;
    public const int STACK_END_ADDRESS = 0x0100;

    public const int RESET_VECTOR = 0xFFFC;
    public const int IRQ_VECTOR = 0xFFFE;
    public const int NMI_VECTOR = 0xFFFA;

    /// <summary>
    /// Program Counter
    /// </summary>
    internal readonly Register PC = new Register(16);

    /// <summary>
    /// Status Register
    /// </summary>
    internal readonly StatusRegister P = new StatusRegister();

    /// <summary>
    /// Internal Register to track place of the Stack Pointer
    /// </summary>
    /// <remarks>
    /// In the 6510, this is actually an 8 bit register, used as an offset to the
    /// 0x0100 Stack End Address. For cleaner implementation in the emulator, the whole 
    /// address is stored in a 16-bit register
    /// </remarks>
    private readonly Register _sp = new Register(16);

    internal int SP => _sp.Value;

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
    /// Address Bus used to specify the memory address for read and write operations.
    /// </summary>
    /// <remarks>
    /// A trigger of the AddressBus will always cause the memory map to drive the value to the DataBus for a subsequent read operation.
    /// </remarks>
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

        // set start of stack pointer offset
        _sp.Value = STACK_START_ADDRESS;
    }

    private void InitializePC()
    {
        // set default memory mode
        Write(0x0001, 0b0000_0111);

        // load PC from reset vector
        PC.Value = RESET_VECTOR;
        var address = ReadNextWord();

        PC.Value = address;
    }

    /// <summary>
    /// Load PC from RESET_VECTOR memory location (0xFFFC/0xFFFD)
    /// in little-endian order
    /// </summary>
    public void Reset()
    {
        InitializePC();

        RunLoop();
    }

    internal void Test()
    {
        InitializePC();

        TestLoop();
    }

    /// <summary> 
    /// Executes the instruction fetch-decode-execute loop. This method will run indefinitely until the emulator is stopped. 
    /// </summary>
    private void RunLoop()
    {
        while(true)
        {
            // fetch
            int instruction = ReadNextByte();

            // decode
            var operation = instructionSet[instruction];

            // execute
            operation.Execute(instruction);
        }
    }
    
    /// <summary> 
    /// Executes the instruction fetch-decode-execute loop. 
    /// This method will run until a NOP instruction is encountered. 
    /// </summary>
    private void TestLoop()
    {
        while(true)
        {
            // fetch
            int instruction = ReadNextByte();

            // NOP instruction
            if(instruction == 0xEA) 
                break;

            // decode
            var operation = instructionSet[instruction];

            // execute
            operation.Execute(instruction);
        }
    }

    /// <summary>
    /// Writes a value to the data bus. If the address bus is set to 0 or 1, it writes to the port bus instead.
    /// </summary>
    /// <remarks>
    /// The MOS6510 has a unique feature where internally the first two addresses (0 and 1) are mapped to special registers that control the port bus. 
    /// This method takes that into account when writing values.
    /// Value is properly masked before stored
    /// </remarks>
    /// <param name="value"></param>
    internal void Write(int address, int value)
    {
        // if address bus is greater than 1, write to data bus
        if(address > 1)
        {
            AddressBus.Drive(address);
            AddressBus.Trigger();

            DataBus.Drive(value);
            DataBus.Trigger();
        }
        // if address bus is 0, write to port bus control register
        else if(address == 0)
        {
            Address0.Value = value;
        }
        // else, address bus is 1, write to port bus data register, and the bus itself
        else
        {
            Address1.Value = value;
            PortBus.Drive(value);
            PortBus.Trigger();
        }
    }

    internal int Read(int address)
    {
        // if address bus is greater than 1, read from data bus
        if(address > 1)
        {
            // drive value to AddressBus and trigger for next read
            AddressBus.Drive(address);
            AddressBus.Trigger();

            return DataBus.Read();
        }
        // if address bus is 0, read from port bus control register
        else if(address == 0)
        {
            return Address0.Value;
        }
        // else, address bus is 1, read from port bus data register
        else
        {
            return Address1.Value;
        }
    }

    /// <summary>
    /// Reads the next byte from memory at the current PC, and increments the PC. 
    /// This is a common operation for fetching instruction operands.
    /// </summary>
    /// <returns></returns>
    public int ReadNextByte()
    {
        var address = PC.Advance();

        return Read(address);
    }

    /// <summary>
    /// Reads the next two bytes from memory at the current PC, 
    /// combines them into a 16-bit address (little-endian), 
    /// and increments the PC by 2.
    /// </summary>
    /// <returns></returns>
    public int ReadNextWord()
    {
        // get low byte
        var address = ReadNextByte();

        // get high byte
        address |= ReadNextByte() << 8;

        return address;
    }

    internal void StackPush(int value)
    {
       Write(
            _sp.Decrement(),
            value);
    }

    internal int StackPop()
    {
        var result = Read(_sp.Value);
        _sp.Increment();
        return result;
    }
}