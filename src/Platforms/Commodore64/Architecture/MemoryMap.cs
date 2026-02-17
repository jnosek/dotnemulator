namespace Dotnemulator.Platforms.Commodore64.Architecture;

using System.Runtime.Intrinsics.Arm;
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
        _dataBus = dataBus;
        _memoryBankBus = memoryBankBus;

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

    public void Write(int value)
    {
        switch(_memoryBankBus.Value)
        {
            // ROM cannot be written to, the underlying RAM will be accessed instead
            case MemoryPla.BASIC_ROM:
            case MemoryPla.CHAR_ROM:
            case MemoryPla.KERNEL_ROM:
            case MemoryPla.RAM:
                _ram.Write(value, _addressBus.Value);
                break;
            case MemoryPla.IO:
                _io.Write(value, _addressBus.Value);
                break;
            case MemoryPla.UNMAPPED:
                // do nothing, unmapped memory writes are ignored
                break;
            default:
                throw new InvalidOperationException($"Invalid memory bank configuration: {_memoryBankBus.Value}");
        }
    }

    public int Read()
    {
        switch(_memoryBankBus.Value)
        {
            case MemoryPla.BASIC_ROM:
                return (byte)_basicRom.Read(_addressBus.Value);
            case MemoryPla.CHAR_ROM:
                return (byte)_charRom.Read(_addressBus.Value);
            case MemoryPla.KERNEL_ROM:
                return (byte)_kernelRom.Read(_addressBus.Value);
            case MemoryPla.RAM:
                return (byte)_ram.Read(_addressBus.Value);
            case MemoryPla.IO:
                return (byte)_io.Read(_addressBus.Value);
            case MemoryPla.UNMAPPED:
                // unmapped memory reads typically return open bus values, but for simplicity we'll return 0
                return 0;
            default:
                throw new InvalidOperationException($"Invalid memory bank configuration: {_memoryBankBus.Value}");
        }
    }
}