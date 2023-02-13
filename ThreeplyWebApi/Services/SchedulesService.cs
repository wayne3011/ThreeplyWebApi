using MongoDB.Driver;
using Microsoft.Extensions.Options;
using ThreeplyWebApi.Models;
namespace ThreeplyWebApi.Services
{

    public class SchedulesService 
    {
        private readonly IMongoCollection<Schedule> _schedulesCollection;
        public SchedulesService(IOptions<ScheduleDatabaseSettings> scheduleDatabaseSettings)
        {
            var MongoClient = new MongoClient(scheduleDatabaseSettings.Value.ConnectionString);
            var MongoDatabase = MongoClient.GetDatabase(scheduleDatabaseSettings.Value.DatabaseName);
            _schedulesCollection = MongoDatabase.GetCollection<Schedule>(scheduleDatabaseSettings.Value.SchedulesCollectionName);
        }
        public async Task<List<Schedule>> GetAsync() => await _schedulesCollection.Find<Schedule>(_ => true).ToListAsync();
        public async Task<Schedule?> GetAsync(string groupName) =>
        await _schedulesCollection.Find(x => x.groupName == groupName).FirstOrDefaultAsync();
        public async Task CreateAsync(Schedule schedule) =>
        await _schedulesCollection.InsertOneAsync(schedule);
        public async Task UpdateAsync(string groupName, Schedule updatedSchedule) =>
            await _schedulesCollection.ReplaceOneAsync(x => x.groupName == groupName, updatedSchedule);
        public async Task RemoveAsync(string groupName) =>
            await _schedulesCollection.DeleteOneAsync(x => x.groupName == groupName);
    }
    
}
