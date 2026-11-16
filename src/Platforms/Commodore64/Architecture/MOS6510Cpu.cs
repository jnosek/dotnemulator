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
    /// The CPU strobes this bus to start every memory cycle. Components that answer cycles subscribe to it and
    /// check <see cref="ReadWrite"/> to decide whether to drive <see cref="DataBus"/> (read) or sample it (write).
    /// </remarks>
    public readonly Bus AddressBus;

    /// <summary>
    /// Data Bus
    /// </summary>
    /// <remarks>
    /// Only ever driven, never triggered. On a write the CPU drives it before strobing <see cref="AddressBus"/>;
    /// on a read the responding component drives it while handling the strobe.
    /// </remarks>
    public readonly Bus DataBus;

    /// <summary>
    /// R/W line, signalling whether the current bus cycle is a read or a write.
    /// </summary>
    /// <remarks>
    /// Driven (without notification) by the CPU before each <see cref="AddressBus"/> strobe, and sampled by
    /// the components handling the cycle. Matches the 6510 pin, which is high for read and low for write.
    /// </remarks>
    public readonly ReadWriteWire ReadWrite;

    /// <summary>
    /// Port Bus
    /// </summary>
    public readonly Bus PortBus;

    /// <summary>
    /// IRQ line. <c>true</c> means at least one device is requesting an interrupt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shared, wired-OR line (active-low on the real pin): each device asserts and releases it through its own
    /// <see cref="OpenCollectorWire.Source"/>, so one device releasing it does not cancel another's request.
    /// </para>
    /// <para>
    /// Level-triggered: the CPU samples it between instructions (see <see cref="Step"/>) and services it while it is
    /// asserted and the I flag is clear. A device must release it (normally when the handler acknowledges the
    /// interrupt at the device), or it is taken again as soon as the handler's <c>RTI</c> clears the I flag.
    /// </para>
    /// </remarks>
    public readonly OpenCollectorWire InterruptRequest;

    /// <summary>
    /// NMI line. <c>true</c> means at least one device is requesting a non-maskable interrupt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shared, wired-OR line (active-low on the real pin), driven through <see cref="OpenCollectorWire.Source"/>s.
    /// </para>
    /// <para>
    /// Edge-triggered: the CPU latches each change from released to asserted and services it at the next
    /// instruction boundary, whatever the I flag, even if the line has been released by then. Holding the line
    /// does not cause a second NMI; it must be released and asserted again. While one device holds the line,
    /// another device asserting it produces no edge, so no NMI (on the C64, RESTORE does nothing while CIA 2 holds NMI).
    /// </para>
    /// </remarks>
    public readonly OpenCollectorWire NonMaskableInterrupt;

    /// <summary>
    /// Set by a rising edge on <see cref="NonMaskableInterrupt"/>, cleared when the NMI is serviced.
    /// </summary>
    private bool _nmiPending;

    internal readonly InstructionSet instructionSet;
    
    public MOS6510Cpu(
        Bus addressBus,
        Bus dataBus,
        Bus portBus,
        ReadWriteWire readWrite,
        OpenCollectorWire interruptRequest,
        OpenCollectorWire nonMaskableInterrupt)
    {
        AddressBus = addressBus;
        DataBus = dataBus;
        PortBus = portBus;
        ReadWrite = readWrite;
        InterruptRequest = interruptRequest;
        NonMaskableInterrupt = nonMaskableInterrupt;

        instructionSet = new MOS6510InstructionSet(this);

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

    /// <summary>
    /// Latches a rising edge on <see cref="NonMaskableInterrupt"/>.
    /// </summary>
    /// <remarks>
    /// Only records the request; the NMI is serviced at the next instruction boundary by <see cref="Step"/>,
    /// never in the middle of an instruction. <see cref="OpenCollectorWire"/> only notifies when its level changes,
    /// so <paramref name="value"/> being <c>true</c> is always a released-to-asserted edge.
    /// </remarks>
    private void NonMaskableInterruptHandler(bool value)
    {
        if(value)
            _nmiPending = true;
    }

    /// <summary>
    /// Runs the interrupt sequence: pushes PC and P, sets the I flag and jumps through <paramref name="vector"/>.
    /// </summary>
    /// <remarks>
    /// The copy of P pushed to the stack has the B flag clear, which is how a handler tells a hardware interrupt
    /// from <c>BRK</c>. The I flag is set after P is pushed, so <c>RTI</c> restores the caller's I flag. Setting it
    /// stops a level-triggered IRQ that is still held from being taken again before the handler runs.
    /// </remarks>
    /// <param name="vector">Address of the interrupt vector (<see cref="IRQ_VECTOR"/> or <see cref="NMI_VECTOR"/>).</param>
    private void ServiceInterrupt(int vector)
    {
        // push PC and status register to stack
        StackPush((PC.Value >> 8) & 0xFF);
        StackPush(PC.Value & 0xFF);
        StackPush(P.Value);

        P.InterruptDisableFlag = true;

        // load PC from the vector
        PC.Value = vector;
        PC.Value = ReadNextWord();
    }

    /// <summary>
    /// Executes one instruction, or services one pending interrupt.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Interrupts are checked at the instruction boundary, before the next opcode is fetched, as on the real 6502.
    /// A pending NMI is taken first; otherwise an IRQ is taken if the line is held and the I flag is clear.
    /// Servicing an interrupt is its own step (the real CPU spends 7 cycles on it, like an instruction), and the
    /// result has <see cref="InstructionResult.IsInterrupt"/> set. The handler's first instruction runs on the
    /// following step, so breakpoints on it are hit.
    /// </para>
    /// <para>
    /// Not modelled yet: the one-instruction delay before a change to the I flag by <c>CLI</c>, <c>SEI</c> or
    /// <c>PLP</c> affects IRQ recognition, and an NMI hijacking an IRQ or <c>BRK</c> sequence that is already in progress.
    /// </para>
    /// </remarks>
    internal InstructionResult Step()
    {
        int address = PC.Value;

        // interrupts are only recognized between instructions
        if(_nmiPending)
        {
            _nmiPending = false;
            ServiceInterrupt(NMI_VECTOR);

            return new InstructionResult(address, InstructionResult.NMI_SEQUENCE, 0);
        }

        if(InterruptRequest.Value && !P.InterruptDisableFlag)
        {
            ServiceInterrupt(IRQ_VECTOR);

            return new InstructionResult(address, InstructionResult.IRQ_SEQUENCE, 0);
        }
    
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
            // set up a write cycle: data must be on the bus before the cycle starts
            ReadWrite.Drive(ReadWriteMode.Write);
            DataBus.Drive(value);
            AddressBus.Strobe(address);
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
            PortBus.Strobe(value);
        }
    }

    internal int Read(int address)
    {
        // if address bus is greater than 1, read from data bus
        if(address > 1)
        {
            // start a read cycle; the responding component drives DataBus before Strobe returns
            ReadWrite.Drive(ReadWriteMode.Read);
            AddressBus.Strobe(address);

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
    /// </remarks>
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
    /// </remarks>
    /// <returns></returns>
    internal int StackPop()
    {
        var address = _sp.Increment();
        return Read(STACK_BASE_ADDRESS | address);
    }
}