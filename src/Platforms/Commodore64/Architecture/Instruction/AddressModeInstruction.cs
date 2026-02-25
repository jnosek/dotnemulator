using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

abstract class AddressModeInstruction : IInstruction
{
    public static IInstruction[] Build<T>(MOS6510Cpu cpu, int[] addressModes) where T : AddressModeInstruction
    {
        var instructions = new IInstruction[addressModes.Length];
        for (int i = 0; i < addressModes.Length; i++)
        {
            instructions[i] = (IInstruction)Activator.CreateInstance(typeof(T), cpu, addressModes[i])!;
        }
        return instructions;
    }

    protected readonly MOS6510Cpu CPU;

    public int OpCode { get; }

    public int BaseOpCode { get; }

    public int AddressModeCode { get; }

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
}
