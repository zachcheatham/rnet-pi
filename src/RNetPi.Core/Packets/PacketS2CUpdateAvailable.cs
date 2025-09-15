namespace RNetPi.Core.Packets;

/// <summary>
/// Server -> Client
/// ID = 0x7D
/// Update Available
/// Notifies the client that there's an update available
/// Data:
///     (String) Update Version
/// </summary>
public class PacketS2CUpdateAvailable : PacketS2C
{
    public const byte ID = 0x7D;

    public PacketS2CUpdateAvailable(string version)
    {
        WriteNullTerminatedString(version);
    }

    public override byte GetID() => ID;
}