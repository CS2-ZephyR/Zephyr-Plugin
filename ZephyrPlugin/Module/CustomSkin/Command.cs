using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module
{
	public override void RegisterCommands()
	{
		Plugin.AddCommand("css_skin", "Skins refresh", OnCommandRefresh);
	}

	private void OnCommandRefresh(CCSPlayerController player, CommandInfo command)
	{
		if (!player.IsValid()) return;

		GetSkinFromDatabase(player.SteamID).Wait();

		RefreshGloves(player);
		RefreshWeapons(player);
	}
}
