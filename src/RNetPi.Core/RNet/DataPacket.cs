using System;
using System.IO;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Data packet (MessageType = 0x00)
/// </summary>
public class DataPacket : RNetPacket
{
    public byte[] TargetPath { get; set; } = Array.Empty<byte>();
    public byte[] SourcePath { get; set; } = Array.Empty<byte>();
    public ushort PacketNumber { get; set; } = 0;
    public ushort PacketCount { get; set; } = 1;
    public byte[] Data { get; set; } = Array.Empty<byte>();

    public DataPacket()
    {
        MessageType = 0x00;
    }

    protected override byte[] GetMessageBody()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Write target path
        writer.Write((byte)TargetPath.Length);
        writer.Write(TargetPath);

        // Write source path
        writer.Write((byte)SourcePath.Length);
        writer.Write(SourcePath);

        // Write packet number and count (little endian)
        writer.Write(PacketNumber);
        writer.Write(PacketCount);

        // Write data length and data
        writer.Write((ushort)Data.Length);
        writer.Write(Data);

        return stream.ToArray();
    }

    public override void CopyToPacket(RNetPacket packet)
    {
        base.CopyToPacket(packet);

        if (packet is DataPacket dataPacket)
        {
            dataPacket.TargetPath = TargetPath;
            dataPacket.SourcePath = SourcePath;
            dataPacket.PacketNumber = PacketNumber;
            dataPacket.PacketCount = PacketCount;
            dataPacket.Data = Data;
        }
        else
        {
            throw new ArgumentException("Cannot copy DataPacket properties to non-DataPacket");
        }
    }

    /// <summary>
    /// Creates a DataPacket from an RNetPacket
    /// </summary>
    public static DataPacket FromPacket(RNetPacket rnetPacket)
    {
        if (rnetPacket.MessageType != 0x00)
        {
            throw new ArgumentException("Cannot create DataPacket from packet with MessageType != 0x00");
        }

        var dataPacket = new DataPacket();
        rnetPacket.CopyToPacket(dataPacket);

        if (rnetPacket.MessageBody == null || rnetPacket.MessageBody.Length == 0)
        {
            return dataPacket;
        }

        using var stream = new MemoryStream(rnetPacket.MessageBody);
        using var reader = new BinaryReader(stream);

        try
        {
            // Read target path
            var targetPathLength = reader.ReadByte();
            dataPacket.TargetPath = reader.ReadBytes(targetPathLength);

            // Read source path
            var sourcePathLength = reader.ReadByte();
            dataPacket.SourcePath = reader.ReadBytes(sourcePathLength);

            // Read packet number and count
            dataPacket.PacketNumber = reader.ReadUInt16();
            dataPacket.PacketCount = reader.ReadUInt16();

            // Read data length and data
            var dataLength = reader.ReadUInt16();
            dataPacket.Data = reader.ReadBytes((int)dataLength);

            if (dataPacket.Data.Length != dataLength)
            {
                Console.WriteLine($"Warning: Data length doesn't match specified length. Expected: {dataLength}, Actual: {dataPacket.Data.Length}");
            }
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("Warning: Incomplete DataPacket - reached end of stream while parsing");
        }

        return dataPacket;
    }
}