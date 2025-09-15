using System;
using System.IO;

namespace RNetPi.Core.RNet;

/// <summary>
/// Base class for all RNet protocol packets
/// </summary>
public class RNetPacket
{
    // Protocol constants
    private const byte BYTE_START_MESSAGE = 0xF0;
    private const byte BYTE_END_MESSAGE = 0xF7;
    private const byte BYTE_INVERT_SIGNAL = 0xF1;

    // Public constants for common controller/keypad IDs
    public const byte CONTROLLER_ALL_KEYPADS = 0x7F;
    public const byte CONTROLLER_ALL = 0x7E;
    public const byte CONTROLLER_ALL_DEVICES = 0x7D;
    public const byte KEYPAD_CONTROLLER = 0x7F;
    public const byte KEYPAD_ALL_IN_ZONE = 0x7D;
    public const byte KEYPAD_ALL_ON_SOURCE = 0x79;

    // Packet header fields
    public byte TargetControllerID { get; set; } = 0x00;
    public byte TargetZoneID { get; set; } = 0x00;
    public byte TargetKeypadID { get; set; } = 0x7F;
    public byte SourceControllerID { get; set; } = 0x00;
    public byte SourceZoneID { get; set; } = 0x00;
    public byte SourceKeypadID { get; set; } = 0x70;
    public byte MessageType { get; set; }

    // Message body (raw data)
    public byte[]? MessageBody { get; set; }

    /// <summary>
    /// Gets the packet as a buffer ready for transmission
    /// </summary>
    public virtual byte[] GetBuffer()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(BYTE_START_MESSAGE);
        writer.Write(TargetControllerID);
        writer.Write(TargetZoneID);
        writer.Write(TargetKeypadID);
        writer.Write(SourceControllerID);
        writer.Write(SourceZoneID);
        writer.Write(SourceKeypadID);
        writer.Write(MessageType);

        var messageBody = GetMessageBody();
        writer.Write(messageBody);

        var checksum = CalculateChecksum(stream.ToArray());
        writer.Write(checksum);
        writer.Write(BYTE_END_MESSAGE);

        return stream.ToArray();
    }

    /// <summary>
    /// Override this method to provide the message body content
    /// </summary>
    protected virtual byte[] GetMessageBody()
    {
        return MessageBody ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Indicates if this packet requires handshake before sending
    /// </summary>
    public virtual bool RequiresHandshake()
    {
        return false;
    }

    /// <summary>
    /// Indicates if this packet causes a response that requires handshake
    /// </summary>
    public virtual bool CausesResponseWithHandshake()
    {
        return false;
    }

    /// <summary>
    /// Calculates the RNet protocol checksum
    /// </summary>
    private byte CalculateChecksum(byte[] buffer)
    {
        var totalBytes = buffer.Length;
        var byteSum = 0;

        foreach (var b in buffer)
        {
            byteSum += b;
        }

        byteSum += totalBytes;
        byteSum = byteSum & 0x007F;

        if (byteSum > 127)
        {
            // This shouldn't happen with proper masking, but log if it does
            Console.WriteLine("Warning: Checksum calculation resulted in value > 127");
        }

        return (byte)byteSum;
    }

    /// <summary>
    /// Writes a UInt16 value with RNet inversion if needed
    /// </summary>
    protected void WriteWithInvertUInt16LE(BinaryWriter writer, ushort value)
    {
        var b0 = (byte)(value & 0x00FF);
        var b1 = (byte)((value & 0xFF00) >> 8);

        if (b0 > 127)
        {
            writer.Write(BYTE_INVERT_SIGNAL);
            writer.Write((byte)(~b0 & 0xFF));
        }
        else
        {
            writer.Write(b0);
        }

        writer.Write(b1);
    }

    /// <summary>
    /// Writes a UInt8 value with RNet inversion if needed
    /// </summary>
    protected void WriteWithInvertUInt8(BinaryWriter writer, byte value)
    {
        if (value > 127)
        {
            writer.Write(BYTE_INVERT_SIGNAL);
            writer.Write((byte)(~value & 0xFF));
        }
        else
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Copies header fields to another packet of the same type
    /// </summary>
    public virtual void CopyToPacket(RNetPacket packet)
    {
        if (packet.MessageType != MessageType)
        {
            throw new ArgumentException("Cannot copy values to packet with different MessageType");
        }

        packet.TargetControllerID = TargetControllerID;
        packet.TargetZoneID = TargetZoneID;
        packet.TargetKeypadID = TargetKeypadID;
        packet.SourceControllerID = SourceControllerID;
        packet.SourceZoneID = SourceZoneID;
        packet.SourceKeypadID = SourceKeypadID;
    }

    /// <summary>
    /// Creates an RNetPacket from raw buffer data
    /// </summary>
    public static RNetPacket FromData(byte[] data)
    {
        if (data.Length < 10) // Minimum packet size
        {
            throw new ArgumentException("Data buffer too small for RNet packet");
        }

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        if (reader.ReadByte() != BYTE_START_MESSAGE)
        {
            throw new ArgumentException("RNetPacket data didn't begin with BYTE_START_MESSAGE");
        }

        var packet = new RNetPacket
        {
            TargetControllerID = reader.ReadByte(),
            TargetZoneID = reader.ReadByte(),
            TargetKeypadID = reader.ReadByte(),
            SourceControllerID = reader.ReadByte(),
            SourceZoneID = reader.ReadByte(),
            SourceKeypadID = reader.ReadByte(),
            MessageType = reader.ReadByte()
        };

        // Read message body (everything except start, header, checksum, and end)
        var bodyLength = data.Length - 10; // Total - start(1) - header(7) - checksum(1) - end(1)
        packet.MessageBody = reader.ReadBytes(bodyLength);

        var checksum = reader.ReadByte(); // TODO: Validate checksum
        
        if (reader.ReadByte() != BYTE_END_MESSAGE)
        {
            throw new ArgumentException("RNetPacket data didn't end with BYTE_END_MESSAGE");
        }

        return packet;
    }
}