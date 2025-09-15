using System;
using System.Text;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents a display message packet (MessageType = 0x04)
/// </summary>
public class DisplayMessagePacket : RNetPacket
{
    public string Message { get; set; } = string.Empty;
    public byte DisplayTime { get; set; } = 5; // Default 5 seconds

    public DisplayMessagePacket()
    {
        MessageType = 0x04;
    }

    public DisplayMessagePacket(byte controllerID, byte zoneID, string message, byte displayTime = 5)
        : this()
    {
        TargetControllerID = controllerID;
        TargetZoneID = zoneID;
        Message = message ?? string.Empty;
        DisplayTime = displayTime;
    }

    protected override byte[] GetMessageBody()
    {
        var messageBytes = Encoding.UTF8.GetBytes(Message);
        var body = new byte[messageBytes.Length + 2]; // +2 for display time and null terminator
        
        body[0] = DisplayTime;
        Array.Copy(messageBytes, 0, body, 1, messageBytes.Length);
        body[^1] = 0; // Null terminator
        
        return body;
    }

    public byte GetControllerID()
    {
        return TargetControllerID;
    }

    public byte GetZoneID()
    {
        return TargetZoneID;
    }

    /// <summary>
    /// Creates a DisplayMessagePacket from an RNetPacket
    /// </summary>
    public static DisplayMessagePacket FromPacket(RNetPacket rnetPacket)
    {
        if (rnetPacket.MessageType != 0x04)
        {
            throw new ArgumentException("Cannot create DisplayMessagePacket from packet with MessageType != 0x04");
        }

        var displayMessagePacket = new DisplayMessagePacket();
        rnetPacket.CopyToPacket(displayMessagePacket);

        if (rnetPacket.MessageBody != null && rnetPacket.MessageBody.Length > 0)
        {
            displayMessagePacket.DisplayTime = rnetPacket.MessageBody[0];
            
            if (rnetPacket.MessageBody.Length > 1)
            {
                var messageLength = Array.IndexOf(rnetPacket.MessageBody, (byte)0, 1);
                if (messageLength == -1)
                    messageLength = rnetPacket.MessageBody.Length - 1;
                else
                    messageLength -= 1;

                if (messageLength > 0)
                {
                    displayMessagePacket.Message = Encoding.UTF8.GetString(rnetPacket.MessageBody, 1, messageLength);
                }
            }
        }

        return displayMessagePacket;
    }
}