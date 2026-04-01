using LogLibrary.Implementations;
using LogLibrary.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LogLibrary
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogLibraryService(
            this IServiceCollection services,
            string logFilePath = "logs/application.log",
            LogLevel minimumLevel = LogLevel.Info)
        {
            return services.AddFileLogger(logFilePath, minimumLevel);
        }

        public static IServiceCollection AddFileLogger(
            this IServiceCollection services,
            string logFilePath, // 日志文件路径,默认logs/application.log
            LogLevel minimumLevel = LogLevel.Info // 默认日志级别为Info,意味着只记录Info及以上级别的日志
            )
        {
            services.AddSingleton<ILogger>(_ => new FileLogger(logFilePath, minimumLevel));
            return services;
        }
    }
}
