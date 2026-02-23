using Dotnemulator.Abstraction.Operations;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Architecture;

class MOS6510InstructionSet : InstructionSet
{
    public MOS6510InstructionSet(MOS6510Cpu cpu) : base(256)
    {
        // build instruction set
        Add(AddressModeInstruction.Build<STA>(cpu, STA.AddressModes));
        Add(AddressModeInstruction.Build<ORA>(cpu, ORA.AddressModes));
        Add(AddressModeInstruction.Build<AND>(cpu, AND.AddressModes));
        Add(AddressModeInstruction.Build<EOR>(cpu, EOR.AddressModes));
        Add(AddressModeInstruction.Build<ADC>(cpu, ADC.AddressModes));
        Add(AddressModeInstruction.Build<LDA>(cpu, LDA.AddressModes));
        Add(AddressModeInstruction.Build<CMP>(cpu, CMP.AddressModes));
    }
}
