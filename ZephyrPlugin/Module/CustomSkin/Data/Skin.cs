using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class Skin
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;

    public ulong SteamId;

    public string Knife;
    public ushort Glove;
    public Dictionary<int, SkinDetail> Details;

    public class SkinDetail
    {
        public int Paint;
        public int Seed;
        public float Wear;
        public string Name;
    }
}