using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ThreeplyWebApi.Models;
using ThreeplyWebApi.Services;

namespace ThreeplyWebApi.Logging
{
    public class MongoLogger : ILogger
    {
        private readonly MongoLoggerProvider _mongoLoggerProvider;
        private readonly IMongoCollection<LogRecord> _mongoLoggerCollection;
        public MongoLogger(MongoLoggerProvider mongoLoggerProvider, MongoDbService mongoDbService)
        {
            _mongoLoggerProvider = mongoLoggerProvider;
            _mongoLoggerCollection = mongoDbService.MongoDatabase.GetCollection<LogRecord>(mongoLoggerProvider.Options.LogCollectionName);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogRecord record = new LogRecord();
            record.LogLevel = logLevel;
            record.Date = DateTime.Now;
            record.Message = formatter(state,exception);
            record.UserId = "Server: " + eventId.Id;
            _mongoLoggerCollection.InsertOne(record);
        }
    }
}
