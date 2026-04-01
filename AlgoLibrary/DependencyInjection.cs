using Microsoft.Extensions.DependencyInjection;
using AlgoLibrary.Interfaces;
using AlgoLibrary.Implementations;
using LogLibrary.Interfaces;

namespace AlgoLibrary
{
    /// <summary>
    /// AlgoLibrary的依赖注入扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加AlgoLibrary服务（使用默认的OpenCVProcessor实现）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddAlgoLibraryService(this IServiceCollection services)
        {
            services.AddSingleton<IImageProcessor>(sp =>
            {
                var logger = sp.GetService<ILogger>() ?? new LogLibrary.Implementations.FileLogger("logs/application.log", LogLevel.Info);
                return new OpenCVProcessor(logger);
            });
            return services;
        }
        
        /// <summary>
        /// 添加AlgoLibrary服务（使用自定义工厂）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="implementationFactory">IImageProcessor实现工厂</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddAlgoLibrary(this IServiceCollection services, 
            Func<IServiceProvider, IImageProcessor> implementationFactory)
        {
            services.AddScoped(implementationFactory);
            return services;
        }
        
        /// <summary>
        /// 添加AlgoLibrary服务（使用指定的实现类型）
        /// </summary>
        /// <typeparam name="TImplementation">IImageProcessor的实现类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddAlgoLibrary<TImplementation>(this IServiceCollection services)
            where TImplementation : class, IImageProcessor
        {
            services.AddScoped<IImageProcessor, TImplementation>();
            return services;
        }
    }
}
