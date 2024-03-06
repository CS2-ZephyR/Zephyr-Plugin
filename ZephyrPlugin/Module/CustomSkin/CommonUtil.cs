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

		if (!string.IsNullOrEmpty(result.Agent.Ct))
			_playerAgent[steamId][CsTeam.CounterTerrorist] = result.Agent.Ct;
		
		if (!string.IsNullOrEmpty(result.Agent.T))
			_playerAgent[steamId][CsTeam.Terrorist] = result.Agent.T;
	}
	
    private void GivePlayerWeaponSkin(CCSPlayerController player, CEconEntity weapon)
	{
		if (!player.IsValid()) return;

		if (!_playerDetails.ContainsKey(player.SteamID)) return;
		int[] newPaints = { 1171, 1170, 1169, 1164, 1162, 1161, 1159, 1175, 1174, 1167, 1165, 1168, 1163, 1160, 1166, 1173 };
		
		var isKnife = weapon.DesignerName.Contains("knife") || weapon.DesignerName.Contains("bayonet");

		switch (isKnife)
		{
			case true when !_playerKnife.ContainsKey(player.SteamID):
			case true when _playerKnife[player.SteamID] == "weapon_knife":
				return;
			case true:
			{
				var newDefIndex = WeaponDefindex.FirstOrDefault(x => x.Value == _playerKnife[player.SteamID]);
				if (newDefIndex.Key == 0) return;

				if (weapon.AttributeManager.Item.ItemDefinitionIndex != newDefIndex.Key)
				{
					SubclassChange(weapon, (ushort)newDefIndex.Key);
				}

				weapon.AttributeManager.Item.ItemDefinitionIndex = (ushort)newDefIndex.Key;
				weapon.AttributeManager.Item.EntityQuality = 3;
				break;
			}
		}
		
		int weaponDefIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;

		if (!_playerDetails[player.SteamID].ContainsKey(weaponDefIndex)) return;
		var weaponInfo = _playerDetails[player.SteamID][weaponDefIndex];
		
		weapon.AttributeManager.Item.ItemID = 16384;
		weapon.AttributeManager.Item.ItemIDLow = 16384;
		weapon.AttributeManager.Item.ItemIDHigh = 0;
		weapon.FallbackPaintKit = weaponInfo.Paint;
		weapon.FallbackSeed = weaponInfo.Seed;
		weapon.FallbackWear = weaponInfo.Wear;
		CAttributeList_SetOrAddAttributeValueByName.Invoke(weapon.AttributeManager.Item.NetworkedDynamicAttributes.Handle, "set item texture prefab", weapon.FallbackPaintKit);

		if (weapon.FallbackPaintKit == 0)
			return;

		if (!isKnife)
		{
			UpdatePlayerWeaponMeshGroupMask(player, weapon, !newPaints.Contains(weapon.FallbackPaintKit));
		}
	}
    
	private void RefreshGloves(CCSPlayerController player)
	{
		if (!player.IsValid() || (LifeState_t)player.LifeState != LifeState_t.LIFE_ALIVE) return;

		var pawn = player.PlayerPawn.Value;
		if (pawn == null || !pawn.IsValid || pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;

		var model = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? string.Empty;
		if (!string.IsNullOrEmpty(model))
		{
			pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
			pawn.SetModel(model);
		}

		Plugin.AddTimer(0.06f, () =>
		{
			try
			{
				if (!player.IsValid || !pawn.IsValid || pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;
				if (!_playerGlove.TryGetValue(player.SteamID, out var gloveInfo) || gloveInfo == 0) return;
				
				var weaponInfo = _playerDetails[player.SteamID][gloveInfo];

				var item = pawn.EconGloves;
				item.ItemDefinitionIndex = gloveInfo;
				item.ItemIDLow = 16384 & 0xFFFFFFFF;
				item.ItemIDHigh = 16384;

				CAttributeList_SetOrAddAttributeValueByName.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture prefab", weaponInfo.Paint);
				CAttributeList_SetOrAddAttributeValueByName.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture seed", weaponInfo.Seed);
				CAttributeList_SetOrAddAttributeValueByName.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture wear", weaponInfo.Wear);

				item.Initialized = true;

				CBaseModelEntity_SetBodygroup.Invoke(pawn, "default_gloves", 1);
			}
			catch (Exception)
			{
				// ignored
			}
		}, TimerFlags.STOP_ON_MAPCHANGE);
	}

	private void SubclassChange(NativeEntity weapon, ushort itemD)
	{
		VirtualFunction.Create<nint, string, int>(GameData.GetSignature("ChangeSubclass"))(weapon.Handle, itemD.ToString());
	}

	private void UpdateWeaponMeshGroupMask(CBaseEntity weapon, bool isLegacy = false)
	{
		if (weapon.CBodyComponent?.SceneNode == null) return;
		
		var skeleton = weapon.CBodyComponent.SceneNode.GetSkeletonInstance();
		{
			var value = (ulong)(isLegacy ? 2 : 1);

			if (skeleton.ModelState.MeshGroupMask != value)
			{
				skeleton.ModelState.MeshGroupMask = value;
			}
		}
	}

	private void UpdatePlayerWeaponMeshGroupMask(CCSPlayerController player, CBaseEntity weapon, bool isLegacy)
	{
		UpdateWeaponMeshGroupMask(weapon, isLegacy);

		var viewModel = GetPlayerViewModel(player);
		if (viewModel == null || viewModel.Weapon.Value == null || viewModel.Weapon.Value.Index != weapon.Index) return;
		
		UpdateWeaponMeshGroupMask(viewModel, isLegacy);
		Utilities.SetStateChanged(viewModel, "CBaseEntity", "m_CBodyComponent");
	}

	private void GivePlayerAgent(CCSPlayerController player)
	{
		if (!_playerAgent.ContainsKey(player.SteamID)) return;

		try
		{
			Server.NextFrame(() =>
			{
				_playerAgent[player.SteamID].TryGetValue(player.Team, out var model);
				if (model == null) return;

				player.PlayerPawn.Value!.SetModel($"characters/models/{model}.vmdl");
			});
		}
		catch (Exception)
		{
			// ignored
		}
	}

	private CCSPlayerController GetPlayerFromItemServices(NativeObject itemServices)
	{
		return Utilities.GetPlayers().FirstOrDefault(player => player.PlayerPawn.Value != null && player.PlayerPawn.Value.ItemServices != null && player.PlayerPawn.Value != null && player.PlayerPawn.Value.ItemServices.Handle == itemServices.Handle);
	}

	private static CBaseViewModel GetPlayerViewModel(CCSPlayerController player)
	{
		if (player.PlayerPawn.Value == null || player.PlayerPawn.Value.ViewModelServices == null) return null;
		
		var viewModelServices = new CCSPlayer_ViewModelServices(player.PlayerPawn.Value.ViewModelServices!.Handle);
		var ptr = viewModelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
		var references = MemoryMarshal.CreateSpan(ref ptr, 3);
		
		var viewModel = (CHandle<CBaseViewModel>)Activator.CreateInstance(typeof(CHandle<CBaseViewModel>), references[0])!;
		if (viewModel == null || viewModel.Value == null) return null;
		
		return viewModel.Value;
	}
}