using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.ItemManager;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_give", "Give command", OnGiveCommand);
    }

    private void OnGiveCommand(CCSPlayerController player, CommandInfo info)
    {
        if (!player.IsValid() || !player.PawnIsAlive) return;

        if (info.ArgCount != 2)
        {
            Logger.Chat(player, "{Red}잘못된 사용법입니다. (사용법: give <Item>)");
            return;
        }

        if (Enum.TryParse(typeof(CsItem), info.GetArg(1), true, out var item))
        {
            player.GiveNamedItem((CsItem)item);
            Logger.ChatAll($"{{Green}}{player.PlayerName}{{White}}님이 {{LightRed}}{item}{{White}}을 획득했습니다.");
        }
        else
        {
            Logger.Chat(player, "{Red}올바르지 않은 아이템입니다.");
        }
    }
}
