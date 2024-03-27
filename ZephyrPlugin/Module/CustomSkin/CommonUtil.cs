using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using MongoDB.Driver;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module
{
	private void GetSkinData(CBasePlayerController player)
	{
		var steamId = player.SteamID;
		
		var result = _collection.Find(x => x.SteamId == steamId).SingleOrDefault();
        
		_playerDetails[steamId] = new Dictionary<int, Skin.SkinDetail>();
		_playerAgent[steamId] = new Dictionary<CsTeam, string>();
        
		foreach (var (weapon, detail) in result.Detail)
		{
			_playerDetails[steamId][weapon] = detail;
		}
        
		if (!string.IsNullOrEmpty(result.Knife))
			_playerKnife[steamId] = result.Knife;

		if (result.Glove != 0)
			_playerGlove[steamId] = result.Glove;

		if (result.Music != 0)
			_playerMusic[steamId] = result.Music;

		if (!string.IsNullOrEmpty(result.Agent.Ct))
			_playerAgent[steamId][CsTeam.CounterTerrorist] = result.Agent.Ct;
		
		if (!string.IsNullOrEmpty(result.Agent.T))
			_playerAgent[steamId][CsTeam.Terrorist] = result.Agent.T;
	}
	
    private void GivePlayerWeaponSkin(CCSPlayerController player, CBasePlayerWeapon weapon)
	{
		if (!player.IsValid() || !weapon.IsValid) return;
		if (!_playerDetails.TryGetValue(player.SteamID, out var playerDetail)) return;

		int[] newPaints = [1171, 1170, 1169, 1164, 1162, 1161, 1159, 1175, 1174, 1167, 1165, 1168, 1163, 1160, 1166, 1173];
		
		var isKnife = weapon.DesignerName.Contains("knife") || weapon.DesignerName.Contains("bayonet");
		switch (isKnife)
		{
			case true when !_playerKnife.ContainsKey(player.SteamID):
			case true when _playerKnife[player.SteamID] == "weapon_knife":
				return;
			
			case true:
			{
				var newDefIndex = WeaponIndex.FirstOrDefault(x => x.Value == _playerKnife[player.SteamID]);
				if (newDefIndex.Key == 0) return;

				if (weapon.AttributeManager.Item.ItemDefinitionIndex != newDefIndex.Key)
				{
					SubclassChange(weapon, (ushort) newDefIndex.Key);
				}

				weapon.AttributeManager.Item.ItemDefinitionIndex = (ushort)newDefIndex.Key;
				weapon.AttributeManager.Item.EntityQuality = 3;
				break;
			}
		}

		if (!playerDetail.TryGetValue(weapon.AttributeManager.Item.ItemDefinitionIndex, out var detail) || detail.Paint == 0) return;

		weapon.AttributeManager.Item.ItemID = 16384;
		weapon.AttributeManager.Item.ItemIDLow = 16384;
		weapon.AttributeManager.Item.ItemIDHigh = 0;
		weapon.FallbackPaintKit = detail.Paint;
		weapon.FallbackSeed = detail.Seed;
		weapon.FallbackWear = detail.Wear;
		
		if (!string.IsNullOrEmpty(detail.Name))
			new SchemaString<CEconItemView>(weapon.AttributeManager.Item, "m_szCustomName").Set(detail.Name);
		
		_vFunc1.Invoke(weapon.AttributeManager.Item.NetworkedDynamicAttributes.Handle, "set item texture prefab", weapon.FallbackPaintKit);

		if (weapon.FallbackPaintKit == 0 || isKnife) return;

		UpdatePlayerWeaponMeshGroupMask(player, weapon, !newPaints.Contains(weapon.FallbackPaintKit));
	}

	private void RefreshGloves(CCSPlayerController player)
	{
		if (!player.IsValid() || (LifeState_t)player.LifeState != LifeState_t.LIFE_ALIVE) return;

		var pawn = player.PlayerPawn.Value;
		if (pawn == null || !pawn.IsValid || pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;

		var model = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance()?.ModelState.ModelName ?? string.Empty;
		if (!string.IsNullOrEmpty(model))
		{
			pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
			pawn.SetModel(model);
		}

		Plugin.AddTimer(0.06f, () =>
		{
			if (!player.IsValid) return;
			if (!_playerGlove.TryGetValue(player.SteamID, out var gloveInfo) || gloveInfo == 0) return;
			if (!_playerDetails.TryGetValue(player.SteamID, out var playerDetail) ||
			    !playerDetail.TryGetValue(gloveInfo, out var gloveDetail)) return;
			
			var pawn2 = player.PlayerPawn.Value;
			if (pawn2 == null || !pawn2.IsValid || pawn2.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;

			var item = pawn2.EconGloves;
			item.ItemDefinitionIndex = (ushort) gloveInfo;
			item.ItemIDLow = 16384 & 0xFFFFFFFF;
			item.ItemIDHigh = 16384;

			_vFunc1.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture prefab", gloveDetail.Paint);
			_vFunc1.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture seed", gloveDetail.Seed);
			_vFunc1.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture wear", gloveDetail.Wear);

			item.Initialized = true;

			_vFunc2.Invoke(pawn2, "default_gloves", 1);
		}, TimerFlags.STOP_ON_MAPCHANGE);
	}

	private void SubclassChange(NativeEntity weapon, ushort itemD)
	{
		var subclassChangeFunc = VirtualFunction.Create<nint, string, int>(
			GameData.GetSignature("ChangeSubclass")
		);

		subclassChangeFunc(weapon.Handle, itemD.ToString());
	}

	private void UpdateWeaponMeshGroupMask(CBaseEntity weapon, bool isLegacy = false)
	{
		if (weapon.CBodyComponent?.SceneNode == null) return;
		
		var skeleton = weapon.CBodyComponent.SceneNode.GetSkeletonInstance();
		var value = (ulong)(isLegacy ? 2 : 1);

		if (skeleton.ModelState.MeshGroupMask != value)
		{
			skeleton.ModelState.MeshGroupMask = value;
		}
	}

	private void UpdatePlayerWeaponMeshGroupMask(CCSPlayerController player, CBasePlayerWeapon weapon, bool isLegacy)
	{
		UpdateWeaponMeshGroupMask(weapon, isLegacy);

		var viewModel = GetPlayerViewModel(player);
		if (viewModel == null || viewModel.Weapon.Value == null || viewModel.Weapon.Value.Index != weapon.Index) return;
		
		UpdateWeaponMeshGroupMask(viewModel, isLegacy);
		Utilities.SetStateChanged(viewModel, "CBaseEntity", "m_CBodyComponent");
	}

	private void GivePlayerAgent(CCSPlayerController player)
	{
		if (!_playerAgent.TryGetValue(player.SteamID, out var playerAgent) || !playerAgent.TryGetValue(player.Team, out var model)) return;
		if (player.PlayerPawn.Value == null) return;
		if (string.IsNullOrEmpty(model)) return;
		
		Server.NextFrame(() =>
		{
			player.PlayerPawn.Value.SetModel(
				$"characters/models/{model}.vmdl"
			);
		});
	}

	private void GiveMusicKit(CCSPlayerController player)
	{
		if (!_playerMusic.ContainsKey(player.SteamID)) return;
		if (player.InventoryServices == null) return;

		player.InventoryServices.MusicID = (ushort)_playerMusic[player.SteamID];
	}

	private CBaseViewModel GetPlayerViewModel(CCSPlayerController player)
	{
		if (player.PlayerPawn.Value == null || player.PlayerPawn.Value.ViewModelServices == null) return null;
		CCSPlayer_ViewModelServices viewModelServices = new(player.PlayerPawn.Value.ViewModelServices!.Handle);
		var ptr = viewModelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
		var references = MemoryMarshal.CreateSpan(ref ptr, 3);
		var viewModel = (CHandle<CBaseViewModel>)Activator.CreateInstance(typeof(CHandle<CBaseViewModel>), references[0])!;
		if (viewModel == null || viewModel.Value == null) return null;
		return viewModel.Value;
	}
}