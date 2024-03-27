using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZephyrPlugin.Module.CustomSkin.Data;

[BsonIgnoreExtraElements]
public class Skin
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;

    public ulong SteamId;

    public string Knife;
    public int Glove;
    public int Music;
    public AgentDetail Agent;
    public Dictionary<int, SkinDetail> Detail;

    public class AgentDetail
    {
        public string Ct;
        public string T;
    }
    
    public class SkinDetail
    {
        public int Paint;
        public int Seed;
        public float Wear;
        public string Name;
    }
}