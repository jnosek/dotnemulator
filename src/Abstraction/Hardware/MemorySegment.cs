using System.Diagnostics;

namespace Dotnemulator.Abstraction.Hardware;

public class MemorySegment
{
    private readonly byte[] _bytes;
    private readonly int _startingAddress;
    private readonly int _endingAddress;

    public MemorySegment(int size, int startingAddress = 0)
    {
        _bytes = new byte[size];
        _startingAddress = startingAddress;
        _endingAddress = startingAddress + size - 1;
    }

    private MemorySegment(int startingAddress, byte[] bytes)
    {
        _bytes = bytes;
        _startingAddress = startingAddress;
        _endingAddress = startingAddress + bytes.Length - 1;
    }

    public MemorySegment CreateSegment(int size, int offset = 0)
    {
        return new MemorySegment(_startingAddress + offset, _bytes.AsSpan(offset, size).ToArray());
    }

    [Conditional("DEBUG")]
    private void AddressCheck(int address)
    {
        if (address < _startingAddress || address > _endingAddress)
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address:X4} is out of range for this memory segment.");
    }

    public int Read(int address)
    {
        AddressCheck(address);
        return _bytes[address - _startingAddress];
    }

    public void Write(int address, int value)
    {
        AddressCheck(address);
        _bytes[address - _startingAddress] = (byte)(value & 0xFF);
    }

    /// <summary>
    /// Loads the memory segment with data from the provided stream. 
    /// The stream should contain at least as many bytes as the size of the memory segment, otherwise the remaining bytes will be left as 0. 
    /// The stream will be read sequentially until either the end of the stream is reached or the memory segment is fully loaded.
    /// </summary>
    /// <param name="stream"></param>
    public void Load(Stream stream)
    {
        Array.Clear(_bytes);

        int offset = 0;
        int bytesRead;
        while (offset < _bytes.Length && (bytesRead = stream.Read(_bytes, offset, _bytes.Length - offset)) > 0)
        {
            offset += bytesRead;
        }
    }

    /// <summary>
    /// Get a SHA256 Hash of the memory segment contents
    /// </summary>
    /// <returns></returns>
    public byte[] GetHash()
    {
        using var hashAlgorithm = System.Security.Cryptography.SHA256.Create();
        return hashAlgorithm.ComputeHash(_bytes);
    }
}