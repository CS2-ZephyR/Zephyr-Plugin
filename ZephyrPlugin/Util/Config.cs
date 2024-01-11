using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace ZephyrPlugin.Util;

public class Config : BasePluginConfig
{
    [JsonPropertyName("MongoDB")]
    public MongoConfig MongoConfig { get; set; } = new();
}

public class MongoConfig
{
    [JsonPropertyName("Hostname")]
    public string Hostname { get; set; } = "127.0.0.1";

    [JsonPropertyName("Port")]
    public int Port { get; set; } = 27017;

    [JsonPropertyName("Username")]
    public string Username { get; set; } = "root";

    [JsonPropertyName("Password")]
    public string Password { get; set; } = "";

    [JsonPropertyName("Database")]
    public string Database { get; set; } = "zephyr";
}
