# Algo-Log Library 解决方案

- 基于C# .NET 8.0开发的算法库和日志库解决方案，采用接口+实现的架构方式。

* 这是一个用于在C#中，创建算法库和日志库的模板。

## 项目结构

### 解决方案文件

- `AlgoLogLibrary.sln` - 主解决方案文件

### 项目组成

1. **AlgoLibrary** - 基于OpenCVSharp的算法库
   - `AlgoLibrary.csproj` - 项目文件
   - `DependencyInjection.cs` - 依赖注入扩展（注册 `IImageProcessor`）
   - `Interfaces/IImageProcessor.cs` - 图像处理器接口
   - `Implementations/OpenCVProcessor.cs` - OpenCVSharp实现
2. **LogLibrary** - 简易的日志库
   - `LogLibrary.csproj` - 项目文件
   - `DependencyInjection.cs` - 依赖注入扩展（注册 `ILogger`）
   - `Interfaces/ILogger.cs` - 日志记录器接口（包含LogLevel枚举）
   - `Implementations/FileLogger.cs` - 文件日志实现
3. **SampleApp** - 示例应用程序
   - `SampleApp.csproj` - 项目文件
   - `Program.cs` - 示例代码
   - `TestDI.cs` - 依赖注入测试
   - `appsettings.json` - 应用配置（示例）

### 顶层文件（根目录）

- `README.md` - 项目说明
- `README-DI.md` - 依赖注入说明

### 文件结构

```
algo-log-library/
├── AlgoLogLibrary.sln
├── AlgoLibrary/
│   ├── AlgoLibrary.csproj
│   ├── DependencyInjection.cs
│   ├── Interfaces/
│   │   └── IImageProcessor.cs
│   └── Implementations/
│       └── OpenCVProcessor.cs
├── LogLibrary/
│   ├── LogLibrary.csproj
│   ├── DependencyInjection.cs
│   ├── Interfaces/
│   │   └── ILogger.cs
│   └── Implementations/
│       └── FileLogger.cs
└── SampleApp/
    ├── SampleApp.csproj
    ├── Program.cs
    ├── TestDI.cs
    └── appsettings.json
```

## 功能特性

### 环境要求

- .NET 8.0 SDK 或更高版本
- Visual Studio 2022（推荐）或 VS Code

### 运行示例

```bash
cd SampleApp
dotnet run
```

### 使用算法库

```csharp
using AlgoLibrary.Interfaces;
using AlgoLibrary.Implementations;
using LogLibrary.Interfaces;
using LogLibrary.Implementations;

// 创建日志记录器（AlgoLibrary 的实现依赖 ILogger）
using ILogger logger = new FileLogger("logs/application.log", LogLevel.Info);

// 创建图像处理器
using IImageProcessor processor = new OpenCVProcessor(logger);

// 加载图像
if (processor.LoadImage("path/to/image.jpg"))
{
    // 转换为灰度
    processor.ConvertToGrayScale();
    
    // 保存结果
    processor.SaveImage("path/to/output.jpg");
    
    // 获取图像信息
    string info = processor.GetImageInfo();
}
```

### 依赖注入示例

```csharp
using AlgoLibrary.Interfaces;
using AlgoLibrary.Implementations;
using LogLibrary.Interfaces;
using LogLibrary.Implementations;
using Microsoft.Extensions.DependencyInjection;

// 注册依赖
using IServiceCollection services = new ServiceCollection();
services.AddLogLibraryService("logs/application.log", LogLevel.Info);
services.AddAlgoLibraryService();

// 获取服务
using IImageProcessor processor = services.GetService<IImageProcessor>();
using ILogger logger = services.GetService<ILogger>();

// 使用服务
if (processor.LoadImage("path/to/image.jpg"))
{
    // 转换为灰度
    processor.ConvertToGrayScale();
    
    // 保存结果
    processor.SaveImage("path/to/output.jpg");
    
    // 获取图像信息
    string info = processor.GetImageInfo();
}
```

### 使用日志库

```csharp
using LogLibrary.Interfaces;
using LogLibrary.Implementations;

// 创建日志记录器
ILogger fileLogger = new FileLogger("logs/app.log", LogLevel.Debug);

// 记录日志
fileLogger.Debug("调试信息");
fileLogger.Info("应用程序启动");

// 记录异常
try
{
    // 业务代码
}
catch (Exception ex)
{
    fileLogger.Error("操作失败", ex);
}

// 清理资源
fileLogger.Dispose();
```

## 项目依赖

### AlgoLibrary

- OpenCvSharp4 (4.10.0.20241108)
- OpenCvSharp4.runtime.win (4.10.0.20241108)

### LogLibrary

- 无外部依赖（仅使用.NET标准库）

### SampleApp

- 引用AlgoLibrary和LogLibrary项目

## 作者

- <nanyangjx@126.com>

