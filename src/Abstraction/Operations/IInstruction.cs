using System;

namespace Dotnemulator.Abstraction.Operations;

public interface IInstruction
{
    int OpCode { get; }

    void Execute(int instruction);
}
