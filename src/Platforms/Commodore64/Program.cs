using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dotnemulator.Platforms.Commodore64.Test")]
[assembly: InternalsVisibleTo("Dotnemulator.Platforms.Commodore64.FunctionalTest")]

namespace Dotnemulator.Platforms.Commodore64;

using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Debug.WriteLine("Commodore64 Emulator Starting...");

        var emulator = new EmulatorBuilder().Build();
        emulator.Start();
    }
}

