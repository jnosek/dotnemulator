using System;
using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64;

internal class TestEmulator : Emulator
{
    /// <summary>
    /// Constructor mainly used for unit test and pre-loading 
    /// emulator memory
    /// </summary>
    /// <param name="initialMemory"></param>
    public TestEmulator(byte[] kernel, (int address, byte value)[]? ram = null) : base(false)
    {
        // set kernel rom
        MemoryMap.LoadTestKernel(kernel);

        // set initial RAM values
        if (ram != null)
        {
            foreach (var (address, value) in ram)
            {
                AddressBus.Drive(address);
                DataBus.Drive(value);
                DataBus.Trigger();
            }
        }
    }

    public int Accumulator
    {
        get => Cpu.A.Read();
        set => Cpu.A.Write(value);
    }

    public new void Start()
    {
        Cpu.Test();
    }
}
