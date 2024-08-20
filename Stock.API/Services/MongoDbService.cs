using MongoDB.Driver;

namespace Stock.API.Services
{
    public class MongoDbService
    {
        readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            MongoClient mongoClient = new(configuration.GetConnectionString("Default"));
            _database = mongoClient.GetDatabase("StockApiDb");
        }

        public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}
