using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

public class RuntimeExecutionStrategy : IExecutionStrategy
{
    IExecutionObserver? IExecutionStrategy.Observer { get; set; }

    public void Execute(MOS6510Cpu cpu)
    {
        cpu.Initialize();

        // watched until the observer detaches, then the plain loop with no per-step cost
        var observer = ((IExecutionStrategy)this).Observer;
        if(observer != null)
        {
            while(observer.BeforeStep(cpu))
            {
                cpu.Step();
            }
        }

        while(true)
        {
            cpu.Step();
        }
    }
}
