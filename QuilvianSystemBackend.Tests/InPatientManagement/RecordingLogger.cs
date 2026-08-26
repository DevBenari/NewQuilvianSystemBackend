using Microsoft.Extensions.Logging;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Logger sederhana yang menyimpan seluruh catatan ke dalam daftar, supaya test dapat
/// membuktikan bahwa sebuah peringatan benar-benar ditulis — bukan hanya diasumsikan ada.
/// </summary>
internal sealed class RecordingLogger<T> : ILogger<T>
{
    public List<RecordedLogEntry> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new RecordedLogEntry(logLevel, formatter(state, exception)));
    }

    public bool HasWarningContaining(string fragment)
        => Entries.Any(x =>
            x.Level == LogLevel.Warning &&
            x.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    public int WarningCount => Entries.Count(x => x.Level == LogLevel.Warning);
}

internal sealed record RecordedLogEntry(LogLevel Level, string Message);
