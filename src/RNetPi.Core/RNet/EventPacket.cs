using System;
using System.IO;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Event packet (MessageType = 0x05)
/// </summary>
public class EventPacket : RNetPacket
{
    public byte[] TargetPath { get; set; } = Array.Empty<byte>();
    public byte[] SourcePath { get; set; } = Array.Empty<byte>();
    public ushort EventID { get; set; }
    public ushort EventTimestamp { get; set; }
    public ushort EventData { get; set; }
    public byte EventPriority { get; set; }

    public EventPacket()
    {
        MessageType = 0x05;
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

        // Write event fields with inversion handling
        WriteWithInvertUInt16LE(writer, EventID);
        WriteWithInvertUInt16LE(writer, EventTimestamp);
        WriteWithInvertUInt16LE(writer, EventData);
        WriteWithInvertUInt8(writer, EventPriority);

        return stream.ToArray();
    }

    public override void CopyToPacket(RNetPacket packet)
    {
        base.CopyToPacket(packet);

        if (packet is EventPacket eventPacket)
        {
            eventPacket.TargetPath = TargetPath;
            eventPacket.SourcePath = SourcePath;
            eventPacket.EventID = EventID;
            eventPacket.EventTimestamp = EventTimestamp;
            eventPacket.EventData = EventData;
            eventPacket.EventPriority = EventPriority;
        }
        else
        {
            throw new ArgumentException("Cannot copy EventPacket properties to non-EventPacket");
        }
    }

    /// <summary>
    /// Creates an EventPacket from an RNetPacket
    /// </summary>
    public static EventPacket FromPacket(RNetPacket rnetPacket)
    {
        if (rnetPacket.MessageType != 0x05)
        {
            throw new ArgumentException("Cannot create EventPacket from packet with MessageType != 0x05");
        }

        var eventPacket = new EventPacket();
        rnetPacket.CopyToPacket(eventPacket);

        if (rnetPacket.MessageBody == null || rnetPacket.MessageBody.Length == 0)
        {
            return eventPacket;
        }

        using var stream = new MemoryStream(rnetPacket.MessageBody);
        using var reader = new BinaryReader(stream);

        try
        {
            // Read target path
            var targetPathLength = reader.ReadByte();
            eventPacket.TargetPath = reader.ReadBytes(targetPathLength);

            // Read source path
            var sourcePathLength = reader.ReadByte();
            eventPacket.SourcePath = reader.ReadBytes(sourcePathLength);

            // Read event fields (little endian)
            eventPacket.EventID = reader.ReadUInt16();
            eventPacket.EventTimestamp = reader.ReadUInt16();
            eventPacket.EventData = reader.ReadUInt16();
            eventPacket.EventPriority = reader.ReadByte();
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("Warning: Incomplete EventPacket - reached end of stream while parsing");
        }

        return eventPacket;
    }
}