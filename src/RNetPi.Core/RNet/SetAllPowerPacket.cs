using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Packet for setting all zones power
/// </summary>
public class SetAllPowerPacket : EventPacket
{
    public SetAllPowerPacket(bool power)
    {
        TargetPath = new byte[] { 0x02, 0x00 };
        TargetControllerID = CONTROLLER_ALL;
        TargetZoneID = CONTROLLER_ALL;
        EventID = power ? (ushort)0xDD : (ushort)0xDC;
        EventTimestamp = 0;
        EventData = 0;
        EventPriority = 1;
    }

    public bool GetPower()
    {
        return EventID == 0xDD;
    }
}