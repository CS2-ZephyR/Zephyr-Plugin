using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZephyrPlugin.Module.CustomSkin.Data;

public class PlayerSkin
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;

    public ulong SteamId;
    public string Knife;
    public Dictionary<int, int> Skin;
}
