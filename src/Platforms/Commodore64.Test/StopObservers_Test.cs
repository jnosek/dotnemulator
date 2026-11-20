using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Diagnostics;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class StopObservers_Test
{
    private static readonly MOS6510Cpu Cpu = new EmulatorBuilder().WithTestKernel([]).Build().Cpu;

    [TestMethod]
    public void StopAtAddress_StopsAfterInstructionAtAddress()
    {
        var observer = new StopAtAddressObserver(0xE002);

        Assert.AreEqual(ObserverAction.Continue, observer.AfterStep(Cpu, new(0xE000, 0xA9, 0)));
        Assert.AreEqual(ObserverAction.Stop, observer.AfterStep(Cpu, new(0xE002, 0xA9, 0)));
    }

    [TestMethod]
    public void StopAtAddress_IgnoresInterruptSequence()
    {
        var observer = new StopAtAddressObserver(0xE002);

        Assert.AreEqual(ObserverAction.Continue, observer.AfterStep(Cpu, new(0xE002, 0x00, 0, InterruptKind.Irq)));
    }

    [TestMethod]
    public void StopOnOpcode_StopsOnMatchingOpcode()
    {
        var observer = new StopOnOpcodeObserver(0xEA);

        Assert.AreEqual(ObserverAction.Continue, observer.AfterStep(Cpu, new(0xE000, 0xA9, 0)));
        Assert.AreEqual(ObserverAction.Stop, observer.AfterStep(Cpu, new(0xE002, 0xEA, 0)));
    }

    [TestMethod]
    public void StopOnOpcode_IgnoresInterruptSequence()
    {
        // an interrupt sequence reports BRK ($00) as its instruction
        var observer = new StopOnOpcodeObserver(0x00);

        Assert.AreEqual(ObserverAction.Continue, observer.AfterStep(Cpu, new(0xE000, 0x00, 0, InterruptKind.Nmi)));
        Assert.AreEqual(ObserverAction.Stop, observer.AfterStep(Cpu, new(0xE000, 0x00, 0)));
    }

    [TestMethod]
    public void StepLimit_ThrowsAfterLimit()
    {
        var trace = new InstructionTraceObserver();
        trace.AfterStep(Cpu, new(0xE000, 0xEA, 0));
        var observer = new StepLimitObserver(3, trace);

        for(int i = 0; i < 3; i++)
            Assert.AreEqual(ObserverAction.Continue, observer.BeforeStep(Cpu));

        var ex = Assert.ThrowsExactly<StepLimitException>(() => observer.BeforeStep(Cpu));
        Assert.HasCount(1, ex.InstructionLog);
    }

    [TestMethod]
    public void StepLimit_WithoutTraceHasEmptyLog()
    {
        var observer = new StepLimitObserver(0);

        var ex = Assert.ThrowsExactly<StepLimitException>(() => observer.BeforeStep(Cpu));
        Assert.IsEmpty(ex.InstructionLog);
    }
}
