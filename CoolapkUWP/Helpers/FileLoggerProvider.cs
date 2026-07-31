using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Windows.Storage;

namespace CoolapkUWP.Helpers
{
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly object _lock = new object();
        private readonly string _folderPath;

        public FileLoggerProvider()
        {
            _folderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs");
            Directory.CreateDirectory(_folderPath);
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

        internal void Write(string categoryName, string message)
        {
            lock (_lock)
            {
                string filePath = Path.Combine(_folderPath, "app.log");
                File.AppendAllText(filePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{categoryName}] {message}{Environment.NewLine}");
            }
        }

        public void Dispose() { }

        private sealed class FileLogger : ILogger
        {
            private readonly FileLoggerProvider _provider;
            private readonly string _categoryName;

            public FileLogger(FileLoggerProvider provider, string categoryName)
            {
                _provider = provider;
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) => default!;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                string message = formatter(state, exception);
                _provider.Write(_categoryName, message);
            }
        }
    }
}
