using Dotnemulator.Platforms.Commodore64.Diagnostics;
using Dotnemulator.Platforms.Commodore64.Display;

namespace Dotnemulator.Platforms.Commodore64.Test;

/// <summary>
/// Boots the real BASIC and KERNAL ROMs and checks the result through screen RAM, without video output.
/// </summary>
[TestClass]
public class KernalBoot_Test
{
    /// <summary>
    /// About three times the ~600,000 steps a boot takes, so a hang fails quickly instead of blocking the test run.
    /// </summary>
    private const long MAX_STEPS = 2_000_000;

    private static (Emulator emulator, BootMonitor monitor) Build()
    {
        BootMonitor? monitor = null;

        var emulator = new EmulatorBuilder()
            .WithRoms()
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                EndAddress = BootMonitor.KEYBOARD_WAIT,
                MaxSteps = MAX_STEPS
            })
            .WithExecutionObserver(e => monitor = new BootMonitor(e.MemoryMap, _ => { }))
            .Build();

        return (emulator, monitor!);
    }

    [TestMethod]
    public void BootsToReady()
    {
        var (emulator, _) = Build();

        emulator.Start();

        var rows = new TextScreen(emulator.MemoryMap).GetRows();
        var screen = string.Join(Environment.NewLine, rows);

        StringAssert.Contains(rows[1], "**** COMMODORE 64 BASIC V2 ****", screen);
        StringAssert.Contains(rows[3], "64K RAM SYSTEM  38911 BASIC BYTES FREE", screen);
        Assert.AreEqual("READY.", rows[5].TrimEnd(), screen);
    }

    [TestMethod]
    public void BootReachesEachStageInOrder()
    {
        var (emulator, monitor) = Build();

        try
        {
            emulator.Start();
        }
        catch(StepLimitException ex)
        {
            Assert.Fail($"{ex.Message}; stages reached: {string.Join(" -> ", monitor.StagesReached.Select(a => $"${a:X4}"))}");
        }

        Assert.AreEqual(BootOutcome.Completed, monitor.Result?.Outcome);
        CollectionAssert.AreEqual(
            new[]
            {
                BootMonitor.RESET, BootMonitor.NO_CARTRIDGE, BootMonitor.IOINIT, BootMonitor.RAMTAS,
                BootMonitor.RESTOR, BootMonitor.CINT, BootMonitor.RASTER_WAIT_DONE, BootMonitor.VIDEO_STANDARD_SET,
                BootMonitor.BASIC_COLD_START, BootMonitor.PRINT_BANNER, BootMonitor.KEYBOARD_WAIT
            },
            monitor.StagesReached.ToArray());
    }
}
