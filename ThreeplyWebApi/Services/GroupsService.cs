﻿using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using ThreeplyWebApi.Services.ScheduleParser;
using ThreeplyWebApi.Models.GroupModel;

namespace ThreeplyWebApi.Services
{

    public class GroupsService
    {
        private readonly IMongoCollection<Group> _groupsCollection;
        public GroupsService(IOptions<GroupsOptions> options,MongoDbService mongoDbService)
        {
            _groupsCollection = mongoDbService.MongoDatabase.GetCollection<Group>(options.Value.GroupsCollectionName);
        }
        public async Task<List<Group>> GetAsync()
        {
          return await _groupsCollection.Find<Group>(_ => true).ToListAsync();
        }
        public async Task<Group> GetAsync(string groupName)
        {
            var group = await _groupsCollection.Find(x => x.GroupName == groupName).FirstOrDefaultAsync();

                if (group == null)
                {
                    ScheduleParserService _scheduleParserService = new ScheduleParserService("https://mai.ru/education/studies/schedule");
                    Schedule groupSchedule = await _scheduleParserService.GetGroupScheduleAsync(groupName);
                    group = new Group(groupName);
                    group.Schedule = groupSchedule;
                    await CreateAsync(group);
                }                     
            return group;
        }
        
        public async Task CreateAsync(Group group) {
            await _groupsCollection.InsertOneAsync(group);
        }
        
        public async Task UpdateAsync(string groupName, Group updatedGroup) =>
            await _groupsCollection.ReplaceOneAsync(x => x.GroupName == groupName, updatedGroup);
        public async Task RemoveAsync(string groupName) =>
            await _groupsCollection.DeleteOneAsync(x => x.GroupName == groupName);
    }
   
}
