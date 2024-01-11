using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace ZephyrPlugin.Util;

public static class PlayerExtensions
{
    public static bool IsValid(this CCSPlayerController player)
    {
        return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
    }

    public static bool IsAdmin(this CCSPlayerController player)
    {
        return AdminManager.PlayerHasPermissions(player, "@css/root");
    }
}
