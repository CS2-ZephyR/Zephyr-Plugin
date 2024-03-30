using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.UserManager;

public partial class Module
{
    public override void RegisterTimers()
    {
        RunTimer1();
    }

    private void RunTimer1()
    {
        Plugin.AddTimer(1f, () =>
        {
            var players = Utilities.GetPlayers().Where(x => x.IsValid()).ToList();

            foreach (var player in players)
            {
                if (!Names.TryGetValue(player.SteamID, out var name)) continue;

                var playerName = new SchemaString<CBasePlayerController>(player, "m_iszPlayerName");
                playerName.Set(name);

                Plugin.AddTimer(0.2f, () =>
                {
                    Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
                });
            }
        }, TimerFlags.REPEAT);
    }
}
