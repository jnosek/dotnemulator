using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64;

public class DebugExecutionStrategy : IExecutionStrategy
{
    const int INSTRUCTION_LOG_WINDOW_SIZE = 100;

    public bool IsStopOnNopEnabled { get; set; } = false;

    public bool IsJumpLoopDetectionEnabled { get; set; } = false;

    public int? StartAddress { get; set; }

    /// <summary>
    /// If execution reaches this address, it will stop. 
    /// </summary>
    /// <remarks>
    /// This can be useful for debugging or testing specific sections of code.
    /// </remarks>
    public int? EndAddress { get; set; }

    public Dictionary<int, Action<MOS6510Cpu>> BreakpointCallbacks { get; init; } = new();

    /// <summary>
    /// Called with the data bus value for every bus cycle, read or write, at the given address.
    /// </summary>
    public Dictionary<int, Action<int>> MemoryWatchCallbacks { get; init; } = new();

    /// <summary>
    /// Called with the value written for every write cycle at the given address. Reads are ignored.
    /// </summary>
    /// <remarks>
    /// Use this to capture output a program sends to a port (for example the functional test's report channel),
    /// where a read of the same address must not be mistaken for output.
    /// </remarks>
    public Dictionary<int, Action<int>> MemoryWriteWatchCallbacks { get; init; } = new();

    private readonly Queue<InstructionResult> _instructionLog = new(INSTRUCTION_LOG_WINDOW_SIZE);

    public void Execute(MOS6510Cpu cpu)
    {
        cpu.Initialize(StartAddress);

        // removed when Execute ends, so running again does not stack up memory-watch handlers
        IDisposable? memoryWatch = null;

        if(MemoryWatchCallbacks.Count > 0 || MemoryWriteWatchCallbacks.Count > 0)
        {
            // subscribed last, so DataBus holds the value written (write cycle) or the value just read (read cycle)
            memoryWatch = cpu.AddressBus.Subscribe(() => {
                var address = cpu.AddressBus.Read();
                if(MemoryWatchCallbacks.TryGetValue(address, out var callback))
                {
                    callback(cpu.DataBus.Read());
                }

                if(cpu.ReadWrite.IsWrite && MemoryWriteWatchCallbacks.TryGetValue(address, out var writeCallback))
                {
                    writeCallback(cpu.DataBus.Read());
                }
            });
        }

        try
        {
            while(true)
            {
                if(BreakpointCallbacks.TryGetValue(cpu.PC.Value, out var callback))
                {
                    callback(cpu);
                }

                var result = cpu.Step();

                // if this was a NOP instruction, break out of the loop and stop execution
                if(IsStopOnNopEnabled && result.Instruction == NOP.OP_CODE)
                    break;

                // if jump loop detection is enabled, check if the current instruction matches the last instruction in the log
                if(IsJumpLoopDetectionEnabled && _instructionLog.Count > 0)
                {
                    var lastInstruction = _instructionLog.Last();
                    if(result.Instruction == lastInstruction.Instruction &&
                       result.Address == lastInstruction.Address)
                    {
                        throw new JumpLoopException($"Detected potential jump loop at address {result.Address:X4} with instruction {result.Instruction:X2}")
                        {
                            InstructionLog = _instructionLog.Reverse().ToList()
                        };
                    }
                }

                // add the instruction result to the log
                _instructionLog.Enqueue(result);

                // maintain a rolling log of the last instructions
                if(_instructionLog.Count > INSTRUCTION_LOG_WINDOW_SIZE)
                    _instructionLog.Dequeue();

                // an interrupt sequence reports the interrupted address, but the instruction there has not run yet
                if(result.Interrupt == InterruptKind.None && result.Address == EndAddress)
                    break;
            }
        }
        finally
        {
            memoryWatch?.Dispose();
        }
    }
}
