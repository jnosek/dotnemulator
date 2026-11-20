namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class RuntimeExecutionStrategy_Test
{
    private static Emulator Build(params IExecutionObserver[] observers)
    {
        var strategy = new RuntimeExecutionStrategy();
        strategy.Observers.AddRange(observers);

        // JMP $E000 forever
        return new EmulatorBuilder()
            .WithTestKernel([0x4C, 0x00, 0xE0])
            .WithExecutionStrategy(strategy)
            .Build();
    }

    [TestMethod]
    public void StopsWhenAnObserverStops()
    {
        var calls = new List<string>();
        var stopper = new ExecutionObservers_Test.ScriptedObserver("stop", calls)
        {
            After = count => count == 10 ? ObserverAction.Stop : ObserverAction.Continue
        };
        var emulator = Build(stopper);

        emulator.Start();

        Assert.AreEqual(10, stopper.AfterCount);
    }

    [TestMethod]
    public void DetachedObserverIsNotCalledWhileOthersWatch()
    {
        var calls = new List<string>();
        var detacher = new ExecutionObservers_Test.ScriptedObserver("detach", calls)
        {
            After = count => count == 3 ? ObserverAction.Detach : ObserverAction.Continue
        };
        var stopper = new ExecutionObservers_Test.ScriptedObserver("stop", calls)
        {
            After = count => count == 10 ? ObserverAction.Stop : ObserverAction.Continue
        };
        var emulator = Build(detacher, stopper);

        emulator.Start();

        Assert.AreEqual(3, detacher.BeforeCount);
        Assert.AreEqual(3, detacher.AfterCount);
        Assert.AreEqual(10, stopper.AfterCount);
    }

    [TestMethod]
    public void BuilderAddsObserversInOrder()
    {
        var calls = new List<string>();
        var emulator = new EmulatorBuilder()
            .WithTestKernel([0x4C, 0x00, 0xE0])
            .WithExecutionStrategy(new RuntimeExecutionStrategy())
            .WithExecutionObserver(_ => new ExecutionObservers_Test.ScriptedObserver("a", calls))
            .WithExecutionObserver(_ => new ExecutionObservers_Test.ScriptedObserver("b", calls) { Before = _ => ObserverAction.Stop })
            .Build();

        emulator.Start();

        CollectionAssert.AreEqual(new[] { "a.before", "b.before" }, calls);
    }
}
