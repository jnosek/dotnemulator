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

    public Dictionary<int, Action<int>> MemoryWatchCallbacks { get; init; } = new();

    private readonly Queue<InstructionResult> _instructionLog = new(INSTRUCTION_LOG_WINDOW_SIZE);

    public void Execute(MOS6510Cpu cpu)
    {
        cpu.Initialize(StartAddress);

        if(MemoryWatchCallbacks.Count > 0)
        {
            cpu.AddressBus.Subscribe(() => {
                var address = cpu.AddressBus.Read();
                if(MemoryWatchCallbacks.TryGetValue(address, out var callback))
                {
                    callback(cpu.DataBus.Read());
                }
            });
        }

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

            if(result.Address == EndAddress)
                break;
        }
    }
}
