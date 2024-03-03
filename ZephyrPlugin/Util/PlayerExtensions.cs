using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace ZephyrPlugin.Util;

public static class PlayerExtensions
{
    public static bool IsValid(this CCSPlayerController player)
    {
        return player is { IsValid: true, IsBot: false, IsHLTV: false, Connected: PlayerConnectedState.PlayerConnected };
    }

    public static bool IsAdmin(this CCSPlayerController player)
    {
        return AdminManager.PlayerHasPermissions(player, "@css/root");
    }

    public static bool CheckPermission(this CCSPlayerController player)
    {
        return CheckPermission(player, x => x.IsAdmin());
    }
    
    public static bool CheckPermission(this CCSPlayerController player, Func<CCSPlayerController, bool> func)
    {
        var isAdmin = func.Invoke(player);

        if (!isAdmin)
        {
            Logger.Chat(player, "{Red}권한이 없습니다.");
        }

        return !isAdmin;
    }

    public static void PlaySound(this CCSPlayerController player, string sound)
    {
        player.ExecuteClientCommand($"play sounds/{sound}");
    }
}
