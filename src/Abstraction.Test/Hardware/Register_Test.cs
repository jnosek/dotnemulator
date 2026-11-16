using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Abstraction.Test.Hardware;

[TestClass]
public class Register_Test
{
    [TestMethod]
    public void Value_IsMaskedToSize()
    {
        var register = new Register(8) { Value = 0x1FF };

        Assert.AreEqual(0xFF, register.Value);
    }

    [TestMethod]
    public void Increment_WrapsAndReturnsNewValue()
    {
        var register = new Register(8) { Value = 0xFF };

        Assert.AreEqual(0x00, register.Increment());
        Assert.AreEqual(0x00, register.Value);
    }

    [TestMethod]
    public void Decrement_WrapsAndReturnsNewValue()
    {
        var register = new Register(8) { Value = 0x00 };

        Assert.AreEqual(0xFF, register.Decrement());
        Assert.AreEqual(0xFF, register.Value);
    }

    [TestMethod]
    public void Advance_ReturnsOldValue_ThenIncrements()
    {
        var register = new Register(16) { Value = 0xFFFF };

        Assert.AreEqual(0xFFFF, register.Advance());
        Assert.AreEqual(0x0000, register.Value);
    }

    [TestMethod]
    public void SetFlag_SetsAndClears_GetFlag_Reads()
    {
        var register = new Register(8);

        register.SetFlag(0b0000_0100, true);
        Assert.IsTrue(register.GetFlag(0b0000_0100));
        Assert.AreEqual(0b0000_0100, register.Value);

        register.SetFlag(0b0000_0100, false);
        Assert.IsFalse(register.GetFlag(0b0000_0100));
        Assert.AreEqual(0, register.Value);
    }
}
