using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module
{
    public override void RegisterCommands()
    {
        Plugin.AddCommand("css_skin", "Refresh Skin", OnSkinCommand);
    }

    private void OnSkinCommand(CCSPlayerController player, CommandInfo _)
    {
        if (!player.IsValid()) return;
        
        GetSkinData(player);
        
        GivePlayerAgent(player);
        GiveMusicKit(player);
        RefreshGloves(player);
        RefreshWeapons(player);
        
        Logger.Chat(player, $"스킨 정보를 불러왔습니다.");
    }
}
