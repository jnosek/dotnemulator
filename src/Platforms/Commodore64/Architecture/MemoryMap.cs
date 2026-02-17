class MemoryMap
{
    private readonly MemorySegment _ram = new MemorySegment(0x1_0000);
    private readonly MemorySegment _rom = new MemorySegment(0x0_4000, 0x8000);
    private readonly MemorySegment _basicRom = new MemorySegment(0x_2000, 0xA000);
    private readonly MemorySegment _charRom = new MemorySegment(0x0_1000, 0xD000);
    private readonly MemorySegment _kernalRom = new MemorySegment(0x0_2000, 0xE000);
    private readonly MemorySegment _io = new MemorySegment(0x0_1000, 0xD000);

    public void Write(byte value, uint address)
    {
        
    }

    public void Read(byte value, uint address)
    {
        
    }
}