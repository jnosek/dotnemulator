namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;

internal class MemoryMap
{
    protected readonly MemorySegment _ram = new MemorySegment(0x1_0000);
    protected readonly MemorySegment _basicRom = new MemorySegment(0x_2000, 0xA000);
    protected readonly MemorySegment _charRom = new MemorySegment(0x0_1000, 0xD000);
    protected readonly MemorySegment _kernelRom = new MemorySegment(0x0_2000, 0xE000);
    protected readonly MemorySegment _io = new MemorySegment(0x0_1000, 0xD000);

    private readonly Bus _addressBus;
    private readonly Bus _dataBus;
    private readonly Bus _memoryBankBus;

    public MemoryMap(Bus addressBus, Bus dataBus, Bus memoryBankBus)
    {
        _addressBus = addressBus;
        _memoryBankBus = memoryBankBus;
        _dataBus = dataBus;
        
        _addressBus.Subscribe(ReadHandler);
        _memoryBankBus.Subscribe(ReadHandler);
        _dataBus.Subscribe(WriteHandler);
    }

    internal void LoadRoms(Stream basicRomStream, Stream charRomStream, Stream kernelRomStream)
    {
       _basicRom.Load(basicRomStream);
       _charRom.Load(charRomStream);
       _kernelRom.Load(kernelRomStream);       
    }

    internal void LoadTestKernel(byte[] kernel)
    {
        for (int i = 0; i < kernel.Length; i++)
        {
            _kernelRom.Write(0xE000 + i, kernel[i]);
        }
        
        // set CPU start address to beginning of kernel rom
        _kernelRom.Write(MOS6510Cpu.RESET_VECTOR,     0x00);
        _kernelRom.Write(MOS6510Cpu.RESET_VECTOR + 1, 0xE0);

        // set IRQ vector to beginning of PAGE 1 of RAM
        _kernelRom.Write(MOS6510Cpu.IRQ_VECTOR,     0x00);
        _kernelRom.Write(MOS6510Cpu.IRQ_VECTOR + 1, 0x01);
    }

    internal void LoadImage(byte[] image)
    {
        var ram = (byte[])image.Clone();
        Array.Clear(ram, 0xA000, 0x2000); // basic rom
        Array.Clear(ram, 0xD000, 0x1000); // char rom
        Array.Clear(ram, 0xE000, 0x2000); // kernel rom
        _ram.Load(new MemoryStream(ram));

        _basicRom.Load(new MemoryStream(image, 0xA000, 0x2000));
        _charRom.Load(new MemoryStream(image, 0xD000, 0x1000));
        _kernelRom.Load(new MemoryStream(image, 0xE000, 0x2000));
    }

    internal void SetRam((int address, int value)[] values)
    {
        foreach (var (address, value) in values)
        {
            _ram.Write(address, value);
        }
    }

    public string[] GetRomHashes()
    {
        return
        [
            Convert.ToHexString(_basicRom.GetHash()),
            Convert.ToHexString(_charRom.GetHash()),
            Convert.ToHexString(_kernelRom.GetHash())
        ];
    }

    private void WriteHandler()
    {
        var memoryBank = _memoryBankBus.Read();
        var address = _addressBus.Read();
        var data = _dataBus.Read();

        switch(_memoryBankBus.Read())
        {
            // ROM cannot be written to, the underlying RAM will be accessed instead
            case MemoryPla.BASIC_ROM:
            case MemoryPla.CHAR_ROM:
            case MemoryPla.KERNEL_ROM:
            case MemoryPla.RAM:
                _ram.Write(address, data);
                break;
            case MemoryPla.IO:
                _io.Write(address, data);
                break;
            case MemoryPla.UNMAPPED:
                // do nothing, unmapped memory writes are ignored
                break;
            default:
                throw new InvalidOperationException($"Invalid memory bank configuration: {memoryBank}");
        }
    }

    private void ReadHandler()
    {
        int value;
        var memoryBank = _memoryBankBus.Read();
        var address = _addressBus.Read();

        switch(memoryBank)
        {
            case MemoryPla.BASIC_ROM:
                value = _basicRom.Read(address);
                break;
            case MemoryPla.CHAR_ROM:
                value = _charRom.Read(address);
                break;
            case MemoryPla.KERNEL_ROM:
                value = _kernelRom.Read(address);
                break;
            case MemoryPla.RAM:
                value = _ram.Read(address);
                break;
            case MemoryPla.IO:
                value = _io.Read(address);
                break;
            case MemoryPla.UNMAPPED:
                // unmapped memory reads typically return open bus values, but for simplicity we'll return 0
                value = 0;
                break;
            default:
                throw new InvalidOperationException($"Invalid memory bank configuration: {memoryBank}");
        }

        _dataBus.Drive(value);
    }

    /// <summary>
    /// Reads a value from the RAM at the specified address without affecting the CPU state.
    /// </summary>
    /// <remarks>
    /// For testing purposes only
    /// </remarks>
    /// <param name="address">The address in RAM to read from.</param>
    /// <returns>The value stored at the specified RAM address.</returns>
    internal int PeekRam(int address)
    {
        return _ram.Read(address);
    }

    /// <summary>
    /// Set a value in the ram at the specified address without affecting the CPU State.
    /// </summary>
    /// <remarks>
    /// For testing purposes only
    /// </remarks>
    /// <param name="address"></param>
    /// <param name="value"></param>
    internal void PokeRam(int address, int value)
    {
        _ram.Write(address, value);
    }
}