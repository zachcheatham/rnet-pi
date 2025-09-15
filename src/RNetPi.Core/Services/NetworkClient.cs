using System;
using System.Threading.Tasks;
using RNetPi.Core.Packets;

namespace RNetPi.Core.Services;

/// <summary>
/// Abstract base class for network clients that handle packet communication
/// </summary>
public abstract class NetworkClient
{
    private ClientIntent _intent = ClientIntent.None;

    /// <summary>
    /// Event fired when a packet is received from the client
    /// </summary>
    public event EventHandler<PacketC2S>? PacketReceived;

    /// <summary>
    /// Event fired when the client subscribes (sets intent to Subscribe)
    /// </summary>
    public event EventHandler? Subscribed;

    /// <summary>
    /// Event fired when the client disconnects
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    /// Gets the current client intent
    /// </summary>
    public ClientIntent Intent => _intent;

    /// <summary>
    /// Checks if the client has a valid intent (not None)
    /// </summary>
    public bool IsValid => _intent != ClientIntent.None;

    /// <summary>
    /// Checks if the client is subscribed
    /// </summary>
    public bool IsSubscribed => _intent == ClientIntent.Subscribe;

    /// <summary>
    /// Gets the client's network address
    /// </summary>
    /// <returns>String representation of the client address</returns>
    public abstract string GetAddress();

    /// <summary>
    /// Sends a packet to the client
    /// </summary>
    /// <param name="packet">The packet to send</param>
    /// <returns>Task representing the async operation</returns>
    public abstract Task SendPacketAsync(PacketS2C packet);

    /// <summary>
    /// Sends raw buffer data to the client
    /// </summary>
    /// <param name="buffer">The buffer to send</param>
    /// <returns>Task representing the async operation</returns>
    public abstract Task SendBufferAsync(byte[] buffer);

    /// <summary>
    /// Disconnects the client
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public abstract Task DisconnectAsync();

    /// <summary>
    /// Handles a received packet by parsing it and invoking appropriate events
    /// </summary>
    /// <param name="packetType">The packet type identifier</param>
    /// <param name="data">The raw packet data</param>
    protected void HandlePacket(byte packetType, byte[] data)
    {
        var packet = PacketFactory.CreatePacket(packetType, data);

        if (packet != null)
        {
            Console.WriteLine($"DEBUG: Received packet {packet.GetType().Name} from {GetAddress()}");

            if (packet is PacketC2SIntent intentPacket)
            {
                _intent = (ClientIntent)intentPacket.GetIntent();
                if (_intent == ClientIntent.Subscribe)
                {
                    Subscribed?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (IsValid)
            {
                PacketReceived?.Invoke(this, packet);
            }
        }
        else
        {
            Console.WriteLine($"Received bad packet from {GetAddress()} <{packetType}:{Convert.ToHexString(data)}>");
        }
    }

    /// <summary>
    /// Notifies that the client has disconnected
    /// </summary>
    protected virtual void OnDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Client intent enumeration
/// </summary>
public enum ClientIntent
{
    None = 0,
    Subscribe = 2
}