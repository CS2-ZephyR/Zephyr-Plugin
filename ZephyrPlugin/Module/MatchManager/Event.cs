using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using MongoDB.Driver;
using ZephyrPlugin.Module.MatchManager.Data;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.MatchManager;

public partial class Module
{
    public override void RegisterEvents()
    {
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.RegisterEventHandler<EventCsIntermission>(OnIntermission);
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
    }

    private void OnMapStart(string map)
    {
        Match = _collection.Find(x => !x.End).Single();
        Logger.All($"매치 ID: {{Green}}{Match.Id}");
        
        ChangeTeamName();
    }
    
    private HookResult OnIntermission(EventCsIntermission @event, GameEventInfo info)
    {
        var count = 16;

        Task.Run(async () =>
        {
            await _collection.UpdateManyAsync(
                Builders<Match>.Filter.Eq(x => x.Id, Match.Id),
                Builders<Match>.Update.Set(x => x.End, true));
        });
        
        Plugin.AddTimer(1.0f, () =>
        {
            Logger.All($"{--count}초 후 서버가 종료됩니다.");

            if (count >= 0) return;
            
            var players = Utilities.GetPlayers().Where(x => x.IsValid()).ToList();
            foreach (var player in players)
            {
                Server.ExecuteCommand($"kickid {player.UserId}");
            }
            
            Server.ExecuteCommand("quit");
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

        return HookResult.Continue;
    }

    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!player.IsValid()) return HookResult.Continue;

        var steamId = player.SteamID;
        
        if (Match.Team1.Member.Contains(steamId))
        {
            Server.NextFrame(() =>
            {
                Plugin.AddTimer(0.5f, () =>
                {
                    player.SwitchTeam(CsTeam.CounterTerrorist);
                    player.Respawn();
                });
            });
        }
        else if (Match.Team2.Member.Contains(steamId))
        {
            Server.NextFrame(() =>
            {
                Plugin.AddTimer(0.5f, () =>
                {
                    player.SwitchTeam(CsTeam.Terrorist);
                    player.Respawn();
                });
            });
        }
        else
        {
            Server.NextFrame(() =>
            {
                Logger.All($"팀에 포함되지 않은 유저 {{Green}}{player.PlayerName}{{White}}를 {{Red}}퇴장{{White}}시켰습니다.");
                Server.ExecuteCommand($"kickid {player.UserId}");
            });
        }

        return HookResult.Continue;
    }
}