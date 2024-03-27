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

        if (projectile.Thrower.Value == null) return;

        Server.NextFrame(() =>
        {
            projectile.SmokeColor.X = projectile.Thrower.Value.TeamNum == 2 ? 255 : 0;
            projectile.SmokeColor.Y = 0;
            projectile.SmokeColor.Z = projectile.Thrower.Value.TeamNum == 2 ? 0 : 255;
        });
    }
}
