using CounterStrikeSharp.API;
using MongoDB.Driver;
using ZephyrPlugin.Module.MatchManager.Data;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.MatchManager;

public partial class Module() : ZephyrModule("MatchManager")
{
    private IMongoCollection<Match> _collection;

    public static Match Match;

    public override void OnLoad(bool hotReload)
    {
        _collection = Database.GetCollection<Match>();

        Match = _collection.Find(x => !x.End).SingleOrDefault();

        if (Match == null)
        {
            Logger.Error("매치가 존재하지 않습니다. 5초 후 인스턴스가 종료됩니다.");
            throw new Exception();
        }

        Logger.All($"매치 ID: {{Green}}{Match.Id}");

        var command = Match.Map.StartsWith("workshop:") ? "host_workshop_map" : "map";
        var map = Match.Map.StartsWith("workshop:") ? Match.Map[9..] : Match.Map;
        
        Server.ExecuteCommand($"{command} {map}");
    }
}
