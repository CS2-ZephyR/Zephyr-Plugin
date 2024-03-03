using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZephyrPlugin.Module.MatchManager.Data;

[BsonIgnoreExtraElements]
public class Match
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime Date;
    
    public bool End;

    public string Map;
    
    public Team Team1;
    public Team Team2;

    public class Team
    {
        public string Name;
        public List<ulong> Member;
    }
}

