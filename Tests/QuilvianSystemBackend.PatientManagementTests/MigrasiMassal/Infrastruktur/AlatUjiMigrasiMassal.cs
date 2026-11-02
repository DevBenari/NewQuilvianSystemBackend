using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur
{
    /// <summary>
    /// Penangkap seluruh keluaran <see cref="LoggerService"/> — pesan berformat, isi <i>scope</i>, dan
    /// galat lengkap — supaya uji dapat membuktikan tidak ada data pribadi yang tertulis (`PAT-DEC-049`).
    /// </summary>
    public sealed class PencatatLogUji : ILogger<LoggerService>
    {
        private readonly object _kunci = new();
        private readonly List<(LogLevel Level, string Teks)> _entri = [];

        public IReadOnlyList<(LogLevel Level, string Teks)> Entri
        {
            get
            {
                lock (_kunci)
                {
                    return _entri.ToList();
                }
            }
        }

        public string SeluruhTeks => string.Join("\n", Entri.Select(x => x.Teks));

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            var teks = state is IEnumerable<KeyValuePair<string, object?>> pasangan
                ? string.Join(" ", pasangan.Select(x => $"{x.Key}={x.Value}"))
                : state.ToString() ?? string.Empty;

            Tambah(LogLevel.None, "[scope] " + teks);
            return new Pelepas();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Tambah(logLevel, formatter(state, exception) + (exception is null ? string.Empty : "\n" + exception));
        }

        private void Tambah(LogLevel level, string teks)
        {
            lock (_kunci)
            {
                _entri.Add((level, teks));
            }
        }

        private sealed class Pelepas : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    /// <summary>Mencatat setiap perintah SQL yang dijalankan EF Core pada konteks service.</summary>
    public sealed class PenghitungPerintahDb : DbCommandInterceptor
    {
        private readonly List<string> _teks = [];

        public IReadOnlyList<string> Teks => _teks;

        /// <summary>
        /// Perintah EF yang mengubah data: pernyataan yang diawali INSERT, UPDATE, atau DELETE. Nama kolom
        /// seperti <c>UpdateDateTime</c> atau <c>IsDelete</c> di dalam SELECT tidak ikut terhitung.
        /// </summary>
        public IReadOnlyList<string> PerintahTulis =>
            _teks.Where(x => Regex.IsMatch(x, @"^\s*(INSERT|UPDATE|DELETE)\s", RegexOptions.IgnoreCase | RegexOptions.Multiline))
                .ToList();

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            _teks.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            _teks.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            _teks.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _teks.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            _teks.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            _teks.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }

    /// <summary>Menghitung transaksi yang dibuka, di-<i>commit</i>, dan dibatalkan pada konteks service.</summary>
    public sealed class PenghitungTransaksi : DbTransactionInterceptor
    {
        public int Dibuka { get; private set; }

        public int Commit { get; private set; }

        public int Rollback { get; private set; }

        public override ValueTask<DbTransaction> TransactionStartedAsync(
            DbConnection connection, TransactionEndEventData eventData, DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            Dibuka++;
            return ValueTask.FromResult(result);
        }

        public override Task TransactionCommittedAsync(
            DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            Commit++;
            return Task.CompletedTask;
        }

        public override Task TransactionRolledBackAsync(
            DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            Rollback++;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Melempar galat <b>sesudah</b> commit ke-N benar-benar terjadi — sisi lain kasus J: dari sudut
    /// pandang service, commit gagal, padahal data sudah tersimpan.
    /// </summary>
    public sealed class GagalSesudahCommitInterceptor(int gagalPadaCommitKe) : DbTransactionInterceptor
    {
        private int _jumlahCommit;

        public override Task TransactionCommittedAsync(
            DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            _jumlahCommit++;

            if (_jumlahCommit == gagalPadaCommitKe)
            {
                throw new InvalidOperationException("Simulasi koneksi putus sesudah commit pada uji.");
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Mengubah satu kolom pasien yang baru disimpan, di transaksi yang sama, sesudah <c>SaveChanges</c>
    /// — mewakili default atau pemicu database yang menyimpang dari nilai yang dikirim. Dipakai untuk
    /// membuktikan <c>VERIFY_PERSISTED</c> membaca nilai fisik, bukan entity yang disiapkan.
    /// </summary>
    public sealed class UbahNilaiTersimpanInterceptor(string kolom, object nilai) : SaveChangesInterceptor
    {
        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var konteks = eventData.Context!;
            var pasien = konteks.ChangeTracker.Entries<MstPatient>().Select(x => x.Entity).ToList();

            // Nama kolom berasal dari uji, bukan masukan; nilainya tetap dikirim sebagai parameter.
            var sql = "UPDATE \"MstPatient\" SET \"" + kolom + "\" = {0} WHERE \"Id\" = {1}";

            foreach (var p in pasien)
            {
                await konteks.Database.ExecuteSqlRawAsync(sql, [nilai, p.Id], cancellationToken);
            }

            return result;
        }
    }
}
