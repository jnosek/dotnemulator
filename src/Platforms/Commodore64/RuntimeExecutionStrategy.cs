using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

/// <summary>
/// Resets the CPU and runs it until an observer stops it, or forever.
/// </summary>
/// <remarks>
/// While any <see cref="Observers"/> are attached each step goes through them. Once all have detached the
/// strategy switches to a plain <see cref="MOS6510Cpu.Step"/> loop, so a finished observer (for example the boot
/// monitor) costs nothing for the rest of the run.
/// </remarks>
public class RuntimeExecutionStrategy : IExecutionStrategy
{
    internal List<IExecutionObserver> Observers { get; } = new();

    IList<IExecutionObserver> IExecutionStrategy.Observers => Observers;

    public void Execute(MOS6510Cpu cpu)
    {
        cpu.Initialize();

        var observers = new ExecutionObservers(Observers);

        while(!observers.IsEmpty)
        {
            if(observers.BeforeStep(cpu))
                return;

            var result = cpu.Step();

            if(observers.AfterStep(cpu, result))
                return;
        }

        while(true)
        {
            cpu.Step();
        }
    }
}
