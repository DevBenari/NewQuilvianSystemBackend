using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.Infrastructure
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
        public ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .EnableSensitiveDataLogging(false)
                .Options;

            return new ApplicationDbContext(options);
        }

        public void Dispose()
        {
            // Menutup koneksi otomatis membuang basis datanya, karena ia hanya ada di memori.
            _connection.Dispose();
        }
    }
}
