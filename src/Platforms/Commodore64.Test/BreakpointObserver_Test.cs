using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Diagnostics;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class BreakpointObserver_Test
{
    private const int LDA_IMMEDIATE = LDA.BASE_OP_CODE | AccumulatorInstruction.Immediate;

    /// <summary>
    /// LDA #$01 at $E000, LDA #$02 at $E002, then JMP $E000.
    /// </summary>
    private static Emulator Build(BreakpointObserver breakpoints, int stopAfterSteps)
    {
        var strategy = new DebugExecutionStrategy { MaxSteps = stopAfterSteps };
        strategy.Observers.Add(breakpoints);

        return new EmulatorBuilder()
            .WithTestKernel([LDA_IMMEDIATE, 0x01, LDA_IMMEDIATE, 0x02, 0x4C, 0x00, 0xE0])
            .WithExecutionStrategy(strategy)
            .Build();
    }

    [TestMethod]
    public void CalledBeforeTheInstructionRuns()
    {
        var seenA = new List<int>();
        var emulator = Build(new BreakpointObserver().Add(0xE002, cpu => seenA.Add(cpu.A.Value)), stopAfterSteps: 2);

        Assert.ThrowsExactly<StepLimitException>(emulator.Start);

        // LDA #$01 has run, LDA #$02 has not
        CollectionAssert.AreEqual(new[] { 0x01 }, seenA);
    }

    [TestMethod]
    public void PermanentBreakpointHitsEveryTime()
    {
        var hits = 0;
        var emulator = Build(new BreakpointObserver().Add(0xE000, _ => hits++), stopAfterSteps: 9);

        Assert.ThrowsExactly<StepLimitException>(emulator.Start);

        Assert.AreEqual(3, hits);
    }

    [TestMethod]
    public void OneShotBreakpointHitsOnce()
    {
        var hits = 0;
        var breakpoints = new BreakpointObserver().Add(0xE000, _ => hits++, oneShot: true);
        var emulator = Build(breakpoints, stopAfterSteps: 9);

        Assert.ThrowsExactly<StepLimitException>(emulator.Start);

        Assert.AreEqual(1, hits);
        Assert.AreEqual(0, breakpoints.Count);
    }

    [TestMethod]
    public void DetachesWhenNoBreakpointsLeft()
    {
        var cpu = new EmulatorBuilder().WithTestKernel([]).Build().Cpu;
        cpu.PC.Value = 0xE000;
        var breakpoints = new BreakpointObserver().Add(0xE000, _ => { }, oneShot: true);

        Assert.AreEqual(ObserverAction.Detach, breakpoints.BeforeStep(cpu));
        Assert.AreEqual(ObserverAction.Detach, new BreakpointObserver().BeforeStep(cpu));
    }
}
