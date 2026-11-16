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

    /// <summary>
    /// Number of I/O port pins on the 6510 (P0-P5).
    /// </summary>
    public const int PORT_PIN_COUNT = 6;

    /// <summary>
    /// Mask of the I/O port pins (P0-P5).
    /// </summary>
    public const int PORT_PIN_MASK = (1 << PORT_PIN_COUNT) - 1;

    /// <summary>
    /// On-chip I/O port data direction register, at address <c>$0000</c>.
    /// </summary>
    /// <remarks>
    /// A 1 bit makes the matching port pin an output, driven from <see cref="Address1"/>; a 0 bit makes it an
    /// input. Cleared at reset, so every pin starts as an input.
    /// </remarks>
    internal readonly Register Address0 = new Register(8);

    /// <summary>
    /// On-chip I/O port data register, at address <c>$0001</c>.
    /// </summary>
    /// <remarks>
    /// Only the bits that <see cref="Address0"/> makes outputs reach the pins. Cleared at reset.
    /// </remarks>
    internal readonly Register Address1 = new Register(8);
    
    /// <summary>
    /// Address Bus used to specify the memory address for read and write operations.
    /// </summary>
    /// <remarks>
    /// The CPU strobes this bus to start every memory cycle. Components that answer cycles subscribe to it and
    /// check <see cref="ReadWrite"/> to decide whether to drive <see cref="DataBus"/> (read) or sample it (write).
    /// </remarks>
    internal readonly Bus AddressBus;

    /// <summary>
    /// Data Bus
    /// </summary>
    /// <remarks>
    /// Only ever driven, never triggered. On a write the CPU drives it before strobing <see cref="AddressBus"/>;
    /// on a read the responding component drives it while handling the strobe.
    /// </remarks>
    internal readonly Bus DataBus;

    /// <summary>
    /// R/W line, signalling whether the current bus cycle is a read or a write.
    /// </summary>
    /// <remarks>
    /// Driven (without notification) by the CPU before each <see cref="AddressBus"/> strobe, and sampled by
    /// the components handling the cycle. Matches the 6510 pin, which is high for read and low for write.
    /// </remarks>
    internal readonly ReadWriteWire ReadWrite;

    /// <summary>
    /// The I/O port pins P0-P5.
    /// </summary>
    /// <remarks>
    /// A board net owned by the <see cref="Emulator"/>. The CPU strobes it with the pin levels whenever
    /// <c>$0000</c> or <c>$0001</c> is written (see <see cref="UpdatePortPins"/>).
    /// </remarks>
    internal readonly Bus PortBus;

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
    internal readonly OpenCollectorWire InterruptRequest;

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
    internal readonly OpenCollectorWire NonMaskableInterrupt;

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

    /// <summary>
    /// Resets the CPU: clears the I/O port registers and loads PC from the reset vector.
    /// </summary>
    /// <remarks>
    /// At reset every port pin is an input, so the pulled-up lines read high and the PLA selects the default
    /// memory configuration (BASIC, KERNAL and I/O visible) until the KERNAL sets <c>$0000</c> to <c>$2F</c>
    /// and <c>$0001</c> to <c>$37</c>.
    /// </remarks>
    /// <param name="pcValue">Start address to use instead of the reset vector (for tests).</param>
    internal void Initialize(int? pcValue = null)
    {
        Address0.Value = 0;
        Address1.Value = 0;
        UpdatePortPins();

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
    /// Puts the I/O port's pin levels on <see cref="PortBus"/>.
    /// </summary>
    /// <remarks>
    /// Output pins (a 1 in <see cref="Address0"/>) carry the matching bit of <see cref="Address1"/>. Input pins are
    /// not driven by the CPU; on the C64 every port line is pulled high, so an input pin reads as 1.
    /// </remarks>
    private void UpdatePortPins()
    {
        var direction = Address0.Value;
        var pins = (Address1.Value & direction) | (~direction & PORT_PIN_MASK);

        PortBus.Strobe(pins);
    }

    /// <summary>
    /// Writes a value to memory, or to the on-chip I/O port registers for addresses 0 and 1.
    /// </summary>
    /// <remarks>
    /// The 6510 maps its data direction register to <c>$0000</c> and its port data register to <c>$0001</c>.
    /// Writing either one updates the port pins. Value is properly masked before stored.
    /// </remarks>
    /// <param name="address">16-bit address to write.</param>
    /// <param name="value">Byte to write.</param>
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
        // if address bus is 0, write to the port data direction register
        else if(address == 0)
        {
            Address0.Value = value;
            UpdatePortPins();
        }
        // else, address bus is 1, write to the port data register
        else
        {
            Address1.Value = value;
            UpdatePortPins();
        }
    }

    /// <summary>
    /// Reads a value from memory, or from the on-chip I/O port registers for addresses 0 and 1.
    /// </summary>
    /// <remarks>
    /// Reading <c>$0001</c> returns the data register for output bits and the pin levels for input bits.
    /// Bits 6-7 are not port pins, so for them only the data register's output bits can read as 1.
    /// </remarks>
    /// <param name="address">16-bit address to read.</param>
    /// <returns>The byte read.</returns>
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
        // if address bus is 0, read the port data direction register
        else if(address == 0)
        {
            return Address0.Value;
        }
        // else, address bus is 1: output bits read the data register, input bits read the pins
        else
        {
            var direction = Address0.Value;
            return (Address1.Value & direction) | (PortBus.Read() & ~direction);
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