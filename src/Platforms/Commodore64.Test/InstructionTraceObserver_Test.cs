using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Diagnostics;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class InstructionTraceObserver_Test
{
    private static readonly MOS6510Cpu Cpu = new EmulatorBuilder().WithTestKernel([]).Build().Cpu;

    [TestMethod]
    public void LogIsNewestFirst()
    {
        var trace = new InstructionTraceObserver();

        trace.AfterStep(Cpu, new(0xE000, 0xA9, 0));
        trace.AfterStep(Cpu, new(0xE002, 0xEA, 0));

        Assert.AreEqual(2, trace.Count);
        Assert.AreEqual(0xE002, trace.Latest.Address);
        CollectionAssert.AreEqual(new[] { 0xE002, 0xE000 }, trace.GetLog().Select(r => r.Address).ToArray());
    }

    [TestMethod]
    public void KeepsOnlyTheLastSteps()
    {
        var trace = new InstructionTraceObserver(capacity: 3);

        for(int address = 0; address < 5; address++)
            trace.AfterStep(Cpu, new(address, 0xEA, 0));

        Assert.AreEqual(3, trace.Count);
        CollectionAssert.AreEqual(new[] { 4, 3, 2 }, trace.GetLog().Select(r => r.Address).ToArray());
    }

    [TestMethod]
    public void JumpLoop_ThrowsWhenSameInstructionRepeats()
    {
        var trace = new InstructionTraceObserver();
        var jumpLoop = new JumpLoopObserver(trace);

        foreach(var result in new InstructionResult[] { new(0xE000, 0xA9, 0), new(0xE002, 0x4C, 0) })
        {
            Assert.AreEqual(ObserverAction.Continue, jumpLoop.AfterStep(Cpu, result));
            trace.AfterStep(Cpu, result);
        }

        var ex = Assert.ThrowsExactly<JumpLoopException>(() => jumpLoop.AfterStep(Cpu, new(0xE002, 0x4C, 0)));
        CollectionAssert.AreEqual(new[] { 0xE002, 0xE000 }, ex.InstructionLog.Select(r => r.Address).ToArray());
    }

    [TestMethod]
    public void JumpLoop_EndToEnd()
    {
        // JMP $E000 at $E000
        var emulator = new EmulatorBuilder()
            .WithTestKernel([0x4C, 0x00, 0xE0])
            .WithExecutionStrategy(new DebugExecutionStrategy { IsJumpLoopDetectionEnabled = true })
            .Build();

        var ex = Assert.ThrowsExactly<JumpLoopException>(emulator.Start);
        Assert.AreEqual(0xE000, ex.InstructionLog[0].Address);
    }
}
