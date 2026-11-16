using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class ReadWriteWire_Test
{
    [TestMethod]
    public void DefaultsToRead()
    {
        var wire = new ReadWriteWire();

        Assert.AreEqual(ReadWriteMode.Read, wire.Mode);
        Assert.IsTrue(wire.IsRead);
        Assert.IsFalse(wire.IsWrite);
    }

    [TestMethod]
    public void Constructor_Write()
    {
        var wire = new ReadWriteWire(ReadWriteMode.Write);

        Assert.AreEqual(ReadWriteMode.Write, wire.Mode);
        Assert.IsFalse(wire.IsRead);
        Assert.IsTrue(wire.IsWrite);
    }

    [TestMethod]
    [DataRow(ReadWriteMode.Read)]
    [DataRow(ReadWriteMode.Write)]
    public void Drive_SetsMode_WithoutNotifying(ReadWriteMode mode)
    {
        var wire = new ReadWriteWire(mode == ReadWriteMode.Read ? ReadWriteMode.Write : ReadWriteMode.Read);
        var notifications = 0;
        wire.Subscribe(_ => notifications++);

        wire.Drive(mode);

        Assert.AreEqual(mode, wire.Mode);
        Assert.AreEqual(0, notifications);
    }

    [TestMethod]
    public void Strobe_SetsMode_ThenNotifiesWithMode()
    {
        var wire = new ReadWriteWire();
        var seen = new List<(ReadWriteMode Arg, ReadWriteMode Mode)>();
        wire.Subscribe(mode => seen.Add((mode, wire.Mode)));

        wire.Strobe(ReadWriteMode.Write);
        wire.Strobe(ReadWriteMode.Read);

        CollectionAssert.AreEqual(
            new[] { (ReadWriteMode.Write, ReadWriteMode.Write), (ReadWriteMode.Read, ReadWriteMode.Read) },
            seen);
    }

    [TestMethod]
    public void Strobe_SameMode_DoesNotNotify()
    {
        var wire = new ReadWriteWire();
        var notifications = 0;
        wire.Subscribe(_ => notifications++);

        wire.Strobe(ReadWriteMode.Read);

        Assert.AreEqual(0, notifications);
    }
}
