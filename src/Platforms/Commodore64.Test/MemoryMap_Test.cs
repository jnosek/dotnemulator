using Dotnemulator.Platforms.Commodore64.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Commodore64.Test;

[TestClass]
public class MemoryMap_Test
{
    [TestMethod]
    public void WriteToRam()
    {
        // arrange
        var emulator = new TestEmulator([]);
        
        emulator.PortBus.Drive(0b0000_0111);
        emulator.PortBus.Trigger();
            
        emulator.AddressBus.Drive(0x03);
        emulator.AddressBus.Trigger();

        // act 
        emulator.DataBus.Drive(0x42);
        emulator.DataBus.Trigger();

        // assert
        Assert.AreEqual(0x42, emulator.PeekRam(0x03));
    }
}
