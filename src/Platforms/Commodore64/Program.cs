namespace Dotnemulator.Platforms.Commodore64;

using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Debug.WriteLine("Commodore64 Emulator Starting...");

        var emulator = new Emulator();
        emulator.Start();
    }
}

