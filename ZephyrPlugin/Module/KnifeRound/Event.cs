﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.KnifeRound;

public partial class Module
{
    public static bool KnifeRound = true;
    
    private bool _voteRound;
    private ulong _winnerLeader;
    
    public override void RegisterEvents()
    {
        Plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (!KnifeRound) return HookResult.Continue;
        
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        if (gameRules is { WarmupPeriod: true }) return HookResult.Continue;
        
        Logger.All("칼전 !");
        
        return HookResult.Continue;
    }
    
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!KnifeRound) return HookResult.Continue;
        
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        if (gameRules is { WarmupPeriod: true }) return HookResult.Continue;
        
        if (@event.Winner is not (2 or 3)) return HookResult.Continue;
        
        KnifeRound = false;
        _voteRound = true;
        
        var team = @event.Winner == 3 ? MatchManager.Module.Match.Team1 : MatchManager.Module.Match.Team2;
        _winnerLeader = team.Member[0];
        
        Logger.All($"{{Magenta}}{team.Name}{{White}} 팀이 이겼습니다. {{Green}}리더 {UserManager.Module.Names[_winnerLeader]}{{White}}님은 진영을 선택해주세요. {{Grey}}(.ct / .t)");
        
        Server.ExecuteCommand("mp_give_player_c4 1");
        Server.ExecuteCommand("mp_freezetime 10");
        Server.ExecuteCommand("mp_warmup_start");
        
        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (!KnifeRound) return HookResult.Continue;
        
        var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        if (gameRules is { WarmupPeriod: true }) return HookResult.Continue;
        
        var player = @event.Userid;

        if (!player.IsValid() || player.PlayerPawn.Value == null) return HookResult.Continue;

        Plugin.AddTimer(0.1f, () =>
        {
            if (player.InGameMoneyServices != null) player.InGameMoneyServices.Account = 0;

            player.PlayerPawn.Value!.Health = 50;
            player.PlayerPawn.Value!.VelocityModifier = 1.5F;
            
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CCSPlayerPawnBase", "m_flVelocityModifier");
            
            foreach (var weapon in player.PlayerPawn.Value.WeaponServices!.MyWeapons)
            {
                if (weapon is not { IsValid: true, Value.IsValid: true } || weapon.Value.DesignerName.Contains("weapon_knife") || weapon.Value.DesignerName.Contains("weapon_bayonet")) continue;
            
                player.ExecuteClientCommand("slot3");
                player.DropActiveWeapon();
                weapon.Value.Remove();
            }

            if (MatchManager.Module.Match.Team1.Member[0] == player.SteamID ||
                MatchManager.Module.Match.Team2.Member[0] == player.SteamID)
            {
                player.GiveNamedItem("weapon_taser");
            }
        });

        return HookResult.Continue;
    }
}
