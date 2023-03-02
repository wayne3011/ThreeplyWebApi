using Microsoft.Extensions.Options;
using ThreeplyWebApi.Services;

namespace ThreeplyWebApi.Logging
{
    public class MongoLoggerProvider : ILoggerProvider
    {
        public readonly MongoLoggerOptions Options;
        private readonly MongoDbService _mongoDbService;
        public MongoLoggerProvider(IOptions<MongoLoggerOptions> options, MongoDbService mongoDbService)
        {
            Options = options.Value;
            _mongoDbService = mongoDbService;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new MongoLogger(this, _mongoDbService);
        }

        public void Dispose()
        {
        }
    }
}
