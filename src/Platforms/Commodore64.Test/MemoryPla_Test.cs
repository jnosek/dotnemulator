using Dotnemulator.Abstraction.Hardware;
using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64.Test;

[TestClass]
public class MemoryPla_Test
{
    private const byte RAM = MemoryPla.RAM;
    private const byte BASIC = MemoryPla.BASIC_ROM;
    private const byte KERNAL = MemoryPla.KERNEL_ROM;
    private const byte CHAR = MemoryPla.CHAR_ROM;
    private const byte IO = MemoryPla.IO;

    /// <summary>
    /// Expected bank per port setting (LORAM/HIRAM/CHAREN) for the regions
    /// $0000, $1000, $8000, $A000, $C000, $D000, $E000.
    /// </summary>
    private static readonly byte[][] EXPECTED = [
        /* 000 */ [ RAM, RAM, RAM, RAM,   RAM, RAM,  RAM    ],
        /* 001 */ [ RAM, RAM, RAM, RAM,   RAM, CHAR, RAM    ],
        /* 010 */ [ RAM, RAM, RAM, RAM,   RAM, CHAR, KERNAL ],
        /* 011 */ [ RAM, RAM, RAM, BASIC, RAM, CHAR, KERNAL ],
        /* 100 */ [ RAM, RAM, RAM, RAM,   RAM, RAM,  RAM    ],
        /* 101 */ [ RAM, RAM, RAM, RAM,   RAM, IO,   RAM    ],
        /* 110 */ [ RAM, RAM, RAM, RAM,   RAM, IO,   KERNAL ],
        /* 111 */ [ RAM, RAM, RAM, BASIC, RAM, IO,   KERNAL ],
    ];

    private static MemoryPla NewPla(Bus port) => new MemoryPla(port, new Wire(), new Wire());

    private static int Region(int address) =>
        address < 0x1000 ? 0 :
        address < 0x8000 ? 1 :
        address < 0xA000 ? 2 :
        address < 0xC000 ? 3 :
        address < 0xD000 ? 4 :
        address < 0xE000 ? 5 :
        6;

    [TestMethod]
    public void SelectBank_MatchesMemoryMap_ForEveryPortSettingAndAddress()
    {
        var port = new Bus(8);
        var pla = NewPla(port);

        for(int setting = 0; setting < 8; setting++)
        {
            port.Strobe(setting);

            for(int address = 0; address <= 0xFFFF; address++)
            {
                var expected = EXPECTED[setting][Region(address)];
                var actual = pla.SelectBank(address);

                if(expected != actual)
                    Assert.Fail($"Port {setting:b3}, address ${address:X4}: expected {expected}, got {actual}");
            }
        }
    }

    [TestMethod]
    [DataRow(0b111, 0x9FFF, RAM)]
    [DataRow(0b111, 0xA000, BASIC)]
    [DataRow(0b111, 0xBFFF, BASIC)]
    [DataRow(0b111, 0xC000, RAM)]
    [DataRow(0b111, 0xD000, IO)]
    [DataRow(0b111, 0xE000, KERNAL)]
    [DataRow(0b111, 0xFFFF, KERNAL)]
    [DataRow(0b011, 0xD000, CHAR)]
    [DataRow(0b000, 0xFFFF, RAM)]
    public void SelectBank_Boundaries(int setting, int address, int expected)
    {
        var port = new Bus(8);
        port.Drive(setting);
        var pla = NewPla(port);

        Assert.AreEqual(expected, pla.SelectBank(address));
    }

    [TestMethod]
    public void UsesPortValue_PresentAtConstruction()
    {
        var port = new Bus(8);
        port.Drive(0b111);

        var pla = NewPla(port);

        Assert.AreEqual(BASIC, pla.SelectBank(0xA000));
    }

    [TestMethod]
    public void PortStrobe_ChangesSelection()
    {
        var port = new Bus(8);
        var pla = NewPla(port);

        port.Strobe(0b111);
        Assert.AreEqual(IO, pla.SelectBank(0xD000));

        port.Strobe(0b011);
        Assert.AreEqual(CHAR, pla.SelectBank(0xD000));
    }

    [TestMethod]
    public void OnlyLowThreePortBitsSelectConfiguration()
    {
        var port = new Bus(8);
        var pla = NewPla(port);

        port.Strobe(0b1111_1000 | 0b011);

        Assert.AreEqual(CHAR, pla.SelectBank(0xD000));
    }
}
