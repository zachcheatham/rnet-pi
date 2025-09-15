using System;
using System.IO;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Handshake packet (MessageType = 0x02)
/// </summary>
public class HandshakePacket : RNetPacket
{
    public byte HandshakeType { get; set; }

    public HandshakePacket() : this(0, 0)
    {
    }

    public HandshakePacket(byte controllerID, byte handshakeType)
    {
        MessageType = 0x02;
        TargetControllerID = controllerID;
        HandshakeType = handshakeType;
    }

    protected override byte[] GetMessageBody()
    {
        return new byte[] { HandshakeType };
    }

    /// <summary>
    /// Creates a HandshakePacket from an RNetPacket
    /// </summary>
    public static HandshakePacket FromPacket(RNetPacket rnetPacket)
    {
        if (rnetPacket.MessageType != 0x02)
        {
            throw new ArgumentException("Cannot create HandshakePacket from packet with MessageType != 0x02");
        }

        var handshakePacket = new HandshakePacket();
        rnetPacket.CopyToPacket(handshakePacket);

        if (rnetPacket.MessageBody != null && rnetPacket.MessageBody.Length > 0)
        {
            handshakePacket.HandshakeType = rnetPacket.MessageBody[0];
        }

        return handshakePacket;
    }
}