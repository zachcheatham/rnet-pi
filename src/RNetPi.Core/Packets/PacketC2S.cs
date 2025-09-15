using System.IO;
using System.Text;

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

    /// <summary>
    /// Reads a null-terminated string from the stream
    /// </summary>
    /// <returns>The string without the null terminator</returns>
    protected string ReadNullTerminatedString()
    {
        var bytes = new List<byte>();
        byte b;
        while ((b = Reader.ReadByte()) != 0)
        {
            bytes.Add(b);
        }
        return Encoding.UTF8.GetString(bytes.ToArray());
    }

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