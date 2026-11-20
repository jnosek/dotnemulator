using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

internal interface IExecutionStrategy
{
    /// <summary>
    /// Observers called before and after every step, in this order, until they detach
    /// (see <see cref="IExecutionObserver"/>).
    /// </summary>
    IList<IExecutionObserver> Observers { get; }

    void Execute(MOS6510Cpu cpu);
}
