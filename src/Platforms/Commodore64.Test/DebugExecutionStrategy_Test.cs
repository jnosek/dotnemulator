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
}
