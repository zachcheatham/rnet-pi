using RNetPi.Core.Constants;

namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x34
/// Source Property
/// Change source properties
/// Data:
///     (Unsigned Char) Source ID
///     (Unsigned Char) Property ID
///     (Variable) Property Value
/// </summary>
public class PacketC2SSourceProperty : PacketC2S
{
    public const byte ID = 0x34;

    public byte SourceID { get; private set; }
    public byte PropertyID { get; private set; }
    public object? PropertyValue { get; private set; }

    public PacketC2SSourceProperty(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        SourceID = Reader.ReadByte();
        PropertyID = Reader.ReadByte();
        
        switch (PropertyID)
        {
            case SourceProperties.AutoOff:
            case SourceProperties.OverrideName:
                PropertyValue = Reader.ReadByte() == 0x01;
                break;
            case SourceProperties.AutoOnZones:
                var zones = new List<(byte ControllerID, byte ZoneID)>();
                while (Reader.BaseStream.Position < Reader.BaseStream.Length)
                {
                    var controllerID = Reader.ReadByte();
                    var zoneID = Reader.ReadByte();
                    zones.Add((controllerID, zoneID));
                }
                PropertyValue = zones.ToArray();
                break;
            default:
                PropertyValue = false;
                break;
        }
    }
}