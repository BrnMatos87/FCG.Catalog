namespace FCG.Catalog.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public string Host { get; set; } = string.Empty;

    public ushort Port { get; set; }

    public string VirtualHost { get; set; } = "/";

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}