using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ThreeplyWebApi.Services
{
    public class MongoDbService
    {
        public IMongoDatabase MongoDatabase { get; }
        public MongoDbService(IOptions<MongoDbOptions> options)
        {
            var MongoClient = new MongoClient(options.Value.ConnectionString);
            MongoDatabase = MongoClient.GetDatabase(options.Value.DatabaseName);
        }
    }
}
