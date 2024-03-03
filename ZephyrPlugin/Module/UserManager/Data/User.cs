using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZephyrPlugin.Module.UserManager.Data;

[BsonIgnoreExtraElements]
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;

    public ulong SteamId;
    public string Name;
}
