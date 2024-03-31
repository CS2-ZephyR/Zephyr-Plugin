using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.MatchManager;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_p", "Pause game", OnPauseCommand);
        Plugin.AddCommand("css_up", "Unpause game", OnUnpauseCommand);
    }

    private void OnPauseCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;
        
        Server.ExecuteCommand("mp_pause_match");
        
        Logger.All($"{{Green}}{player.PlayerName}{{White}}님이 경기를 {{Red}}일시 정지{{White}}했습니다.");
    }
    
    private void OnUnpauseCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;
        
        Server.ExecuteCommand("mp_unpause_match");
        
        Logger.All($"{{Green}}{player.PlayerName}{{White}}님이 경기를 {{Green}}재개{{White}}했습니다.");
    }
}
