using MongoDB.Driver;
using Microsoft.Extensions.Options;
using ThreeplyWebApi.Models;
namespace ThreeplyWebApi.Services
{

    public class GroupsService 
    {
        private readonly IMongoCollection<Group> _groupsCollection;
        public GroupsService(IOptions<GroupsDatabaseSettings> groupDatabaseSettings)
        {
            var MongoClient = new MongoClient(groupDatabaseSettings.Value.ConnectionString);
            var MongoDatabase = MongoClient.GetDatabase(groupDatabaseSettings.Value.DatabaseName);
            _groupsCollection = MongoDatabase.GetCollection<Group>(groupDatabaseSettings.Value.GroupsCollectionName);
        }
        public async Task<List<Group>> GetAsync() => await _groupsCollection.Find<Group>(_ => true).ToListAsync();
        public async Task<Group?> GetAsync(string groupName) =>
        await _groupsCollection.Find(x => x.groupName == groupName).FirstOrDefaultAsync();
        public async Task CreateAsync(Group group) =>
        await _groupsCollection.InsertOneAsync(group);
        public async Task UpdateAsync(string groupName, Group updatedGroup) =>
            await _groupsCollection.ReplaceOneAsync(x => x.groupName == groupName, updatedGroup);
        public async Task RemoveAsync(string groupName) =>
            await _groupsCollection.DeleteOneAsync(x => x.groupName == groupName);
    }
    
}
