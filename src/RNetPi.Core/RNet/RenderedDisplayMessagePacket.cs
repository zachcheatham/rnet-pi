using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents a rendered display message packet (MessageType = 0x06)
/// </summary>
public class RenderedDisplayMessagePacket : RNetPacket
{
    public byte[] DisplayData { get; set; } = Array.Empty<byte>();

    public RenderedDisplayMessagePacket()
    {
        MessageType = 0x06;
    }

    protected override byte[] GetMessageBody()
    {
        return DisplayData;
    }

    public byte GetControllerID()
    {
        return SourceControllerID;
    }

    public byte GetZoneID()
    {
        return SourceZoneID;
    }

    public byte GetKeypadID()
    {
        return SourceKeypadID;
    }

    /// <summary>
    /// Creates a RenderedDisplayMessagePacket from an RNetPacket
    /// </summary>
    public static RenderedDisplayMessagePacket FromPacket(RNetPacket rnetPacket)
    {
        if (rnetPacket.MessageType != 0x06)
        {
            throw new ArgumentException("Cannot create RenderedDisplayMessagePacket from packet with MessageType != 0x06");
        }

        var displayMessagePacket = new RenderedDisplayMessagePacket();
        rnetPacket.CopyToPacket(displayMessagePacket);

        if (rnetPacket.MessageBody != null)
        {
            displayMessagePacket.DisplayData = rnetPacket.MessageBody;
        }

        return displayMessagePacket;
    }
}