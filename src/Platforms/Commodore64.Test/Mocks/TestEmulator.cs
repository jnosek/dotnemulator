using System;
using System.ComponentModel;
using Commodore64.Test.Mocks;
using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64.Test;

internal class TestEmulator : Emulator
{   
    private TestMemoryMap _testMemoryMap;

    private static TestMemoryMap MemoryMapFactory(Bus addressBus, Bus dataBus, Bus memoryBankBus)
    {
        return new TestMemoryMap(addressBus, dataBus, memoryBankBus);
    }

    /// <summary>
    /// Constructor mainly used for unit test and pre-loading 
    /// emulator memory
    /// </summary>
    /// <param name="initialMemory"></param>
    public TestEmulator(byte[] kernel, (int address, byte value)[]? ram = null) : 
        base(false, MemoryMapFactory)
    {
        _testMemoryMap = (TestMemoryMap)MemoryMap;

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

    public int PeekRam(int address)
    {
        return _testMemoryMap.PeekRam(address);
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
