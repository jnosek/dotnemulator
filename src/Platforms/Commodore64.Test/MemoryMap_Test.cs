namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class MemoryMap_Test
{
    [TestMethod]
    public void WriteToRam()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([])
            .Build();

        emulator.PortBus.Drive(0b0000_0111);
        emulator.PortBus.Trigger();

        emulator.AddressBus.Drive(0x03);
        emulator.AddressBus.Trigger();

        // act 
        emulator.DataBus.Drive(0x42);
        emulator.DataBus.Trigger();

        // assert
        Assert.AreEqual(0x42, emulator.MemoryMap.PeekRam(0x03));
    }
}
