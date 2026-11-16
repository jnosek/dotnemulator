using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class Interrupt_Test
{
    private const int PROGRAM_START = 0xE000;
    private const int IRQ_HANDLER = 0x0100;     // set by WithTestKernel
    private const int NMI_HANDLER = 0x0200;

    private const int LDA_IMMEDIATE = LDA.BASE_OP_CODE | AccumulatorInstruction.Immediate;

    /// <summary>
    /// Builds a machine with <paramref name="program"/> at $E000, the IRQ vector at $0100 and the NMI vector at
    /// $0200 (both handlers start with a NOP), and resets the CPU without running anything.
    /// </summary>
    private static Emulator Build(params byte[] program)
    {
        var kernel = new byte[0x2000];
        program.CopyTo(kernel, 0);

        kernel[MOS6510Cpu.NMI_VECTOR - PROGRAM_START] = NMI_HANDLER & 0xFF;
        kernel[MOS6510Cpu.NMI_VECTOR - PROGRAM_START + 1] = NMI_HANDLER >> 8;

        var emulator = new EmulatorBuilder()
            .WithTestKernel(kernel)
            .WithRam([(IRQ_HANDLER, NOP.OP_CODE), (NMI_HANDLER, NOP.OP_CODE)])
            .Build();

        emulator.Cpu.Initialize();

        return emulator;
    }

    private static int PeekStack(Emulator emulator, int offset) =>
        emulator.MemoryMap.PeekRam(MOS6510Cpu.STACK_BASE_ADDRESS | ((emulator.Cpu.SP + offset) & 0xFF));

    // --- IRQ ---

    [TestMethod]
    public void Irq_IsNotTakenInsideTheCallerThatAssertsIt()
    {
        var emulator = Build(LDA_IMMEDIATE, 0x01, NOP.OP_CODE);
        var sp = emulator.Cpu.SP;

        emulator.Cpu.InterruptRequest.Strobe(true);

        Assert.AreEqual(PROGRAM_START, emulator.Cpu.PC.Value);
        Assert.AreEqual(sp, emulator.Cpu.SP);
    }

    [TestMethod]
    public void Irq_IsTakenAtNextInstructionBoundary()
    {
        var emulator = Build(LDA_IMMEDIATE, 0x01, LDA_IMMEDIATE, 0x02, NOP.OP_CODE);
        emulator.Cpu.Step();                                   // LDA #$01

        emulator.Cpu.InterruptRequest.Strobe(true);
        var result = emulator.Cpu.Step();

        Assert.IsTrue(result.IsInterrupt);
        Assert.AreEqual(InstructionResult.IRQ_SEQUENCE, result.Instruction);
        Assert.AreEqual(0xE002, result.Address);
        Assert.AreEqual(IRQ_HANDLER, emulator.Cpu.PC.Value);
        Assert.AreEqual(0x01, emulator.Cpu.A.Value);           // second LDA has not run

        // return address and status pushed
        Assert.AreEqual(0xE0, PeekStack(emulator, 3));
        Assert.AreEqual(0x02, PeekStack(emulator, 2));
    }

    [TestMethod]
    public void Irq_HandlerFirstInstruction_RunsOnFollowingStep()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Cpu.InterruptRequest.Strobe(true);
        emulator.Cpu.Step();                                   // interrupt sequence

        var result = emulator.Cpu.Step();

        Assert.IsFalse(result.IsInterrupt);
        Assert.AreEqual(IRQ_HANDLER, result.Address);
        Assert.AreEqual(NOP.OP_CODE, result.Instruction);
    }

    [TestMethod]
    public void Irq_SetsInterruptDisableFlag_AndPushesItClear()
    {
        var emulator = Build(NOP.OP_CODE);
        Assert.IsFalse(emulator.Cpu.P.InterruptDisableFlag);

        emulator.Cpu.InterruptRequest.Strobe(true);
        emulator.Cpu.Step();

        Assert.IsTrue(emulator.Cpu.P.InterruptDisableFlag);

        var pushed = PeekStack(emulator, 1);
        Assert.AreEqual(0, pushed & StatusFlag.InterruptDisable, "pushed P keeps the caller's I flag");
        Assert.AreEqual(0, pushed & StatusFlag.BreakCommand, "hardware interrupts push B clear");
    }

    [TestMethod]
    public void Irq_HeldHigh_IsNotRetakenWhileInterruptsDisabled()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Memory(IRQ_HANDLER, LDA_IMMEDIATE, 0x01, LDA_IMMEDIATE, 0x02, NOP.OP_CODE);
        emulator.Cpu.InterruptRequest.Strobe(true);

        emulator.Cpu.Step();                                   // interrupt sequence
        var sp = emulator.Cpu.SP;
        var first = emulator.Cpu.Step();
        var second = emulator.Cpu.Step();

        Assert.IsFalse(first.IsInterrupt);
        Assert.IsFalse(second.IsInterrupt);
        Assert.AreEqual(sp, emulator.Cpu.SP);
        Assert.AreEqual(0x02, emulator.Cpu.A.Value);
    }

    [TestMethod]
    public void Irq_IsIgnoredWhileInterruptDisableFlagSet()
    {
        var emulator = Build(SEI.OP_CODE, LDA_IMMEDIATE, 0x01, NOP.OP_CODE);
        emulator.Cpu.Step();                                   // SEI

        emulator.Cpu.InterruptRequest.Strobe(true);
        var result = emulator.Cpu.Step();

        Assert.IsFalse(result.IsInterrupt);
        Assert.AreEqual(0x01, emulator.Cpu.A.Value);
    }

    [TestMethod]
    public void Irq_ReleasedBeforeBoundary_IsNotTaken()
    {
        var emulator = Build(LDA_IMMEDIATE, 0x01, NOP.OP_CODE);

        emulator.Cpu.InterruptRequest.Strobe(true);
        emulator.Cpu.InterruptRequest.Strobe(false);
        var result = emulator.Cpu.Step();

        Assert.IsFalse(result.IsInterrupt);
    }

    [TestMethod]
    public void Irq_DrivenWithoutNotification_IsStillTaken()
    {
        var emulator = Build(NOP.OP_CODE);

        emulator.Cpu.InterruptRequest.Drive(true);
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InstructionResult.IRQ_SEQUENCE, result.Instruction);
    }

    [TestMethod]
    public void Irq_StillHeldAfterRti_IsTakenAgain()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Memory(IRQ_HANDLER, RTI.OP_CODE);
        emulator.Cpu.InterruptRequest.Strobe(true);

        emulator.Cpu.Step();                                   // interrupt sequence
        emulator.Cpu.Step();                                   // RTI restores I = 0
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InstructionResult.IRQ_SEQUENCE, result.Instruction);
    }

    // --- NMI ---

    [TestMethod]
    public void Nmi_IsTakenAtNextBoundary_EvenWithInterruptsDisabled()
    {
        var emulator = Build(SEI.OP_CODE, NOP.OP_CODE);
        emulator.Cpu.Step();                                   // SEI

        emulator.Cpu.NonMaskableInterrupt.Strobe(true);
        Assert.AreEqual(0xE001, emulator.Cpu.PC.Value, "not taken inside the Strobe call");

        var result = emulator.Cpu.Step();

        Assert.AreEqual(InstructionResult.NMI_SEQUENCE, result.Instruction);
        Assert.AreEqual(0xE001, result.Address);
        Assert.AreEqual(NMI_HANDLER, emulator.Cpu.PC.Value);
        Assert.IsTrue(emulator.Cpu.P.InterruptDisableFlag);
    }

    [TestMethod]
    public void Nmi_Pulse_IsLatched()
    {
        var emulator = Build(NOP.OP_CODE);

        emulator.Cpu.NonMaskableInterrupt.Strobe(true);
        emulator.Cpu.NonMaskableInterrupt.Strobe(false);
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InstructionResult.NMI_SEQUENCE, result.Instruction);
    }

    [TestMethod]
    public void Nmi_HeldHigh_IsTakenOnce()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Memory(NMI_HANDLER, RTI.OP_CODE);

        emulator.Cpu.NonMaskableInterrupt.Strobe(true);
        emulator.Cpu.Step();                                   // NMI sequence
        emulator.Cpu.Step();                                   // RTI
        emulator.Cpu.NonMaskableInterrupt.Strobe(true);        // still high: no edge
        var result = emulator.Cpu.Step();

        Assert.IsFalse(result.IsInterrupt);
        Assert.AreEqual(PROGRAM_START, result.Address);
    }

    [TestMethod]
    public void Nmi_NewEdge_IsTakenAgain()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Memory(NMI_HANDLER, RTI.OP_CODE);

        emulator.Cpu.NonMaskableInterrupt.Strobe(true);
        emulator.Cpu.Step();                                   // NMI sequence
        emulator.Cpu.Step();                                   // RTI
        emulator.Cpu.NonMaskableInterrupt.Strobe(false);
        emulator.Cpu.NonMaskableInterrupt.Strobe(true);
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InstructionResult.NMI_SEQUENCE, result.Instruction);
    }

    [TestMethod]
    public void Nmi_TakesPriorityOverIrq()
    {
        var emulator = Build(NOP.OP_CODE);

        emulator.Cpu.InterruptRequest.Strobe(true);
        emulator.Cpu.NonMaskableInterrupt.Strobe(true);
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InstructionResult.NMI_SEQUENCE, result.Instruction);
        Assert.AreEqual(NMI_HANDLER, emulator.Cpu.PC.Value);
    }

    // --- execution strategy ---

    [TestMethod]
    public void DebugStrategy_DoesNotStopAtEndAddress_OnInterruptSequence()
    {
        var irqRaised = false;
        var emulator = new EmulatorBuilder()
            .WithTestKernel([LDA_IMMEDIATE, 0x07, NOP.OP_CODE])
            .WithRam([(IRQ_HANDLER, RTI.OP_CODE)])
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                IsStopOnNopEnabled = false,
                EndAddress = PROGRAM_START,
                BreakpointCallbacks = new()
                {
                    // assert IRQ once, just before the first instruction; the handler releases it
                    [PROGRAM_START] = cpu =>
                    {
                        if(irqRaised)
                            return;

                        irqRaised = true;
                        cpu.InterruptRequest.Strobe(true);
                    },
                    [IRQ_HANDLER] = cpu => cpu.InterruptRequest.Strobe(false)
                }
            })
            .Build();

        emulator.Start();

        Assert.AreEqual(0x07, emulator.Cpu.A.Value, "stopped only after the instruction at EndAddress ran");
    }
}

internal static class InterruptTestExtensions
{
    /// <summary>Writes consecutive bytes to RAM starting at <paramref name="address"/>.</summary>
    public static void Memory(this Emulator emulator, int address, params int[] bytes)
    {
        for(int i = 0; i < bytes.Length; i++)
            emulator.MemoryMap.PokeRam(address + i, bytes[i]);
    }
}
