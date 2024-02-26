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
    private async Task GetSkinFromDatabase(ulong steamId)
    {
        var result = await (await _collection.FindAsync(x => x.SteamId == steamId)).SingleOrDefaultAsync();
        
        var weaponInfos = new Dictionary<int, Skin.SkinDetail>();
        foreach (var (weapon, detail) in result.Details)
        {
            weaponInfos[weapon] = detail;
        }
        
        gPlayerWeaponsInfo[steamId] = weaponInfos;
        
        if (string.IsNullOrEmpty(result.Knife))
	        g_playersKnife[steamId] = result.Knife;
        
        if (result.Glove != 0)
	        g_playersGlove[steamId] = result.Glove;
    }
    
    private void ChangeWeaponAttributes(CBasePlayerWeapon weapon, CCSPlayerController player, bool isKnife = false)
	{
		if (!player.IsValid()) return;
		if (weapon is null || !weapon.IsValid || !gPlayerWeaponsInfo.ContainsKey(player.SteamID)) return;
		if (isKnife && !g_playersKnife.ContainsKey(player.SteamID) || isKnife && g_playersKnife[player.SteamID] == "weapon_knife") return;

		int weaponIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;
		
		if (!gPlayerWeaponsInfo[player.SteamID].ContainsKey(weaponIndex)) return;
		var detail = gPlayerWeaponsInfo[player.SteamID][weaponIndex];
		
		weapon.AttributeManager.Item.ItemID = 16384;
		weapon.AttributeManager.Item.ItemIDLow = 16384;
		weapon.AttributeManager.Item.ItemIDHigh = 0;
		weapon.FallbackPaintKit = detail.Paint;
		weapon.FallbackSeed = detail.Seed;
		weapon.FallbackWear = detail.Wear;
		weapon.AttributeManager.Item.EntityQuality = isKnife ? 3 : 0;
		if (!string.IsNullOrEmpty(detail.Name)) new SchemaString<CEconItemView>(weapon.AttributeManager.Item, "m_szCustomName").Set(detail.Name);

		if (weapon.FallbackPaintKit == 0) return;

		if (!isKnife && weapon.CBodyComponent != null && weapon.CBodyComponent.SceneNode != null)
		{
			var skeleton = GetSkeletonInstance(weapon.CBodyComponent.SceneNode);
			int[] newPaints = { 1171, 1170, 1169, 1164, 1162, 1161, 1159, 1175, 1174, 1167, 1165, 1168, 1163, 1160, 1166, 1173 };
			if (newPaints.Contains(weapon.FallbackPaintKit))
			{
				skeleton.ModelState.MeshGroupMask = 1;
			}
			else
			{
				if (skeleton.ModelState.MeshGroupMask != 2)
				{
					skeleton.ModelState.MeshGroupMask = 2;
				}
			}
		}

		var viewModels1 = GetPlayerViewModels(player);
		if (viewModels1 == null || viewModels1.Length == 0) return;

		var viewModel1 = viewModels1[0];
		if (viewModel1 == null || viewModel1.Value == null || viewModel1.Value.Weapon.Value == null) return;

		Utilities.SetStateChanged(viewModel1.Value, "CBaseEntity", "m_CBodyComponent");
	}

	private void GiveKnifeToPlayer(CCSPlayerController player)
	{
		if (!player.IsValid()) return;

		Plugin.AddTimer(1.0f, () =>
		{
			if (PlayerHasKnife(player)) return;

			var exist = g_playersKnife.TryGetValue(player.SteamID, out var knife);
			if (!exist) knife = player.Team == CsTeam.Terrorist ? "weapon_knife_t" : "weapon_knife";

			player.GiveNamedItem(knife);
		});
	}

	private bool PlayerHasKnife(CCSPlayerController player)
	{
		if (!player.IsValid() || player.PlayerPawn.Value == null || player.PlayerPawn.Value.WeaponServices == null || player.PlayerPawn.Value.ItemServices == null) return false;
		
		var weapons = player.PlayerPawn.Value.WeaponServices?.MyWeapons;
		
		return weapons != null &&
		       weapons
			       .Where(weapon => weapon.IsValid && weapon.Value != null && weapon.Value.IsValid)
			       .Any(weapon => weapon.Value.DesignerName.Contains("knife") || weapon.Value.DesignerName.Contains("bayonet"));
	}

	private void RefreshWeapons(CCSPlayerController player)
	{
		if (!player.IsValid() || player.PlayerPawn.Value == null || player.PlayerPawn.Value.WeaponServices == null || player.PlayerPawn.Value.ItemServices == null) return;

		var weapons = player.PlayerPawn.Value.WeaponServices.MyWeapons;
		if (weapons.Count == 0) return;
		if (player.Team is CsTeam.None or CsTeam.Spectator) return;

		var weaponsWithAmmo = new Dictionary<string, List<(int, int)>>();

		foreach (var weapon in weapons)
		{
			if (!weapon.IsValid || weapon.Value == null || !weapon.Value.IsValid || !weapon.Value.DesignerName.Contains("weapon_")) continue;

			var gun = weapon.Value.As<CCSWeaponBaseGun>();
			if (weapon.Value.Entity == null || !weapon.Value.OwnerEntity.IsValid || gun.Entity == null || !gun.IsValid || !gun.VisibleinPVS) continue;

			try
			{
				var weaponData = weapon.Value.As<CCSWeaponBase>().VData;

				if (weaponData == null) continue;

				if (weaponData.GearSlot is gear_slot_t.GEAR_SLOT_RIFLE or gear_slot_t.GEAR_SLOT_PISTOL)
				{
					if (!WeaponDefindex.TryGetValue(weapon.Value.AttributeManager.Item.ItemDefinitionIndex, out var weaponIndex)) continue;

					var clip1 = weapon.Value.Clip1;
					var reservedAmmo = weapon.Value.ReserveAmmo[0];

					if (weaponIndex != null && !weaponsWithAmmo.ContainsKey(weaponIndex))
					{
						weaponsWithAmmo.Add(weaponIndex, new List<(int, int)>());
					}

					if (weaponIndex != null) weaponsWithAmmo[weaponIndex].Add((clip1, reservedAmmo));
				}
			}
			catch (Exception)
			{
				// ignore
			}
		}

		for (var i = 1; i <= 3; i++)
		{
			player.ExecuteClientCommand($"slot {i}");
			player.ExecuteClientCommand($"slot {i}");

			Plugin.AddTimer(0.2f, () =>
			{
				var weapon = player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value;
				if (weapon is null || !weapon.IsValid) return;

				var gun = weapon.As<CCSWeaponBaseGun>();
				if (gun.VData == null || gun.VData.GearSlot is gear_slot_t.GEAR_SLOT_C4 or gear_slot_t.GEAR_SLOT_GRENADES) return;
				
				player.DropActiveWeapon();

				Plugin.AddTimer(0.25f, () =>
				{
					if (gun.IsValid && gun.State == CSWeaponState_t.WEAPON_NOT_CARRIED)
					{
						weapon.Remove();
					}
				});
			});
		}

		Plugin.AddTimer(1.2f, () =>
		{
			GiveKnifeToPlayer(player);

			foreach (var entry in weaponsWithAmmo)
			{
				foreach (var ammo in entry.Value)
				{
					var newWeapon = new CBasePlayerWeapon(player.GiveNamedItem(entry.Key));
					Server.NextFrame(() =>
					{
						try
						{
							newWeapon.Clip1 = ammo.Item1;
							newWeapon.ReserveAmmo[0] = ammo.Item2;
						}
						catch (Exception)
						{
							// ignore
						}
					});
				}
			}
		}, TimerFlags.STOP_ON_MAPCHANGE);
	}

	private void RefreshGloves(CCSPlayerController player)
	{
		if (!player.IsValid()) return;

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
				if (!player.IsValid()) return;
				if (!g_playersGlove.TryGetValue(player.SteamID, out var gloveId) || gloveId == 0) return;
				
				var detail = gPlayerWeaponsInfo[player.SteamID][gloveId];

				var item = pawn.EconGloves;
				item.ItemDefinitionIndex = gloveId;
				item.ItemIDLow = 16384;
				item.ItemIDHigh = 16384;

				CAttributeList_SetOrAddAttributeValueByName.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture prefab", detail.Paint);
				CAttributeList_SetOrAddAttributeValueByName.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture seed", detail.Seed);
				CAttributeList_SetOrAddAttributeValueByName.Invoke(item.NetworkedDynamicAttributes.Handle, "set item texture wear", detail.Wear);

				item.Initialized = true;

				CBaseModelEntity_SetBodygroup.Invoke(pawn, "default_gloves", 1);
			}
			catch (Exception)
			{
				// ignored
			}
		}, TimerFlags.STOP_ON_MAPCHANGE);
	}

	private static CSkeletonInstance GetSkeletonInstance(NativeObject node)
	{
		return new CSkeletonInstance(VirtualFunction.Create<nint, nint>(node.Handle, 8)(node.Handle));
	}

	private static CHandle<CBaseViewModel>[] GetPlayerViewModels(CCSPlayerController player)
	{
		return !player.IsValid() ? null :
			GetFixedArray<CHandle<CBaseViewModel>>(new CCSPlayer_ViewModelServices(player.PlayerPawn.Value.ViewModelServices!.Handle).Handle, "CCSPlayer_ViewModelServices", "m_hViewModel", 3);
	}

	private static T[] GetFixedArray<T>(nint pointer, string @class, string member, int length) where T : CHandle<CBaseViewModel>
	{
		var ptr = pointer + Schema.GetSchemaOffset(@class, member);
		var references = MemoryMarshal.CreateSpan(ref ptr, length);
		var values = new T[length];

		for (var i = 0; i < length; i++)
		{
			values[i] = (T)Activator.CreateInstance(typeof(T), references[i])!;
		}

		return values;
	}
}