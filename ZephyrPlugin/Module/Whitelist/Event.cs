using System.Collections.Generic;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MongoDB.Driver;
using ZephyrPlugin.Module.Whitelist.Util;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.Whitelist;

public partial class Module
{
    private static readonly Dictionary<ulong, string> Names = new();

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
                Server.NextFrame(() =>
                {
                    new PlayerName<CBasePlayerController>(player).Set(Names[player.SteamID] = result.Name);
                });
            }
            else
            {
                if (isAdmin) return;

                Server.NextFrame(() =>
                {
                    Server.ExecuteCommand($"kickid {player.UserId}");

                    var message = $"허가되지 않은 유저 {{Green}}{player.PlayerName}{{Grey}}({steamId})를 퇴장시켰습니다.";

                    Logger.Warn(message);
                    Logger.ChatAll(message);
                });
            }
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        info.DontBroadcast = true;

        return HookResult.Continue;
    }

    private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        var player = @event.Userid;

        info.DontBroadcast = true;

        if (!player.IsValid()) return HookResult.Continue;

        var existsName = Names.TryGetValue(player.SteamID, out var name);
        var message = $"{{Green}}{(existsName ? name : player.PlayerName)}{{White}}님이 {(@event.Team == 2 ? "{Red}테러리스트" : "{Blue}대테러리스트")}{{White}}에 들어왔습니다.";

        Logger.Info(message);
        Logger.ChatAll(message);

        return HookResult.Continue;
    }
}
