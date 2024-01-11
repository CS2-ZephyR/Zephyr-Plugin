using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.MatchManager;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_pause", "Pause game", OnPauseCommand);
        Plugin.AddCommand("css_unpause", "Unpause game", OnUnpauseCommand);

        Plugin.AddCommand("css_warmup_start", "Start warmup", OnWarmupStartCommand);
        Plugin.AddCommand("css_warmup_end", "End warmup", OnWarmupEndCommand);
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

    private void OnWarmupStartCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;

        if (!player.IsAdmin())
        {
            Logger.Chat(player, "{Red}권한이 없습니다.");
            return;
        }

        Server.ExecuteCommand("mp_warmup_start; mp_warmup_pausetimer 1");

        const string message = "연습 모드를 {Green}실행{White}했습니다.";

        Logger.Info(message);
        Logger.ChatAll(message);
    }

    private void OnWarmupEndCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;

        if (!player.IsAdmin())
        {
            Logger.Chat(player, "{Red}권한이 없습니다.");
            return;
        }

        Server.ExecuteCommand("mp_warmup_end");

        const string message = "연습 모드를 {Red}종료{White}했습니다.";

        Logger.Info(message);
        Logger.ChatAll(message);
    }
}
