using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using MongoDB.Driver;
using ZephyrPlugin.Util;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace ZephyrPlugin.Module.UserManager;

public partial class Module
{
    private static Timer _timer1;

    public override void RegisterTimers()
    {
        RunTimer1();
    }

    private void RunTimer1()
    {
        _timer1 = new Timer(1f, () =>
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
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }
}
