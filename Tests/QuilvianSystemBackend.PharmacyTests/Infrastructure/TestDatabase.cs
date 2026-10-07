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

            // Penegakan foreign key dimatikan dengan sengaja, dan hanya pada basis data uji ini.
            //
            // Yang diuji aturan alur Farmasi. Satu resep menunjuk pasien, kunjungan, konsultasi,
            // dan dokter yang seluruhnya milik modul lain; menyediakan baris sungguhan untuk
            // semuanya berarti menyeret pendaftaran, penjadwalan, dan master dokter ke dalam uji
            // yang tidak membuktikan apa pun tentang Farmasi, sekaligus membuat uji ini pecah
            // setiap kali modul lain menambah kolom wajib.
            //
            // Keutuhan acuan tetap ditegakkan PostgreSQL pada lingkungan sungguhan.
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = OFF;";
                pragma.ExecuteNonQuery();
            }

            // Dua fungsi PostgreSQL disediakan tiruannya, dan HANYA pada basis data uji ini.
            //
            // `DrugReturnService.VerifyAsync` mengambil kunci penasihat sebelum menambah stok:
            //
            //     SELECT pg_advisory_xact_lock(hashtext({0}));
            //
            // dijaga `Database.IsRelational()`, yang bernilai benar juga untuk SQLite — sehingga
            // SQLite ikut menjalankannya dan gagal dengan "no such function: hashtext".
            //
            // Yang disediakan di sini bentuknya saja, bukan perilakunya. `hashtext` mengembalikan
            // hash yang stabil untuk satu masukan, dan `pg_advisory_xact_lock` tidak mengunci apa
            // pun. Itu memadai karena uji ini berjalan pada basis data dalam memori yang dipakai
            // satu uji saja, sehingga tidak ada pesaing yang perlu dikunci.
            //
            // AKIBATNYA, DAN INI PENTING: perilaku penguncian itu sendiri TIDAK diuji di sini.
            // Serialisasi dua verifikasi bersamaan hanya dapat dibuktikan pada PostgreSQL, dan
            // itu dicatat sebagai keterbatasan, bukan ditutup dengan uji yang terlihat hijau.
            //
            // Sumber perilaku produksi tetap PostgreSQL. Source aplikasi tidak diubah sedikit pun
            // untuk membuat uji ini lulus.
            connection.CreateFunction<string?, long>(
                "hashtext",
                teks => teks == null ? 0L : StableHash(teks));

            connection.CreateFunction<long, object?>(
                "pg_advisory_xact_lock",
                _ => null);

            return database;
        }

        /// <summary>
        /// Hash stabil untuk tiruan <c>hashtext</c>. Nilainya tidak perlu sama dengan milik
        /// PostgreSQL — yang dibutuhkan hanya bahwa masukan yang sama menghasilkan nilai yang
        /// sama di dalam satu uji.
        /// </summary>
        private static long StableHash(string value)
        {
            unchecked
            {
                var hash = 5381L;
                foreach (var c in value) hash = ((hash << 5) + hash) + c;
                return hash;
            }
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

                // Indeks unik TERSARING dilepas keunikannya di SQLite. Saringannya memakai tanda
                // kutip ganda PostgreSQL — misalnya `"IsDelete" = false` — dan SQLite tidak
                // membawanya, sehingga indeksnya menjadi unik TANPA syarat dan baris yang sudah
                // ditandai terhapus tetap memegang tempatnya.
                //
                // Keunikan bersyaratnya tetap ditegakkan PostgreSQL pada lingkungan sungguhan.
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
}
