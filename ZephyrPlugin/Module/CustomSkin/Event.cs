using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
		
		VirtualFunctions.GiveNamedItemFunc.Hook(OnGiveNamedItemPost, HookMode.Post);
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

		GivePlayerAgent(player);
		Server.NextFrame(() =>
		{
			RefreshGloves(player);
		});

		return HookResult.Continue;
	}
	
	private HookResult OnGiveNamedItemPost(DynamicHook hook)
	{
		var itemServices = hook.GetParam<CCSPlayer_ItemServices>(0);
		var weapon = hook.GetReturn<CBasePlayerWeapon>(0);
		if (!weapon.DesignerName.Contains("weapon")) return HookResult.Continue;
		
		var player = GetPlayerFromItemServices(itemServices);
		if (player != null) GivePlayerWeaponSkin(player, weapon);

		return HookResult.Continue;
	}

	private void OnEntitySpawned(CEntityInstance entity)
	{
		var designerName = entity.DesignerName;
		if (!designerName.Contains("weapon")) return;
		
		Server.NextFrame(() =>
		{
			var weapon = new CBasePlayerWeapon(entity.Handle);
			if (!weapon.IsValid || weapon.OwnerEntity.Value == null) return;

			var player = Utilities.GetPlayerFromIndex((int)weapon.OwnerEntity.Value.Index);
			if (!player.IsValid()) return;

			GivePlayerWeaponSkin(player, weapon);
		});
	}
}