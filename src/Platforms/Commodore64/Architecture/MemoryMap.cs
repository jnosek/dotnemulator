namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;

class MemoryMap
{
    private readonly MemorySegment _ram = new MemorySegment(0x1_0000);
    private readonly MemorySegment _basicRom = new MemorySegment(0x_2000, 0xA000);
    private readonly MemorySegment _charRom = new MemorySegment(0x0_1000, 0xD000);
    private readonly MemorySegment _kernelRom = new MemorySegment(0x0_2000, 0xE000);
    private readonly MemorySegment _io = new MemorySegment(0x0_1000, 0xD000);

    private readonly Bus _addressBus;
    private readonly Bus _dataBus;
    private readonly Bus _memoryBankBus;

    public MemoryMap(Bus addressBus, Bus dataBus, Bus memoryBankBus)
    {
        _addressBus = addressBus;
        _memoryBankBus = memoryBankBus;
        _dataBus = dataBus;
        
        _addressBus.Subscribe(Read);
        _memoryBankBus.Subscribe(Read);
        _dataBus.Subscribe(Write);
    }

    public void LoadRoms(Stream basicRomStream, Stream charRomStream, Stream kernelRomStream)
    {
       _basicRom.Load(basicRomStream);
       _charRom.Load(charRomStream);
       _kernelRom.Load(kernelRomStream);       
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

    private void Write()
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
                _ram.Write(data, address);
                break;
            case MemoryPla.IO:
                _io.Write(data, address);
                break;
            case MemoryPla.UNMAPPED:
                // do nothing, unmapped memory writes are ignored
                break;
            default:
                throw new InvalidOperationException($"Invalid memory bank configuration: {memoryBank}");
        }
    }

    private void Read()
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
}