using System.IO;

namespace RNetPi.Core.Packets;

public abstract class PacketC2S
{
    protected BinaryReader Reader { get; }

    protected PacketC2S(byte[] data)
    {
        Reader = new BinaryReader(new MemoryStream(data));
        ParseData();
    }

    public abstract byte GetID();
    protected abstract void ParseData();

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Reader?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }
}