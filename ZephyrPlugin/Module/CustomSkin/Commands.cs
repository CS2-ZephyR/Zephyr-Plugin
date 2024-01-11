using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using ZephyrPlugin.Module.CustomSkin.Util;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module
{
	public override void RegisterCommands()
	{
		Plugin.AddCommand($"css_skin", "Skins refresh", OnCommandRefresh);
	}

	private void OnCommandRefresh(CCSPlayerController player, CommandInfo command)
	{
		if (!player.IsValid()) return;

		var steamId = player.SteamID;

		Task.Run(async () =>
		{
			await SkinDatabase.Fetch(steamId);
		});

		SkinUtil.RefreshWeapons(player);
		Logger.Chat(player, "스킨을 불러왔습니다.");
	}
}
