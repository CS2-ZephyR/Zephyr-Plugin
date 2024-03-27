using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module
{
	public override void RegisterEvents()
	{
		Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnClientFullConnect);
		Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
		
		Plugin.RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
	}
	
	private HookResult OnClientFullConnect(EventPlayerConnectFull @event, GameEventInfo info)
	{
		var player = @event.Userid;

		if (!player.IsValid()) return HookResult.Continue;
		
		GetSkinData(player);

		return HookResult.Continue;
	}

	private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		var player = @event.Userid;
		if (!player.IsValid()) return HookResult.Continue;

		var pawn = player.PlayerPawn.Value;
		if (pawn == null || !pawn.IsValid) return HookResult.Continue;

		GiveMusicKit(player);
		GivePlayerAgent(player);
		Server.NextFrame(() =>
		{
			RefreshGloves(player);
		});

		return HookResult.Continue;
	}
	
	private void OnEntitySpawned(CEntityInstance entity)
	{
		var designerName = entity.DesignerName;
		if (!designerName.Contains("weapon")) return;
		
		Server.NextFrame(() =>
		{
			try
			{
				var weapon = new CBasePlayerWeapon(entity.Handle);
				if (!weapon.IsValid || weapon.OwnerEntity.Value == null) return;

				SteamID steamId = null;
				CCSPlayerController player;

				if (weapon.OriginalOwnerXuidLow > 0)
				{
					steamId = new SteamID(weapon.OriginalOwnerXuidLow);
				}

				if (steamId != null && steamId.IsValid())
				{
					player = Utilities.GetPlayers().FirstOrDefault(p => p.IsValid() && p.SteamID == steamId.SteamId64);

					if (player == null)
					{
						player = Utilities.GetPlayerFromSteamId(weapon.OriginalOwnerXuidLow);
					}
				}
				else
				{
					var gun = weapon.As<CCSWeaponBaseGun>();
					player = Utilities.GetPlayerFromIndex((int)weapon.OwnerEntity.Index);
				}

				if (string.IsNullOrEmpty(player?.PlayerName)) return;
				if (!player.IsValid()) return;

				GivePlayerWeaponSkin(player, weapon);
			}
			catch (Exception)
			{
				// ignored
			}
		});
		
	}
}