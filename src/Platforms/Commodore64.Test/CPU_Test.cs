namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class CPU_Test
{

    [TestMethod]
    public void Stack_Overflow()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([])
            .Build();

        // act 
        for(int i = 1; i < 256; i++)
        {
            emulator.Cpu.StackPush(i);
        }

        // assert
        Assert.AreEqual(0, emulator.Cpu.SP);
    }

    [TestMethod]
    public void Stack_Underflow()
    {
        // arrange
        var emulator = new EmulatorBuilder()
            .WithTestKernel([])
            .Build();

        // act 
        emulator.Cpu.StackPop();

        // assert
        Assert.AreEqual(0x00, emulator.Cpu.SP);
    }
}
