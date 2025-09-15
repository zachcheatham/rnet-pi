using RNetPi.Core.Constants;

namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x34
/// Source Property
/// Data:
///     (Unsigned Char) Source ID
///     (Unsigned Char) Property ID
///     (Variable) Property Value
/// </summary>
public class PacketS2CSourceProperty : PacketS2C
{
    public const byte ID = 0x34;

    public PacketS2CSourceProperty(byte sourceID, byte propertyID, object? propertyValue)
    {
        Writer.Write(sourceID);
        Writer.Write(propertyID);
        
        switch (propertyID)
        {
            case SourceProperties.AutoOff:
            case SourceProperties.OverrideName:
                Writer.Write((byte)((propertyValue is bool boolValue && boolValue) ? 0x01 : 0x00));
                break;
            case SourceProperties.AutoOnZones:
                if (propertyValue is IEnumerable<(byte, byte)> zones)
                {
                    foreach (var (controllerID, zoneID) in zones)
                    {
                        Writer.Write(controllerID);
                        Writer.Write(zoneID);
                    }
                }
                break;
        }
    }

    public override byte GetID() => ID;
}