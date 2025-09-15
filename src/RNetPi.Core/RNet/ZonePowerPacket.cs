using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Zone Power packet
/// </summary>
public class ZonePowerPacket : DataPacket
{
    public ZonePowerPacket()
    {
        MessageType = 0x00;
    }

    public byte GetControllerID()
    {
        return SourceControllerID;
    }

    public byte GetZoneID()
    {
        return SourcePath.Length > 2 ? SourcePath[2] : (byte)0;
    }

    public bool GetPower()
    {
        return Data.Length > 0 && Data[0] == 1;
    }

    /// <summary>
    /// Creates a ZonePowerPacket from a DataPacket
    /// </summary>
    public static ZonePowerPacket FromPacket(DataPacket dataPacket)
    {
        if (dataPacket.MessageType != 0x00)
        {
            throw new ArgumentException("Cannot create ZonePowerPacket from packet with MessageType != 0x00");
        }

        var zonePowerPacket = new ZonePowerPacket();
        dataPacket.CopyToPacket(zonePowerPacket);
        return zonePowerPacket;
    }
}