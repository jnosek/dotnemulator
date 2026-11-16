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
        Assert.IsTrue(wire.Value);
    }

    [TestMethod]
    public void Constructor_Write()
    {
        var wire = new ReadWriteWire(ReadWriteMode.Write);

        Assert.AreEqual(ReadWriteMode.Write, wire.Mode);
        Assert.IsFalse(wire.Value);
    }

    [TestMethod]
    [DataRow(ReadWriteMode.Read, true)]
    [DataRow(ReadWriteMode.Write, false)]
    public void Drive_SetsMode_WithoutNotifying(ReadWriteMode mode, bool level)
    {
        var wire = new ReadWriteWire(mode == ReadWriteMode.Read ? ReadWriteMode.Write : ReadWriteMode.Read);
        var notifications = 0;
        wire.Subscribe(_ => notifications++);

        wire.Drive(mode);

        Assert.AreEqual(mode, wire.Mode);
        Assert.AreEqual(level, wire.Value);
        Assert.AreEqual(0, notifications);
    }

    [TestMethod]
    public void Strobe_SetsMode_ThenNotifiesWithLevel()
    {
        var wire = new ReadWriteWire();
        var seen = new List<(bool Level, ReadWriteMode Mode)>();
        wire.Subscribe(level => seen.Add((level, wire.Mode)));

        wire.Strobe(ReadWriteMode.Write);
        wire.Strobe(ReadWriteMode.Read);

        CollectionAssert.AreEqual(
            new[] { (false, ReadWriteMode.Write), (true, ReadWriteMode.Read) },
            seen);
    }
}
