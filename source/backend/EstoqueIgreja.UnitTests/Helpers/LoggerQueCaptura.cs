using Microsoft.Extensions.Logging;

namespace EstoqueIgreja.UnitTests.Helpers;

/// <summary>
/// Guarda as mensagens já formatadas, para o teste conferir o que foi escrito no log.
/// </summary>
public class LoggerQueCaptura<T> : ILogger<T>
{
    public List<string> Mensagens { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) => Mensagens.Add(formatter(state, exception));
}
