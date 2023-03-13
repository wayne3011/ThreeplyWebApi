using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using ThreeplyWebApi.Services.ScheduleParser;
using ThreeplyWebApi.Models.GroupModel;
using ThreeplyWebApi.Models;
using MongoDB.Bson;
using ThreeplyWebApi.Services.ScheduleParser.ScheduleParserExceptions;
using Microsoft.VisualBasic.FileIO;
using ThreeplyWebApi.Services.ServicesOptions;

namespace ThreeplyWebApi.Services
{

    public class GroupsService
    {
        private readonly IMongoCollection<Group> _groupsCollection;
        private readonly ScheduleParserService _scheduleParserService;
        public GroupsService(IOptions<GroupsOptions> options,MongoDbService mongoDbService, ScheduleParserService scheduleParserService)
        {
            _groupsCollection = mongoDbService.MongoDatabase.GetCollection<Group>(options.Value.GroupsCollectionName);
            _scheduleParserService = scheduleParserService;
        }
        public async Task<List<Group>> GetAsync()
        {
          return await _groupsCollection.Find<Group>(_ => true).ToListAsync();
        }
        public async Task<Group> GetAsync(string groupName)
        {
            var group = await _groupsCollection.Find(x => x.GroupName == groupName).FirstOrDefaultAsync();//TODO: database dont respond
            if (group == null)
            {
                group = new Group();
                group.GroupName = await _scheduleParserService.FormatGroupNameAsync(groupName);
                Schedule groupSchedule = await _scheduleParserService.GetGroupScheduleAsync(groupName);
                group.LastTimeUpdate = DateTime.UtcNow;
                group.Schedule = groupSchedule;
                await CreateAsync(group);
                return group;
            }
            BsonDocument documentGroupName = new BsonDocument { { "GroupName", groupName } };
            BsonDocument documentTimeUpdate = new BsonDocument("$set", new BsonDocument { { "LastTimeUpdate", DateTime.UtcNow } });
            _groupsCollection.UpdateOne(documentGroupName, documentTimeUpdate);
            return group;

            
            
        }
        public async Task<GroupValidation> GetGroupValidationAsync(string groupName)
        {
            GroupValidation groupValidation = new GroupValidation();
            groupValidation.isValid = true;
            groupValidation.requestedGroup = groupName;
            try
            {
                groupValidation.formattedGroup = await _scheduleParserService.FormatGroupNameAsync(groupName);
            }
            catch (InvalidGroupNameException)
            {
                groupValidation.isValid = false;
            }
            return groupValidation;
        }
        
        public async Task CreateAsync(Group group) {
            await _groupsCollection.InsertOneAsync(group);
        }
        
        public async Task UpdateAsync(string groupName, Group updatedGroup) =>
            await _groupsCollection.ReplaceOneAsync(x => x.GroupName == groupName, updatedGroup);
        public async Task UpdateScheduleAsync(string groupName, Schedule updatedSchedule)
        {
            var updateDefenition = Builders<Group>.Update.Set(doc => doc.Schedule, updatedSchedule);
            await _groupsCollection.UpdateOneAsync(doc => doc.GroupName == groupName, updateDefenition);
        }
        public async Task RemoveAsync(string groupName) =>
            await _groupsCollection.DeleteOneAsync(x => x.GroupName == groupName);
    }
   
}
