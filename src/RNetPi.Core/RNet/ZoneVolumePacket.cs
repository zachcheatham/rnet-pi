using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Zone Volume packet
/// </summary>
public class ZoneVolumePacket : DataPacket
{
    public ZoneVolumePacket()
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

    public int GetVolume()
    {
        return Data.Length > 0 ? Data[0] * 2 : 0;
    }

    /// <summary>
    /// Creates a ZoneVolumePacket from a DataPacket
    /// </summary>
    public static ZoneVolumePacket FromPacket(DataPacket dataPacket)
    {
        if (dataPacket.MessageType != 0x00)
        {
            throw new ArgumentException("Cannot create ZoneVolumePacket from packet with MessageType != 0x00");
        }

        var zoneVolumePacket = new ZoneVolumePacket();
        dataPacket.CopyToPacket(zoneVolumePacket);
        return zoneVolumePacket;
    }
}