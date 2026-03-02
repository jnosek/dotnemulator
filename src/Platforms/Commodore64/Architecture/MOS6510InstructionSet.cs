using Dotnemulator.Abstraction.Operations;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Architecture;

class MOS6510InstructionSet : InstructionSet
{
    public MOS6510InstructionSet(MOS6510Cpu cpu) : base(256)
    {
        // system instructions
        Add(new BRK(cpu));

        // stack instructions
        Add(new PHP(cpu));
        Add(new PLP(cpu));
        Add(new PHA(cpu));
        Add(new PLA(cpu));

        // branch instructions
        Add(new BPL(cpu));
        Add(new BMI(cpu));
        Add(new BVC(cpu));
        Add(new BVS(cpu));

        // process/status flag instructions
        AddSet<BIT>(cpu);
        Add(new CLC(cpu));
        Add(new SEC(cpu));
        Add(new CLI(cpu));
        Add(new SEI(cpu));

        // jump instructions
        Add(new JSR(cpu));
        Add(JMP.Build(cpu));
        Add(new RTS(cpu));

        // interrupt instructions
        Add(new RTI(cpu));

        // build Accumulator Instructions
        AddSet<STA>(cpu);
        AddSet<ORA>(cpu);
        AddSet<AND>(cpu);
        AddSet<EOR>(cpu);
        AddSet<ADC>(cpu);
        AddSet<LDA>(cpu);
        AddSet<CMP>(cpu);
        AddSet<SBC>(cpu);
    }

    public void AddSet<T>(MOS6510Cpu cpu) where T : AddressInstruction
    {
        var addressModes = typeof(T).GetField("AddressModes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null) as int[] ??
            throw new InvalidOperationException($"Instruction {typeof(T).Name} does not define Static AddressModes field");

        foreach(var addressMode in addressModes)
        {
            Add((IInstruction)Activator.CreateInstance(typeof(T), cpu, addressMode)!);
        }
    }
}
