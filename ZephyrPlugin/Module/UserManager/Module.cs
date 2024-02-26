using MongoDB.Driver;
using ZephyrPlugin.Module.UserManager.Data;
using ZephyrPlugin.Util;

namespace ZephyrPlugin.Module.UserManager;

public partial class Module : ZephyrModule
{
    private IMongoCollection<User> _collection;
    
    public static readonly Dictionary<ulong, string> Names = new();

    public Module() : base("UserManager")
    { }

    public override void OnLoad()
    {
        _collection = Database.GetCollection<User>();
    }
}
