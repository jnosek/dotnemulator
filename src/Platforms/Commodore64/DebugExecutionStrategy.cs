using Dotnemulator.Platforms.Commodore64.Architecture;
using Dotnemulator.Platforms.Commodore64.Architecture.Instruction;

namespace Dotnemulator.Platforms.Commodore64;

public class DebugExecutionStrategy : IExecutionStrategy
{
    const int INSTRUCTION_LOG_WINDOW_SIZE = 100;

    public bool IsStopOnNopEnabled { get; set; } = false;

    public bool IsJumpLoopDetectionEnabled { get; set; } = false;

    public int? StartAddress { get; set; }

    private readonly Queue<InstructionResult> _instructionLog = new(INSTRUCTION_LOG_WINDOW_SIZE);

    public void Execute(MOS6510Cpu cpu)
    {
        cpu.Initialize(StartAddress);

        while(true)
        {
            var result = cpu.Step();

            // if this was a NOP instruction, break out of the loop and stop execution
            if(IsStopOnNopEnabled && result.Instruction == NOP.OP_CODE)
                break;

            // if jump loop detection is enabled, check if the current instruction matches the last instruction in the log
            if(IsJumpLoopDetectionEnabled)
            {
                var lastInstruction = _instructionLog.Last();
                if(result.Instruction == lastInstruction.Instruction &&
                   result.Address == lastInstruction.Address)
                {
                    throw new Exception($"Detected potential jump loop at address {result.Address:X4} with instruction {result.Instruction:X2}");
                }
            }

            // add the instruction result to the log
            _instructionLog.Enqueue(result);

            // maintain a rolling log of the last instructions
            if(_instructionLog.Count > INSTRUCTION_LOG_WINDOW_SIZE)
                _instructionLog.Dequeue();
        }
    }
}
