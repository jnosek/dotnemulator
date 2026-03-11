using Dotnemulator.Platforms.Commodore64;

namespace Commodore64.FunctionalTest;

[TestClass]
public sealed class OpCodeTest
{
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
            .Build();

        // act
        emulator.Cpu.Start(0x0400);

        // assert
    }
}
