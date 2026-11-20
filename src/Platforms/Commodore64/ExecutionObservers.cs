using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64;

/// <summary>
/// The observers attached for one run, called in order, with detached observers removed as they go.
/// </summary>
/// <remarks>
/// Used by the execution strategies. Calling the observers does not allocate; only a detach changes the list.
/// </remarks>
internal sealed class ExecutionObservers
{
    private readonly List<IExecutionObserver> _observers;

    public ExecutionObservers(IEnumerable<IExecutionObserver> observers)
    {
        _observers = new List<IExecutionObserver>(observers);
    }

    /// <summary>
    /// <c>true</c> once every observer has detached (or there were none).
    /// </summary>
    public bool IsEmpty => _observers.Count == 0;

    /// <summary>
    /// Calls <see cref="IExecutionObserver.BeforeStep"/> on each observer.
    /// </summary>
    /// <returns><c>true</c> if an observer asked to stop; the step must not run.</returns>
    public bool BeforeStep(MOS6510Cpu cpu)
    {
        for(int i = 0; i < _observers.Count; )
        {
            switch(_observers[i].BeforeStep(cpu))
            {
                case ObserverAction.Stop:
                    return true;
                case ObserverAction.Detach:
                    _observers.RemoveAt(i);
                    break;
                default:
                    i++;
                    break;
            }
        }

        return false;
    }

    /// <summary>
    /// Calls <see cref="IExecutionObserver.AfterStep"/> on each observer.
    /// </summary>
    /// <returns><c>true</c> if an observer asked to stop.</returns>
    public bool AfterStep(MOS6510Cpu cpu, InstructionResult result)
    {
        for(int i = 0; i < _observers.Count; )
        {
            switch(_observers[i].AfterStep(cpu, result))
            {
                case ObserverAction.Stop:
                    return true;
                case ObserverAction.Detach:
                    _observers.RemoveAt(i);
                    break;
                default:
                    i++;
                    break;
            }
        }

        return false;
    }
}
