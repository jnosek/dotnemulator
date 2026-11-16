using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class Wire_Test
{
    [TestMethod]
    public void Drive_SetsValue_WithoutNotifying()
    {
        var wire = new Wire();
        var notifications = 0;
        wire.Subscribe(_ => notifications++);

        wire.Drive(true);

        Assert.IsTrue(wire.Value);
        Assert.AreEqual(0, notifications);
    }

    [TestMethod]
    public void Strobe_SetsValue_ThenNotifiesWithNewLevel()
    {
        var wire = new Wire();
        var seen = new List<(bool Arg, bool Value)>();
        wire.Subscribe(level => seen.Add((level, wire.Value)));

        wire.Strobe(true);
        wire.Strobe(true);
        wire.Strobe(false);

        CollectionAssert.AreEqual(new[] { (true, true), (true, true), (false, false) }, seen);
    }
}
