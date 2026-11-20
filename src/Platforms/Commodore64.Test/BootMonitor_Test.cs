using Dotnemulator.Platforms.Commodore64.Diagnostics;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class BootMonitor_Test
{
    private const long MAX_STEPS = 2_000_000;

    private readonly List<string> _log = new();
    private readonly List<BootResult> _finished = new();

    private (Emulator emulator, BootMonitor monitor) Build(
        Func<EmulatorBuilder, EmulatorBuilder> configure, int endAddress, long stepLimit = BootMonitor.DEFAULT_STEP_LIMIT)
    {
        BootMonitor? monitor = null;

        var builder = new EmulatorBuilder()
            .WithExecutionStrategy(new DebugExecutionStrategy { EndAddress = endAddress, MaxSteps = MAX_STEPS })
            .WithExecutionObserver(e => monitor = new BootMonitor(e.MemoryMap, _log.Add, stepLimit)
            {
                Finished = _finished.Add
            });

        var emulator = configure(builder).Build();

        return (emulator, monitor!);
    }

    [TestMethod]
    public void LogsEachStageOfANormalBoot()
    {
        var (emulator, monitor) = Build(b => b.WithRoms(), endAddress: BootMonitor.KEYBOARD_WAIT);

        emulator.Start();

        CollectionAssert.AreEqual(
            new[]
            {
                BootMonitor.RESET, BootMonitor.NO_CARTRIDGE, BootMonitor.IOINIT, BootMonitor.RAMTAS,
                BootMonitor.RESTOR, BootMonitor.CINT, BootMonitor.RASTER_WAIT_DONE, BootMonitor.VIDEO_STANDARD_SET,
                BootMonitor.BASIC_COLD_START, BootMonitor.PRINT_BANNER, BootMonitor.KEYBOARD_WAIT
            },
            monitor.StagesReached.ToArray(),
            string.Join(Environment.NewLine, _log));

        var log = string.Join(Environment.NewLine, _log);
        StringAssert.Contains(log, "No cartridge signature");
        StringAssert.Contains(log, "RAM ends at $A000");
        StringAssert.Contains(log, "Video standard: PAL");
        StringAssert.Contains(log, "Boot complete");
    }

    [TestMethod]
    public void FinishesOnceWithCompletedResult()
    {
        var (emulator, monitor) = Build(b => b.WithRoms(), endAddress: BootMonitor.KEYBOARD_WAIT);

        emulator.Start();

        Assert.HasCount(1, _finished);
        Assert.AreEqual(BootOutcome.Completed, monitor.Result!.Outcome);
        Assert.AreEqual(BootMonitor.KEYBOARD_WAIT, monitor.Result.Address);
        Assert.IsGreaterThan(0L, monitor.Result.Steps);
    }

    [TestMethod]
    public void DetachesWhenBootCompletes()
    {
        var (emulator, monitor) = Build(b => b.WithRoms(), endAddress: BootMonitor.KEYBOARD_WAIT);
        emulator.Start();
        var lines = _log.Count;

        Assert.AreEqual(ObserverAction.Detach, monitor.BeforeStep(emulator.Cpu));
        Assert.HasCount(lines, _log);
        Assert.HasCount(1, _finished);
    }

    [TestMethod]
    public void CartridgeAutostart()
    {
        // "CBM80" at $8004 and a cold start vector pointing at $8009 (in RAM, as no cartridge is modelled)
        const int CARTRIDGE_START = 0x8009;
        var (emulator, monitor) = Build(
            b => b.WithRoms().WithRam([
                (0x8000, CARTRIDGE_START & 0xFF), (0x8001, CARTRIDGE_START >> 8),
                (0x8004, 0xC3), (0x8005, 0xC2), (0x8006, 0xCD), (0x8007, 0x38), (0x8008, 0x30),
                (CARTRIDGE_START, 0xEA) // NOP
            ]),
            endAddress: CARTRIDGE_START);

        emulator.Start();

        Assert.AreEqual(BootOutcome.CartridgeAutostart, monitor.Result!.Outcome);
        CollectionAssert.AreEqual(new[] { BootMonitor.RESET, BootMonitor.CARTRIDGE_AUTOSTART }, monitor.StagesReached.ToArray());
        StringAssert.Contains(_log[^1], "autostart through ($8000) to $8009");
    }

    [TestMethod]
    public void ReportsStall()
    {
        // JMP $E000 forever: never reaches a stage
        var (emulator, monitor) = Build(
            b => b.WithTestKernel([0x4C, 0x00, 0xE0]),
            endAddress: -1,
            stepLimit: 100);

        Assert.ThrowsExactly<StepLimitException>(emulator.Start);

        Assert.AreEqual(BootOutcome.Stalled, monitor.Result!.Outcome);
        Assert.AreEqual(100L, monitor.Result.Steps);
        Assert.AreEqual(0xE000, monitor.Result.Address);
        StringAssert.Contains(_log[^1], "Boot stalled: 100 steps without completing, no stage reached");
        Assert.HasCount(1, _finished);
    }
}
