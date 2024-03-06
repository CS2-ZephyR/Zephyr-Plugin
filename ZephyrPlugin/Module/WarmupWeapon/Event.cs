using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.WarmupWeapon;

public partial class Module
{
    public override void RegisterEvents()
    {
        Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        
        Plugin.AddCommandListener("mp_warmup_start", OnWarmupStart);
        Plugin.AddCommandListener("mp_warmup_end", OnWarmupEnd);
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!player.IsValid()) return HookResult.Continue;

        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        if (gameRules is { WarmupPeriod: false }) return HookResult.Continue;

        if (player.InGameMoneyServices != null) player.InGameMoneyServices.Account = 0;
        
        player.GiveNamedItem("weapon_deagle");
        
        return HookResult.Continue;
    }

    private HookResult OnWarmupStart(CCSPlayerController player, CommandInfo info)
    {
        Server.ExecuteCommand("mp_death_drop_gun 0");

        return HookResult.Continue;
    }

    private HookResult OnWarmupEnd(CCSPlayerController player, CommandInfo info)
    {
        Server.ExecuteCommand("mp_death_drop_gun 1");

        return HookResult.Continue;
    }
}
