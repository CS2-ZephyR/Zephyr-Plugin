using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module;

public abstract class ZephyrModule(string moduleName)
{
    public readonly string ModuleName = moduleName;

    protected readonly Logger Logger = new(moduleName);

    protected ZephyrPlugin Plugin;

    public static readonly List<ZephyrModule> Modules = [
        new MatchManager.Module(),
        new UserManager.Module(),
        new WarmupWeapon.Module(),
        new KnifeRound.Module(),
        new ColoredSmoke.Module(),
        new C4Timer.Module(),
        new DamageInfo.Module(),
        new CustomSkin.Module()
    ];

    public void InjectPlugin(ZephyrPlugin plugin)
    {
        Plugin = plugin;
    }

    public virtual void OnLoad()
    { }

    public virtual void RegisterCommands()
    { }

    public virtual void RegisterEvents()
    { }

    public virtual void RegisterTimers()
    { }
}
