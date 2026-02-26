using Dotnemulator.Abstraction.Operations;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Microsoft.VisualBasic;

namespace Dotnemulator.Platforms.Commodore64.Architecture;

class MOS6510InstructionSet : InstructionSet
{
    public MOS6510InstructionSet(MOS6510Cpu cpu) : base(256)
    {
        // system instructions
        Add(new BRK(cpu));

        // stack instructions
        Add(new PHP(cpu));

        // branch instructions
        Add(new BPL(cpu));

        // process/status flag instructions
        Add(new CLC(cpu));

        // jump instructions
        Add(new JSR(cpu));

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

    public void AddSet<T>(MOS6510Cpu cpu) where T : AddressModeInstruction
    {
        var addressModes = typeof(T).GetField("AddressModes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null) as int[] ??
            throw new InvalidOperationException($"Instruction {typeof(T).Name} does not define Static AddressModes field");

        foreach(var addressMode in addressModes)
        {
            Add((IInstruction)Activator.CreateInstance(typeof(T), cpu, addressMode)!);
        }
    }
}
