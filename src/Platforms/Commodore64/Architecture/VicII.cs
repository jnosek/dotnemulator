namespace Dotnemulator.Platforms.Commodore64.Architecture;

using Dotnemulator.Abstraction.Hardware;

/// <summary>
/// Minimal MOS 6569 VIC-II (PAL): register storage plus a running raster counter.
/// </summary>
/// <remarks>
/// <para>
/// This is a stub that does just enough for the KERNAL to boot. CINT (<c>$FF5B</c>) waits for the raster counter
/// to reach line 0 before it detects PAL or NTSC, and that never happens unless <c>$D012</c> moves.
/// </para>
/// <para>
/// The chip answers bus cycles at <c>$D000-$D3FF</c> while the PLA selects the I/O bank. Its 64 registers repeat
/// every 64 bytes in that range (only address lines A0-A5 reach the chip). <see cref="MemoryMap"/> leaves that
/// range to this chip, so exactly one component responds to each cycle.
/// </para>
/// <para>
/// Reading <c>$D012</c> returns bits 0-7 of the current raster line and bit 7 of <c>$D011</c> returns bit 8.
/// Writing them sets the raster compare line instead. Every other register just stores what is written.
/// Raster interrupts, sprites, bad lines and video output are not modelled.
/// </para>
/// <para>
/// <c>$D019</c> (interrupt latches) is plain storage, which makes CINT detect PAL by accident: its register
/// table writes <c>$0F</c> there, and bit 0 reads back as 1, which CINT takes to mean the raster reached line 311
/// (PAL only). On a real VIC-II, writing 1s to <c>$D019</c> clears the latches; bit 0 would be set by the raster
/// reaching the compare line.
/// </para>
/// <para>
/// There is no clock wire yet, so every bus cycle is counted as one φ2 cycle: on a 6510 each memory access takes
/// one cycle. PAL timing is 63 cycles per line and 312 lines per frame.
/// </para>
/// </remarks>
internal class VicII
{
    public const int FIRST_ADDRESS = 0xD000;
    public const int LAST_ADDRESS = 0xD3FF;

    public const int REGISTER_COUNT = 0x40;
    public const int CONTROL_1 = 0x11;
    public const int RASTER = 0x12;

    /// <summary>
    /// φ2 cycles per raster line (PAL 6569).
    /// </summary>
    public const int CYCLES_PER_LINE = 63;

    /// <summary>
    /// Raster lines per frame (PAL 6569): the counter runs 0-311.
    /// </summary>
    public const int LINES_PER_FRAME = 312;

    private readonly Bus _addressBus;
    private readonly Bus _dataBus;
    private readonly ReadWriteWire _readWrite;
    private readonly IMemoryBankSelector _bankSelector;

    private readonly int[] _registers = new int[REGISTER_COUNT];
    private int _cycle;

    public VicII(Bus addressBus, Bus dataBus, ReadWriteWire readWrite, IMemoryBankSelector bankSelector)
    {
        _addressBus = addressBus;
        _dataBus = dataBus;
        _readWrite = readWrite;
        _bankSelector = bankSelector;

        _addressBus.Subscribe(BusCycleHandler);
    }

    /// <summary>
    /// The line the raster beam is on, 0-311.
    /// </summary>
    internal int RasterLine { get; private set; }

    /// <summary>
    /// The line written to <c>$D012</c> (bits 0-7) and bit 7 of <c>$D011</c> (bit 8).
    /// </summary>
    internal int RasterCompare { get; private set; }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="address"/> is in the VIC-II's part of the I/O area.
    /// </summary>
    internal static bool IsVicAddress(int address) => address >= FIRST_ADDRESS && address <= LAST_ADDRESS;

    /// <summary>
    /// Advances the beam by one φ2 cycle.
    /// </summary>
    internal void Clock()
    {
        if(++_cycle < CYCLES_PER_LINE)
            return;

        _cycle = 0;

        if(++RasterLine == LINES_PER_FRAME)
            RasterLine = 0;
    }

    private void BusCycleHandler()
    {
        // every bus cycle stands in for one φ2 cycle until there is a clock wire
        Clock();

        var address = _addressBus.Read();

        if(!IsVicAddress(address) || _bankSelector.SelectBank(address) != MemoryPla.IO)
            return;

        var register = address & (REGISTER_COUNT - 1);

        if(_readWrite.IsRead)
            _dataBus.Drive(ReadRegister(register));
        else
            WriteRegister(register, _dataBus.Read());
    }

    private int ReadRegister(int register) => register switch
    {
        CONTROL_1 => (_registers[CONTROL_1] & 0x7F) | ((RasterLine & 0x100) >> 1),
        RASTER => RasterLine & 0xFF,
        _ => _registers[register]
    };

    private void WriteRegister(int register, int value)
    {
        switch(register)
        {
            case CONTROL_1:
                _registers[CONTROL_1] = value;
                RasterCompare = (RasterCompare & 0xFF) | ((value & 0x80) << 1);
                break;
            case RASTER:
                RasterCompare = (RasterCompare & 0x100) | value;
                break;
            default:
                _registers[register] = value;
                break;
        }
    }
}
