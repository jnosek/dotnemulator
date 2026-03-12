using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

internal interface IExecutionStrategy
{
    void Execute(MOS6510Cpu cpu);
}