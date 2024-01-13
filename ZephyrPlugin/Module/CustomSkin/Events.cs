using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using ZephyrPlugin.Module.CustomSkin.Util;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module
{
	public override void RegisterEvents()
	{
		Plugin.RegisterListener<Listeners.OnEntityCreated>(OnEntityCreated);
		Plugin.RegisterListener<Listeners.OnClientAuthorized>(OnClientAuthorized);
		Plugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
		Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);

		Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
		Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
		Plugin.RegisterEventHandler<EventItemPurchase>(OnEventItemPurchasePost);
		Plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart, HookMode.Pre);

		Plugin.HookEntityOutput("weapon_knife", "OnPlayerPickup", OnPickup);
	}

	private void OnClientAuthorized(int playerSlot, SteamID steamId)
	{
		var player = Utilities.GetPlayerFromIndex(playerSlot + 1);

		if (!player.IsValid()) return;

		Task.Run(async () => await SkinDatabase.Fetch(steamId.SteamId64));
	}

	private void OnClientDisconnect(int playerSlot)
	{
		var player = Utilities.GetPlayerFromSlot(playerSlot);

		if (!player.IsValid()) return;

		PlayerKnife.Remove(player.SteamID);
		PlayerSkin.Remove(player.SteamID);
	}

	private void OnEntityCreated(CEntityInstance entity)
	{
		var designerName = entity.DesignerName;

		if (!SkinUtil.WeaponList.ContainsKey(designerName)) return;

		var weapon = new CBasePlayerWeapon(entity.Handle);

		Server.NextFrame(() =>
		{
			if (!weapon.IsValid || weapon.OwnerEntity.Value == null || weapon.OwnerEntity.Index <= 0) return;

			var pawn = new CBasePlayerPawn(NativeAPI.GetEntityFromIndex((int)weapon.OwnerEntity.Index));
			if (!pawn.IsValid) return;

			var player = Utilities.GetPlayerFromIndex((int)pawn.Controller.Index);
			if (!player.IsValid()) return;

			SkinUtil.ChangeWeaponAttributes(weapon, player);
		});
	}

	private HookResult OnPickup(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
	{
		var player = Utilities.GetEntityFromIndex<CCSPlayerPawn>((int)activator.Index).OriginalController.Value;

		if (!player.IsValid()) return HookResult.Continue;
		if (!PlayerPickup.TryGetValue(player!.SteamID, out var index) || !PlayerKnife.TryGetValue(player.SteamID, out var knife)) return HookResult.Continue;

		var weapon = new CBasePlayerWeapon(caller.Handle);

		if (weapon.AttributeManager.Item.ItemDefinitionIndex != 42 && weapon.AttributeManager.Item.ItemDefinitionIndex != 59) return HookResult.Continue;

		if (index >= 2 || knife == "weapon_knife") return HookResult.Continue;

		PlayerPickup[player.SteamID] = ++index;
		player.RemoveItemByDesignerName(weapon.DesignerName);

		return HookResult.Continue;
	}

	private void OnMapStart(string mapName)
	{
		Plugin.AddTimer(2.0f, () =>
		{
			NativeAPI.IssueServerCommand("mp_t_default_melee \"\"");
			NativeAPI.IssueServerCommand("mp_ct_default_melee \"\"");
			NativeAPI.IssueServerCommand("mp_equipment_reset_rounds 0");
		});

		Plugin.AddTimer(10.0f, () =>
		{
			foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid()).Where(player => !PlayerSkin.ContainsKey(player.SteamID)))
			{
				var steamId = player.SteamID;

				_ = SkinDatabase.Fetch(steamId);
			}
		}, CounterStrikeSharp.API.Modules.Timers.TimerFlags.STOP_ON_MAPCHANGE | CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
	}

	private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
	{
		var player = @event.Userid;

		if (!player.IsValid()) return HookResult.Continue;

		var steamId = player.SteamID;

		if (PlayerSkin.ContainsKey(player!.SteamID)) return HookResult.Continue;

		Task.Run(async () => await SkinDatabase.Fetch(steamId));

		return HookResult.Continue;
	}

	private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		var player = @event.Userid;

		if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

		if (SkinUtil.HasKnife(player)) return HookResult.Continue;

		PlayerPickup[player.SteamID] = 0;
		Plugin.AddTimer(0.1f, () => SkinUtil.GiveKnife(player));

		Plugin.AddTimer(0.3f, () => SkinUtil.RefreshSkins(player));

		return HookResult.Continue;
	}

	private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
	{
		NativeAPI.IssueServerCommand("mp_t_default_melee \"\"");
		NativeAPI.IssueServerCommand("mp_ct_default_melee \"\"");
		NativeAPI.IssueServerCommand("mp_equipment_reset_rounds 0");

		return HookResult.Continue;
	}

	private HookResult OnEventItemPurchasePost(EventItemPurchase @event, GameEventInfo info)
	{
		var player = @event.Userid;

		if (!player.IsValid()) return HookResult.Continue;

		Plugin.AddTimer(0.2f, () => SkinUtil.RefreshSkins(player));

		return HookResult.Continue;
	}
}

