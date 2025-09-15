using RNetPi.Core.Utilities;

namespace RNetPi.Core.Packets;

/// <summary>
/// Client -> Server
/// ID = 0x0B
/// Zone Parameter
/// Sets extra parameters in a zone
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Parameter ID
///     (Unsigned/Signed Char) Parameter Value
/// </summary>
public class PacketC2SZoneParameter : PacketC2S
{
    public const byte ID = 0x0B;

    public byte ControllerID { get; private set; }
    public byte ZoneID { get; private set; }
    public byte ParameterID { get; private set; }
    public object? ParameterValue { get; private set; }

    public PacketC2SZoneParameter(byte[] data) : base(data)
    {
    }

    public override byte GetID() => ID;

    protected override void ParseData()
    {
        ControllerID = Reader.ReadByte();
        ZoneID = Reader.ReadByte();
        ParameterID = Reader.ReadByte();

        if (ParameterUtils.IsParameterSigned(ParameterID))
        {
            ParameterValue = Reader.ReadSByte();
        }
        else if (ParameterUtils.IsParameterBoolean(ParameterID))
        {
            ParameterValue = Reader.ReadByte() == 0x01;
        }
        else
        {
            ParameterValue = Reader.ReadByte();
        }
    }
}