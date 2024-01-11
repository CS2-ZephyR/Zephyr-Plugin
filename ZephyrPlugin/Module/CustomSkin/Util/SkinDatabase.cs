using MongoDB.Driver;

namespace ZephyrPlugin.Module.CustomSkin.Util;

public static class SkinDatabase
{
	private static IMongoCollection<Data.PlayerSkin> _collection;

	public static void InjectCollection(IMongoCollection<Data.PlayerSkin> collection)
	{
		_collection = collection;
	}

	public static async Task Fetch(ulong steamId)
	{
		if (!Module.PlayerSkin.TryGetValue(steamId, out _))
		{
			Module.PlayerSkin[steamId] = new Dictionary<int, int>();
		}

		var result = await (await _collection.FindAsync(x => x.SteamId == steamId)).SingleOrDefaultAsync();

		if (result != null)
		{
			Module.PlayerKnife[steamId] = result.Knife;

			foreach (var (weapon, skin) in result.Skin.ToList())
			{
				Module.PlayerSkin[steamId][weapon] = skin;
			}
		}
	}
}
