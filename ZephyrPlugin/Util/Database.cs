using MongoDB.Driver;

namespace ZephyrPlugin.Util;

public static class Database
{
    public static MongoConfig Config { get; set; }

    private static IMongoDatabase _database;

    public static void Init()
    {
        var client = new MongoClient($"mongodb://{Config.Username}:{Config.Password}@{Config.Hostname}:{Config.Port}/?authSource=admin");
        _database = client.GetDatabase(Config.Database);
    }

    public static IMongoCollection<T> GetCollection<T>()
    {
        return _database.GetCollection<T>(typeof(T).Name);
    }
}
