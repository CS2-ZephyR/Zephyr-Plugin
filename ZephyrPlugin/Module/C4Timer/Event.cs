using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using ZephyrPlugin.Util;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace ZephyrPlugin.Module.C4Timer;

public partial class Module
{
    private bool _planted;
    private float _count;
    
    private Timer _timer;
    
    public override void RegisterEvents()
    {
        Plugin.RegisterEventHandler<EventBombPlanted>(OnBombPlanted);

        Plugin.RegisterEventHandler<EventRoundStart>((_, _) => { _planted = false; return HookResult.Continue;});
        Plugin.RegisterEventHandler<EventBombExploded>((_, _) => { _planted = false; return HookResult.Continue; });
        Plugin.RegisterEventHandler<EventBombDefused>((_, _) => { _planted = false; return HookResult.Continue; });
    }

    private HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        var bomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();

        if (bomb == null) return HookResult.Continue;
        
        _planted = true;
        _count = bomb.TimerLength + 1.0f;

        _timer = new Timer(1.0f, () =>
        {
            Logger.CenterAll($"-[ C4 : {--_count}초]-");

            if (_count > 0 && _planted) return;
        
            _count = 0;
        
            _timer.Kill();
            _timer = null;
        }, TimerFlags.REPEAT);

        return HookResult.Continue;
    }
}
