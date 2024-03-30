using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Timers;
using ZephyrPlugin.Util;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace ZephyrPlugin.Module.MatchManager;

public partial class Module
{
    private static int _totalPlayer = 10;

    private static Timer _timer1;
    private static Timer _timer2;
    
    public override void RegisterTimers()
    {
        _totalPlayer = Match.Team1.Member.Count + Match.Team2.Member.Count;
        
        RunTimer1();
        RunTimer2();
    }

    private void RunTimer1()
    {
        _timer1 = Plugin.AddTimer(1.0f, () =>
        {
            var players = Utilities.GetPlayers().Where(x => x.IsValid()).ToList();
            var playerCount = players.Count(x => x.TeamNum is 2 or 3);

            Logger.CenterAll($"접속한 플레이어: {playerCount}/{_totalPlayer} 명", red: true);

            if (playerCount < _totalPlayer) return;

            if (_timer1 == null) return;
            _timer1.Kill();
            _timer1 = null;
        }, TimerFlags.REPEAT);
    }

    private void RunTimer2()
    {
        var count = 5;
        var flag = false;

        _timer2 = Plugin.AddTimer(1.0f, () =>
        {
            if (_timer1 != null) return;

            if (!flag)
            {
                flag = true;
                Logger.ChatAll("모든 플레이어가 접속했습니다.");
            }

            Logger.CenterAll($"{count}초 후 게임을 시작합니다.", red: true);
            foreach (var player in Utilities.GetPlayers())
            {
                player.PlaySound("ui/counter_beep");
            }

            if (--count > 0) return;

            Server.ExecuteCommand("mp_warmup_end");

            if (_timer2 == null) return;
            _timer2.Kill();
            _timer2 = null;
        }, TimerFlags.REPEAT);
    }
}
