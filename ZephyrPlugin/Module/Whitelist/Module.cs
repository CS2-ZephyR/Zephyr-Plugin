using MongoDB.Driver;
using ZephyrPlugin.Module.Whitelist.Data;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.Whitelist;

public partial class Module : ZephyrModule
{
    private IMongoCollection<Data.Whitelist> _collection;

    public Module() : base("Whitelist")
    {

    }

    public override void OnLoad()
    {
        _collection = Database.GetCollection<Data.Whitelist>();
    }
}
