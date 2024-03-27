using CounterStrikeSharp.API;
using MongoDB.Driver;
using ZephyrPlugin.Module.MatchManager.Data;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.MatchManager;

public partial class Module : ZephyrModule
{
    private IMongoCollection<Match> _collection;

    public static Match Match;

    public Module() : base("MatchManager")
    { }

    public override void OnLoad()
    {
        _collection = Database.GetCollection<Match>();

        OnMapStart(string.Empty);

        var command = Match.Map.StartsWith("workshop:") ? "host_workshop_map" : "map";
        var map = Match.Map.StartsWith("workshop:") ? Match.Map[9..] : Match.Map;
        
        Server.ExecuteCommand($"{command} {map}");
    }

    private void ChangeTeamName()
    {
        Server.NextFrame(() =>
        {
            Server.ExecuteCommand($"mp_teamname_1 {Match.Team1.Name}");
            Server.ExecuteCommand($"mp_teamname_2 {Match.Team2.Name}");
        });
    }
}
