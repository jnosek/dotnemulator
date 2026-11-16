using System.Diagnostics;
using Dotnemulator.Platforms.Commodore64;

namespace Commodore64.FunctionalTest;

[TestClass]
public sealed class OpCodeTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Full()
    {
        // arrange
        using var romStream = GetType().Assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.FunctionalTest.Roms.6502_functional_test.bin") ?? 
            throw new InvalidOperationException("Failed to load TEST ROM");

        var romBytes = new byte[romStream.Length];
        romStream.ReadExactly(romBytes);

        var emulator = new EmulatorBuilder()
            .WithImage(romBytes)
            // skip test 0 and 1
            .WithRam([ (0x0200, 0x002A) ])
            .WithExecutionStrategy(new DebugExecutionStrategy
            {
                StartAddress = 0x0400,
                EndAddress = 0x3469,
                IsStopOnNopEnabled = false,
                IsJumpLoopDetectionEnabled = true,
                MemoryWatchCallbacks = new()
                {
                    [0x0200] = (value) => Debug.WriteLine($"[0x0200] Test # {value:X2}")
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

        // assert
    }
}
