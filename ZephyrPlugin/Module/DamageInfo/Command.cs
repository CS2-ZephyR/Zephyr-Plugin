using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.DamageInfo;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_hp", "Show hp", OnHpCommand);
    }

    private void OnHpCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;
        if (player.CheckPermission(x => x.LifeState == (byte)LifeState_t.LIFE_ALIVE)) return;
        
        foreach (var player1 in Utilities.GetPlayers().Where(x => x.IsValid()).Where(x => x.Team != player.Team))
        {
            if (!_damage.TryGetValue(new Tuple<ulong, ulong>(player1.SteamID, player.SteamID), out var damageInfo)) continue;
            if (player1.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
            
            foreach (var player2 in Utilities.GetPlayers().Where(x => x.IsValid()).Where(x => x.Team == player.Team))
            {
                Logger.Chat(player2, $"{{Red}}{player.PlayerName}{{Default}}이 {{Lime}}{player1.PlayerName} {{Default}}에게 준 피해: {{LightRed}}{damageInfo.Item2}{{Default}}딜 {{Grey}}({damageInfo.Item1}대)");
            }
        }
    }
}
