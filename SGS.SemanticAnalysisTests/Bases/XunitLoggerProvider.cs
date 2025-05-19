using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SGS.SemanticAnalysisTests.Bases;


/// <summary>
/// A logger provider that logs to XUnit test output
/// </summary>
public class XunitLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(testOutputHelper, categoryName);
    }

    public void Dispose()
    {
    }

    private class XunitLogger(ITestOutputHelper testOutputHelper, string categoryName) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                testOutputHelper.WriteLine($"[{logLevel}] {categoryName}: {formatter(state, exception)}");
                if (exception != null)
                {
                    testOutputHelper.WriteLine($"Exception: {exception}");
                }
            }
            catch
            {
                // If we can't write to the test output (e.g., if the test has already completed), just ignore
            }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }
}