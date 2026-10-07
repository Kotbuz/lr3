namespace Backend.Logging
{
    // Простой логгер: пишет сообщения в файл logs/log-ГГГГММДД.txt
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _directory;
        private readonly object _lock = new();

        public FileLoggerProvider(string directory)
        {
            _directory = directory;
            Directory.CreateDirectory(_directory);
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

        public void Dispose() { }

        internal void Write(string line)
        {
            var path = Path.Combine(_directory, $"log-{DateTime.Now:yyyyMMdd}.txt");
            lock (_lock)
            {
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }

        private class FileLogger : ILogger
        {
            private readonly FileLoggerProvider _provider;
            private readonly string _category;

            public FileLogger(FileLoggerProvider provider, string category)
            {
                _provider = provider;
                _category = category;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            // В файл пишем только логи нашего приложения
            public bool IsEnabled(LogLevel logLevel) =>
                logLevel >= LogLevel.Information
                && (_category.StartsWith("Backend") || _category.StartsWith("UsersProxy"));

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {formatter(state, exception)}";
                if (exception != null) line += Environment.NewLine + exception;
                _provider.Write(line);
            }
        }
    }
}
