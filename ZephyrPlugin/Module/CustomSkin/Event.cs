using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module
{
	public override void RegisterEvents()
	{
		Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnClientFullConnect);
		Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
		Plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart, HookMode.Pre);
		
		Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
		Plugin.RegisterListener<Listeners.OnEntitySpawned>(OnEntityCreated);
		Plugin.RegisterListener<Listeners.OnTick>(OnTick);
	}

	private HookResult OnClientFullConnect(EventPlayerConnectFull @event, GameEventInfo info)
	{
		var player = @event.Userid;

		if (!player.IsValid()) return HookResult.Continue;

		var steamId = player.SteamID;

		Task.Run(async () =>
		{
			await GetSkinFromDatabase(steamId);
		});

		return HookResult.Continue;
	}

	private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		var player = @event.Userid;
		if (!player.IsValid()) return HookResult.Continue;

		var pawn = player.PlayerPawn.Value;
		if (pawn == null || !pawn.IsValid) return HookResult.Continue;

		if (!PlayerHasKnife(player))
		{
			GiveKnifeToPlayer(player);
		}

		Server.NextFrame(() =>
		{
			RefreshGloves(player);
		});

		return HookResult.Continue;
	}

	private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
	{
		NativeAPI.IssueServerCommand("mp_t_default_melee \"\"");
		NativeAPI.IssueServerCommand("mp_ct_default_melee \"\"");
		NativeAPI.IssueServerCommand("mp_equipment_reset_rounds 0");

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
	}
	
	private void OnEntityCreated(CEntityInstance entity)
	{
		if (entity == null || !entity.IsValid || string.IsNullOrEmpty(entity.DesignerName)) return;
		
		if (!WeaponList.ContainsKey(entity.DesignerName)) return;
		
		var weapon = new CBasePlayerWeapon(entity.Handle);
		var isKnife = entity.DesignerName.Contains("knife") || entity.DesignerName.Contains("bayonet");

		Server.NextFrame(() =>
		{
			if (!weapon.IsValid || weapon.OwnerEntity.Value == null || weapon.OwnerEntity.Index <= 0) return;
			
			var weaponOwner = (int)weapon.OwnerEntity.Index;
			
			var pawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>(weaponOwner);
			if (!pawn.IsValid) return;

			var player = Utilities.GetPlayerFromIndex((int)pawn.Controller.Index);
			if (!player.IsValid()) return;

			ChangeWeaponAttributes(weapon, player, isKnife);
		});
	}
	
	private void OnTick()
	{
		try
		{
			foreach (var player in Utilities.GetPlayers().Where(x => x.IsValid()))
			{
				if (player.PlayerPawn.Value != null &&
				    (player.PlayerPawn.IsValid != true || player.PlayerPawn.Value.IsValid != true)) continue;

				var viewModels = GetPlayerViewModels(player);
				if (viewModels == null || viewModels.Length == 0) continue;

				var viewModel = viewModels[0];
				if (viewModel == null || viewModel.Value == null || viewModel.Value.Weapon.Value == null) continue;
				if (viewModel.Value.VMName.Contains("knife")) continue;

				var weapon = viewModel.Value.Weapon.Value;
				if (weapon == null || !weapon.IsValid || weapon.FallbackPaintKit == 0) continue;

				var sceneNode = viewModel.Value.CBodyComponent?.SceneNode;
				if (sceneNode == null) continue;

				var skeleton = GetSkeletonInstance(sceneNode);
				if (skeleton == null) continue;

				var skeletonChange = false;

				int[] newPaints =
					{ 1171, 1170, 1169, 1164, 1162, 1161, 1159, 1175, 1174, 1167, 1165, 1168, 1163, 1160, 1166, 1173 };
				if (newPaints.Contains(weapon.FallbackPaintKit))
				{
					if (skeleton.ModelState.MeshGroupMask != 1)
					{
						skeleton.ModelState.MeshGroupMask = 1;
						skeletonChange = true;
					}
				}
				else
				{
					if (skeleton.ModelState.MeshGroupMask != 2)
					{
						skeleton.ModelState.MeshGroupMask = 2;
						skeletonChange = true;
					}
				}

				if (skeletonChange)
				{
					Utilities.SetStateChanged(viewModel.Value, "CBaseEntity", "m_CBodyComponent");
				}
			}
		}
		catch (Exception)
		{
			// ignored
		}
	}
}