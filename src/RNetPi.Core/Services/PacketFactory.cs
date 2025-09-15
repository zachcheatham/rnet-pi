using System;
using RNetPi.Core.Packets;

namespace RNetPi.Core.Services;

/// <summary>
/// Factory for creating packet instances from raw packet data
/// </summary>
public static class PacketFactory
{
    /// <summary>
    /// Creates a packet instance from packet type and data
    /// </summary>
    /// <param name="packetType">The packet type identifier</param>
    /// <param name="data">The raw packet data</param>
    /// <returns>A packet instance or null if the packet type is unknown</returns>
    public static PacketC2S? CreatePacket(byte packetType, byte[] data)
    {
        return packetType switch
        {
            0x01 => new PacketC2SIntent(data),
            0x02 => new PacketC2SProperty(data),
            0x03 => new PacketC2SDisconnect(data),
            0x04 => new PacketC2SZoneName(data),
            0x05 => new PacketC2SDeleteZone(data),
            0x06 => new PacketC2SSourceInfo(data),
            0x07 => new PacketC2SDeleteSource(data),
            0x08 => new PacketC2SZonePower(data),
            0x09 => new PacketC2SZoneVolume(data),
            0x0A => new PacketC2SZoneSource(data),
            0x0B => new PacketC2SZoneParameter(data),
            0x0C => new PacketC2SAllPower(data),
            0x0D => new PacketC2SMute(data),
            0x32 => new PacketC2SSourceControl(data),
            0x33 => new PacketC2SRequestSourceProperties(data),
            0x34 => new PacketC2SSourceProperty(data),
            0x64 => new PacketC2SZoneMaxVolume(data),
            0x65 => new PacketC2SZoneMute(data),
            0x7D => new PacketC2SUpdate(data),
            _ => null
        };
    }
}