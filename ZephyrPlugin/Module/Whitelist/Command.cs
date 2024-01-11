using System;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using MongoDB.Driver;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.Whitelist;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_whitelist", "Whitelist management", OnToggleCommand);
    }

    private void OnToggleCommand(CCSPlayerController player, CommandInfo info)
    {
        if (!player.IsValid()) return;

        if (!player.IsAdmin())
        {
            Logger.Chat(player, "{Red}권한이 없습니다.");
            return;
        }

        if (info.ArgCount != 3)
        {
            Logger.Chat(player, "{Red}잘못된 사용법입니다. (사용법: whitelist <SteamID> <Name>)");
            return;
        }

        if (!ulong.TryParse(info.GetArg(1), out var steamId))
        {
            Logger.Chat(player, "{Red}올바른 스팀 ID가 아닙니다.");
            return;
        }

        var name = info.GetArg(2);

        Task.Run(async () =>
        {
            var filter = Builders<Data.Whitelist>.Filter.Eq(x => x.SteamId, steamId);

            var result = await (await _collection.FindAsync(filter)).SingleOrDefaultAsync();

            string message;

            if (result == null)
            {
                var whitelist = new Data.Whitelist
                {
                    SteamId = steamId,
                    Name = name
                };

                await _collection.InsertOneAsync(whitelist);

                message = $"화이트리스트에 {{Green}}{name}{{Grey}}({steamId}){{White}}님을 추가했습니다.";
            }
            else
            {
                var query = Builders<Data.Whitelist>.Update.Set(x => x.Name, name);

                await _collection.UpdateOneAsync(filter, query);

                message = $"{{Green}}{result.Name}{{Grey}}({steamId}){{White}}님의 이름을 {{Green}}{name}{{White}}로 변경했습니다.";
            }

            Server.NextFrame(() =>
            {
                Logger.Warn(message);
                Logger.ChatAll(message);
            });
        });
    }
}
