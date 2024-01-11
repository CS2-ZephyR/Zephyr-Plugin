using ZephyrPlugin.Module.CustomSkin.Util;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.CustomSkin;

public partial class Module : ZephyrModule
{
	public static readonly Dictionary<ulong, string> PlayerKnife = new();
	public static readonly Dictionary<ulong, Dictionary<int, int>> PlayerSkin = new();
	private static readonly Dictionary<ulong, int> PlayerPickup = new();

	public Module() : base("CustomSkin")
	{
	}

	public override void OnLoad()
	{
		SkinDatabase.InjectCollection(Database.GetCollection<Data.PlayerSkin>());
		SkinUtil.InjectPlugin(Plugin);
	}
}
