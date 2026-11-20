namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

using System.Diagnostics;
using Dotnemulator.Platforms.Commodore64.Architecture;

/// <summary>
/// Watches the KERNAL's reset sequence and logs each stage as the CPU reaches it, then detaches.
/// </summary>
/// <remarks>
/// <para>
/// A stage is recognised when PC reaches its entry address in the rev 3 KERNAL (901227-03), checked between
/// instructions, so it is exact: a data read of the same address never counts. Results that are only known once
/// a stage has finished (the RAM test's top of memory, the PAL/NTSC flag) are read from RAM with
/// <see cref="MemoryMap.PeekRam"/> when a later address is reached, so watching adds no bus cycles.
/// </para>
/// <para>
/// Watching ends, and <see cref="Finished"/> is called once, when the boot completes (keyboard wait loop at
/// <c>$E5CD</c>), when the KERNAL hands over to an autostart cartridge, or when <c>stepLimit</c> steps have run
/// without either (a stall). The execution strategy then stops calling this observer.
/// </para>
/// </remarks>
internal sealed class BootMonitor : IExecutionObserver
{
    /// <summary>
    /// About three times the ~600,000 steps a normal boot takes.
    /// </summary>
    public const long DEFAULT_STEP_LIMIT = 2_000_000;

    public const int RESET = 0xFCE2;
    public const int CARTRIDGE_AUTOSTART = 0xFCEC;
    public const int NO_CARTRIDGE = 0xFCEF;
    public const int IOINIT = 0xFDA3;
    public const int RAMTAS = 0xFD50;
    public const int RESTOR = 0xFD15;
    public const int CINT = 0xFF5B;
    public const int RASTER_WAIT_DONE = 0xFF63;
    public const int VIDEO_STANDARD_SET = 0xFF6B;
    public const int BASIC_COLD_START = 0xE394;
    public const int PRINT_BANNER = 0xE422;
    public const int KEYBOARD_WAIT = 0xE5CD;

    private const int CARTRIDGE_VECTOR = 0x8000;
    private const int MEMORY_TOP = 0x0283;
    private const int PAL_NTSC_FLAG = 0x02A6;
    private const int RAM_TEST_START = 0x0400;

    private readonly MemoryMap _memoryMap;
    private readonly Action<string> _log;
    private readonly long _stepLimit;
    private readonly Dictionary<int, Func<MOS6510Cpu, string>> _stages;
    private readonly List<int> _stagesReached = new();
    private readonly Stopwatch _stopwatch = new();

    private long _steps;
    private long _cintStep;

    /// <param name="memoryMap">Read with <see cref="MemoryMap.PeekRam"/> for stage results.</param>
    /// <param name="log">Receives one line per stage.</param>
    /// <param name="stepLimit">Steps allowed before the boot is reported as stalled.</param>
    public BootMonitor(MemoryMap memoryMap, Action<string> log, long stepLimit = DEFAULT_STEP_LIMIT)
    {
        _memoryMap = memoryMap;
        _log = log;
        _stepLimit = stepLimit;

        _stages = new()
        {
            [RESET] = _ => "Reset",
            [CARTRIDGE_AUTOSTART] = _ => $"Cartridge signature CBM80 found: autostart through ($8000) to ${ReadWord(CARTRIDGE_VECTOR):X4}",
            [NO_CARTRIDGE] = _ => "No cartridge signature at $8004",
            [IOINIT] = _ => "IOINIT: CIA, SID and I/O port setup",
            [RAMTAS] = _ => "RAMTAS: clearing pages 0, 2 and 3, testing RAM from $0400",
            [RESTOR] = _ => $"RAM test done: RAM ends at ${ReadWord(MEMORY_TOP):X4} ({ReadWord(MEMORY_TOP) - RAM_TEST_START:N0} bytes from ${RAM_TEST_START:X4}); RESTOR: default vectors",
            [CINT] = _ =>
            {
                _cintStep = _steps;
                return "CINT: VIC-II and screen setup";
            },
            [RASTER_WAIT_DONE] = _ => $"Raster wait done ({_steps - _cintStep:N0} steps since CINT)",
            [VIDEO_STANDARD_SET] = _ => $"Video standard: {(_memoryMap.PeekRam(PAL_NTSC_FLAG) == 0 ? "NTSC" : "PAL")}",
            [BASIC_COLD_START] = _ => "BASIC cold start",
            [PRINT_BANNER] = _ => "Printing banner",
            [KEYBOARD_WAIT] = _ => "READY.: waiting for keyboard input",
        };
    }

    /// <summary>
    /// Called once when watching ends.
    /// </summary>
    public Action<BootResult>? Finished { get; init; }

    /// <summary>
    /// How the boot ended, or <c>null</c> while it is still being watched.
    /// </summary>
    public BootResult? Result { get; private set; }

    /// <summary>
    /// The stage addresses reached, in order, each once.
    /// </summary>
    public IReadOnlyList<int> StagesReached => _stagesReached;

    public ObserverAction BeforeStep(MOS6510Cpu cpu)
    {
        if(Result != null)
            return ObserverAction.Detach;

        if(_steps == 0)
            _stopwatch.Start();

        var address = cpu.PC.Value;

        if(_stages.TryGetValue(address, out var describe) && !_stagesReached.Contains(address))
        {
            _stagesReached.Add(address);
            Log(address, describe(cpu));

            if(address == KEYBOARD_WAIT)
                return Finish(BootOutcome.Completed, address);

            if(address == CARTRIDGE_AUTOSTART)
                return Finish(BootOutcome.CartridgeAutostart, address);
        }

        if(_steps >= _stepLimit)
        {
            var lastStage = _stagesReached.Count > 0 ? $"last stage reached ${_stagesReached[^1]:X4}" : "no stage reached";
            Log(address, $"Boot stalled: {_steps:N0} steps without completing, {lastStage}");
            return Finish(BootOutcome.Stalled, address);
        }

        _steps++;
        return ObserverAction.Continue;
    }

    private ObserverAction Finish(BootOutcome outcome, int address)
    {
        _stopwatch.Stop();
        Result = new BootResult(outcome, _steps, _stopwatch.Elapsed, address);

        if(outcome == BootOutcome.Completed)
            Log(address, $"Boot complete: {_steps:N0} steps in {_stopwatch.Elapsed.TotalMilliseconds:N0} ms");

        Finished?.Invoke(Result);
        return ObserverAction.Detach;
    }

    private void Log(int address, string text) => _log($"boot {_steps,11:N0}  ${address:X4}  {text}");

    private int ReadWord(int address) => _memoryMap.PeekRam(address) | (_memoryMap.PeekRam(address + 1) << 8);
}
