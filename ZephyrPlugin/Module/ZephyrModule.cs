using CounterStrikeSharp.API.Core;
using Serilog;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module;

public abstract class ZephyrModule
{
    public readonly string ModuleName;

    protected readonly Logger Logger;

    protected ZephyrPlugin Plugin;

    public static readonly List<ZephyrModule> Modules = new()
    {
        new Whitelist.Module(),
        new MatchPause.Module(),
        new ColoredSmoke.Module(),
        new CustomSkin.Module()
    };

    protected ZephyrModule(string moduleName)
    {
        ModuleName = moduleName;
        Logger = new Logger(moduleName);
    }

    public void InjectPlugin(ZephyrPlugin plugin)
    {
        Plugin = plugin;
    }

    public virtual void OnLoad()
    {

    }

    public virtual void RegisterCommands()
    {

    }

    public virtual void RegisterEvents()
    {

    }
}
