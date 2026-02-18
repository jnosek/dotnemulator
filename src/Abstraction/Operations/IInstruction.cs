using System;

namespace Dotnemulator.Abstraction.Operations;

public interface IInstruction
{
    int Cycles { get; }

    int OpCode { get; }

    void Execute(int instruction);
}
