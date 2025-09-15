namespace RNetPi.Core.Models;

public class Configuration
{
    public string ServerName { get; set; } = "Untitled RNet Controller";
    public string? ServerHost { get; set; }
    public int ServerPort { get; set; } = 3000;
    public string? WebHost { get; set; }
    public int? WebPort { get; set; }
    public string SerialDevice { get; set; } = "/dev/tty-usbserial1";
    public string WebHookPassword { get; set; } = string.Empty;
    public bool Simulate { get; set; } = false;
}