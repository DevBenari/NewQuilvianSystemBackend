using Xunit;

namespace QuilvianSystemBackend.BillingTests.Infrastructure
{
    /// <summary>
    /// Menjaga gerbang pemilihan database test. Seluruh test di sini murni: tidak ada koneksi
    /// yang dibuka dan tidak ada environment variable yang diubah, karena yang diuji justru
    /// penolakan yang terjadi sebelum koneksi dibuka.
    ///
    /// Keputusan pemilik pada RJ-BIL-BE-007 mengizinkan test berjalan terhadap database dev
    /// bersama melalui opt-in yang harus diketik sengaja. Test di sini mengunci batas izin
    /// tersebut, terutama satu hal: opt-in itu tidak boleh pernah membuka database production.
    /// </summary>
    public sealed class BillingTestDatabaseGuardTests
    {
        private const string OptInAktif = BillingTestDatabaseFixture.SharedDatabaseOptInValue;

        private static string Koneksi(string namaDatabase) =>
            $"Host=localhost;Database={namaDatabase};Username=x;Password=y";

        /// <summary>
        /// Inti dari seluruh berkas ini. Apa pun isi opt-in, database yang melayani pengguna
        /// nyata harus tetap ditolak. Bila suatu saat seseorang menyederhanakan gerbang ini
        /// menjadi satu daftar dengan satu jalan keluar, test inilah yang gagal lebih dulu.
        /// </summary>
        [Theory]
        [InlineData("QuilvianProduction")]
        [InlineData("QuilvianProdTest")]
        [InlineData("quilvian_live_test")]
        [InlineData("QuilvianStagingTest")]
        [InlineData("QuilvianUatTest")]
        public void OptInTidakPernahMembukaDatabaseProduksi(string namaDatabase)
        {
            var pesan = PesanPenolakan(Koneksi(namaDatabase), OptInAktif);

            Assert.Contains(BillingTestDatabaseFixture.BlockedMarker, pesan);
            Assert.Contains("mutlak", pesan);
        }

        /// <summary>
        /// Tanpa opt-in, database bersama tetap ditolak. Inilah perilaku bawaan yang menutup
        /// jalur diam penyebab insiden RJ-BIL-BE-002.
        /// </summary>
        [Theory]
        [InlineData("QuilvianNewDevTim01")]
        [InlineData("QuilvianShared")]
        [InlineData("DatabaseTanpaPenanda")]
        public void TanpaOptIn_DatabaseBersamaDitolak(string namaDatabase)
        {
            var pesan = PesanPenolakan(Koneksi(namaDatabase), optInValue: null);

            Assert.Contains(BillingTestDatabaseFixture.BlockedMarker, pesan);
            Assert.Contains(BillingTestDatabaseFixture.SharedDatabaseOptInVariable, pesan);
        }

        /// <summary>
        /// Dengan opt-in, database dev bersama diterima. Ini keputusan pemilik pada
        /// RJ-BIL-BE-007, dan test ini yang membuktikan izin itu benar-benar berlaku.
        /// </summary>
        [Fact]
        public void DenganOptIn_DatabaseDevBersamaDiterima()
        {
            var koneksi = Koneksi("QuilvianNewDevTim01");

            Assert.Equal(
                koneksi,
                BillingTestDatabaseFixture.ValidateTargetDatabase(koneksi, OptInAktif));
        }

        /// <summary>
        /// Nilai opt-in harus persis. Nilai yang mirip tetapi tidak sama tidak boleh membuka
        /// gerbang, sehingga "true", "1", atau beda besar-kecil huruf tidak pernah cukup.
        /// </summary>
        [Theory]
        [InlineData("true")]
        [InlineData("1")]
        [InlineData("yes")]
        [InlineData("i_accept_shared_db_mutation")]
        public void NilaiOptInYangTidakPersis_TidakMembukaGerbang(string nilai)
        {
            var pesan = PesanPenolakan(Koneksi("QuilvianNewDevTim01"), nilai);

            Assert.Contains(BillingTestDatabaseFixture.BlockedMarker, pesan);
        }

        /// <summary>
        /// Connection string kosong tetap gagal tertutup, dengan atau tanpa opt-in. Opt-in
        /// hanya melonggarkan penilaian nama database, bukan menghidupkan kembali fallback ke
        /// berkas konfigurasi yang menyebabkan insiden RJ-BIL-BE-002.
        /// </summary>
        [Fact]
        public void OptInTidakMenghidupkanKembaliFallbackKonfigurasi()
        {
            var pesan = PesanPenolakan(fromEnvironment: null, optInValue: OptInAktif);

            Assert.Contains(BillingTestDatabaseFixture.BlockedMarker, pesan);
            Assert.Contains(BillingTestDatabaseFixture.ConnectionStringVariable, pesan);
        }

        /// <summary>
        /// Nama database test tersendiri tetap diterima tanpa opt-in apa pun, sehingga jalur
        /// yang benar tidak ikut dipersulit oleh pelonggaran ini.
        /// </summary>
        [Fact]
        public void DatabaseTestTersendiri_DiterimaTanpaOptIn()
        {
            var koneksi = Koneksi("QuilvianBillingTest");

            Assert.Equal(
                koneksi,
                BillingTestDatabaseFixture.ValidateTargetDatabase(koneksi, optInValue: null));
        }

        /// <summary>
        /// Connection string yang tidak sah ditolak tanpa memuat isinya pada pesan, agar
        /// kredensial tidak pernah bocor ke output test.
        /// </summary>
        [Fact]
        public void ConnectionStringTidakSah_DitolakTanpaMembocorkanIsinya()
        {
            const string rahasia = "Password=SangatRahasia";

            var pesan = PesanPenolakan($"bukan=connection;string;{rahasia}", OptInAktif);

            Assert.Contains(BillingTestDatabaseFixture.BlockedMarker, pesan);
            Assert.DoesNotContain("SangatRahasia", pesan);
        }

        /// <summary>
        /// Memanggil gerbang dan mengembalikan pesan penolakannya. Gagal bila gerbang justru
        /// meloloskan target, karena gerbang yang diam adalah kegagalan paling berbahaya.
        /// </summary>
        private static string PesanPenolakan(string? fromEnvironment, string? optInValue)
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => BillingTestDatabaseFixture.ValidateTargetDatabase(fromEnvironment, optInValue));

            return exception.Message;
        }
    }
}
