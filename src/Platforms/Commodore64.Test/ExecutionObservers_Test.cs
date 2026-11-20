using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class ExecutionObservers_Test
{
    /// <summary>
    /// Records its calls in a shared list and returns scripted actions.
    /// </summary>
    internal sealed class ScriptedObserver(string name, List<string> calls) : IExecutionObserver
    {
        public Func<int, ObserverAction> Before { get; init; } = _ => ObserverAction.Continue;
        public Func<int, ObserverAction> After { get; init; } = _ => ObserverAction.Continue;

        public int BeforeCount { get; private set; }
        public int AfterCount { get; private set; }

        public ObserverAction BeforeStep(MOS6510Cpu cpu)
        {
            calls.Add($"{name}.before");
            return Before(++BeforeCount);
        }

        public ObserverAction AfterStep(MOS6510Cpu cpu, InstructionResult result)
        {
            calls.Add($"{name}.after");
            return After(++AfterCount);
        }
    }

    private static readonly MOS6510Cpu Cpu = new EmulatorBuilder().WithTestKernel([]).Build().Cpu;
    private static readonly InstructionResult Result = new(0xE000, 0xEA, 0);

    private readonly List<string> _calls = new();

    [TestMethod]
    public void CallsObserversInOrder()
    {
        var observers = new ExecutionObservers([new ScriptedObserver("a", _calls), new ScriptedObserver("b", _calls)]);

        Assert.IsFalse(observers.BeforeStep(Cpu));
        Assert.IsFalse(observers.AfterStep(Cpu, Result));

        CollectionAssert.AreEqual(new[] { "a.before", "b.before", "a.after", "b.after" }, _calls);
    }

    [TestMethod]
    public void DetachedObserverIsNotCalledAgain()
    {
        var observers = new ExecutionObservers([
            new ScriptedObserver("a", _calls) { Before = _ => ObserverAction.Detach },
            new ScriptedObserver("b", _calls)
        ]);

        observers.BeforeStep(Cpu);
        observers.AfterStep(Cpu, Result);
        observers.BeforeStep(Cpu);

        CollectionAssert.AreEqual(new[] { "a.before", "b.before", "b.after", "b.before" }, _calls);
        Assert.IsFalse(observers.IsEmpty);
    }

    [TestMethod]
    public void EmptyOnceAllDetach()
    {
        var observers = new ExecutionObservers([new ScriptedObserver("a", _calls) { After = _ => ObserverAction.Detach }]);

        observers.BeforeStep(Cpu);
        observers.AfterStep(Cpu, Result);

        Assert.IsTrue(observers.IsEmpty);
    }

    [TestMethod]
    public void StopSkipsLaterObservers()
    {
        var observers = new ExecutionObservers([
            new ScriptedObserver("a", _calls) { Before = _ => ObserverAction.Stop },
            new ScriptedObserver("b", _calls)
        ]);

        Assert.IsTrue(observers.BeforeStep(Cpu));
        CollectionAssert.AreEqual(new[] { "a.before" }, _calls);
    }

    [TestMethod]
    public void StopFromAfterStep()
    {
        var observers = new ExecutionObservers([new ScriptedObserver("a", _calls) { After = _ => ObserverAction.Stop }]);

        Assert.IsFalse(observers.BeforeStep(Cpu));
        Assert.IsTrue(observers.AfterStep(Cpu, Result));
    }
}
