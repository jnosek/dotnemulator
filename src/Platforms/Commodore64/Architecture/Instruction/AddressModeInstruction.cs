using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

abstract class AddressModeInstruction : IInstruction
{
    protected readonly MOS6510Cpu CPU;

    public int OpCode { get; }

    public int BaseOpCode { get; }

    public int AddressModeCode { get; }

    /// <summary>
    /// Constructor for instructions without specific address mode. It will set the OpCode to base code and address mode code to undefined.
    /// </summary>
    /// <remarks>
    /// The implement instruction can use the address mode helper methods to decode the operand as needed.
    /// </remarks>
    /// <param name="cpu"></param>
    /// <param name="opCode"></param>
    protected AddressModeInstruction(MOS6510Cpu cpu, int opCode)
    {
        CPU = cpu;

        BaseOpCode = opCode;
        AddressModeCode = AddressMode.Undefined;
        OpCode = opCode;

        DecodeOperand = GetUndefinedOperand;
    }

    /// <summary>
    /// Constructor for instructions with specific address mode. It will set the OpCode by combining base code and address mode code.
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="baseCode"></param>
    /// <param name="addressMode"></param>
    /// <exception cref="InvalidOperationException"></exception>
    protected AddressModeInstruction(MOS6510Cpu cpu, int baseCode, int addressMode)
    {
        CPU = cpu;

        BaseOpCode = baseCode;
        AddressModeCode = addressMode;
        OpCode = baseCode | addressMode;

        DecodeOperand = addressMode switch 
        {
            AddressMode.Immediate => GetImmediateOperand,
            AddressMode.ZeroPage => GetZeroPageOperand,
            AddressMode.ZeroPageX => GetZeroPageXOperand,
            AddressMode.Absolute => GetAbsoluteOperand,
            AddressMode.AbsoluteX => GetAbsoluteXOperand,
            AddressMode.AbsoluteY => GetAbsoluteYOperand,
            AddressMode.Indexed_Indirect => GetIndexedIndirectOperand,
            AddressMode.Indirect_Indexed => GetIndirectIndexedOperand,
            _ => throw new InvalidOperationException($"Unsupported address mode: {addressMode}")
        };
    }

    public abstract void Execute(int instruction);

    public Func<int> DecodeOperand { get; }

    protected int GetImmediateOperand()
    {
        var operand = CPU.PC.Advance();
        return operand;
    }

    protected int GetZeroPageOperand()
    {
        var operand = CPU.ReadNextByte();

        return operand;
    }

    protected int GetZeroPageXOperand()
    {
        var operand = CPU.ReadNextByte();
        operand = (operand + CPU.X.Value) & 0xFF;

        return operand;
    }

    protected int GetAbsoluteOperand()
    {
        var operand = CPU.ReadNextWord();

        return operand;
    } 

    protected int GetAbsoluteXOperand()
    {
        var operand = CPU.ReadNextWord();

        operand = (operand + CPU.X.Value) & 0xFFFF;

        return operand;
    }

    protected int GetAbsoluteYOperand()
    {
        var operand = CPU.ReadNextWord();

        operand = (operand + CPU.Y.Value) & 0xFFFF;

        return operand;
    }

    protected int GetIndexedIndirectOperand()
    {
        var address = CPU.ReadNextByte();
        address = (address + CPU.X.Value) & 0xFF;

        // get low byte
        var operand = CPU.Read(address);

        // get high byte
        operand |= CPU.Read(address + 1) << 8;

        return operand;
    }

    protected int GetIndirectIndexedOperand()
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

    protected int GetUndefinedOperand()
    {
        return 0;   
    }
}
