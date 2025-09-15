using System.IO;

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
    }

    public abstract byte GetID();

    public virtual byte[] GetBuffer()
    {
        // Write packet length at position 1 (after packet ID)
        var position = Stream.Position;
        Stream.Position = 1;
        Writer.Write((byte)(position - 1));
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