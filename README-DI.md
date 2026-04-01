# AlgoLibrary 和 LogLibrary 依赖注入实现方案

## 概述

本文档介绍了为 AlgoLibrary 和 LogLibrary 实现的依赖注入（DI）方案。该方案基于 Microsoft.Extensions.DependencyInjection，提供了灵活、可测试的服务注册和解析机制。

## 实现内容

### 1. AlgoLibrary 依赖注入扩展

**文件**: `AlgoLibrary/DependencyInjection.cs`

**提供的方法**:
- `AddAlgoLibrary()` - 注册默认的 OpenCVProcessor 实现
- `AddAlgoLibrary(Func<IServiceProvider, IImageProcessor>)` - 使用自定义工厂注册
- `AddAlgoLibrary<TImplementation>()` - 注册指定的实现类型

### 2. LogLibrary 依赖注入扩展

**文件**: `LogLibrary/DependencyInjection.cs`

**提供的方法**:
- `AddLogLibrary(string logFilePath = "logs/application.log", LogLevel minimumLevel = LogLevel.Info)` - 注册默认的文件日志记录器（`FileLogger`）
- `AddFileLogger(string logFilePath, LogLevel minimumLevel = LogLevel.Info)` - 显式注册文件日志记录器（`FileLogger`）

## 使用示例

### 基本使用

```csharp
// 在 Startup 或 Program.cs 中配置服务
services.AddAlgoLibrary();
services.AddLogLibrary();
```

### 配置日志记录器

```csharp
// 使用文件日志（默认路径与级别）
services.AddLogLibrary();

// 显式指定文件路径与最低级别
services.AddFileLogger("logs/app.log", LogLevel.Debug);
```

### 在应用程序中使用

```csharp
public class MyService
{
    private readonly IImageProcessor _processor;
    private readonly ILogger _logger;
    
    // 通过构造函数注入
    public MyService(IImageProcessor processor, ILogger logger)
    {
        _processor = processor;
        _logger = logger;
    }
    
    public async Task ProcessImage(string imagePath)
    {
        _logger.Info($"开始处理图像: {imagePath}");
        
        if (_processor.LoadImage(imagePath))
        {
            await _processor.ConvertToGrayScaleAsync();
            _logger.Info("图像处理完成");
        }
    }
}
```

## 优势

### 1. 松耦合
- 客户端代码不依赖具体实现，只依赖接口
- 易于替换实现（如更换图像处理算法或日志记录方式）

### 2. 可测试性
- 可以轻松注入 Mock 对象进行单元测试
- 支持测试不同的配置场景

### 3. 可配置性
- 支持通过参数传入日志文件路径与最低日志级别

### 4. 生命周期管理
- DI 容器自动管理对象生命周期
- 支持 Singleton、Scoped、Transient 等生命周期

### 5. 可扩展性
- 轻松添加新的 IImageProcessor 或 ILogger 实现

## 测试

项目包含完整的测试示例：

1. **基本 DI 测试**: 验证基本的服务注册和解析
2. **不同日志配置测试**: 测试各种日志记录器配置
3. **自定义注册测试**: 测试工厂方法和自定义注册

运行测试:
```bash
cd SampleApp
dotnet run
# 选择选项 2 运行依赖注入测试
```

## 文件结构

```
algo-log-library/
├── AlgoLibrary/
│   ├── DependencyInjection.cs          # DI 扩展方法
│   ├── Interfaces/IImageProcessor.cs   # 图像处理器接口
│   └── Implementations/OpenCVProcessor.cs
├── LogLibrary/
│   ├── DependencyInjection.cs          # DI 扩展方法
│   ├── Interfaces/ILogger.cs           # 日志记录器接口
│   └── Implementations/
│       ├── FileLogger.cs
└── SampleApp/
    ├── Program.cs                      # 更新后的主程序
    ├── appsettings.json                # 配置文件
    └── TestDI.cs                       # DI 测试
```

## 迁移指南

### 从旧版本迁移

1. **添加包引用**:
   ```xml
   <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
   ```

2. **更新服务注册**:
   - 将 `new OpenCVProcessor()` 替换为 `services.AddAlgoLibrary()`
   - 将 `new FileLogger(...)` 替换为 `services.AddLogLibrary(...)` 或 `services.AddFileLogger(...)`

3. **更新客户端代码**:
   - 通过构造函数注入获取服务，而不是直接实例化
   - 让 DI 容器管理对象生命周期

### 最佳实践

1. **优先使用接口**: 始终通过接口引用服务
2. **合理使用生命周期**:
   - Singleton: 无状态服务、配置对象
   - Scoped: 数据库上下文、请求相关服务
   - Transient: 轻量级、无状态服务
3. **避免服务定位器模式**: 尽量使用构造函数注入
4. **配置外部化**: 将配置信息放在配置文件中

## 总结

本依赖注入实现方案为 AlgoLibrary 和 LogLibrary 提供了现代化、可维护的服务管理机制。通过统一的 DI 容器，应用程序可以更灵活地配置和使用这些库，同时提高了代码的可测试性和可维护性。
