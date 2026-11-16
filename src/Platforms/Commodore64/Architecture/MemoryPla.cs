namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;
using System.Diagnostics;

/// <summary>
/// Programmable Logic Array for Memory Bank Switching
/// </summary>
/// <remarks>
/// <para>
/// The real PLA is combinational: its chip-select outputs are a function of address lines A12-A15, the 6510
/// port lines LORAM/HIRAM/CHAREN and the cartridge lines GAME/EXROM, with no clock or stored state. It is modelled
/// as an <see cref="IMemoryBankSelector"/> that components query during a bus cycle, rather than as a subscriber
/// of the address bus.
/// </para>
/// <para>
/// <b>How the mapping works.</b> Selecting a bank is split into a row choice, made only when the port changes,
/// and a single array lookup, made on every memory access. Everything else is built once, when the class is
/// first used:
/// </para>
/// <list type="number">
///   <item>
///     <term><see cref="MEMORY_CONFIGS"/></term>
///     <description>
///     The readable source table, and the one to edit if the mapping changes. Rows are the low 3 bits of the
///     6510 port at <c>$0001</c> (bit 0 LORAM, bit 1 HIRAM, bit 2 CHAREN; <c>111</c> is the default after reset).
///     Columns are the 7 regions of the memory map, which differ in size: <c>$0000</c> (4 KB), <c>$1000</c> (28 KB),
///     <c>$8000</c> (8 KB), <c>$A000</c> (8 KB), <c>$C000</c> (4 KB), <c>$D000</c> (4 KB) and <c>$E000</c> (8 KB).
///     </description>
///   </item>
///   <item>
///     <term><see cref="BLOCK_TO_COLUMN"/></term>
///     <description>
///     Every region boundary is a multiple of <c>$1000</c> (4 KB), so the top 4 bits of an address
///     (<c>address &gt;&gt; 12</c>, address lines A12-A15, the same lines the real PLA decodes) identify its region.
///     This 16-entry table maps each 4 KB block to its column of <see cref="MEMORY_CONFIGS"/>.
///     </description>
///   </item>
///   <item>
///     <term><see cref="BLOCK_BANKS"/></term>
///     <description>
///     The two tables above combined into an 8 x 16 table indexed by [port setting][4 KB block], 128 bytes in
///     total. It is static, built once and shared by every instance.
///     </description>
///   </item>
///   <item>
///     <term><see cref="_blockBanks"/></term>
///     <description>
///     The row of <see cref="BLOCK_BANKS"/> for the current port setting. The PLA subscribes to the port bus, so
///     a write to <c>$0001</c> swaps this reference (see <see cref="UpdateConfiguration"/>); the constructor sets it
///     from whatever is already on the port bus. Only the reference changes: the row is shared and never modified,
///     and a port change does not start a memory cycle.
///     </description>
///   </item>
/// </list>
/// <para>
/// <see cref="SelectBank"/> is then one shift and one array index: <c>_blockBanks[address &gt;&gt; 12]</c>.
/// For example, with <c>$37</c> written to <c>$0001</c>, <c>$37 &amp; 0b111 = 0b111</c> selects row 7. A read of
/// <c>$D020</c> gives <c>$D020 &gt;&gt; 12 = 13</c>, and <c>_blockBanks[13]</c> is <see cref="IO"/>. After writing
/// <c>$33</c> (CHAREN cleared), row 3 is selected and the same lookup returns <see cref="CHAR_ROM"/>.
/// </para>
/// <para>
/// <c>$0000</c> and <c>$1000</c> are separate columns even though both are always RAM here: with cartridges, in
/// Ultimax mode, <c>$1000-$7FFF</c> becomes unmapped while <c>$0000-$0FFF</c> stays RAM.
/// </para>
/// <para>
/// <b>Cartridge lines.</b> When <see cref="Game"/> and <see cref="ExRom"/> are modelled they become two more bits
/// of the row index (32 rows), <see cref="MEMORY_CONFIGS"/> gains the extra layouts, and both wires subscribe to
/// <see cref="UpdateConfiguration"/>. The cartridge ROM windows (<c>$8000</c>, <c>$A000</c>, <c>$E000</c>) also fall on
/// 4 KB boundaries, so the 16-block layout and <see cref="SelectBank"/> stay the same.
/// </para>
/// </remarks>
public class MemoryPla : IMemoryBankSelector
{
    // TODO: use later, input signals for cartridge detection.
    // When modelled, they should also call UpdateConfiguration and extend the table index.
    public Wire Game = new Wire(true);
    public Wire ExRom = new Wire(true);

    private readonly Bus _port;

    /// <summary>
    /// Bank for each 4 KB block under the current port setting; a row of <see cref="BLOCK_BANKS"/>.
    /// </summary>
    private byte[] _blockBanks;

    public MemoryPla(Bus port)
    {
        _port = port;
        _port.Subscribe(UpdateConfiguration);

        _blockBanks = BLOCK_BANKS[_port.Read() & 0b111];
    }
    
    public const byte UNMAPPED = 0b00000;
    public const byte BASIC_ROM = 0b00001;
    public const byte KERNEL_ROM = 0b00010;
    public const byte CHAR_ROM = 0b00100;  
    public const byte IO = 0b01000;
    public const byte RAM = 0b10000;

    /// <summary>
    /// Bank selected for each region of the memory map, per port setting (LORAM/HIRAM/CHAREN).
    /// </summary>
    /// <remarks>
    /// This is the source of truth; <see cref="BLOCK_BANKS"/> is generated from it. Static fields are initialized
    /// in declaration order, so this must stay above <see cref="BLOCK_BANKS"/>.
    /// </remarks>
    private static readonly byte[][] MEMORY_CONFIGS = [
        // PageBlocks 0x0000  0x1000 0x8000 0xA000      0xC000 0xD000    0xE000    
        /* Port 
           bus */
        /* 000 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,      RAM ], 
        /* 001 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   CHAR_ROM, RAM ],
        /* 010 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   CHAR_ROM, KERNEL_ROM ],
        /* 011 */   [ RAM,    RAM,   RAM,   BASIC_ROM,  RAM,   CHAR_ROM, KERNEL_ROM ],
        /* 100 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   RAM,      RAM ],
        /* 101 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   IO,       RAM ],
        /* 110 */   [ RAM,    RAM,   RAM,   RAM,        RAM,   IO,       KERNEL_ROM ],
        /* 111 */   [ RAM,    RAM,   RAM,   BASIC_ROM,  RAM,   IO,       KERNEL_ROM ]
        
     ];

    /// <summary>
    /// Column of <see cref="MEMORY_CONFIGS"/> that covers each 4 KB block (<c>address &gt;&gt; 12</c>).
    /// </summary>
    /// <remarks>
    /// Must stay above <see cref="BLOCK_BANKS"/>, which is built from it during static initialization.
    /// </remarks>
    private static readonly int[] BLOCK_TO_COLUMN = [
        0,                      // 0x0000
        1, 1, 1, 1, 1, 1, 1,    // 0x1000 - 0x7FFF
        2, 2,                   // 0x8000 - 0x9FFF
        3, 3,                   // 0xA000 - 0xBFFF
        4,                      // 0xC000
        5,                      // 0xD000
        6, 6                    // 0xE000 - 0xFFFF
    ];

    /// <summary>
    /// <see cref="MEMORY_CONFIGS"/> expanded to [port setting][4 KB block].
    /// </summary>
    /// <remarks>
    /// For example, row <c>111</c> (the default) is:
    /// <code>
    /// block:  0   1   2   3   4   5   6   7   8   9   A      B      C   D   E       F
    /// bank:   RAM RAM RAM RAM RAM RAM RAM RAM RAM RAM BASIC  BASIC  RAM IO  KERNAL  KERNAL
    /// </code>
    /// Rows are shared by every instance through <see cref="_blockBanks"/> and must never be modified.
    /// </remarks>
    private static readonly byte[][] BLOCK_BANKS = BuildBlockBanks();

    private static byte[][] BuildBlockBanks()
    {
        var table = new byte[MEMORY_CONFIGS.Length][];

        for(int config = 0; config < MEMORY_CONFIGS.Length; config++)
        {
            table[config] = new byte[BLOCK_TO_COLUMN.Length];

            for(int block = 0; block < BLOCK_TO_COLUMN.Length; block++)
            {
                table[config][block] = MEMORY_CONFIGS[config][BLOCK_TO_COLUMN[block]];
            }
        }

        return table;
    }

    /// <summary>
    /// Selects the row of <see cref="BLOCK_BANKS"/> for the current LORAM/HIRAM/CHAREN lines (the low 3 bits of the port bus).
    /// </summary>
    /// <remarks>
    /// Runs when the port bus is strobed (a CPU write to <c>$0001</c>). It only swaps a reference, so a bank
    /// switch costs nothing on later memory accesses and does not start a memory cycle.
    /// </remarks>
    private void UpdateConfiguration()
    {
        _blockBanks = BLOCK_BANKS[_port.Read() & 0b111];
    }

    /// <inheritdoc/>
    /// <remarks>
    /// One shift and one array index: the top 4 address bits (A12-A15) pick the 4 KB block in the current row.
    /// The address range is only checked in DEBUG builds.
    /// </remarks>
    public int SelectBank(int address)
    {
        Debug.Assert(address >= 0 && address <= 0xFFFF, $"Address {address:X} is outside the 16-bit address space.");

        return _blockBanks[address >> 12];
    }
}
