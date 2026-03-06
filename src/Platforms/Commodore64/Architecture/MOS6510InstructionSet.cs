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
        Add(new BCS(cpu));
        Add(new BNE(cpu));
        Add(new BEQ(cpu));

        // process/status flag instructions
        Add(AddressInstruction.Build<BIT>(cpu));
        Add(new CLC(cpu));
        Add(new SEC(cpu));
        Add(new CLI(cpu));
        Add(new SEI(cpu));
        Add(new CLV(cpu));
        Add(new CLD(cpu));
        Add(new SED(cpu));

        // jump instructions
        Add(new JSR(cpu));
        Add(JMP.Build(cpu));
        Add(new RTS(cpu));
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
        Add(AddressInstruction.Build<LDY>(cpu));
        Add(new DEY(cpu));
        Add(new INY(cpu));
        Add(new TYA(cpu));
        Add(new TAY(cpu));
        Add(AddressInstruction.Build<CPY>(cpu));

        Add(AddressInstruction.Build<STX>(cpu));
        Add(new INX(cpu));
        Add(new TXA(cpu));
        Add(new TXS(cpu));
        Add(AddressInstruction.Build<CPX>(cpu));

        // Read Modify Write Instructions
        Add(AddressInstruction.Build<ASL>(cpu));
        Add(new ASLA(cpu));
        Add(AddressInstruction.Build<ROL>(cpu));
        Add(new ROLA(cpu));
        Add(AddressInstruction.Build<LSR>(cpu));
        Add(new LSRA(cpu));
        Add(AddressInstruction.Build<ROR>(cpu));
        Add(new RORA(cpu));
    }
}
