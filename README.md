# Algo-Log Library 解决方案

基于C# .NET 8.0开发的算法库和日志库解决方案，采用接口+实现的架构方式。

- 这是一个用于在C#中，创建算法库和日志库的模板。

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

### 文件结构（简化）
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

## 命名规范

遵循C#标准命名规范：
- 接口以大写"I"开头：`IImageProcessor`, `ILogger`
- 实现类使用描述性名称：`OpenCVProcessor`, `FileLogger`
- 命名空间按功能组织：`AlgoLibrary.Interfaces`, `AlgoLibrary.Implementations`等

## 功能特性

### 算法库 (AlgoLibrary)

#### 接口：`IImageProcessor`
- 图像加载/保存（同步/异步）
- 图像转换：灰度转换、大小调整、边缘检测
- 图像信息获取
- 支持异步操作
- 实现`IDisposable`接口

#### 实现：`OpenCVProcessor`
- 基于OpenCVSharp 4.10.0
- 完整的错误处理
- 资源自动管理
- 线程安全操作

### 日志库 (LogLibrary)

#### 接口：`ILogger`
- 支持5种日志级别：Debug, Info, Warning, Error, Fatal
- 同步/异步日志记录
- 异常信息记录
- 日志级别过滤
- 实现`IDisposable`接口

#### 实现类
1. **FileLogger** - 文件日志
   - 自动创建日志目录
   - 日志文件头部/尾部信息
   - 自动刷新和文件管理
   - 异常情况下的降级处理（输出到控制台）

## 快速开始

### 环境要求
- .NET 8.0 SDK 或更高版本
- Visual Studio 2022（推荐）或 VS Code

### 打开项目
1. 使用Visual Studio 2022打开 `AlgoLogLibrary.sln`
2. 或者使用命令行：`dotnet build`

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

### 依赖注入示例，使用AddLogLibraryService和AddAlgoLibraryService注册服务
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

## 架构设计

### 接口隔离原则
每个库都通过接口暴露功能，实现细节被隐藏，便于：
- 单元测试（可模拟接口）
- 实现替换（可轻松更换不同的实现）
- 依赖注入

### 异步支持
所有耗时操作都提供同步和异步版本

### 资源管理
所有实现类都实现`IDisposable`接口，确保资源正确释放

### 错误处理
全面的异常处理和错误恢复机制

## 扩展建议

### 算法库扩展
1. 添加更多图像处理算法
2. 支持视频处理
3. 添加机器学习功能
4. 支持GPU加速

### 日志库扩展
1. 添加网络日志（发送到日志服务器）
2. 添加数据库日志
3. 支持结构化日志（JSON格式）
4. 添加日志轮转和归档

## 许可证

本项目仅供学习和参考使用。

## 作者
- nanyangjx@126.com
