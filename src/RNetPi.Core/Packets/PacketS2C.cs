using System.IO;
using System.Text;

namespace RNetPi.Core.Packets;

public abstract class PacketS2C
{
    protected MemoryStream Stream { get; }
    protected BinaryWriter Writer { get; }

    protected PacketS2C()
    {
        Stream = new MemoryStream();
        Writer = new BinaryWriter(Stream);
        Writer.Write(GetID());
        Writer.Write((byte)0); // Placeholder for length
    }

    public abstract byte GetID();

    /// <summary>
    /// Writes a null-terminated string to the stream
    /// </summary>
    /// <param name="value">The string to write (null will be written as empty string)</param>
    protected void WriteNullTerminatedString(string? value)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        Writer.Write(bytes);
        Writer.Write((byte)0); // null terminator
    }

    public virtual byte[] GetBuffer()
    {
        // Write packet length at position 1 (after packet ID)
        var position = Stream.Position;
        Stream.Position = 1;
        Writer.Write((byte)(position - 2)); // Length excludes ID and length byte itself
        Stream.Position = position;
        
        return Stream.ToArray();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Writer?.Dispose();
            Stream?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }
}