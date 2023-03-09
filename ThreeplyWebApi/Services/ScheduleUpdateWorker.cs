using System.Diagnostics;

class ScheduleUpdateWorker : BackgroundService
{
    private IHostApplicationLifetime _hostApplicationLifetime;
    private Logger<ScheduleUpdateWorker> _logger;
    public ScheduleUpdateWorker(Logger<ScheduleUpdateWorker> logger, IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _hostApplicationLifetime.ApplicationStarted.Register(() => _logger.LogInformation("ScheduleUpdateWorker started."));
        _hostApplicationLifetime.ApplicationStopping.Register(() => _logger.LogInformation("ScheduleUpdateWorker stopping"));
        _hostApplicationLifetime.ApplicationStopped.Register(() => _logger.LogInformation("ScheduleUpdateWorker stopped"));
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogInformation("ScheduleUpdateWorker service token is cancelled."));
        try
        {
            ///doing method
        }
        catch (Exception ex)
        {
            _logger.LogCritical("ScheduleWorkerUpdate exception",ex);
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}