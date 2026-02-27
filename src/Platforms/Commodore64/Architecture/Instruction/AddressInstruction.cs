using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

abstract class AddressInstruction : IInstruction
{
    protected readonly MOS6510Cpu CPU;

    public int OpCode { get; }

    public int BaseOpCode { get; }

    public int AddressModeCode { get; }

    /// <summary>
    /// Constructor for instructions with specific address mode. It will set the OpCode by combining base code and address mode code.
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="baseCode"></param>
    /// <param name="addressMode"></param>
    /// <exception cref="InvalidOperationException"></exception>
    protected AddressInstruction(MOS6510Cpu cpu, int baseCode, int addressMode)
    {
        CPU = cpu;

        BaseOpCode = baseCode;
        AddressModeCode = addressMode;

        // if this is an explicit mode address
        if((addressMode & 0xFF00) > 0)
        {
            // basecode is the opcode
            OpCode = baseCode;
        }
        else
        {
            OpCode = baseCode | addressMode;    
        }

        DecodeOperand = addressMode switch 
        {
            // intrinsic address modes
            // these modes are include in the coding of opcodes
            AddressMode.Immediate => GetImmediateOperand,
            AddressMode.ZeroPage => GetZeroPageOperand,
            AddressMode.ZeroPageX => GetZeroPageXOperand,
            AddressMode.Absolute => GetAbsoluteOperand,
            AddressMode.AbsoluteX => GetAbsoluteXOperand,
            AddressMode.AbsoluteY => GetAbsoluteYOperand,
            AddressMode.Indexed_Indirect => GetIndexedIndirectOperand,
            AddressMode.Indirect_Indexed => GetIndirectIndexedOperand,

            // explicit address modes
            // these codes are used to specify the exact operand logic to use for an opcode
            AddressMode.Relative => GetRelativeOperand,
            AddressMode.Indirect => GetIndirectOperand,
            AddressMode.Explicit_Immediate => GetImmediateOperand,
            AddressMode.Explicit_ZeroPage => GetZeroPageOperand,
            AddressMode.Explicit_ZeroPageX => GetZeroPageXOperand,
            AddressMode.Explicit_Absolute => GetAbsoluteOperand,
            AddressMode.Explicit_AbsoluteX => GetAbsoluteXOperand,
            AddressMode.Explicit_AbsoluteY => GetAbsoluteYOperand,
            AddressMode.Explicit_Indexed_Indirect => GetIndexedIndirectOperand,
            AddressMode.Explicit_Indirect_Indexed => GetIndirectIndexedOperand,

            _ => throw new InvalidOperationException($"Unsupported address mode: {addressMode}")
        };
    }

    public void Execute(int instruction)
    {
        var operand = DecodeOperand();
        Execute(instruction, operand);
    }

    public abstract void Execute(int instruction, int address);

    private Func<int> DecodeOperand { get; }

    private int GetImmediateOperand()
    {
        var operand = CPU.PC.Advance();
        return operand;
    }

    private int GetZeroPageOperand()
    {
        var operand = CPU.ReadNextByte();

        return operand;
    }

    private int GetZeroPageXOperand()
    {
        var operand = CPU.ReadNextByte();
        operand = (operand + CPU.X.Value) & 0xFF;

        return operand;
    }

    private int GetAbsoluteOperand()
    {
        var operand = CPU.ReadNextWord();

        return operand;
    } 

    private int GetAbsoluteXOperand()
    {
        var operand = CPU.ReadNextWord();

        operand = (operand + CPU.X.Value) & 0xFFFF;

        return operand;
    }

    private int GetAbsoluteYOperand()
    {
        var operand = CPU.ReadNextWord();

        operand = (operand + CPU.Y.Value) & 0xFFFF;

        return operand;
    }

    private int GetIndexedIndirectOperand()
    {
        var address = CPU.ReadNextByte();
        address = (address + CPU.X.Value) & 0xFF;

        // get low byte
        var operand = CPU.Read(address);

        // get high byte
        operand |= CPU.Read(address + 1) << 8;

        return operand;
    }

    private int GetIndirectIndexedOperand()
    {
        var address = CPU.ReadNextByte();
        
        // get low byte
        var operand = CPU.Read(address);

        // get high byte
        operand |= CPU.Read(address + 1) << 8;
        
        // add y
        operand = (operand + CPU.Y.Value) & 0xFFFF;

        return operand;
    }

    private int GetUndefinedOperand()
    {
        throw new InvalidOperationException("should not be using undefined operand addressing");  
    }

    private int GetRelativeOperand()
    {
        var operand = CPU.ReadNextByte();

        return (CPU.PC.Value + operand) & 0xFFFF;
    }

    private int GetIndirectOperand()
    {
        var address = CPU.ReadNextWord();

        // get low byte
        var operand = CPU.Read(address);

        // get high byte
        operand |= CPU.Read(address + 1) << 8;

        return operand;
    }
}
