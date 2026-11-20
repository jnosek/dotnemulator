using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

internal interface IExecutionStrategy
{
    /// <summary>
    /// Optional observer called before each step until it detaches (see <see cref="IExecutionObserver"/>).
    /// </summary>
    IExecutionObserver? Observer { get; set; }

    void Execute(MOS6510Cpu cpu);
}
