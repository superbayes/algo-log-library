# 项目规则（持久化）

## 终端/沙箱命令（Windows）

- 在沙箱终端中执行任何 dotnet 命令时，一律通过项目根目录脚本 `.\dotnet-utf8.ps1` 运行，避免中文输出乱码。

示例：

```powershell
.\dotnet-utf8.ps1 build .\AlgoLogLibrary.sln -c Release
.\dotnet-utf8.ps1 test
.\dotnet-utf8.ps1 run --project .\SampleApp\SampleApp.csproj
```

