using System.Net.WebSockets;
using CounterStrikeSharp.API;
using MongoDB.Driver;
using ZephyrPlugin.Module.MatchManager.Data;
using ZephyrPlugin.Util;
using WebSocket = WebSocketSharp.WebSocket;

namespace ZephyrPlugin.Module.MatchManager;

public partial class Module() : ZephyrModule("MatchManager")
{
    private IMongoCollection<Match> _collection;

    private WebSocket _socket;
    
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

        _socket = new WebSocket("ws://127.0.0.1:27001");
        _socket.Connect();
        
        var command = Match.Map.StartsWith("workshop:") ? "host_workshop_map" : "map";
        var map = Match.Map.StartsWith("workshop:") ? Match.Map[9..] : Match.Map;

        Server.ExecuteCommand($"{command} {map}");
    }
}
