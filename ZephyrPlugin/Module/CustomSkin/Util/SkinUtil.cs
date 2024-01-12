using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin.Util;

public static class SkinUtil
{
	private static ZephyrPlugin _plugin;

	public static readonly Dictionary<string, string> WeaponList = new()
	{
		{"weapon_deagle", "Desert Eagle"},
		{"weapon_elite", "Dual Berettas"},
		{"weapon_fiveseven", "Five-SeveN"},
		{"weapon_glock", "Glock-18"},
		{"weapon_ak47", "AK-47"},
		{"weapon_aug", "AUG"},
		{"weapon_awp", "AWP"},
		{"weapon_famas", "FAMAS"},
		{"weapon_g3sg1", "G3SG1"},
		{"weapon_galilar", "Galil AR"},
		{"weapon_m249", "M249"},
		{"weapon_m4a1", "M4A1"},
		{"weapon_mac10", "MAC-10"},
		{"weapon_p90", "P90"},
		{"weapon_mp5sd", "MP5-SD"},
		{"weapon_ump45", "UMP-45"},
		{"weapon_xm1014", "XM1014"},
		{"weapon_bizon", "PP-Bizon"},
		{"weapon_mag7", "MAG-7"},
		{"weapon_negev", "Negev"},
		{"weapon_sawedoff", "Sawed-Off"},
		{"weapon_tec9", "Tec-9"},
		{"weapon_hkp2000", "P2000"},
		{"weapon_mp7", "MP7"},
		{"weapon_mp9", "MP9"},
		{"weapon_nova", "Nova"},
		{"weapon_p250", "P250"},
		{"weapon_scar20", "SCAR-20"},
		{"weapon_sg556", "SG 553"},
		{"weapon_ssg08", "SSG 08"},
		{"weapon_m4a1_silencer", "M4A1-S"},
		{"weapon_usp_silencer", "USP-S"},
		{"weapon_cz75a", "CZ75-Auto"},
		{"weapon_revolver", "R8 Revolver"},
		{ "weapon_knife", "Default Knife" },
		{ "weapon_knife_m9_bayonet", "M9 Bayonet" },
		{ "weapon_knife_karambit", "Karambit" },
		{ "weapon_bayonet", "Bayonet" },
		{ "weapon_knife_survival_bowie", "Bowie Knife" },
		{ "weapon_knife_butterfly", "Butterfly Knife" },
		{ "weapon_knife_falchion", "Falchion Knife" },
		{ "weapon_knife_flip", "Flip Knife" },
		{ "weapon_knife_gut", "Gut Knife" },
		{ "weapon_knife_tactical", "Huntsman Knife" },
		{ "weapon_knife_push", "Shadow Daggers" },
		{ "weapon_knife_gypsy_jackknife", "Navaja Knife" },
		{ "weapon_knife_stiletto", "Stiletto Knife" },
		{ "weapon_knife_widowmaker", "Talon Knife" },
		{ "weapon_knife_ursus", "Ursus Knife" },
		{ "weapon_knife_css", "Classic Knife" },
		{ "weapon_knife_cord", "Paracord Knife" },
		{ "weapon_knife_canis", "Survival Knife" },
		{ "weapon_knife_outdoor", "Nomad Knife" },
		{ "weapon_knife_skeleton", "Skeleton Knife" }
	};

	public static Dictionary<int, string> WeaponIndex { get; } = new()
	{
		{ 1, "weapon_deagle" },
		{ 2, "weapon_elite" },
		{ 3, "weapon_fiveseven" },
		{ 4, "weapon_glock" },
		{ 7, "weapon_ak47" },
		{ 8, "weapon_aug" },
		{ 9, "weapon_awp" },
		{ 10, "weapon_famas" },
		{ 11, "weapon_g3sg1" },
		{ 13, "weapon_galilar" },
		{ 14, "weapon_m249" },
		{ 16, "weapon_m4a1" },
		{ 17, "weapon_mac10" },
		{ 19, "weapon_p90" },
		{ 23, "weapon_mp5sd" },
		{ 24, "weapon_ump45" },
		{ 25, "weapon_xm1014" },
		{ 26, "weapon_bizon" },
		{ 27, "weapon_mag7" },
		{ 28, "weapon_negev" },
		{ 29, "weapon_sawedoff" },
		{ 30, "weapon_tec9" },
		{ 32, "weapon_hkp2000" },
		{ 33, "weapon_mp7" },
		{ 34, "weapon_mp9" },
		{ 35, "weapon_nova" },
		{ 36, "weapon_p250" },
		{ 38, "weapon_scar20" },
		{ 39, "weapon_sg556" },
		{ 40, "weapon_ssg08" },
		{ 60, "weapon_m4a1_silencer" },
		{ 61, "weapon_usp_silencer" },
		{ 63, "weapon_cz75a" },
		{ 64, "weapon_revolver" },
		{ 500, "weapon_bayonet" },
		{ 503, "weapon_knife_css" },
		{ 505, "weapon_knife_flip" },
		{ 506, "weapon_knife_gut" },
		{ 507, "weapon_knife_karambit" },
		{ 508, "weapon_knife_m9_bayonet" },
		{ 509, "weapon_knife_tactical" },
		{ 512, "weapon_knife_falchion" },
		{ 514, "weapon_knife_survival_bowie" },
		{ 515, "weapon_knife_butterfly" },
		{ 516, "weapon_knife_push" },
		{ 517, "weapon_knife_cord" },
		{ 518, "weapon_knife_canis" },
		{ 519, "weapon_knife_ursus" },
		{ 520, "weapon_knife_gypsy_jackknife" },
		{ 521, "weapon_knife_outdoor" },
		{ 522, "weapon_knife_stiletto" },
		{ 523, "weapon_knife_widowmaker" },
		{ 525, "weapon_knife_skeleton" }
	};

	public static void InjectPlugin(ZephyrPlugin plugin)
	{
		_plugin = plugin;
	}

	public static void ChangeWeaponAttributes(CBasePlayerWeapon weapon, CCSPlayerController player)
	{
		if (!player.IsValid()) return;

		var isKnife = weapon.DesignerName.Contains("knife") || weapon.DesignerName.Contains("bayonet");

		if (isKnife && !Module.PlayerKnife.ContainsKey(player.SteamID) || isKnife && Module.PlayerKnife[player.SteamID] == "weapon_knife") return;

		if (!Module.PlayerSkin.TryGetValue(player.SteamID, out var skin)) return;
		if (!skin.TryGetValue(weapon.AttributeManager.Item.ItemDefinitionIndex, out var paint)) return;

		if (isKnife) weapon.AttributeManager.Item.EntityQuality = 3;
		weapon.AttributeManager.Item.ItemID = 16384;
		weapon.AttributeManager.Item.ItemIDLow = 16384;
		weapon.AttributeManager.Item.ItemIDHigh = 0;
		weapon.FallbackPaintKit = paint;
		weapon.FallbackSeed = 0;
		weapon.FallbackWear = 0.00001F;
		weapon.FallbackStatTrak = isKnife ? -1 : 1234;
	}

	public static void GiveKnife(CCSPlayerController player)
	{
		if (!player.IsValid()) return;

		if (Module.PlayerKnife.TryGetValue(player.SteamID, out var knife))
		{
			player.GiveNamedItem(knife);
		}
		else
		{
			player.GiveNamedItem(player.TeamNum == 2 ? "weapon_knife_t" : "weapon_knife");
		}
	}

	public static bool HasKnife(CCSPlayerController player)
	{
		if (!player.IsValid() || !player.PlayerPawn.IsValid || !player.PawnIsAlive) return false;
		if (player.PlayerPawn.Value == null || player.PlayerPawn.Value.WeaponServices == null || player.PlayerPawn.Value.ItemServices == null) return false;

		var weapons = player.PlayerPawn.Value.WeaponServices?.MyWeapons;
		return weapons != null && weapons.Where(weapon => weapon.IsValid && weapon.Value != null && weapon.Value.IsValid).Any(weapon => weapon.Value.DesignerName.Contains("knife") || weapon.Value.DesignerName.Contains("bayonet"));
	}

	public static void RefreshWeapons(CCSPlayerController player)
	{
		if (!player.IsValid() || !player.PawnIsAlive || player.PlayerPawn.Value?.WeaponServices == null) return;

		foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
		{
			if (!weapon.IsValid || weapon.Value == null || !weapon.Value.IsValid || !weapon.Value.DesignerName.Contains("weapon_")) continue;

			if (weapon.Value.DesignerName.Contains("knife") || weapon.Value.DesignerName.Contains("bayonet"))
			{
				player.RemoveItemByDesignerName(weapon.Value.DesignerName, true);
				GiveKnife(player);
			}
			else
			{
				if (!WeaponIndex.TryGetValue(weapon.Value.AttributeManager.Item.ItemDefinitionIndex, out var weaponIndex)) return;

				player.RemoveItemByDesignerName(weapon.Value.DesignerName, true);
				player.GiveNamedItem(weaponIndex);
			}
		}
	}
}
