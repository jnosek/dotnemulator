using Dotnemulator.Platforms.Commodore64.Architecture;

namespace Dotnemulator.Platforms.Commodore64.Test;

/// <summary>
/// The 6510 on-chip I/O port: data direction register at $0000, data register at $0001, pins on PortBus.
/// </summary>
[TestClass]
public class Port_Test
{
    private static Emulator Build()
    {
        var emulator = new EmulatorBuilder()
            .WithTestKernel([])
            .Build();

        emulator.Cpu.Initialize();

        return emulator;
    }

    [TestMethod]
    public void PortBus_IsSixBits()
    {
        var emulator = Build();

        Assert.AreEqual(MOS6510Cpu.PORT_PIN_COUNT, emulator.PortBus.Size);
        Assert.AreEqual(0b0011_1111, emulator.PortBus.Mask);
    }

    [TestMethod]
    public void Reset_AllPinsInputs_ReadHigh_DefaultMemoryConfiguration()
    {
        var emulator = Build();

        Assert.AreEqual(0x00, emulator.Cpu.Address0.Value);
        Assert.AreEqual(0x00, emulator.Cpu.Address1.Value);
        Assert.AreEqual(0b0011_1111, emulator.PortBus.Read());
        Assert.AreEqual(MemoryPla.BASIC_ROM, emulator.Pla.SelectBank(0xA000));
        Assert.AreEqual(MemoryPla.IO, emulator.Pla.SelectBank(0xD000));
        Assert.AreEqual(MemoryPla.KERNEL_ROM, emulator.Pla.SelectBank(0xE000));
    }

    [TestMethod]
    public void WriteData_WhilePinsAreInputs_DoesNotChangePins()
    {
        var emulator = Build();

        emulator.Cpu.Write(0x0001, 0x00);

        Assert.AreEqual(0b0011_1111, emulator.PortBus.Read());
        Assert.AreEqual(MemoryPla.BASIC_ROM, emulator.Pla.SelectBank(0xA000));
    }

    [TestMethod]
    public void KernalSetup_2F_37_GivesDefaultConfiguration()
    {
        var emulator = Build();

        emulator.Cpu.Write(0x0000, 0x2F);
        emulator.Cpu.Write(0x0001, 0x37);

        // outputs P0-P3, P5 from $37; P4 (cassette sense) is an input and reads high
        Assert.AreEqual(0b0011_0111, emulator.PortBus.Read());
        Assert.AreEqual(0x37, emulator.Cpu.Read(0x0001));
        Assert.AreEqual(0x2F, emulator.Cpu.Read(0x0000));
        Assert.AreEqual(MemoryPla.IO, emulator.Pla.SelectBank(0xD000));
    }

    [TestMethod]
    public void OutputPins_SwitchBanks()
    {
        var emulator = Build();
        emulator.Cpu.Write(0x0000, 0x2F);

        emulator.Cpu.Write(0x0001, 0x33);   // CHAREN low

        Assert.AreEqual(MemoryPla.CHAR_ROM, emulator.Pla.SelectBank(0xD000));

        emulator.Cpu.Write(0x0001, 0x30);   // all RAM

        Assert.AreEqual(MemoryPla.RAM, emulator.Pla.SelectBank(0xA000));
        Assert.AreEqual(MemoryPla.RAM, emulator.Pla.SelectBank(0xD000));
        Assert.AreEqual(MemoryPla.RAM, emulator.Pla.SelectBank(0xE000));
    }

    [TestMethod]
    public void WritingDirectionRegister_UpdatesPins()
    {
        var emulator = Build();
        emulator.Cpu.Write(0x0001, 0x30);   // stored, but pins are still inputs

        emulator.Cpu.Write(0x0000, 0x07);   // P0-P2 become outputs, driven low

        Assert.AreEqual(0b0011_1000, emulator.PortBus.Read());
        Assert.AreEqual(MemoryPla.RAM, emulator.Pla.SelectBank(0xE000));
    }

    [TestMethod]
    public void ReadData_InputBitsReadPins_OutputBitsReadRegister()
    {
        var emulator = Build();
        emulator.Cpu.Write(0x0000, 0b0000_0011);   // P0, P1 outputs
        emulator.Cpu.Write(0x0001, 0b1111_0000);   // P0, P1 low; other bits not outputs

        // P0-P1 from the register (0), P2-P5 pins pulled high, bits 6-7 not pins
        Assert.AreEqual(0b0011_1100, emulator.Cpu.Read(0x0001));
    }

    [TestMethod]
    public void EnablingOutputsBeforeSettingData_SwitchesRomsOut()
    {
        var emulator = Build();

        emulator.Cpu.Write(0x0000, 0x2F);   // $01 is still 0 from reset

        Assert.AreEqual(MemoryPla.RAM, emulator.Pla.SelectBank(0xE000));
    }

    [TestMethod]
    public void StaToPortRegister_SwitchesBanks()
    {
        var emulator = new EmulatorBuilder()
            .WithUnitTestMode()
            .WithTestKernel([
                // data register first, as the KERNAL does: enabling outputs while $01 is still 0
                // would switch the KERNAL ROM (and this program) out
                0xA9, 0x33, 0x85, 0x01,     // LDA #$33, STA $01
                0xA9, 0x2F, 0x85, 0x00,     // LDA #$2F, STA $00
                0xEA
            ])
            .Build();

        emulator.Start();

        Assert.AreEqual(MemoryPla.CHAR_ROM, emulator.Pla.SelectBank(0xD000));
    }
}
