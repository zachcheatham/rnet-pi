using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents an RNet Zone Info packet containing comprehensive zone information
/// </summary>
public class ZoneInfoPacket : DataPacket
{
    public ZoneInfoPacket()
    {
        MessageType = 0x00;
    }

    public override bool RequiresHandshake()
    {
        return true;
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

    public byte GetSourceID()
    {
        return Data.Length > 1 ? Data[1] : (byte)0;
    }

    public int GetVolume()
    {
        return Data.Length > 2 ? Data[2] * 2 : 0;
    }

    public int GetBassLevel()
    {
        return Data.Length > 3 ? Data[3] - 10 : 0;
    }

    public int GetTrebleLevel()
    {
        return Data.Length > 4 ? Data[4] - 10 : 0;
    }

    public bool GetLoudness()
    {
        return Data.Length > 5 && Data[5] == 1;
    }

    public int GetBalance()
    {
        return Data.Length > 6 ? Data[6] - 10 : 0;
    }

    public byte GetPartyMode()
    {
        return Data.Length > 8 ? Data[8] : (byte)0;
    }

    public byte GetDoNotDisturbMode()
    {
        return Data.Length > 9 ? Data[9] : (byte)0;
    }

    /// <summary>
    /// Creates a ZoneInfoPacket from a DataPacket
    /// </summary>
    public static ZoneInfoPacket FromPacket(DataPacket dataPacket)
    {
        if (dataPacket.MessageType != 0x00)
        {
            throw new ArgumentException("Cannot create ZoneInfoPacket from packet with MessageType != 0x00");
        }

        var zoneInfoPacket = new ZoneInfoPacket();
        dataPacket.CopyToPacket(zoneInfoPacket);
        return zoneInfoPacket;
    }
}