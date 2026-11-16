using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

public class RuntimeExecutionStrategy : IExecutionStrategy
{
    public void Execute(MOS6510Cpu cpu)
    {
        cpu.Initialize();

        while(true)
        {
            cpu.Step();
        }
    }
}
