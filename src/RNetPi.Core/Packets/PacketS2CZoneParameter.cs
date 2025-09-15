using RNetPi.Core.Utilities;

namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x0B
/// Zone Parameter
/// Sends extra parameter values
/// Data:
///     (Unsigned Char) Controller ID
///     (Unsigned Char) Zone ID
///     (Unsigned Char) Parameter ID
///     (Unsigned/Signed Char) Parameter Value
/// </summary>
public class PacketS2CZoneParameter : PacketS2C
{
    public const byte ID = 0x0B;

    public PacketS2CZoneParameter(byte controllerID, byte zoneID, byte parameterID, object value)
    {
        Writer.Write(controllerID);
        Writer.Write(zoneID);
        Writer.Write(parameterID);

        if (ParameterUtils.IsParameterSigned(parameterID))
        {
            Writer.Write((sbyte)Convert.ToSByte(value));
        }
        else if (ParameterUtils.IsParameterBoolean(parameterID))
        {
            Writer.Write((byte)((value is bool boolValue && boolValue) ? 1 : 0));
        }
        else
        {
            Writer.Write(Convert.ToByte(value));
        }
    }

    public override byte GetID() => ID;
}