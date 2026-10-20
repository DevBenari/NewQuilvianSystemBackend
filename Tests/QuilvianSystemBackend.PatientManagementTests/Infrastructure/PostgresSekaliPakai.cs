using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    /// <summary>
    /// Basis data PostgreSQL 15 sekali pakai untuk bukti provider-real (`PAT-DEC-065`, kontrak
    /// `1.2.1` B15).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Server disediakan pemilik atau developer — lokal atau kontainer — dan alamatnya diberikan
    /// lewat environment variable <see cref="VariabelKoneksi"/>, berupa connection string ke basis
    /// data pemeliharaan server itu (misalnya <c>postgres</c>) dengan hak <c>CREATE DATABASE</c>.
    /// Connection string tidak pernah ditulis ke repository maupun laporan.
    /// </para>
    /// <para>
    /// Setiap pemakaian membuat basis data baru bernama acak, membentuk skema dari model EF aplikasi
    /// yang sebenarnya (<c>EnsureCreated</c>) sehingga default kolom, indeks unik, dan tipe kolom
    /// sama dengan konfigurasi aplikasi, lalu menghapusnya kembali — juga saat uji gagal. Basis data
    /// lain di server itu tidak dibaca dan tidak diubah. Staging bersama dan Produksi tidak boleh
    /// dipakai sebagai server ini.
    /// </para>
    /// <para>
    /// Bila <see cref="VariabelWajib"/> bernilai <c>1</c>, bukti dianggap diminta: server yang tidak
    /// tersedia atau bukan PostgreSQL 15 membuat uji <b>gagal</b>, tidak dilewati.
    /// </para>
    /// <para>
    /// <b>Prasyarat server bukti sekali pakai (`BE-PAT-MIG-003A`, temuan `F-14`).</b> Model
    /// <c>ApplicationDbContext</c> kini cukup besar sehingga <c>EnsureCreatedAsync</c> gagal pada
    /// nilai bawaan PostgreSQL 15 <c>max_locks_per_transaction = 64</c> dengan SQLSTATE <c>53200</c>
    /// (<i>out of shared memory</i>, petunjuk server: <i>increase max_locks_per_transaction</i>).
    /// Server bukti yang lulus 23/23 pada 19 Oktober 2026 memakai
    /// <c>max_locks_per_transaction = 2048</c>, misalnya
    /// <c>docker run … postgres:15 -c max_locks_per_transaction=2048</c>. Ini <b>hanya</b> syarat
    /// server uji sekali pakai — <b>bukan</b> anjuran mengubah PostgreSQL DEV, STAGING, maupun
    /// PRODUCTION, dan tidak menyentuh <c>appsettings*</c> maupun infrastruktur deployment.
    /// </para>
    /// <para>
    /// <b>Jalankan suite provider-real secara serial</b> pada server bukti sekali pakai saat ini:
    /// <c>-- xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=1</c>. Pada 20 Oktober 2026
    /// run paralel menghasilkan 21/23: dua kasus gagal saat menyiapkan basis data sekali pakai
    /// (<see cref="BuatAsync"/> → <c>JalankanAdminAsync</c>, <c>NpgsqlException</c> "Exception while reading
    /// from stream", soket transport terputus) tanpa crash server; keduanya lulus saat diulang
    /// tersendiri, dan run serial lulus 23/23. Ini temuan operasional harness bukti, bukan kegagalan
    /// fungsi, dan <b>bukan</b> syarat PostgreSQL DEV, STAGING, maupun PRODUCTION.
    /// </para>
    /// </remarks>
    public sealed class PostgresSekaliPakai : IAsyncDisposable
    {
        public const string VariabelKoneksi = "QUILVIAN_PG_PROOF_CONNECTION";
        public const string VariabelWajib = "QUILVIAN_PG_PROOF_REQUIRED";
        public const int VersiMayorDiwajibkan = 15;

        private readonly string _koneksiAdmin;

        private PostgresSekaliPakai(string koneksiAdmin, string namaDatabase, string koneksiDatabase, Version versiServer)
        {
            _koneksiAdmin = koneksiAdmin;
            NamaDatabase = namaDatabase;
            KoneksiDatabase = koneksiDatabase;
            VersiServer = versiServer;
        }

        public static string? KoneksiAdmin => Environment.GetEnvironmentVariable(VariabelKoneksi);

        public static bool BuktiDiminta =>
            string.Equals(Environment.GetEnvironmentVariable(VariabelWajib), "1", StringComparison.Ordinal);

        public string NamaDatabase { get; }

        public string KoneksiDatabase { get; }

        public Version VersiServer { get; }

        public static async Task<PostgresSekaliPakai> BuatAsync()
        {
            var koneksiAdmin = KoneksiAdmin;

            if (string.IsNullOrWhiteSpace(koneksiAdmin))
            {
                throw new InvalidOperationException(
                    $"Bukti PostgreSQL sungguhan diminta, tetapi {VariabelKoneksi} tidak diisi. " +
                    "Sediakan server PostgreSQL 15 sekali pakai; bukti SQLite saja tidak cukup (PAT-DEC-065).");
            }

            var admin = new NpgsqlConnectionStringBuilder(koneksiAdmin);

            // Pengaman: basis data aplikasi tidak boleh dijadikan titik masuk server bukti.
            if (!string.IsNullOrWhiteSpace(admin.Database) &&
                admin.Database.StartsWith("Quilvian", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{VariabelKoneksi} menunjuk basis data aplikasi. Pakai basis data pemeliharaan server sekali pakai.");
            }

            Version versi;

            await using (var koneksi = new NpgsqlConnection(admin.ConnectionString))
            {
                await koneksi.OpenAsync();
                versi = koneksi.PostgreSqlVersion;
            }

            if (versi.Major != VersiMayorDiwajibkan)
            {
                throw new InvalidOperationException(
                    $"Bukti provider-real membutuhkan PostgreSQL {VersiMayorDiwajibkan}; server melaporkan {versi}.");
            }

            var namaDatabase = "pat_proof_" + Guid.NewGuid().ToString("N");

            // TEMPLATE template0, bukan template1 bawaan: CREATE DATABASE dari template1 gagal 55006
            // ("source database template1 is being accessed by other users") selama ada sesi lain di
            // template1. Model EF hanya memakai fungsi inti PostgreSQL, tanpa ekstensi, sehingga tidak
            // bergantung pada isi template1. template1 tidak diubah dan sesinya tidak dihentikan.
            await JalankanAdminAsync(
                admin.ConnectionString,
                $"CREATE DATABASE \"{namaDatabase}\" WITH TEMPLATE template0;");

            var target = new NpgsqlConnectionStringBuilder(admin.ConnectionString)
            {
                Database = namaDatabase,
                Pooling = false
            };

            var hasil = new PostgresSekaliPakai(admin.ConnectionString, namaDatabase, target.ConnectionString, versi);

            try
            {
                await using var konteks = hasil.BuatKonteks();
                await konteks.Database.EnsureCreatedAsync();
            }
            catch
            {
                await hasil.DisposeAsync();
                throw;
            }

            return hasil;
        }

        /// <summary>Konteks aplikasi dengan provider Npgsql, seperti registrasi di <c>Program.cs</c>.</summary>
        public ApplicationDbContext BuatKonteks() =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(KoneksiDatabase)
                .Options);

        public async ValueTask DisposeAsync()
        {
            NpgsqlConnection.ClearAllPools();

            await JalankanAdminAsync(_koneksiAdmin, $"DROP DATABASE IF EXISTS \"{NamaDatabase}\" WITH (FORCE);");
        }

        private static async Task JalankanAdminAsync(string koneksiAdmin, string sql)
        {
            await using var koneksi = new NpgsqlConnection(koneksiAdmin);
            await koneksi.OpenAsync();
            await using var perintah = new NpgsqlCommand(sql, koneksi);
            await perintah.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Uji provider-real. Tanpa server dan tanpa permintaan bukti, uji dilewati dengan alasan yang
    /// terlihat — tidak pernah dilaporkan lulus. Dengan <c>QUILVIAN_PG_PROOF_REQUIRED=1</c>, uji
    /// selalu dijalankan sehingga server yang hilang membuatnya gagal.
    /// </summary>
    public sealed class FactPostgresSungguhanAttribute : FactAttribute
    {
        public FactPostgresSungguhanAttribute()
        {
            if (string.IsNullOrWhiteSpace(PostgresSekaliPakai.KoneksiAdmin) && !PostgresSekaliPakai.BuktiDiminta)
            {
                Skip =
                    $"Bukti PostgreSQL sungguhan tidak diminta: isi {PostgresSekaliPakai.VariabelKoneksi} " +
                    $"(server PostgreSQL 15 sekali pakai) dan {PostgresSekaliPakai.VariabelWajib}=1.";
            }
        }
    }

    /// <inheritdoc cref="FactPostgresSungguhanAttribute"/>
    public sealed class TheoryPostgresSungguhanAttribute : TheoryAttribute
    {
        public TheoryPostgresSungguhanAttribute()
        {
            if (string.IsNullOrWhiteSpace(PostgresSekaliPakai.KoneksiAdmin) && !PostgresSekaliPakai.BuktiDiminta)
            {
                Skip =
                    $"Bukti PostgreSQL sungguhan tidak diminta: isi {PostgresSekaliPakai.VariabelKoneksi} " +
                    $"(server PostgreSQL 15 sekali pakai) dan {PostgresSekaliPakai.VariabelWajib}=1.";
            }
        }
    }
}
