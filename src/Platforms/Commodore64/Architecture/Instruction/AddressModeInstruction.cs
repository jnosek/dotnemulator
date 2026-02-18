using Dotnemulator.Abstraction.Operations;

namespace Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

abstract class AddressModeInstruction : IInstruction
{
    public static IInstruction[] Build<T>(MOS6510Cpu cpu, AddressMode[] addressModes) where T : AddressModeInstruction
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

    public byte BaseOpCode { get; }
    
    public AddressMode AddressMode { get; }

    public int Cycles => throw new NotImplementedException();

    protected AddressModeInstruction(MOS6510Cpu cpu, byte baseCode, AddressMode addressMode)
    {
        CPU = cpu;
        OpCode = (byte)(baseCode | ((byte)addressMode << 2));
        BaseOpCode = baseCode;
        AddressMode = addressMode;

        DecodeAddress = addressMode switch 
        {
            AddressMode.Immediate => GetImmediateAddress,
            AddressMode.ZeroPage => GetZeroPageAddress,
            AddressMode.ZeroPageX => GetZeroPageXAddress,
            AddressMode.Absolute => GetAbsoluteAddress,
            AddressMode.AbsoluteX => GetAbsoluteXAddress,
            AddressMode.AbsoluteY => GetAbsoluteYAddress,
            AddressMode.Indexed_Indirect => GetIndexIndirectAddress,
            AddressMode.Indirect_Indexed => GetIndirectIndexedAddress,
            _ => throw new InvalidOperationException($"Unsupported address mode: {addressMode}")
        };
    }

    public abstract void Execute(int instruction);

    public Func<int, int> DecodeAddress { get; }

    private int GetImmediateAddress(int instruction) => (instruction & 0x00FF0000) >> 16;
    private int GetZeroPageAddress(int instruction) => (instruction & 0x00FF0000) >> 16;
    private int GetZeroPageXAddress(int instruction) => ((instruction & 0x00FF0000) >> 16) + CPU.X.Read();
    private int GetAbsoluteAddress(int instruction) => (instruction & 0x00FFFF00) >> 8;
    private int GetAbsoluteXAddress(int instruction) => ((instruction & 0x00FFFF00) >> 8) + CPU.X.Read();
    private int GetAbsoluteYAddress(int instruction) => ((instruction & 0x00FFFF00) >> 8) + CPU.Y.Read();

    private int GetIndexIndirectAddress(int instruction)
    {
         var zeroPageAddress = (instruction & 0x00FF0000) >> 16;
        var zeroPageAddressOffset = zeroPageAddress + CPU.X.Read();

        CPU.AddressBus.Drive(zeroPageAddressOffset);
        CPU.AddressBus.Trigger();
        var addressHigh = CPU.Read() << 8;

        CPU.AddressBus.Drive(zeroPageAddressOffset + 1);
        CPU.AddressBus.Trigger();
        var addressLow = CPU.Read();

        return addressHigh | addressLow;
    }

    private int GetIndirectIndexedAddress(int instruction)
    {
        var zeroPageAddress = (instruction & 0x00FF0000) >> 16;

        CPU.AddressBus.Drive(zeroPageAddress);
        CPU.AddressBus.Trigger();
        var addressHigh = CPU.Read() << 8;

        CPU.AddressBus.Drive(zeroPageAddress + 1);
        CPU.AddressBus.Trigger();
        var addressLow = CPU.Read();

        return (addressHigh | addressLow) + CPU.Y.Read();
    }
}
