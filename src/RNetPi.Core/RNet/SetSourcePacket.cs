using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Packet for setting zone source
/// </summary>
public class SetSourcePacket : EventPacket
{
    public SetSourcePacket(byte controllerID, byte zoneID, byte sourceID)
    {
        TargetPath = new byte[] { 0x02, 0x00 };
        TargetControllerID = controllerID;
        EventID = 0xDF;
        EventTimestamp = sourceID;
        EventData = zoneID;
        EventPriority = 1;
    }

    public byte GetControllerID()
    {
        return TargetControllerID;
    }

    public byte GetZoneID()
    {
        return (byte)EventData;
    }

    public byte GetSourceID()
    {
        return (byte)EventTimestamp;
    }
}