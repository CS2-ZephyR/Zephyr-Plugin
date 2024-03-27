using MongoDB.Driver;
using ZephyrPlugin.Module.UserManager.Data;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.UserManager;

public partial class Module() : ZephyrModule("UserManager")
{
    private IMongoCollection<User> _collection;
    
    public static readonly Dictionary<ulong, string> Names = new();

    public override void OnLoad(bool hotReload)
    {
        _collection = Database.GetCollection<User>();
    }
}
