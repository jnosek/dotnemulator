using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dotnemulator.Platforms.Commodore64.Test")]
[assembly: InternalsVisibleTo("Dotnemulator.Platforms.Commodore64.FunctionalTest")]

namespace Dotnemulator.Platforms.Commodore64;

using System.Diagnostics;
using Dotnemulator.Platforms.Commodore64.Diagnostics;
using Dotnemulator.Platforms.Commodore64.Display;

class Program
{
    static void Main(string[] args)
    {
        Debug.WriteLine("Commodore64 Emulator Starting...");

        ConsoleScreenRenderer? renderer = null;

        // log each boot stage to the console; once the boot ends (or stalls), show the screen below the log
        var emulator = new EmulatorBuilder()
            .WithRoms()
            .WithExecutionObserver(e => new BootMonitor(e.MemoryMap, Console.WriteLine)
            {
                Finished = _ =>
                {
                    Console.WriteLine();
                    renderer?.Start();
                }
            })
            .Build();

        // no video yet: show the text screen in the console, redrawn in place
        renderer = new ConsoleScreenRenderer(
            new TextScreen(emulator.MemoryMap),
            () => $"PC ${emulator.Cpu.PC.Value:X4}  (Ctrl+C to quit)")
        {
            ClearOnStart = false
        };

        Console.CancelKeyPress += (_, _) => renderer.Dispose();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => renderer.Dispose();

        try
        {
            emulator.Start();
        }
        finally
        {
            renderer.Dispose();
        }
    }
}
