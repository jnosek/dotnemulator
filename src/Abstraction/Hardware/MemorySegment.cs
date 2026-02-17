public class MemorySegment
{
    private readonly byte[] _bytes;
    private readonly int _startingAddress;

    public MemorySegment(int size, int startingAddress = 0)
    {
        _bytes = new byte[size];
        _startingAddress = startingAddress;
    }

    private MemorySegment(int startingAddress, byte[] bytes)
    {
        _bytes = bytes;
        _startingAddress = startingAddress;
    }

    public MemorySegment CreateSegment(int size, int offset = 0)
    {
        return new MemorySegment(_startingAddress + offset, _bytes.AsSpan(offset, size).ToArray());
    }
}