﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.DamageInfo;

public partial class Module
{
    public override void RegisterEvents()
    {
        Plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        Plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _damage.Clear();
        _death.Clear();
        
        foreach (var player1 in Utilities.GetPlayers().Where(player => player.IsValid()))
        {
            foreach (var player2 in Utilities.GetPlayers().Where(player => player.IsValid()).Where(player => player.Team != player1.Team))
            {
                _damage[new Tuple<ulong, ulong>(player1.SteamID, player2.SteamID)] = new ValueTuple<int, int>(0, 0);
            }
        }

        foreach (var player in Utilities.GetPlayers().Where(player => !player.IsValid()))
        {
            _death[player.SteamID] = 0;
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (KnifeRound.Module.KnifeRound) return HookResult.Continue;
        
        foreach (var player1 in Utilities.GetPlayers().Where(player => player.IsValid()))
        {
            Logger.Chat(player1, "================ 데미지 정보 ================");
            
            foreach (var player2 in Utilities.GetPlayers().Where(player => player.IsValid()).Where(player => player.Team != player1.Team))
            {
                if (!_damage.TryGetValue(new Tuple<ulong, ulong>(player2.SteamID, player1.SteamID), out var damageInfo)) continue;
                
                var targetColor = _death.TryGetValue(player2.SteamID, out var killer) && killer == player1.SteamID ? "{Red}" : "{Lime}";
            
                Logger.Chat(player1, $"{targetColor}{player2.PlayerName} {{Default}}에게 준 피해: {{LightRed}}{damageInfo.Item2}{{Default}}딜 {{Grey}}({damageInfo.Item1}대)");
            }
            
            Logger.Chat(player1, "=========================================");
        }
        
        return HookResult.Continue;
    }
    
    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;

        if (!victim.IsValid() || !attacker.IsValid()) return HookResult.Continue;
        if (victim.Team == attacker.Team) return HookResult.Continue;

        var key = new Tuple<ulong, ulong>(victim.SteamID, attacker.SteamID);
        if (!_damage.TryGetValue(key, out var damageInfo)) return HookResult.Continue;
        
        damageInfo.Item1 += 1;
        damageInfo.Item2 += @event.DmgHealth;

        damageInfo.Item2 = Math.Min(100, damageInfo.Item2);

        _damage[key] = damageInfo;
        
        return HookResult.Continue;
    }
    
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;

        if (!victim.IsValid() || !attacker.IsValid()) return HookResult.Continue;
        if (victim.Team == attacker.Team) return HookResult.Continue;

        _death[victim.SteamID] = attacker.SteamID;

        return HookResult.Continue;
    }
}
