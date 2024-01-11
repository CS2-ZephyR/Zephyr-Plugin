using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZephyrPlugin.Module.Whitelist.Data;

public class Whitelist
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;

    public ulong SteamId;
    public string Name;
}
