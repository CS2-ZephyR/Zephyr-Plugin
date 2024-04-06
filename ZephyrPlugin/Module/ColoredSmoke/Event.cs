using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace ZephyrPlugin.Module.ColoredSmoke;

public partial class Module
{
    private bool _isColoredSmoke = true;

    public override void RegisterEvents()
    {
        Plugin.RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
    }

    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (!_isColoredSmoke) return;

        if (entity.DesignerName != "smokegrenade_projectile") return;
        
        var projectile = new CSmokeGrenadeProjectile(entity.Handle);
        
        var player = projectile.Thrower.Value;
        if (player == null) return;

        if (!CustomSkin.Module.PlayerSmoke.TryGetValue(player.Controller.Value!.SteamID, out var smoke)) return;

        Server.NextFrame(() =>
        {
            projectile.SmokeColor.X = smoke.Item1;
            projectile.SmokeColor.Y = smoke.Item2;
            projectile.SmokeColor.Z = smoke.Item3;
        });
    }
}
