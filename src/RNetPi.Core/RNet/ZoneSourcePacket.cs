using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Zone Source packet
/// </summary>
public class ZoneSourcePacket : DataPacket
{
    public ZoneSourcePacket()
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

    public byte GetSourceID()
    {
        return Data.Length > 0 ? Data[0] : (byte)0;
    }

    /// <summary>
    /// Creates a ZoneSourcePacket from a DataPacket
    /// </summary>
    public static ZoneSourcePacket FromPacket(DataPacket dataPacket)
    {
        if (dataPacket.MessageType != 0x00)
        {
            throw new ArgumentException("Cannot create ZoneSourcePacket from packet with MessageType != 0x00");
        }

        var zoneSourcePacket = new ZoneSourcePacket();
        dataPacket.CopyToPacket(zoneSourcePacket);
        return zoneSourcePacket;
    }
}