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
        Add(new BCC(cpu));

        // process/status flag instructions
        Add(AddressInstruction.Build<BIT>(cpu));
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
        Add(AddressInstruction.Build<STA>(cpu));
        Add(AddressInstruction.Build<ORA>(cpu));
        Add(AddressInstruction.Build<AND>(cpu));
        Add(AddressInstruction.Build<EOR>(cpu));
        Add(AddressInstruction.Build<ADC>(cpu));
        Add(AddressInstruction.Build<LDA>(cpu));
        Add(AddressInstruction.Build<CMP>(cpu));
        Add(AddressInstruction.Build<SBC>(cpu));

        // build X/Y Instructions
        Add(AddressInstruction.Build<STY>(cpu));
        Add(new DEY(cpu));
        Add(new TYA(cpu));
    }
}
