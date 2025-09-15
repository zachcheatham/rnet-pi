using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Packet for setting zone power
/// </summary>
public class SetPowerPacket : EventPacket
{
    public SetPowerPacket(byte controllerID, byte zoneID, bool power)
    {
        TargetPath = new byte[] { 0x02, 0x00 };
        TargetControllerID = controllerID;
        EventID = power ? (ushort)0xDD : (ushort)0xDC;
        EventTimestamp = 0;
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

    public bool GetPower()
    {
        return EventID == 0xDD;
    }
}