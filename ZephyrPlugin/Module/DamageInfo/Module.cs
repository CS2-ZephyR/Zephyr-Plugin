namespace ZephyrPlugin.Module.DamageInfo;

public partial class Module() : ZephyrModule("DamageInfo")
{
    private readonly Dictionary<Tuple<ulong, ulong>, ValueTuple<int, int>> _damage = new();
    private readonly Dictionary<ulong, ulong> _death = new();
}
