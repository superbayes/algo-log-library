using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AlgoLibrary;
using AlgoLibrary.Interfaces;
using LogLibrary;
using LogLibrary.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using AlgoLibrary.Implementations.Utils;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string choice;
            
            // 如果有命令行参数，使用第一个参数作为选择
            if (args.Length > 0)
            {
                choice = args[0];
                Console.WriteLine($"=== 算法库和日志库示例应用 (命令行模式: {choice}) ===");
            }
            else
            {
                Console.WriteLine("=== 算法库和日志库示例应用 (依赖注入版本) ===");
                Console.WriteLine();
                Console.WriteLine("请选择运行模式:");
                Console.WriteLine("1. 运行完整示例应用");
                Console.WriteLine("2. 运行依赖注入测试");
                Console.WriteLine("3. 运行连通域检测测试");
                Console.WriteLine("4. 测试直方图峰值查找函数");
                Console.WriteLine("5. 测试轮廓特征提取函数");
                Console.WriteLine("6. 测试 AlgoUtils 工具函数（包括ROI平均亮度计算）");
                Console.WriteLine("7. 退出");
                Console.Write("请输入选择 (1-7): ");
                
                choice = Console.ReadLine();
            }
            
            switch (choice)
            {
                case "1":
                    await RunFullApplication();
                    break;
                case "2":
                    await TestDI.RunTest();
                    break;
                case "3":
                    Console.WriteLine("程序退出");
                    return;
                case "4":
                    TestHistogramPeak();
                    break;
                case "5":
                    TestContourFeatureExtractor.RunTest();
                    break;
                case "6":
                    TestAlgoUtils.RunTest();
                    break;
                case "7":
                    Console.WriteLine("程序退出");
                    return;
                default:
                    Console.WriteLine("无效选择，运行完整示例应用");
                    await RunFullApplication();
                    break;
            }

            Console.WriteLine();
            if (!Console.IsInputRedirected && args.Length == 0)
            {
                Console.WriteLine("程序执行完成。按任意键退出...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("程序执行完成。");
            }
        }
        
        static async Task RunFullApplication()
        {
            Console.WriteLine("=== 运行完整示例应用 ===");
            Console.WriteLine();

            // 创建主机并配置服务
            using var host = CreateHostBuilder(Array.Empty<string>()).Build();
            
            using (var scope = host.Services.CreateScope()/* 创建作用域，用于创建服务 */)
            {
                // 从作用域获取服务提供程序
                // 服务提供程序用于从DI容器获取服务
                // 服务提供程序是DI容器的入口点，用于获取服务
                var services = scope.ServiceProvider;
                
                try
                {
                    // 从DI容器获取服务
                    var processor = services.GetRequiredService<IImageProcessor>();
                    var logger = services.GetRequiredService<ILogger>();
                    
                    // 运行应用程序
                    await RunApplication(processor, logger);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"应用程序启动失败: {ex.Message}");
                    Console.WriteLine($"详细错误: {ex}");
                }
            }
        }
        
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // 注册AlgoLibrary服务
                    services.AddAlgoLibraryService();
                    // 注册LogLibrary服务
                    services.AddLogLibraryService("logs/application.log", LogLevel.Info);
                    
                    // 注册应用服务,作为单例服务
                    services.AddSingleton<ApplicationService>();
                });
        
        static async Task RunApplication(IImageProcessor processor, ILogger logger)
        {
            try
            {
                // 创建应用服务并运行
                using (var scope = new ServiceCollection()
                    .AddSingleton(processor)
                    .AddSingleton(logger)
                    .AddScoped<ApplicationService>()
                    .BuildServiceProvider()
                    .CreateScope())
                {
                    var appService = scope.ServiceProvider.GetRequiredService<ApplicationService>();
                    await appService.RunAsync();
                }
            }
            catch (Exception ex)
            {
                logger.Error("应用程序运行失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 测试直方图峰值查找函数
        /// </summary>
        static void TestHistogramPeak()
        {
            Console.WriteLine("=== 测试直方图峰值查找函数 ===");
            Console.WriteLine();
            
            // 测试1: 简单数据
            Console.WriteLine("测试1: 简单数据");
            var data1 = new System.Collections.Generic.List<double> { 1.0, 2.0, 2.0, 3.0, 3.0, 3.0, 4.0, 4.0, 5.0 };
            var result1 = MathUtils.FindPeakInHistogram(data1);
            Console.WriteLine($"数据: [{string.Join(", ", data1)}]");
            Console.WriteLine($"最大峰: count={result1.count}, value={result1.value:F2}");
            Console.WriteLine($"预期: count=3 (值3.0出现3次)");
            Console.WriteLine($"测试结果: {(result1.count == 3 && Math.Abs(result1.value - 3.0) < 0.5 ? "通过" : "失败")}");
            Console.WriteLine();
            
            // 测试2: 所有值相同
            Console.WriteLine("测试2: 所有值相同");
            var data2 = new System.Collections.Generic.List<int> { 5, 5, 5, 5, 5, 5 };
            var result2 = MathUtils.FindPeakInHistogram(data2);
            Console.WriteLine($"数据: [{string.Join(", ", data2)}]");
            Console.WriteLine($"最大峰: count={result2.count}, value={result2.value:F2}");
            Console.WriteLine($"预期: count=6, value=5.0");
            Console.WriteLine($"测试结果: {(result2.count == 6 && Math.Abs(result2.value - 5.0) < 0.1 ? "通过" : "失败")}");
            Console.WriteLine();
            
            // 测试3: 指定bin数量
            Console.WriteLine("测试3: 指定bin数量");
            var data3 = new System.Collections.Generic.List<float> { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f };
            var result3 = MathUtils.FindPeakInHistogram(data3, 3);
            Console.WriteLine($"数据: [{string.Join(", ", data3)}]");
            Console.WriteLine($"使用3个bins: count={result3.count}, value={result3.value:F2}");
            Console.WriteLine($"测试结果: 通过（函数执行无异常）");
            Console.WriteLine();
            
            // 测试4: 边缘情况 - 单个元素
            Console.WriteLine("测试4: 边缘情况 - 单个元素");
            var data4 = new System.Collections.Generic.List<int> { 42 };
            var result4 = MathUtils.FindPeakInHistogram(data4);
            Console.WriteLine($"数据: [{string.Join(", ", data4)}]");
            Console.WriteLine($"最大峰: count={result4.count}, value={result4.value:F2}");
            Console.WriteLine($"预期: count=1, value=42.0");
            Console.WriteLine($"测试结果: {(result4.count == 1 && Math.Abs(result4.value - 42.0) < 0.1 ? "通过" : "失败")}");
            Console.WriteLine();
            
            // 测试5: 测试异常情况
            Console.WriteLine("测试5: 边缘情况 - 空列表");
            try
            {
                var data5 = new System.Collections.Generic.List<double>();
                var result5 = MathUtils.FindPeakInHistogram(data5);
                Console.WriteLine($"测试结果: 失败（应抛出异常但未抛出）");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"测试结果: 通过（正确抛出ArgumentException: {ex.Message}）");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败（抛出错误类型的异常: {ex.GetType().Name}）");
            }
            
            Console.WriteLine("\n所有测试完成！");
            
            // 只有在控制台输入可用时才等待按键
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("按任意键继续...");
                Console.ReadKey();
            }
        }

    }
    
    /// <summary>
    /// 应用服务类，演示依赖注入的使用
    /// </summary>
    public class ApplicationService
    {
        private readonly IImageProcessor _processor;
        private readonly ILogger _logger;
        
        /// <summary>
        /// 构造函数，通过依赖注入获取所需服务
        /// </summary>
        /// <param name="processor">图像处理器</param>
        /// <param name="logger">日志记录器</param>
        public ApplicationService(IImageProcessor processor, ILogger logger)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 运行应用程序
        /// </summary>
        public async Task RunAsync()
        {
            Console.WriteLine("应用程序服务开始运行");
            
            // 示例1: 使用算法库处理图像
            DemonstrateImageProcessing();
            
            Console.WriteLine("图像处理演示完成");
            
            // 示例2: 使用日志库记录不同级别的日志
            await DemonstrateLogging();
            
            Console.WriteLine("日志记录演示完成");
            
            // 示例3: 结合使用算法库和日志库
            await DemonstrateIntegration();
            
            Console.WriteLine("集成演示完成");
            Console.WriteLine("应用程序服务运行完成");
        }
        
        private void DemonstrateImageProcessing()
        {
            Console.WriteLine("开始演示图像处理功能");
            
            // 检查测试图像是否存在
            string testImagePath = "test_data/sample.jpg";
            string outputImagePath = "test_data/output";
            
            if (!Directory.Exists("test_data"))
            {
                Directory.CreateDirectory("test_data");
                Console.WriteLine($"测试数据目录不存在，已创建: test_data");
                Console.WriteLine($"请将测试图像放入 {Path.GetFullPath(testImagePath)} 以运行完整示例");
                return;
            }
            
            if (!File.Exists(testImagePath))
            {
                Console.WriteLine($"测试图像不存在: {testImagePath}");
                _logger.Warning($"请将测试图像放入该路径以运行完整示例");
                return;
            }
            
            try
            {
                // 加载图像
                Console.WriteLine($"加载图像: {testImagePath}");
                bool loaded = _processor.LoadImage(testImagePath);
                
                if (loaded)
                {
                    Console.WriteLine("图像加载成功");
                    
                    // 获取图像信息
                    string imageInfo = _processor.GetImageInfo();
                    _logger.Info($"图像信息: {imageInfo}");
                    
                    // 转换为灰度图像
                    Console.WriteLine("开始转换为灰度图像...");
                    bool graySuccess = _processor.ConvertToGrayScale();
                    
                    if (graySuccess)
                    {
                        string grayOutputPath = $"{outputImagePath}_gray.jpg";
                        _processor.SaveImage(grayOutputPath);
                        Console.WriteLine($"灰度图像已保存: {grayOutputPath}");
                    }
                    
                    // 重新加载原始图像进行其他处理
                    _processor.LoadImage(testImagePath);
                    
                    // 调整图像大小
                    Console.WriteLine("开始调整图像大小...");
                    bool resizeSuccess = _processor.Resize(800, 600);
                    
                    if (resizeSuccess)
                    {
                        string resizeOutputPath = $"{outputImagePath}_resized.jpg";
                        _processor.SaveImage(resizeOutputPath);
                        Console.WriteLine($"调整大小后的图像已保存: {resizeOutputPath}");
                    }
                    
                    // 重新加载原始图像进行边缘检测
                    _processor.LoadImage(testImagePath);
                    
                    // 应用边缘检测
                    Console.WriteLine("开始应用边缘检测...");
                    bool edgeSuccess = _processor.ApplyEdgeDetection();
                    
                    if (edgeSuccess)
                    {
                        string edgeOutputPath = $"{outputImagePath}_edges.jpg";
                        _processor.SaveImage(edgeOutputPath);
                        Console.WriteLine($"边缘检测图像已保存: {edgeOutputPath}");
                    }
                    
                    Console.WriteLine("图像处理演示完成");
                }
                else
                {
                    Console.WriteLine("图像加载失败");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"图像处理过程中发生错误: {ex.Message}");
            }
        }
        
        private async Task DemonstrateLogging()
        {
            _logger.Info("开始演示日志记录功能");
            
            // 记录不同级别的日志
            _logger.Debug("这是一条调试信息");
            _logger.Info("这是一条一般信息");
            _logger.Warning("这是一条警告信息");
            _logger.Error("这是一条错误信息");
            _logger.Fatal("这是一条致命错误信息");
            
            // 演示异常记录
            try
            {
                throw new InvalidOperationException("这是一个测试异常");
            }
            catch (Exception ex)
            {
                _logger.Error("捕获到测试异常", ex);
            }
            
            // 演示异步日志记录
            await _logger.InfoAsync("这是一条异步记录的一般信息");
            
            _logger.Info("日志记录演示完成");
        }
        
        private async Task DemonstrateIntegration()
        {
            _logger.Info("开始演示算法库和日志库的集成使用");
            
            // 模拟图像处理流程
            _logger.Info("模拟图像处理流程开始");
            
            // 步骤1: 初始化
            _logger.Debug("初始化图像处理器");
            
            // 步骤2: 模拟处理
            for (int i = 1; i <= 5; i++)
            {
                string message = $"正在处理图像第 {i}/5 步";
                _logger.Info(message);
                
                // 模拟处理时间
                await Task.Delay(100);
                
                // 模拟进度更新
                _logger.Debug($"处理进度: {i * 20}%");
            }
            
            // 步骤3: 完成
            _logger.Info("图像处理流程完成");
            
            _logger.Info("集成演示完成");
        }
    }
}
