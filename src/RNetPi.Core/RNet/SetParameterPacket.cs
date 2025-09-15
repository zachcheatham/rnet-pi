using System;
using System.IO;

namespace RNetPi.Core.RNet;

/// <summary>
/// Packet for setting zone parameter values
/// </summary>
public class SetParameterPacket : DataPacket
{
    public SetParameterPacket(byte controllerID, byte zoneID, byte parameterID, byte value)
    {
        MessageType = 0x00;
        TargetControllerID = controllerID;
        TargetZoneID = zoneID;
        
        TargetPath = new byte[] { 0x02, 0x00, controllerID, 0x00, parameterID };
        SourcePath = Array.Empty<byte>();
        PacketNumber = 0;
        PacketCount = 1;
        Data = new byte[] { value };
    }

    public override bool RequiresHandshake()
    {
        return true;
    }

    public byte GetControllerID()
    {
        return TargetControllerID;
    }

    public byte GetZoneID()
    {
        return TargetZoneID;
    }

    public byte GetParameterID()
    {
        return TargetPath.Length > 4 ? TargetPath[4] : (byte)0;
    }

    public byte GetParameterValue()
    {
        return Data.Length > 0 ? Data[0] : (byte)0;
    }
}