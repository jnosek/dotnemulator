namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Abstraction.Operations;

public class MOS6510Cpu
{
    public const int STACK_START_ADDRESS = 0x01FF;
    public const int STACK_BASE_ADDRESS = 0x0100;

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
    /// This 8 bit register will correctly replicate the overflow 
    /// and underflow behavior of the stack pointer
    /// </remarks>
    private readonly Register _sp = new Register(8);

    /// <summary>
    /// Current value of the Stack Pointer
    /// Set, only for use by the TXS Instruction
    /// </summary>
    internal int SP 
    {
        get => _sp.Value;
        set => _sp.Value = value;
    }

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
    public readonly Bus AddressBus;

    /// <summary>
    /// Data Bus
    /// </summary>
    public readonly Bus DataBus;

    /// <summary>
    /// Port Bus
    /// </summary>
    public readonly Bus PortBus;

    public readonly Wire InterruptRequest = new Wire();
    public readonly Wire NonMaskableInterrupt = new Wire();

    internal readonly InstructionSet instructionSet;
    
    public MOS6510Cpu(Bus addressBus, Bus dataBus, Bus portBus)
    {
        AddressBus = addressBus;
        DataBus = dataBus;
        PortBus = portBus;

        instructionSet = new MOS6510InstructionSet(this);

        InterruptRequest.Subscribe(InterruptRequestHandler);
        NonMaskableInterrupt.Subscribe(NonMaskableInterruptHandler);

        // set start of stack pointer offset (last byte)
        _sp.Value = STACK_START_ADDRESS & 0xFF;
    }

    internal void Initialize(int? pcValue = null)
    {
        // set default memory mode
        Write(0x0001, 0b0000_0111);

        // use provided PC value for testing
        if(pcValue.HasValue)
        {
            PC.Value = pcValue.Value;
        }
        // otherwise load from reset vector
        else
        {
            PC.Value = RESET_VECTOR;
            var address = ReadNextWord();

            PC.Value = address;
        }
    }

    internal void InterruptRequestHandler(bool value)
    {
        // if the signal goes low, or if the interrupt disable flag is set, ignore the interrupt request
        if(!value || P.InterruptDisableFlag)
            return;

        // push PC and status register to stack
        StackPush((PC.Value >> 8) & 0xFF);
        StackPush(PC.Value & 0xFF);
        StackPush(P.Value);

        // load PC from interrupt vector
        PC.Value = IRQ_VECTOR;
        var address = ReadNextWord();

        PC.Value = address;
    }

    internal void NonMaskableInterruptHandler(bool value)
    {
        // if the signal goes low, ignore the interrupt request
        if(!value)
            return;

        // push PC and status register to stack
        StackPush((PC.Value >> 8) & 0xFF);
        StackPush(PC.Value & 0xFF);
        StackPush(P.Value);

        // load PC from NMI vector
        PC.Value = NMI_VECTOR;
        var address = ReadNextWord();

        PC.Value = address;
    }

    internal InstructionResult Step()
    {
        int address = PC.Value;
    
        // fetch
        int instruction = ReadNextByte();

        // decode
        var operation = instructionSet[instruction];

        // execute
        operation.Execute(instruction);

        return new InstructionResult(address, instruction, 0);
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

    /// <summary>
    /// Pushes a value onto the stack. The stack grows downwards from 0x01FF to 0x0100.
    /// </summary>
    /// <remarks>
    /// For a stack overflow, the value of 0x00 will wrap around to 0xFF without an error
    /// <remarks>
    /// <param name="value"></param>
    internal void StackPush(int value)
    {
       Write(STACK_BASE_ADDRESS | _sp.Value, value);
       _sp.Decrement();
    }

    /// <summary>
    /// Pops a value from the stack. The stack shrinks upwards from 0x0100 to 0x01FF.
    /// </summary>
    /// <remarks>
    /// For a stack underflow, the value of 0xFF will wrap around to 0x00 without an error
    /// <remarks>
    /// <returns></returns>
    internal int StackPop()
    {
        var address = _sp.Increment();
        return Read(STACK_BASE_ADDRESS | address);
    }
}