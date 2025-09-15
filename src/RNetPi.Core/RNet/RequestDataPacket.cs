using System;

namespace RNetPi.Core.RNet;

/// <summary>
/// Packet for requesting data from the RNet system
/// </summary>
public class RequestDataPacket : DataPacket
{
    public enum DataType : byte
    {
        ZoneVolume = 0x01,
        ZoneSource = 0x02,
        ZonePower = 0x06,
        ZoneInfo = 0x07
    }

    public RequestDataPacket(byte controllerID, byte zoneID, DataType dataType)
    {
        MessageType = 0x01;
        TargetControllerID = controllerID;
        TargetZoneID = zoneID;
        TargetPath = new byte[] { 0x02, 0x00, zoneID, (byte)dataType };
        SourcePath = Array.Empty<byte>();
        PacketNumber = 0;
        PacketCount = 1;
        Data = Array.Empty<byte>();
    }

    public RequestDataPacket(byte[] targetPath)
    {
        MessageType = 0x00;
        TargetPath = targetPath ?? throw new ArgumentNullException(nameof(targetPath));
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

    /// <summary>
    /// Creates a request for zone information
    /// </summary>
    public static RequestDataPacket CreateZoneInfoRequest(byte controllerID, byte zoneID)
    {
        var targetPath = new byte[] { 0x02, 0x00, controllerID, 0x07 };
        var packet = new RequestDataPacket(targetPath)
        {
            TargetControllerID = controllerID,
            TargetZoneID = zoneID
        };
        return packet;
    }

    /// <summary>
    /// Creates a request for zone power status
    /// </summary>
    public static RequestDataPacket CreateZonePowerRequest(byte controllerID, byte zoneID)
    {
        var targetPath = new byte[] { 0x02, 0x00, controllerID, 0x06 };
        var packet = new RequestDataPacket(targetPath)
        {
            TargetControllerID = controllerID,
            TargetZoneID = zoneID
        };
        return packet;
    }

    /// <summary>
    /// Creates a request for zone source status
    /// </summary>
    public static RequestDataPacket CreateZoneSourceRequest(byte controllerID, byte zoneID)
    {
        var targetPath = new byte[] { 0x02, 0x00, controllerID, 0x02 };
        var packet = new RequestDataPacket(targetPath)
        {
            TargetControllerID = controllerID,
            TargetZoneID = zoneID
        };
        return packet;
    }

    /// <summary>
    /// Creates a request for zone volume status
    /// </summary>
    public static RequestDataPacket CreateZoneVolumeRequest(byte controllerID, byte zoneID)
    {
        var targetPath = new byte[] { 0x02, 0x00, controllerID, 0x01 };
        var packet = new RequestDataPacket(targetPath)
        {
            TargetControllerID = controllerID,
            TargetZoneID = zoneID
        };
        return packet;
    }
}