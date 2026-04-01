using System;
using System.IO;
using System.Threading.Tasks;
using LogLibrary.Interfaces;

namespace LogLibrary.Implementations
{
    /// <summary>
    /// 文件日志记录器实现
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logDirectory;
        private readonly string _fileNameBase;
        private StreamWriter? _writer;
        private string? _activeDateStamp;
        private LogLevel _minimumLevel = LogLevel.Debug;
        private bool _disposed = false;
        private readonly object _lockObject = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logFilePath">日志文件路径</param>
        public FileLogger(string logFilePath)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                throw new ArgumentException("日志文件路径不能为空", nameof(logFilePath));
            }

            var location = ResolveLogLocation(logFilePath);
            _logDirectory = location.logDirectory;
            _fileNameBase = location.fileNameBase;
            Directory.CreateDirectory(_logDirectory);
        }

        /// <summary>
        /// 构造函数，指定最低日志级别
        /// </summary>
        /// <param name="logFilePath">日志文件路径</param>
        /// <param name="minimumLevel">最低日志级别</param>
        public FileLogger(string logFilePath, LogLevel minimumLevel) : this(logFilePath)
        {
            _minimumLevel = minimumLevel;
        }

        /// <summary>
        /// 解析日志文件路径，确定日志目录和文件名基础
        /// </summary>
        /// <param name="configuredPath"></param>
        /// <returns>包含日志目录和文件名基础的元组</returns>
        private static (string logDirectory, string fileNameBase) ResolveLogLocation(string configuredPath)
        {
            var trimmed = configuredPath.Trim();
            var combined = Path.IsPathRooted(trimmed) ? trimmed : Path.Combine(AppContext.BaseDirectory, trimmed);
            var fullPath = Path.GetFullPath(combined);

            if (!Path.HasExtension(fullPath))
            {
                // 如果没有扩展名，假设是目录路径
                return (fullPath, "application");
            }

            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = AppContext.BaseDirectory;
            }

            var fileNameBase = Path.GetFileNameWithoutExtension(fullPath);
            if (string.IsNullOrWhiteSpace(fileNameBase))
            {
                fileNameBase = "application";
            }

            return (directory, fileNameBase);
        }


        public void Debug(string message, Exception? exception = null)
        {
            Log(LogLevel.Debug, message, exception);
        }
        /// <summary>
        /// 异步记录调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async Task DebugAsync(string message, Exception? exception = null)
        {
            Debug(message, exception);
            await Task.CompletedTask;
        }

        public void Info(string message, Exception? exception = null)
        {
            Log(LogLevel.Info, message, exception);
        }

        public async Task InfoAsync(string message, Exception? exception = null)
        {
            Info(message, exception);
            await Task.CompletedTask;
        }

        public void Warning(string message, Exception? exception = null)
        {
            Log(LogLevel.Warning, message, exception);
        }

        public async Task WarningAsync(string message, Exception? exception = null)
        {
            Warning(message, exception);
            await Task.CompletedTask;
        }

        public void Error(string message, Exception? exception = null)
        {
            Log(LogLevel.Error, message, exception);
        }

        public async Task ErrorAsync(string message, Exception? exception = null)
        {
            Error(message, exception);
            await Task.CompletedTask;
        }

        public void Fatal(string message, Exception? exception = null)
        {
            Log(LogLevel.Fatal, message, exception);
        }

        public async Task FatalAsync(string message, Exception? exception = null)
        {
            Fatal(message, exception);
            await Task.CompletedTask;
        }

        public void Log(LogLevel level, string message, Exception? exception = null)
        {
            LogInternal(level, message, exception);
        }

        public async Task LogAsync(LogLevel level, string message, Exception? exception = null)
        {
            Log(level, message, exception);
            await Task.CompletedTask;
        }

        public void SetMinimumLevel(LogLevel minLevel)
        {
            lock (_lockObject)
            {
                _minimumLevel = minLevel;
            }
        }

        public LogLevel GetMinimumLevel()
        {
            lock (_lockObject)
            {
                return _minimumLevel;
            }
        }

        private void LogInternal(LogLevel level, string message, Exception? exception)
        {
            if (level < _minimumLevel)
            {
                return;
            }

            lock (_lockObject)
            {
                if (_disposed)
                {
                    return;
                }

                var now = DateTime.Now;
                var dateStamp = now.ToString("yyyyMMdd");
                EnsureWriter(dateStamp);

                var timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelString = GetLevelString(level);
                var logMessage = $"[{timestamp}] [{levelString}] {message}";

                _writer?.WriteLine(logMessage);
                if (exception != null)
                {
                    _writer?.WriteLine(exception.ToString());
                }
            }
        }

        private void EnsureWriter(string dateStamp)
        {
            if (string.Equals(_activeDateStamp, dateStamp, StringComparison.Ordinal))
            {
                return;
            }

            CloseWriter();
            _activeDateStamp = dateStamp;
            Directory.CreateDirectory(_logDirectory);
            var filePath = Path.Combine(_logDirectory, $"{_fileNameBase}_{dateStamp}.log");
            _writer = new StreamWriter(filePath, true) { AutoFlush = true };
        }

        private string GetLevelString(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "DEBUG",
                LogLevel.Info => "INFO",
                LogLevel.Warning => "WARNING",
                LogLevel.Error => "ERROR",
                LogLevel.Fatal => "FATAL",
                _ => "UNKNOWN"
            };
        }

        private void CloseWriter()
        {
            try
            {
                _writer?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _writer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    lock (_lockObject)
                    {
                        CloseWriter();
                    }
                }

                _disposed = true;
            }
        }

        ~FileLogger()
        {
            Dispose(false);
        }
    }
}
