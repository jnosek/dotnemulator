using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class Bus_Test
{
    [TestMethod]
    public void Drive_StoresMaskedValue_WithoutNotifying()
    {
        var bus = new Bus(8);
        var notifications = 0;
        bus.Subscribe(() => notifications++);

        bus.Drive(0x1FF);

        Assert.AreEqual(0xFF, bus.Read());
        Assert.AreEqual(0, notifications);
    }

    [TestMethod]
    public void Strobe_StoresMaskedValue_ThenNotifies()
    {
        var bus = new Bus(8);
        var seen = new List<int>();
        bus.Subscribe(() => seen.Add(bus.Read()));

        bus.Strobe(0x1FF);

        CollectionAssert.AreEqual(new[] { 0xFF }, seen);
    }

    [TestMethod]
    public void Strobe_NotifiesAllSubscribers_InOrder()
    {
        var bus = new Bus(8);
        var order = new List<int>();
        bus.Subscribe(() => order.Add(1));
        bus.Subscribe(() => order.Add(2));

        bus.Strobe(0x01);

        CollectionAssert.AreEqual(new[] { 1, 2 }, order);
    }

    [TestMethod]
    public void Strobe_ResponderDrive_IsVisibleAfterReturn()
    {
        var address = new Bus(16);
        var data = new Bus(8);
        address.Subscribe(() => data.Drive(address.Read() & 0xFF));

        address.Strobe(0x1234);

        Assert.AreEqual(0x34, data.Read());
    }
}
