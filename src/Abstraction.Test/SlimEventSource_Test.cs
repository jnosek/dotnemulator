using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Abstraction.Test;

/// <summary>
/// SlimEventSource behaviour, exercised through Bus (no value) and Wire (with a value),
/// since Notify is protected.
/// </summary>
[TestClass]
public class SlimEventSource_Test
{
    [TestMethod]
    public void NoSubscribers_StrobeDoesNothing()
    {
        new Bus(8).Strobe(0x42);
        new Wire().Strobe(true);
    }

    [TestMethod]
    public void Subscribers_AreCalledInOrder()
    {
        var bus = new Bus(8);
        var order = new List<int>();
        bus.Subscribe(() => order.Add(1));
        bus.Subscribe(() => order.Add(2));
        bus.Subscribe(() => order.Add(3));

        bus.Strobe(0);

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, order);
    }

    [TestMethod]
    public void Dispose_RemovesOnlyThatSubscriber()
    {
        var bus = new Bus(8);
        var order = new List<int>();
        bus.Subscribe(() => order.Add(1));
        var second = bus.Subscribe(() => order.Add(2));
        bus.Subscribe(() => order.Add(3));

        second.Dispose();
        bus.Strobe(0);

        CollectionAssert.AreEqual(new[] { 1, 3 }, order);
    }

    [TestMethod]
    public void Dispose_LastSubscriber_LeavesNoSubscribers()
    {
        var bus = new Bus(8);
        var calls = 0;
        var subscription = bus.Subscribe(() => calls++);

        subscription.Dispose();
        bus.Strobe(0);

        Assert.AreEqual(0, calls);
    }

    [TestMethod]
    public void Dispose_Twice_DoesNothingMore()
    {
        var bus = new Bus(8);
        var calls = 0;
        var subscription = bus.Subscribe(() => calls++);
        bus.Subscribe(() => calls += 10);

        subscription.Dispose();
        subscription.Dispose();
        bus.Strobe(0);

        Assert.AreEqual(10, calls);
    }

    [TestMethod]
    public void SameHandlerTwice_IsCalledTwice_DisposingOneLeavesTheOther()
    {
        var bus = new Bus(8);
        var calls = 0;
        Action handler = () => calls++;
        var first = bus.Subscribe(handler);
        bus.Subscribe(handler);

        bus.Strobe(0);
        Assert.AreEqual(2, calls);

        first.Dispose();
        bus.Strobe(0);
        Assert.AreEqual(3, calls);
    }

    [TestMethod]
    public void SubscribingDuringNotification_DoesNotAffectThatNotification()
    {
        var bus = new Bus(8);
        var calls = new List<string>();
        bus.Subscribe(() =>
        {
            calls.Add("a");
            if(calls.Count == 1)
                bus.Subscribe(() => calls.Add("late"));
        });
        bus.Subscribe(() => calls.Add("b"));

        bus.Strobe(0);
        CollectionAssert.AreEqual(new[] { "a", "b" }, calls);

        bus.Strobe(0);
        CollectionAssert.AreEqual(new[] { "a", "b", "a", "b", "late" }, calls);
    }

    [TestMethod]
    public void DisposingDuringNotification_DoesNotAffectThatNotification()
    {
        var bus = new Bus(8);
        var calls = new List<string>();
        IDisposable? second = null;
        bus.Subscribe(() => { calls.Add("a"); second!.Dispose(); });
        second = bus.Subscribe(() => calls.Add("b"));

        bus.Strobe(0);
        CollectionAssert.AreEqual(new[] { "a", "b" }, calls);

        bus.Strobe(0);
        CollectionAssert.AreEqual(new[] { "a", "b", "a" }, calls);
    }

    [TestMethod]
    public void Generic_PassesValue_AndDisposeWorks()
    {
        var wire = new Wire();
        var seen = new List<bool>();
        var subscription = wire.Subscribe(seen.Add);

        wire.Strobe(true);
        subscription.Dispose();
        wire.Strobe(false);

        CollectionAssert.AreEqual(new[] { true }, seen);
    }

    [TestMethod]
    public void Subscribe_Null_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Bus(8).Subscribe(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new Wire().Subscribe(null!));
    }
}
