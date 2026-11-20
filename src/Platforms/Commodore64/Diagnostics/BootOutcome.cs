namespace Dotnemulator.Platforms.Commodore64.Diagnostics;

/// <summary>
/// How the boot sequence watched by <see cref="BootMonitor"/> ended.
/// </summary>
internal enum BootOutcome
{
    /// <summary>The KERNAL reached its keyboard wait loop after printing <c>READY.</c>.</summary>
    Completed,

    /// <summary>The KERNAL found a cartridge signature and handed control to the cartridge.</summary>
    CartridgeAutostart,

    /// <summary>The step limit ran out before the boot completed.</summary>
    Stalled
}

/// <summary>
/// The result of a watched boot.
/// </summary>
/// <param name="Outcome">How the boot ended.</param>
/// <param name="Steps">CPU steps from reset until it ended.</param>
/// <param name="Elapsed">Wall-clock time from reset until it ended.</param>
/// <param name="Address">PC when it ended.</param>
internal record BootResult(BootOutcome Outcome, long Steps, TimeSpan Elapsed, int Address);
