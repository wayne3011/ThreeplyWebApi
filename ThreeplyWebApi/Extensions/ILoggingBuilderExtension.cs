using ThreeplyWebApi.Logging;

namespace ThreeplyWebApi.Extensions
{
    public static class ILoggingBuilderExtension
    {
        public static ILoggingBuilder AddMongoLogger(this ILoggingBuilder builder,Action<MongoLoggerOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, MongoLoggerProvider>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
