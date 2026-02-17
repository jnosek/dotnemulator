namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;
using System.Diagnostics;

/// <summary>
/// Programmable Logic Array for Memory Bank Switching
/// </summary>
class MemoryPla
{
    // TODO: expand to 7 for Cartridge High and Low lines
    public Bus MemoryBankBus = new Bus(5);

    // TODO: use later, input signals for cartridge detection
    public Wire Game = new Wire(true);
    public Wire ExRom = new Wire(true);

    private readonly Bus _address;
    private readonly Bus _port;

    public MemoryPla(Bus Address, Bus Port)
    {
        _address = Address;
        _port = Port;
    }
    
    public const byte UNMAPPED = 0b00000;
    public const byte BASIC_ROM = 0b00001;
    public const byte KERNEL_ROM = 0b00010;
    public const byte CHAR_ROM = 0b00100;  
    public const byte IO = 0b01000;
    public const byte RAM = 0b10000;

    readonly byte[][] MEMORY_CONFIGS = [
        // PageBlocks 0x0000  0x1000 0x8000 0xA000      0xC000 0xD000 0xE000    0xF000
        /* Port 
           bus */
        /* 000 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,   RAM,      RAM ], 
        /* 001 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,   CHAR_ROM, RAM ],
        /* 010 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,   CHAR_ROM, KERNEL_ROM ],
        /* 011 */   [ RAM,    RAM,   RAM,   BASIC_ROM,  RAM,   RAM,   CHAR_ROM, KERNEL_ROM ],
        /* 100 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,   RAM,      RAM ],
        /* 101 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,   IO,       RAM ],
        /* 110 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,   IO,       KERNEL_ROM ],
        /* 111 */   [ RAM,    RAM,   RAM,   BASIC_ROM,  RAM,   RAM,   IO,       KERNEL_ROM ]
        
     ];

    /// <summary>
    /// Evaluates the port bus and address bus to determine which memory segments are enabled, and asserts the corresponding wires
    /// </summary>
    /// <param name="_">throw away value, could be either address or port value change</param>
    private void Evaluate()
    {
        // top 3 bits
        var controlLines = _port.Read() & 0b111;
        var pageNumber = _address.Read() >> 13;

        byte memoryConfig = 
            pageNumber < 16 ? MEMORY_CONFIGS[controlLines][0] :
            pageNumber < 128 ? MEMORY_CONFIGS[controlLines][1] : 
            pageNumber < 160 ? MEMORY_CONFIGS[controlLines][2] :
            pageNumber < 192 ? MEMORY_CONFIGS[controlLines][3] :
            pageNumber < 208 ? MEMORY_CONFIGS[controlLines][4] :
            pageNumber < 224 ? MEMORY_CONFIGS[controlLines][5] :
            pageNumber < 240 ? MEMORY_CONFIGS[controlLines][6] :
            pageNumber < 256 ? MEMORY_CONFIGS[controlLines][7] :
            UNMAPPED;
                
        Debug.Assert(memoryConfig != UNMAPPED, $"Illegal page number {pageNumber}");

        // write values memory bank bus
        MemoryBankBus.Write(memoryConfig);
    }
}