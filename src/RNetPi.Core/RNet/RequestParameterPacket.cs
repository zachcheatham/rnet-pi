using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Packet for requesting specific parameter data
/// </summary>
public class RequestParameterPacket : DataPacket
{
    public RequestParameterPacket(byte controllerID, byte zoneID, byte parameterID)
    {
        MessageType = 0x00;
        TargetControllerID = controllerID;
        TargetZoneID = zoneID;
        
        TargetPath = new byte[] { 0x02, 0x00, controllerID, 0x00, parameterID };
        SourcePath = Array.Empty<byte>();
        PacketNumber = 0;
        PacketCount = 1;
        Data = Array.Empty<byte>();
    }

    public override bool RequiresHandshake()
    {
        return true;
    }

    public override bool CausesResponseWithHandshake()
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
}