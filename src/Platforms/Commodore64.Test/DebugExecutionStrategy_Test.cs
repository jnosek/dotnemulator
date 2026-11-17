using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class DebugExecutionStrategy_Test
{
    [TestMethod]
    public void DebugStrategy_RunTwice_MemoryWatchIsNotDuplicated()
    {
        var seen = new List<int>();
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDA.BASE_OP_CODE | AccumulatorInstruction.Immediate, 0x42,
                STA.BASE_OP_CODE | AccumulatorInstruction.Absolute, 0x00, 0x03,
                NOP.OP_CODE
            ])
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                IsStopOnNopEnabled = true,
                MemoryWatchCallbacks = new() { [0x0300] = seen.Add }
            })
            .Build();

        emulator.Start();
        emulator.Start();

        // one write per run; a leftover handler from the first run would make it three
        CollectionAssert.AreEqual(new[] { 0x42, 0x42 }, seen);
    }

    [TestMethod]
    public void DebugStrategy_MemoryWriteWatch_IgnoresReads()
    {
        var allCycles = new List<int>();
        var writes = new List<int>();
        var emulator = new EmulatorBuilder()
            .WithTestKernel([
                LDA.BASE_OP_CODE | AccumulatorInstruction.Absolute, 0x00, 0x03,   // read $0300 (0x11)
                LDA.BASE_OP_CODE | AccumulatorInstruction.Immediate, 0x42,
                STA.BASE_OP_CODE | AccumulatorInstruction.Absolute, 0x00, 0x03,   // write $0300 (0x42)
                NOP.OP_CODE
            ])
            .WithRam([(0x0300, 0x11)])
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                IsStopOnNopEnabled = true,
                MemoryWatchCallbacks = new() { [0x0300] = allCycles.Add },
                MemoryWriteWatchCallbacks = new() { [0x0300] = writes.Add }
            })
            .Build();

        emulator.Start();

        CollectionAssert.AreEqual(new[] { 0x11, 0x42 }, allCycles);
        CollectionAssert.AreEqual(new[] { 0x42 }, writes);
    }
}
