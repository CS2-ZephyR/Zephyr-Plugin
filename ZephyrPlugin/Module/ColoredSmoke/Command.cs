using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.ColoredSmoke;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_smoke", "Toggle colored smoke", OnToggleCommand);
    }

    private void OnToggleCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;
        if (player.CheckPermission()) return;
        
        _isColoredSmoke = !_isColoredSmoke;
        
        Logger.All($"{{Green}}{player.PlayerName}{{White}}님이 연막 색상을 {(_isColoredSmoke ? "{Green}활성화" : "{Red}비활성화")}{{White}}했습니다.");
    }
}
