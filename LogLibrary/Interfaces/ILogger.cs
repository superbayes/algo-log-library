using System;
using System.Threading.Tasks;

namespace LogLibrary.Interfaces
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试信息
        /// </summary>
        Debug,
        
        /// <summary>
        /// 一般信息
        /// </summary>
        Info,
        
        /// <summary>
        /// 警告信息
        /// </summary>
        Warning,
        
        /// <summary>
        /// 错误信息
        /// </summary>
        Error,
        
        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal
    }

    /// <summary>
    /// 日志记录器接口
    /// 定义日志记录的基本操作
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        void Debug(string message, Exception? exception = null);

        /// <summary>
        /// 异步记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task DebugAsync(string message, Exception? exception = null);

        /// <summary>
        /// 记录一般信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        void Info(string message, Exception? exception = null);

        /// <summary>
        /// 异步记录一般信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task InfoAsync(string message, Exception? exception = null);

        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        void Warning(string message, Exception? exception = null);

        /// <summary>
        /// 异步记录警告信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task WarningAsync(string message, Exception? exception = null);

        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        void Error(string message, Exception? exception = null);

        /// <summary>
        /// 异步记录错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task ErrorAsync(string message, Exception? exception = null);

        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        void Fatal(string message, Exception? exception = null);

        /// <summary>
        /// 异步记录致命错误信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task FatalAsync(string message, Exception? exception = null);

        /// <summary>
        /// 记录指定级别的日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        void Log(LogLevel level, string message, Exception? exception = null);

        /// <summary>
        /// 异步记录指定级别的日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息（可选）</param>
        Task LogAsync(LogLevel level, string message, Exception? exception = null);

        /// <summary>
        /// 设置日志级别过滤器
        /// </summary>
        /// <param name="minLevel">最低日志级别</param>
        void SetMinimumLevel(LogLevel minLevel);

        /// <summary>
        /// 获取当前日志级别过滤器
        /// </summary>
        /// <returns>当前最低日志级别</returns>
        LogLevel GetMinimumLevel();
    }
}
