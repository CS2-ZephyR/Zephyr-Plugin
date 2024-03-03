using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.KnifeRound;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_ct", "Knife Team Select CT", OnCtCommand);
        Plugin.AddCommand("css_t", "Knife Team Select T", OnTCommand);
    }

    private void OnCtCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;
        if (player.CheckPermission(x => _voteRound && x.SteamID == _winnerLeader)) return;

        if (player.TeamNum == 2)
        {
            Server.ExecuteCommand("mp_swapteams");
        }

        _voteRound = false;
        
        Server.ExecuteCommand("mp_warmup_end");
    }
    
    private void OnTCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;
        if (player.CheckPermission(x => _voteRound && x.SteamID == _winnerLeader)) return;

        if (player.TeamNum == 3)
        {
            Server.ExecuteCommand("mp_swapteams");
        }
        
        _voteRound = false;
        
        Server.ExecuteCommand("mp_warmup_end");
        
        Server.ExecuteCommand($"tv_record {MatchManager.Module.Match.Id}");
    }
}
