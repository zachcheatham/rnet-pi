using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Zone Parameter packet
/// </summary>
public class ZoneParameterPacket : DataPacket
{
    public ZoneParameterPacket()
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

    public byte GetParameterID()
    {
        return SourcePath.Length > 4 ? SourcePath[4] : (byte)0;
    }

    public byte GetParameterValue()
    {
        return Data.Length > 0 ? Data[0] : (byte)0;
    }

    /// <summary>
    /// Creates a ZoneParameterPacket from a DataPacket
    /// </summary>
    public static ZoneParameterPacket FromPacket(DataPacket dataPacket)
    {
        if (dataPacket.MessageType != 0x00)
        {
            throw new ArgumentException("Cannot create ZoneParameterPacket from packet with MessageType != 0x00");
        }

        var zoneParameterPacket = new ZoneParameterPacket();
        dataPacket.CopyToPacket(zoneParameterPacket);
        return zoneParameterPacket;
    }
}