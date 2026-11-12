namespace Dotnemulator.Platforms.Commodore64;

using Dotnemulator.Platforms.Commodore64.Architecture;

public sealed class EmulatorBuilder
{
    private List<Action<MemoryMap>> _memoryMapConfigurations = new();

    private Func<Emulator, MemoryMap>? _memoryMapFactory = null;

    private IExecutionStrategy? _executionStrategy = null;

    public EmulatorBuilder WithRoms()
    {
        _memoryMapConfigurations.Add((map) => {
            var assembly = typeof(Emulator).Assembly;
            using var basicRomStream = assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.basic.901226-01.bin") ??
                throw new InvalidOperationException("Failed to load BASIC ROM");
            using var charRomStream = assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.char.901225-01.bin") ??
                throw new InvalidOperationException("Failed to load CHAR ROM");
            using var kernelRomStream = assembly.GetManifestResourceStream("Dotnemulator.Platforms.Commodore64.Roms.kernel.901227-03.bin") ??
                    throw new InvalidOperationException("Failed to load KERNEL ROM");

            map.LoadRoms(basicRomStream, charRomStream, kernelRomStream);
        });

        return this;
    }

    internal EmulatorBuilder WithTestKernel(byte[] data)
    {
        _memoryMapConfigurations.Add((map) => map.LoadTestKernel(data));

        return this;
    }

    internal EmulatorBuilder WithRam((int address, int value)[] values)
    {
        _memoryMapConfigurations.Add((map) => map.SetRam(values));
        return this;
    }

    public EmulatorBuilder WithImage(byte[] image)
    {
        _memoryMapConfigurations.Add((map) => map.LoadImage(image));
        return this;
    }

    internal EmulatorBuilder WithMemoryMap(Func<Emulator, MemoryMap> factory)
    {
        _memoryMapFactory = factory;
        return this;
    }

    internal EmulatorBuilder WithExecutionStrategy(IExecutionStrategy executionStrategy)
    {
        _executionStrategy = executionStrategy;
        return this;
    }

    internal EmulatorBuilder WithUnitTestMode(int? startAddress = null)
    {
        _executionStrategy = new DebugExecutionStrategy
        {
            IsStopOnNopEnabled = true,
            StartAddress = startAddress
        };

        return this;
    }

    public Emulator Build()
    {
        var emulator = new Emulator(_memoryMapFactory);

        if(_executionStrategy != null)
            emulator.ExecutionStrategy = _executionStrategy;

        foreach (var configure in _memoryMapConfigurations)
        {
            configure(emulator.MemoryMap);
        }

        return emulator;
    }
}
