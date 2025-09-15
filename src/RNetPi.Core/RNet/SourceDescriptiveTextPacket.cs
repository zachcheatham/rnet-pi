using System;
using System.Text;

namespace RNetPi.Core.RNet;

/// <summary>
/// Represents a packet containing source descriptive text
/// </summary>
public class SourceDescriptiveTextPacket : DataPacket
{
    public SourceDescriptiveTextPacket()
    {
        MessageType = 0x00;
    }

    public byte GetControllerID()
    {
        return SourceControllerID;
    }

    public byte GetSourceID()
    {
        return SourcePath.Length > 2 ? SourcePath[2] : (byte)0;
    }

    public string GetDescriptiveText()
    {
        if (Data.Length == 0)
            return string.Empty;

        // Find null terminator or use entire data array
        var length = Array.IndexOf(Data, (byte)0);
        if (length == -1)
            length = Data.Length;

        return Encoding.UTF8.GetString(Data, 0, length);
    }

    /// <summary>
    /// Creates a SourceDescriptiveTextPacket from a DataPacket
    /// </summary>
    public static SourceDescriptiveTextPacket FromPacket(DataPacket dataPacket)
    {
        if (dataPacket.MessageType != 0x00)
        {
            throw new ArgumentException("Cannot create SourceDescriptiveTextPacket from packet with MessageType != 0x00");
        }

        var sourceTextPacket = new SourceDescriptiveTextPacket();
        dataPacket.CopyToPacket(sourceTextPacket);
        return sourceTextPacket;
    }
}