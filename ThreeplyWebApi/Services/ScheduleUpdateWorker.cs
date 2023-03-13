using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Diagnostics;
using ThreeplyWebApi.Models.GroupModel;
using ThreeplyWebApi.Services;
using ThreeplyWebApi.Services.ScheduleParser;
using ThreeplyWebApi.Services.ServicesOptions;

class ScheduleUpdateWorker : BackgroundService
{
    private IHostApplicationLifetime _hostApplicationLifetime;
    private ILogger<ScheduleUpdateWorker> _logger;
    private IMongoCollection<Group> _groupsCollection;
    private ScheduleParserService _scheduleParserService;
    private GroupsService _groupsService;
    private string[] _updateTime;
    public ScheduleUpdateWorker(ILogger<ScheduleUpdateWorker> logger, IHostApplicationLifetime hostApplicationLifetime, 
        MongoDbService mongoDbService, IOptions<GroupsOptions> groupOptions,IOptions<ScheduleUpdateOptions> options, ScheduleParserService scheduleParserService, GroupsService groupsService)
    {
        _logger = logger;
        _scheduleParserService = scheduleParserService;
        _groupsService = groupsService;
        _groupsCollection = mongoDbService.MongoDatabase.GetCollection<Group>(groupOptions.Value.GroupsCollectionName);
        _hostApplicationLifetime = hostApplicationLifetime;
        _hostApplicationLifetime.ApplicationStarted.Register(() => _logger.LogInformation("ScheduleUpdateWorker started."));
        _hostApplicationLifetime.ApplicationStopping.Register(() => _logger.LogInformation("ScheduleUpdateWorker stopping"));
        _hostApplicationLifetime.ApplicationStopped.Register(() => _logger.LogInformation("ScheduleUpdateWorker stopped"));
        _updateTime = options.Value.scheduleUpdateTime;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            return base.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return Task.CompletedTask;
        }

    }

    public async override Task StopAsync(CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await base.StopAsync(cancellationToken);
        _logger.LogInformation($"ScheduleUpdateWorker stopped in {stopwatch.ElapsedMilliseconds}");

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await WaitForAppStartup(_hostApplicationLifetime, stoppingToken)) return;
        stoppingToken.Register(() => _logger.LogInformation("ScheduleUpdateWorker service token is cancelled."));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await updateScheduleAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ScheduleWorkerUpdate exception", ex);
            }
        }
        
    }
    async private Task<bool> updateScheduleAsync()
    {
        string time = DateTime.UtcNow.ToShortTimeString();
        if (_updateTime.Contains<string>(time))
        {
            _logger.LogInformation("Groups update started.");
            Queue<string> updateQueue;
            var list = _groupsCollection.Find(o => o.LastTimeUpdate.AddDays(30) > DateTime.UtcNow).Project(doc => doc.GroupName).ToEnumerable();
            updateQueue = new Queue<string>(list);
            while (updateQueue.Count > 0)
            {
                string updatedGroup = updateQueue.Dequeue();
                Schedule newSchedule = await _scheduleParserService.GetGroupScheduleAsync(updatedGroup);
                await _groupsService.UpdateScheduleAsync(updatedGroup, newSchedule);
                _logger.LogInformation("Group {GroupName} updated.", updatedGroup);
                await Task.Delay(10000);
            }
            _logger.LogInformation("Updating groups is finished");
        }
        return true;
    }
    static async private Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var startedSource = new TaskCompletionSource();
        using var reg1 = lifetime.ApplicationStarted.Register(() => startedSource.SetResult());

        var cancelledSource = new TaskCompletionSource();
        using var reg2 = lifetime.ApplicationStopped.Register(() => cancelledSource.SetResult());

        Task completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task).ConfigureAwait(false);

        return completedTask == startedSource.Task;
    }
}