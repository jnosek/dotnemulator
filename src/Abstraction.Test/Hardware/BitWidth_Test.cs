using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Abstraction.Test.Hardware;

[TestClass]
public class BitWidth_Test
{
    [TestMethod]
    [DataRow(1, 0x1)]
    [DataRow(5, 0x1F)]
    [DataRow(8, 0xFF)]
    [DataRow(16, 0xFFFF)]
    [DataRow(31, 0x7FFF_FFFF)]
    [DataRow(32, -1)]
    public void GetMask(int size, int expected)
    {
        Assert.AreEqual(expected, BitWidth.GetMask(size));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(33)]
    public void GetMask_OutOfRange_Throws(int size)
    {
        var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => BitWidth.GetMask(size));
        Assert.AreEqual("size", ex.ParamName);
    }

    [TestMethod]
    public void Register32_KeepsAllBits()
    {
        var register = new Register(32);
        register.Value = unchecked((int)0xDEAD_BEEF);

        Assert.AreEqual(-1, register.Mask);
        Assert.AreEqual(unchecked((int)0xDEAD_BEEF), register.Value);
        Assert.AreEqual(0, new Register(32) { Value = -1 }.Increment());
    }

    [TestMethod]
    public void Bus32_KeepsAllBits()
    {
        var bus = new Bus(32);
        bus.Drive(unchecked((int)0x8000_0001));

        Assert.AreEqual(-1, bus.Mask);
        Assert.AreEqual(unchecked((int)0x8000_0001), bus.Read());
        Assert.IsTrue(bus.ReadBit(31));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(33)]
    public void Constructors_OutOfRange_Throw(int size)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Bus(size));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Register(size));
    }
}
