using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.MatchPause;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_pause", "Pause game", OnPauseCommand);
        Plugin.AddCommand("css_unpause", "Unpause game", OnUnpauseCommand);
    }

    private void OnPauseCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;

        if (!player.IsAdmin())
        {
            Logger.Chat(player, "{Red}권한이 없습니다.");
            return;
        }

        Server.ExecuteCommand("mp_pause_match");

        const string message = "게임을 {Red}정지{White}했습니다.";

        Logger.Info(message);
        Logger.ChatAll(message);
    }

    private void OnUnpauseCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;

        if (!player.IsAdmin())
        {
            Logger.Chat(player, "{Red}권한이 없습니다.");
            return;
        }

        Server.ExecuteCommand("mp_unpause_match");

        const string message = "게임을 {Green}재개{White}했습니다.";

        Logger.Info(message);
        Logger.ChatAll(message);
    }
}
