using System.Reflection;
using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

/// <summary>
/// An instruction that takes a memory address as an operand
/// </summary>
/// <remarks>
/// There are several methods of handling addressing. 
/// They are present in this class, but the logic to select them must occur in the inherited class
/// </remarks>
abstract class AddressInstruction : IInstruction
{
    public static T[] Build<T>(MOS6510Cpu cpu) where T : AddressInstruction
    {
        var addressModes = typeof(T)
            .GetField("AddressModes", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null) as int[]
            ?? throw new InvalidOperationException(
                $"Instruction {typeof(T).Name} does not define static AddressModes field");

        var ctor = typeof(T).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            [typeof(MOS6510Cpu), typeof(int)])
            ?? throw new InvalidOperationException(
                $"Instruction {typeof(T).Name} does not define a private constructor (MOS6510Cpu, int)");

        return addressModes
            .Select(mode => (T)ctor.Invoke([cpu, mode]))
            .ToArray();
    }

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

        // combine base code and address mode to get the opcode
        OpCode = baseCode | addressMode; 
    }



    public void Execute(int instruction)
    {
        var operand = DecodeOperand();
        Execute(instruction, operand);
    }

    public abstract void Execute(int instruction, int address);

    protected abstract Func<int> DecodeOperand { get; }

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

    protected int GetZeroPageYOperand()
    {
        var operand = CPU.ReadNextByte();
        operand = (operand + CPU.Y.Value) & 0xFF;

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
        throw new InvalidOperationException("should not be using undefined operand addressing");  
    }

    protected int GetIndirectOperand()
    {
        var address = CPU.ReadNextWord();

        // get low byte
        var operand = CPU.Read(address);

        // get high byte
        operand |= CPU.Read(address + 1) << 8;

        return operand;
    }
}
