using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

/// <summary>
/// Logger penangkap untuk <see cref="LoggerService"/>, supaya uji dapat membuktikan jejak audit
/// benar-benar ditulis (aksi, tingkat log, pelaku) tanpa menulis ke berkas maupun ke layanan log.
/// </summary>
public sealed class CatatanLogUji : ILogger<LoggerService>
{
    private readonly List<(LogLevel Tingkat, string Pesan)> _entri = [];
    private readonly object _kunci = new();

    public IReadOnlyList<(LogLevel Tingkat, string Pesan)> Entri
    {
        get
        {
            lock (_kunci) return [.. _entri];
        }
    }

    /// <summary>Entri yang memuat aksi <c>Action="..."</c> tertentu dari <see cref="LoggerService"/>.</summary>
    public IReadOnlyList<(LogLevel Tingkat, string Pesan)> UntukAksi(string aksi) =>
        Entri.Where(x => x.Pesan.Contains($"Action=\"{aksi}\"", StringComparison.Ordinal)).ToList();

    /// <summary>
    /// Nilai kolom pelaku <c>UserId="..."</c> pada satu baris log <see cref="LoggerService"/>. Kolom
    /// ini diambil dari properti data <c>UserId</c> atau <c>Id</c>, baru kemudian dari klaim login,
    /// sehingga data audit yang memuat <c>Id</c> milik permintaan akan mencatat id permintaan sebagai
    /// pelaku. Uji membandingkan nilai ini persis, bukan sekadar mencari teks di seluruh pesan.
    /// </summary>
    public static string? PelakuPada(string pesan)
    {
        var cocok = System.Text.RegularExpressions.Regex.Match(pesan, "UserId=\"([^\"]*)\"");
        return cocok.Success ? cocok.Groups[1].Value : null;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        lock (_kunci) _entri.Add((logLevel, formatter(state, exception)));
    }
}
