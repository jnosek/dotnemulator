using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;
using Dotnemulator.Platforms.Commodore64.Diagnostics;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class MemoryWatch_Test
{
    /// <summary>
    /// Reads $0300 (0x11), writes 0x42 to it, then stops on NOP.
    /// </summary>
    private static Emulator Build(IExecutionStrategy? strategy = null) =>
        new EmulatorBuilder()
            .WithTestKernel([
                LDA.BASE_OP_CODE | AccumulatorInstruction.Absolute, 0x00, 0x03,
                LDA.BASE_OP_CODE | AccumulatorInstruction.Immediate, 0x42,
                STA.BASE_OP_CODE | AccumulatorInstruction.Absolute, 0x00, 0x03,
                NOP.OP_CODE
            ])
            .WithRam([(0x0300, 0x11)])
            .WithExecutionStrategy(strategy ?? new DebugExecutionStrategy { IsStopOnNopEnabled = true })
            .Build();

    [TestMethod]
    public void OnAccessSeesReadsAndWrites()
    {
        var emulator = Build();
        var seen = new List<int>();
        using var watch = new MemoryWatch(emulator).OnAccess(0x0300, seen.Add);

        emulator.Start();

        CollectionAssert.AreEqual(new[] { 0x11, 0x42 }, seen);
    }

    [TestMethod]
    public void OnWriteIgnoresReads()
    {
        var emulator = Build();
        var writes = new List<int>();
        using var watch = new MemoryWatch(emulator).OnWrite(0x0300, writes.Add);

        emulator.Start();

        CollectionAssert.AreEqual(new[] { 0x42 }, writes);
    }

    [TestMethod]
    public void SeveralCallbacksOnOneAddress()
    {
        var emulator = Build();
        var first = new List<int>();
        var second = new List<int>();
        using var watch = new MemoryWatch(emulator).OnWrite(0x0300, first.Add).OnWrite(0x0300, second.Add);

        emulator.Start();

        CollectionAssert.AreEqual(new[] { 0x42 }, first);
        CollectionAssert.AreEqual(new[] { 0x42 }, second);
    }

    [TestMethod]
    public void NothingAfterDispose()
    {
        var emulator = Build();
        var seen = new List<int>();
        new MemoryWatch(emulator).OnAccess(0x0300, seen.Add).Dispose();

        emulator.Start();

        Assert.IsEmpty(seen);
    }

    [TestMethod]
    public void WorksWithRuntimeStrategy()
    {
        var strategy = new RuntimeExecutionStrategy();
        strategy.Observers.Add(new StopOnOpcodeObserver(NOP.OP_CODE));
        var emulator = Build(strategy);
        var writes = new List<int>();
        using var watch = new MemoryWatch(emulator).OnWrite(0x0300, writes.Add);

        emulator.Start();

        CollectionAssert.AreEqual(new[] { 0x42 }, writes);
    }
}
