using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    /// <summary>
    /// Basis data uji SQLite di dalam memori, dibentuk dari konfigurasi EF Core aplikasi.
    /// </summary>
    /// <remarks>
    /// Sama dengan basis data uji modul Farmasi. Basis data ini TIDAK PERNAH menyentuh basis data
    /// mana pun yang tercatat di appsettings, termasuk staging. Tabel <c>migration.rsmmc_*</c>
    /// tidak ada di sini; pembacanya diganti tiruan pada uji.
    /// </remarks>
    public sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection)
        {
            _connection = connection;
        }

        public static TestDatabase Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var database = new TestDatabase(connection);

            using (var context = database.CreateContext())
            {
                context.Database.EnsureCreated();
            }

            // Foreign key dimatikan hanya pada basis data uji, supaya pasien uji tidak perlu
            // menyeret master wilayah, membership, dan modul lain. PostgreSQL tetap menegakkannya.
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = OFF;";
                pragma.ExecuteNonQuery();
            }

            return database;
        }

        public ApplicationDbContext CreateContext(params IInterceptor[] interceptors)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, ModelUjiSqlite>()
                .EnableSensitiveDataLogging(false);

            if (interceptors.Length > 0)
            {
                builder = builder.AddInterceptors(interceptors);
            }

            return new ApplicationDbContext(builder.Options);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

    /// <summary>
    /// Melepas potongan SQL khas PostgreSQL sebelum tabel uji dibentuk. Sama dengan uji Farmasi.
    /// </summary>
    public sealed class ModelUjiSqlite(ModelCustomizerDependencies dependencies)
        : RelationalModelCustomizer(dependencies)
    {
        private static readonly string[] FungsiKhususPostgres = ["num_nonnulls"];

        private const string CastPostgres = "::";

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            foreach (var entitas in modelBuilder.Model.GetEntityTypes())
            {
                var bermasalah = entitas.GetDeclaredCheckConstraints()
                    .Where(x => KhasPostgres(x.Sql))
                    .Select(x => x.ModelName)
                    .ToList();

                foreach (var nama in bermasalah) entitas.RemoveCheckConstraint(nama);

                foreach (var properti in entitas.GetDeclaredProperties())
                {
                    if (KhasPostgres(properti.GetDefaultValueSql())) properti.SetDefaultValueSql(null);
                    if (KhasPostgres(properti.GetComputedColumnSql())) properti.SetComputedColumnSql(null);
                }

                foreach (var indeks in entitas.GetDeclaredIndexes()
                    .Where(x => x.IsUnique && x.GetFilter() is not null)
                    .ToList())
                {
                    indeks.IsUnique = false;
                }
            }
        }

        private static bool KhasPostgres(string? sql) =>
            sql is not null &&
            (sql.Contains(CastPostgres, StringComparison.Ordinal) ||
                FungsiKhususPostgres.Any(f => sql.Contains(f, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Menggagalkan <c>SaveChanges</c> pada pemanggilan ke-N, untuk mensimulasikan kegagalan
    /// database sesudah QR baru dibuat. <paramref name="sebelumGagal"/> dijalankan tepat sebelum
    /// kegagalan, misalnya untuk mensimulasikan pihak lain mengganti berkas QR baru.
    /// </summary>
    public sealed class GagalSimpanInterceptor(int gagalPadaPemanggilanKe, Action? sebelumGagal = null)
        : SaveChangesInterceptor
    {
        private int _jumlahPemanggilan;

        public int JumlahPemanggilan => _jumlahPemanggilan;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _jumlahPemanggilan++;

            if (_jumlahPemanggilan == gagalPadaPemanggilanKe)
            {
                sebelumGagal?.Invoke();
                throw new DbUpdateException("Simulasi kegagalan database pada uji.");
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    /// <summary>
    /// Menggagalkan <c>CommitAsync</c> pada commit ke-N. Dari sudut pandang service, hasil commit
    /// semacam ini tidak dapat dipastikan.
    /// </summary>
    public sealed class GagalCommitInterceptor(int gagalPadaCommitKe) : DbTransactionInterceptor
    {
        private int _jumlahCommit;

        public int JumlahCommit => _jumlahCommit;

        public override ValueTask<InterceptionResult> TransactionCommittingAsync(
            System.Data.Common.DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            _jumlahCommit++;

            if (_jumlahCommit == gagalPadaCommitKe)
            {
                throw new InvalidOperationException("Simulasi koneksi putus saat commit pada uji.");
            }

            return base.TransactionCommittingAsync(transaction, eventData, result, cancellationToken);
        }
    }
}
