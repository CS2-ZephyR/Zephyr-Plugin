using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using MongoDB.Driver;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.UserManager;

public partial class Module
{
    public override void RegisterEvents()
    {
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect, HookMode.Pre);
        Plugin.RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam, HookMode.Pre);
    }

    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!player.IsValid()) return HookResult.Continue;

        var steamId = player.SteamID;
        var isAdmin = player.IsAdmin();

        Task.Run(async () =>
        {
            var result = await (await _collection.FindAsync(x => x.SteamId == steamId)).SingleOrDefaultAsync();

            if (result != null)
            {
                Names[steamId] = result.Name;

                Server.NextFrame(() =>
                {
                    player.Clan = "ZephyR";
                    Plugin.AddTimer(0.2f, () =>
                    {
                        Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
                    });
                    
                    Logger.All($"{{Green}}{result.Name}{{White}}님이 {{Lime}}접속{{White}}하셨습니다.");
                });
            }
            else
            {
                Server.NextFrame(() =>
                {
                    if (!isAdmin)
                    {
                        Server.ExecuteCommand($"kickid {player.UserId}");
                    }

                    Logger.All($"허가되지 않은 유저 {{Green}}{player.PlayerName}{{White}}를 {{Red}}퇴장{{White}}시켰습니다.");
                });
            }
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        info.DontBroadcast = true;

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        info.DontBroadcast = true;

        var player = @event.Userid;

        if (!player.IsValid()) return HookResult.Continue;

        var steamId = player.SteamID;

        Names.TryGetValue(steamId, out var name);
        Logger.All($"{{Green}}{name ?? player.PlayerName}{{White}}님이 {{LightRed}}퇴장{{White}}하셨습니다.");

        return HookResult.Continue;
    }
}
