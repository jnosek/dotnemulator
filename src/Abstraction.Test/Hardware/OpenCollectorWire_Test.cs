using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Abstraction.Test.Hardware;

[TestClass]
public class OpenCollectorWire_Test
{
    [TestMethod]
    public void StartsReleased()
    {
        var wire = new OpenCollectorWire();
        var source = wire.RegisterSource();

        Assert.IsFalse(wire.Value);
        Assert.IsFalse(source.IsAsserted);
    }

    [TestMethod]
    public void AnySourceAssertsTheLine()
    {
        var wire = new OpenCollectorWire();
        var a = wire.RegisterSource();
        var b = wire.RegisterSource();

        b.Assert();

        Assert.IsTrue(wire.Value);
        Assert.IsFalse(a.IsAsserted);
        Assert.IsTrue(b.IsAsserted);
    }

    [TestMethod]
    public void LineStaysAsserted_UntilEverySourceReleases()
    {
        var wire = new OpenCollectorWire();
        var a = wire.RegisterSource();
        var b = wire.RegisterSource();

        a.Assert();
        b.Assert();
        a.Release();
        Assert.IsTrue(wire.Value, "b still holds the line");

        b.Release();
        Assert.IsFalse(wire.Value);
    }

    [TestMethod]
    public void NotifiesOnlyOnEdgesOfTheCombinedLevel()
    {
        var wire = new OpenCollectorWire();
        var a = wire.RegisterSource();
        var b = wire.RegisterSource();
        var seen = new List<bool>();
        wire.Subscribe(seen.Add);

        a.Assert();      // edge: released -> asserted
        a.Assert();      // no change
        b.Assert();      // already asserted
        a.Release();     // b still holds it
        b.Release();     // edge: asserted -> released
        b.Release();     // no change

        CollectionAssert.AreEqual(new[] { true, false }, seen);
    }

    [TestMethod]
    public void SubscribersSeeTheNewLevel()
    {
        var wire = new OpenCollectorWire();
        var a = wire.RegisterSource();
        var levels = new List<bool>();
        wire.Subscribe(_ => levels.Add(wire.Value));

        a.Assert();
        a.Release();

        CollectionAssert.AreEqual(new[] { true, false }, levels);
    }

    [TestMethod]
    public void SupportsMaxSources_ThenThrows()
    {
        var wire = new OpenCollectorWire();
        var sources = new List<OpenCollectorWire.Source>();
        for(int i = 0; i < OpenCollectorWire.MaxSources; i++)
            sources.Add(wire.RegisterSource());

        // the last source (bit 31) works like any other
        sources[^1].Assert();
        Assert.IsTrue(wire.Value);
        sources[^1].Release();
        Assert.IsFalse(wire.Value);

        Assert.ThrowsExactly<InvalidOperationException>(() => wire.RegisterSource());
    }
}
