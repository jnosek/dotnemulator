using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dotnemulator.Platforms.Commodore64.Test")]
[assembly: InternalsVisibleTo("Dotnemulator.Platforms.Commodore64.FunctionalTest")]

namespace Dotnemulator.Platforms.Commodore64;

using System.Diagnostics;
using Dotnemulator.Platforms.Commodore64.Display;

class Program
{
    static void Main(string[] args)
    {
        Debug.WriteLine("Commodore64 Emulator Starting...");

        var emulator = new EmulatorBuilder()
            .WithRoms()
            .Build();

        // no video yet: show the text screen in the console, redrawn in place
        using var renderer = new ConsoleScreenRenderer(
            new TextScreen(emulator.MemoryMap),
            () => $"PC ${emulator.Cpu.PC.Value:X4}  (Ctrl+C to quit)");

        Console.CancelKeyPress += (_, _) => renderer.Dispose();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => renderer.Dispose();

        renderer.Start();
        emulator.Start();
    }
}
