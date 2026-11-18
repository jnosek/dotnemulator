using System.Diagnostics;
using Dotnemulator.Platforms.Commodore64;

namespace Commodore64.FunctionalTest;

/// <summary>
/// Runs Klaus Dormann's 6502 functional test. The ROMs are built by <c>tools/functional-test-roms/build.sh</c>.
/// </summary>
[TestClass]
public sealed class OpCodeTest
{
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// The upstream default build (<c>report = 0</c>): a failure ends in a trap loop, caught by jump-loop detection.
    /// </summary>
    [TestMethod]
    public void Full()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithImage(LoadRom("6502_functional_test.bin"))
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                StartAddress = FunctionalTestSymbols.Trap.Start,
                EndAddress = FunctionalTestSymbols.Trap.Success,
                IsStopOnNopEnabled = false,
                IsJumpLoopDetectionEnabled = true,
                MemoryWatchCallbacks = new()
                {
                    [FunctionalTestSymbols.Trap.TestCase] = (value) => Debug.WriteLine($"[0x0200] Test # {value:X2}")
                }
            })
            .Build();

        // act
        try
        {
            emulator.Start();
        }
        catch(JumpLoopException ex)
        {
            Assert.Fail($"Emulator entered a jump loop. Instruction log:\n{string.Join("\n", ex.InstructionLog.Select(r => $"Address: {r.Address:X4}, Instruction: {r.Instruction:X2}"))}");
        }
    }

    /// <summary>
    /// The I/O build (<c>report = 1</c>): the ROM prints its progress, and on a failure a dump of the registers,
    /// stack and workspace, through <c>report.i65</c>. That output is written to the console, along with a line for
    /// each test case that passes.
    /// </summary>
    /// <remarks>
    /// The ROM writes each character to <see cref="FunctionalTestSymbols.Io.OutputPort"/> (<c>$F001</c>, in the
    /// KERNAL ROM area, so the write lands in the RAM underneath) and would then wait for a key on
    /// <see cref="FunctionalTestSymbols.Io.InputPort"/>. The test stops when it reaches either wait loop instead.
    /// </remarks>
    [TestMethod]
    public void FullWithReport()
    {
        // arrange
        var console = new FunctionalTestConsole();
        var progress = new TestCaseProgress(FunctionalTestSymbols.Io.AllTestsComplete);
        Emulator? emulator = null;
        emulator = new EmulatorBuilder()
            .WithImage(LoadRom("6502_functional_test_io.bin"))
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                StartAddress = FunctionalTestSymbols.Io.Start,
                // "All tests completed, press R to repeat" has been printed; rsuc1 would wait for the key
                EndAddress = FunctionalTestSymbols.Io.SuccessWait,
                IsStopOnNopEnabled = false,
                IsJumpLoopDetectionEnabled = true,
                MemoryWriteWatchCallbacks = new()
                {
                    [FunctionalTestSymbols.Io.OutputPort] = console.Write,
                    [FunctionalTestSymbols.Io.TestCase] = progress.Write
                },
                BreakpointCallbacks = new()
                {
                    // the error report has been printed; rerr1 would wait for C (continue) or S (skip)
                    [FunctionalTestSymbols.Io.ErrorWait] = _ =>
                    {
                        console.Flush();
                        var testCase = emulator!.MemoryMap.PeekRam(FunctionalTestSymbols.Io.TestCase);
                        Assert.Fail($"Functional test reported an error in test case {testCase:X2}:\n{console.Text}");
                    }
                }
            })
            .Build();

        // act
        try
        {
            emulator.Start();
        }
        catch(JumpLoopException ex)
        {
            console.Flush();
            Assert.Fail($"Emulator entered a jump loop. Output so far:\n{console.Text}\nInstruction log:\n{string.Join("\n", ex.InstructionLog.Select(r => $"Address: {r.Address:X4}, Instruction: {r.Instruction:X2}"))}");
        }

        console.Flush();

        // assert
        StringAssert.Contains(console.Text, "Started testing");
        StringAssert.Contains(console.Text, "All tests completed");
        Assert.IsTrue(progress.IsComplete, "The ROM did not mark the tests as complete.");
        Assert.AreEqual(FunctionalTestSymbols.Io.LastTestCase + 1, progress.PassedCount, "Number of test cases passed.");
    }

    private byte[] LoadRom(string name)
    {
        using var romStream = GetType().Assembly.GetManifestResourceStream($"Dotnemulator.Platforms.Commodore64.FunctionalTest.Roms.{name}") ??
            throw new InvalidOperationException($"Failed to load test ROM {name}");

        var romBytes = new byte[romStream.Length];
        romStream.ReadExactly(romBytes);
        return romBytes;
    }
}
