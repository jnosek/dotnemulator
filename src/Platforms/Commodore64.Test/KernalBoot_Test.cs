using Dotnemulator.Platforms.Commodore64.Display;

namespace Dotnemulator.Platforms.Commodore64.Test;

/// <summary>
/// Boots the real BASIC and KERNAL ROMs and checks the result through screen RAM, without video output.
/// </summary>
[TestClass]
public class KernalBoot_Test
{
    /// <summary>
    /// KERNAL keyboard wait loop (<c>LDA $C6</c> ...), where the boot ends up once <c>READY.</c> is printed.
    /// </summary>
    private const int KEYBOARD_WAIT = 0xE5CD;

    /// <summary>
    /// About three times the ~600,000 steps a boot takes, so a hang fails quickly instead of blocking the test run.
    /// </summary>
    private const long MAX_STEPS = 2_000_000;

    private static (Emulator emulator, DebugExecutionStrategy strategy) Build()
    {
        var strategy = new DebugExecutionStrategy
        {
            EndAddress = KEYBOARD_WAIT,
            MaxSteps = MAX_STEPS
        };

        var emulator = new EmulatorBuilder()
            .WithRoms()
            .WithExecutionStrategy(strategy)
            .Build();

        return (emulator, strategy);
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
        var (emulator, strategy) = Build();

        var stages = new (int address, string name)[]
        {
            (0xFCE2, "RESET"),
            (0xFDA3, "IOINIT"),
            (0xFD50, "RAMTAS"),
            (0xFD15, "RESTOR"),
            (0xFF5B, "CINT"),
            (0xE394, "BASIC cold start"),
        };

        var reached = new List<string>();
        foreach(var (address, name) in stages)
        {
            strategy.BreakpointCallbacks[address] = _ =>
            {
                if(!reached.Contains(name))
                    reached.Add(name);
            };
        }

        try
        {
            emulator.Start();
        }
        catch(StepLimitException ex)
        {
            Assert.Fail($"{ex.Message}; stages reached: {string.Join(" -> ", reached)}");
        }

        CollectionAssert.AreEqual(stages.Select(stage => stage.name).ToList(), reached);
    }
}
