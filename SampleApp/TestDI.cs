using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AlgoLibrary;
using AlgoLibrary.Interfaces;
using LogLibrary;
using LogLibrary.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SampleApp
{
    /// <summary>
    /// 依赖注入测试类
    /// </summary>
    public class TestDI
    {
        public static async Task RunTest()
        {
            Console.WriteLine("=== 依赖注入测试 ===");
            Console.WriteLine();
            
            // 测试1: 基本DI配置
            await TestBasicDI();
            
            Console.WriteLine();
            
            // 测试2: 不同日志配置
            await TestDifferentLogConfigs();
            
            Console.WriteLine();
            
            // 测试3: 自定义服务注册
            await TestCustomRegistration();
            
            Console.WriteLine("=== 依赖注入测试完成 ===");
        }
        
        private static async Task TestBasicDI()
        {
            Console.WriteLine("测试1: 基本DI配置");
            
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAlgoLibraryService();
                    services.AddFileLogger("logs/di_basic.log", LogLevel.Info);
                })
                .Build();
            
            using var scope = host.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IImageProcessor>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
            
            Console.WriteLine($"图像处理器类型: {processor.GetType().Name}");
            Console.WriteLine($"日志记录器类型: {logger.GetType().Name}");
            
            // 测试日志记录
            logger.Info("基本DI配置测试 - 日志记录正常");
            
            Console.WriteLine("✓ 基本DI配置测试通过");
            await Task.CompletedTask;
        }
        
        private static async Task TestDifferentLogConfigs()
        {
            Console.WriteLine("测试2: 不同日志配置");
            
            Console.WriteLine("  2.1 文件日志配置");
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAlgoLibraryService();
                    services.AddFileLogger("logs/test.log", LogLevel.Info);
                })
                .Build();
            
            using (var scope = host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.Debug("文件日志调试信息");
                logger.Info("文件日志一般信息");
                Console.WriteLine("  ✓ 文件日志配置测试通过");
            }

            Console.WriteLine("✓ 不同日志配置测试通过");
            await Task.CompletedTask;
        }
        
        private static async Task TestCustomRegistration()
        {
            Console.WriteLine("测试3: 自定义服务注册");
            
            // 测试自定义工厂方法
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    // 使用工厂方法注册自定义IImageProcessor
                    services.AddAlgoLibrary(provider =>
                    {
                        // 这里可以返回任何IImageProcessor的实现
                        return new AlgoLibrary.Implementations.OpenCVProcessor(
                            provider.GetRequiredService<ILogger>());
                    });
                    
                    services.AddFileLogger("logs/custom.log", LogLevel.Debug);
                })
                .Build();
            
            using var scope = host.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IImageProcessor>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
            
            Console.WriteLine($"自定义图像处理器类型: {processor.GetType().Name}");
            Console.WriteLine($"自定义日志记录器类型: {logger.GetType().Name}");
            
            // 测试功能
            logger.Info("自定义注册测试 - 日志记录正常");
            
            Console.WriteLine("✓ 自定义服务注册测试通过");
            await Task.CompletedTask;
        }
    }
}
