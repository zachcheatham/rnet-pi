using System;
using System.IO;

namespace RNetPi.Core.RNet;

/// <summary>
/// Packet for setting zone volume
/// </summary>
public class SetVolumePacket : EventPacket
{
    public SetVolumePacket(byte controllerID, byte zoneID, int volume)
    {
        if (volume < 0 || volume > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be between 0 and 100");
        }

        TargetPath = new byte[] { 0x02, 0x00 };
        TargetControllerID = controllerID;
        EventID = 0xDE;
        EventTimestamp = (ushort)(volume / 2); // Translate range 0-100 to 0-50
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

    public int GetVolume()
    {
        return EventTimestamp * 2;
    }
}