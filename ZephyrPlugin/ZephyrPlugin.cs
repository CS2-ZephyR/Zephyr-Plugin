using CounterStrikeSharp.API.Core;
using ZephyrPlugin.Module;
using ZephyrPlugin.Util;

namespace ZephyrPlugin;

public class ZephyrPlugin : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "ZephyrPlugin";
    public override string ModuleVersion => "1.0.0";

    public Config Config { get; set; }

    private readonly Logger _logger = new("Core");

    public override void Load(bool hotReload)
    {
        Util.Logger.PrintLogo();
        Database.Init();

        foreach (var zephyrModule in ZephyrModule.Modules)
        {
            zephyrModule.InjectPlugin(this);
            zephyrModule.OnLoad(hotReload);
            zephyrModule.RegisterCommands();
            zephyrModule.RegisterEvents();
            
            RegisterListener<Listeners.OnMapStart>((_) =>
            {
                zephyrModule.RegisterTimers();
            });
            
            _logger.Info($"{zephyrModule.ModuleName} is enabled.");
        }
    }

    public void OnConfigParsed(Config config)
    {
        Config = config;
        Database.Config = config.MongoConfig;

        if (config.MongoConfig.Password.Length == 0)
        {
            _logger.Error("MongoDB 인증 정보가 비어있습니다. 설정 후 서버를 실행해주세요.");
        }
    }
}
