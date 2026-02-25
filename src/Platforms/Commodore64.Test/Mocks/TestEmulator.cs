using Dotnemulator.Abstraction.Hardware;

namespace Dotnemulator.Platforms.Commodore64.Test.Mocks;

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
                _testMemoryMap.PokeRam(address, value);
            }
        }
    }

    public int PeekRam(int address)
    {
        return _testMemoryMap.PeekRam(address);
    }

    public int Accumulator
    {
        get => Cpu.A.Value;
        set => Cpu.A.Value = value;
    }

    public int X
    {
        get => Cpu.X.Value;
        set => Cpu.X.Value = value;
    }

    public int Y
    {
        get => Cpu.Y.Value;
        set => Cpu.Y.Value = value;
    }

    public new void Start()
    {
        Cpu.Test();
    }
}
