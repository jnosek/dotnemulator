using Dotnemulator.Abstraction.Hardware;
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

    // MSTest creates a new instance per test, so each test gets its own sources
    private OpenCollectorWire.Source _irq = null!;
    private OpenCollectorWire.Source _nmi = null!;

    /// <summary>
    /// Builds a machine with <paramref name="program"/> at $E000, the IRQ vector at $0100 and the NMI vector at
    /// $0200 (both handlers start with a NOP), registers one IRQ and one NMI source for the test, and resets the
    /// CPU without running anything.
    /// </summary>
    private Emulator Build(params byte[] program)
    {
        var kernel = new byte[0x2000];
        program.CopyTo(kernel, 0);

        kernel[MOS6510Cpu.NMI_VECTOR - PROGRAM_START] = NMI_HANDLER & 0xFF;
        kernel[MOS6510Cpu.NMI_VECTOR - PROGRAM_START + 1] = NMI_HANDLER >> 8;

        var emulator = new EmulatorBuilder()
            .WithTestKernel(kernel)
            .WithRam([(IRQ_HANDLER, NOP.OP_CODE), (NMI_HANDLER, NOP.OP_CODE)])
            .Build();

        _irq = emulator.InterruptRequest.RegisterSource();
        _nmi = emulator.NonMaskableInterrupt.RegisterSource();

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

        _irq.Assert();

        Assert.AreEqual(PROGRAM_START, emulator.Cpu.PC.Value);
        Assert.AreEqual(sp, emulator.Cpu.SP);
    }

    [TestMethod]
    public void Irq_IsTakenAtNextInstructionBoundary()
    {
        var emulator = Build(LDA_IMMEDIATE, 0x01, LDA_IMMEDIATE, 0x02, NOP.OP_CODE);
        emulator.Cpu.Step();                                   // LDA #$01

        _irq.Assert();
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.Irq, result.Interrupt);
        Assert.AreEqual(BRK.OP_CODE, result.Instruction, "interrupt sequence reports a forced BRK");
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
        _irq.Assert();
        emulator.Cpu.Step();                                   // interrupt sequence

        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.None, result.Interrupt);
        Assert.AreEqual(IRQ_HANDLER, result.Address);
        Assert.AreEqual(NOP.OP_CODE, result.Instruction);
    }

    [TestMethod]
    public void Irq_SetsInterruptDisableFlag_AndPushesItClear()
    {
        var emulator = Build(NOP.OP_CODE);
        Assert.IsFalse(emulator.Cpu.P.InterruptDisableFlag);

        _irq.Assert();
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
        _irq.Assert();

        emulator.Cpu.Step();                                   // interrupt sequence
        var sp = emulator.Cpu.SP;
        var first = emulator.Cpu.Step();
        var second = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.None, first.Interrupt);
        Assert.AreEqual(InterruptKind.None, second.Interrupt);
        Assert.AreEqual(sp, emulator.Cpu.SP);
        Assert.AreEqual(0x02, emulator.Cpu.A.Value);
    }

    [TestMethod]
    public void Irq_IsIgnoredWhileInterruptDisableFlagSet()
    {
        var emulator = Build(SEI.OP_CODE, LDA_IMMEDIATE, 0x01, NOP.OP_CODE);
        emulator.Cpu.Step();                                   // SEI

        _irq.Assert();
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.None, result.Interrupt);
        Assert.AreEqual(0x01, emulator.Cpu.A.Value);
    }

    [TestMethod]
    public void Irq_ReleasedBeforeBoundary_IsNotTaken()
    {
        var emulator = Build(LDA_IMMEDIATE, 0x01, NOP.OP_CODE);

        _irq.Assert();
        _irq.Release();
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.None, result.Interrupt);
    }

    [TestMethod]
    public void Irq_SharedLine_StaysAssertedUntilEverySourceReleases()
    {
        var emulator = Build(SEI.OP_CODE, NOP.OP_CODE);
        var otherDevice = emulator.InterruptRequest.RegisterSource();
        emulator.Memory(IRQ_HANDLER, RTI.OP_CODE);

        _irq.Assert();
        otherDevice.Assert();
        _irq.Release();                                        // other device still holds the line
        emulator.Cpu.Step();                                   // SEI: nothing taken yet
        emulator.Cpu.P.InterruptDisableFlag = false;
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.Irq, result.Interrupt);

        otherDevice.Release();
        emulator.Cpu.Step();                                   // RTI
        result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.None, result.Interrupt, "line released by every source");
    }

    [TestMethod]
    public void Irq_StillHeldAfterRti_IsTakenAgain()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Memory(IRQ_HANDLER, RTI.OP_CODE);
        _irq.Assert();

        emulator.Cpu.Step();                                   // interrupt sequence
        emulator.Cpu.Step();                                   // RTI restores I = 0
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.Irq, result.Interrupt);
    }

    // --- NMI ---

    [TestMethod]
    public void Nmi_IsTakenAtNextBoundary_EvenWithInterruptsDisabled()
    {
        var emulator = Build(SEI.OP_CODE, NOP.OP_CODE);
        emulator.Cpu.Step();                                   // SEI

        _nmi.Assert();
        Assert.AreEqual(0xE001, emulator.Cpu.PC.Value, "not taken inside the Assert call");

        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.Nmi, result.Interrupt);
        Assert.AreEqual(0xE001, result.Address);
        Assert.AreEqual(NMI_HANDLER, emulator.Cpu.PC.Value);
        Assert.IsTrue(emulator.Cpu.P.InterruptDisableFlag);
    }

    [TestMethod]
    public void Nmi_Pulse_IsLatched()
    {
        var emulator = Build(NOP.OP_CODE);

        _nmi.Assert();
        _nmi.Release();
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.Nmi, result.Interrupt);
    }

    [TestMethod]
    public void Nmi_HeldHigh_IsTakenOnce()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Memory(NMI_HANDLER, RTI.OP_CODE);

        _nmi.Assert();
        emulator.Cpu.Step();                                   // NMI sequence
        emulator.Cpu.Step();                                   // RTI
        _nmi.Assert();        // already asserting: no edge
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.None, result.Interrupt);
        Assert.AreEqual(PROGRAM_START, result.Address);
    }

    [TestMethod]
    public void Nmi_NewEdge_IsTakenAgain()
    {
        var emulator = Build(NOP.OP_CODE);
        emulator.Memory(NMI_HANDLER, RTI.OP_CODE);

        _nmi.Assert();
        emulator.Cpu.Step();                                   // NMI sequence
        emulator.Cpu.Step();                                   // RTI
        _nmi.Release();
        _nmi.Assert();
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.Nmi, result.Interrupt);
    }

    [TestMethod]
    public void Nmi_SecondSourceWhileLineHeld_GivesNoNewEdge()
    {
        var emulator = Build(NOP.OP_CODE);
        var restoreKey = emulator.NonMaskableInterrupt.RegisterSource();
        emulator.Memory(NMI_HANDLER, RTI.OP_CODE);

        _nmi.Assert();                                         // e.g. CIA 2 holding NMI
        emulator.Cpu.Step();                                   // NMI sequence
        emulator.Cpu.Step();                                   // RTI

        restoreKey.Assert();                                   // line already asserted: no edge
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.None, result.Interrupt);
    }

    [TestMethod]
    public void Nmi_TakesPriorityOverIrq()
    {
        var emulator = Build(NOP.OP_CODE);

        _irq.Assert();
        _nmi.Assert();
        var result = emulator.Cpu.Step();

        Assert.AreEqual(InterruptKind.Nmi, result.Interrupt);
        Assert.AreEqual(NMI_HANDLER, emulator.Cpu.PC.Value);
    }

    // --- execution strategy ---

    [TestMethod]
    public void DebugStrategy_DoesNotStopAtEndAddress_OnInterruptSequence()
    {
        var irqRaised = false;
        OpenCollectorWire.Source? irq = null;
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
                        irq!.Assert();
                    },
                    [IRQ_HANDLER] = cpu => irq!.Release()
                }
            })
            .Build();

        irq = emulator.InterruptRequest.RegisterSource();
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
