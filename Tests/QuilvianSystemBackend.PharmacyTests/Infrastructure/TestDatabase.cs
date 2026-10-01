using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.Pharmacy.Infrastructure
{
    /// <summary>
    /// Menyediakan satu basis data uji yang hidup di dalam memori.
    ///
    /// Cara kerjanya: setiap pemanggilan <see cref="Create"/> membuat basis data SQLite baru
    /// yang berdiri sendiri, lalu membentuk seluruh tabel dari konfigurasi EF Core yang sama
    /// dengan yang dipakai aplikasi. Basis data itu hidup selama koneksinya terbuka, dan ikut
    /// terhapus begitu <see cref="Dispose"/> dipanggil.
    ///
    /// Akibatnya, satu uji tidak pernah melihat data milik uji lain, walaupun keduanya berjalan
    /// bersamaan. Tidak ada berkas yang tertinggal di disk dan tidak ada yang perlu dibersihkan
    /// secara manual.
    ///
    /// PENTING: basis data uji ini TIDAK PERNAH menyentuh basis data mana pun yang tercatat di
    /// appsettings. Uji yang mengarah ke basis data bersama akan mengganggu pekerjaan orang lain,
    /// dan itu dilarang.
    /// </summary>
    public sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Membuat basis data uji baru yang kosong beserta seluruh tabelnya.
        /// </summary>
        public static TestDatabase Create()
        {
            // "Filename=:memory:" berarti basis data hanya ada di memori.
            // Setiap koneksi memiliki basis datanya sendiri, sehingga uji saling terpisah.
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var database = new TestDatabase(connection);

            using (var context = database.CreateContext())
            {
                context.Database.EnsureCreated();
            }

            return database;
        }

        /// <summary>
        /// Membuat konteks basis data baru yang menunjuk ke basis data uji yang sama.
        ///
        /// Membuat konteks baru berguna untuk membuktikan bahwa data benar-benar tersimpan,
        /// bukan sekadar masih tertahan di memori konteks sebelumnya.
        /// </summary>
        /// <param name="catatEksekusi">
        /// Bila diisi, setiap baris log EF Core diteruskan ke sana. Dipakai uji yang perlu
        /// menghitung berapa perintah SQL yang benar-benar dijalankan sebuah pembacaan, misalnya
        /// untuk membuktikan sebuah daftar tidak melahirkan satu query tambahan per baris.
        /// Dibiarkan kosong pada pemakaian biasa sehingga tidak ada biaya logging sama sekali.
        /// </param>
        public ApplicationDbContext CreateContext(Action<string>? catatEksekusi = null)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, ModelUjiSqlite>()
                .EnableSensitiveDataLogging(false);

            if (catatEksekusi != null)
            {
                builder = builder.LogTo(
                    catatEksekusi,
                    new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted });
            }

            return new ApplicationDbContext(builder.Options);
        }

        public void Dispose()
        {
            // Menutup koneksi otomatis membuang basis datanya, karena ia hanya ada di memori.
            _connection.Dispose();
        }
    }

    /// <summary>
    /// Melepas potongan SQL yang hanya dimengerti PostgreSQL sebelum tabel uji dibentuk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Model aplikasi memuat dua bentuk SQL yang tidak dikenal SQLite, keduanya milik modul di
    /// luar Operasi:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// check constraint <c>num_nonnulls(...)</c> — dipakai Finance, Clinical, dan Pharmacy untuk
    /// menyatakan "tepat satu foreign key boleh terisi";
    /// </item>
    /// <item>
    /// nilai bawaan dengan cast PostgreSQL seperti <c>'{}'::jsonb</c>.
    /// </item>
    /// </list>
    /// <para>
    /// Keduanya membuat <c>EnsureCreated</c> gagal di tengah jalan — <c>no such function</c> atau
    /// <c>unrecognized token: ":"</c> — dan ketika itu terjadi tak satu pun tabel terbentuk,
    /// sehingga seluruh uji gagal dengan sebab yang menyesatkan.
    /// </para>
    /// <para>
    /// Yang dilepas hanya bentuknya di basis data uji, bukan di model aplikasi maupun di
    /// migration; PostgreSQL tetap menerimanya apa adanya. Aturan itu juga tidak menjaga apa pun
    /// yang diuji di sini, karena tabel yang memilikinya milik modul lain dan tidak disentuh uji
    /// modul Operasi. Pelepasannya sengaja sempit — hanya yang benar-benar memuat bentuk khas
    /// PostgreSQL — supaya check constraint dan nilai bawaan lain tetap berlaku dan tetap dapat
    /// menggagalkan uji bila memang dilanggar.
    /// </para>
    /// </remarks>
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
            }
        }

        private static bool KhasPostgres(string? sql) =>
            sql is not null &&
            (sql.Contains(CastPostgres, StringComparison.Ordinal) ||
                FungsiKhususPostgres.Any(f => sql.Contains(f, StringComparison.OrdinalIgnoreCase)));
    }
}
