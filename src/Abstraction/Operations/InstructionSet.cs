using System;
using System.Diagnostics;

namespace Dotnemulator.Abstraction.Operations;

public class InstructionSet
{
    private readonly IInstruction[] _instructions;

    public InstructionSet(int count)
    {   
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0, nameof(count));
        _instructions = new IInstruction[count];
    }

    public void Add(params IInstruction[] entries)
    {
        foreach (var entry in entries)
        {
            if(_instructions[entry.OpCode] != null)
                throw new InvalidOperationException($"Instruction with opcode {entry.OpCode} is already defined.");

            _instructions[entry.OpCode] = entry;
        }
    }

    public IInstruction this[int opCode]
    {
        get
        {
            Debug.Assert(opCode >= 0 && opCode < _instructions.Length, "Opcode is out of range.");

            var instruction = _instructions[opCode];

            Debug.Assert(instruction != null, $"No instruction defined for opcode {opCode}.");

            return instruction;
        }
    }
}
