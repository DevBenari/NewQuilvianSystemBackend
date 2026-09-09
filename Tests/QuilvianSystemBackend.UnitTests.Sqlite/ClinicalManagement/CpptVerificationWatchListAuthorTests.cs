using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-067</c> - daftar pantau verifikasi menyebut nama penulis,
    /// bukan nomor pengguna.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sebelum task ini, butir daftar pantau hanya membawa <c>ProviderUserId</c>. Supervisor yang
    /// ingin mengingatkan penulis catatan harus membuka catatannya satu per satu, dan layar
    /// terpaksa menuliskan "Nama penulis belum tersedia".
    /// </para>
    /// <para>
    /// <b>Daftar pantau adalah catatan historis.</b> Namanya diambil snapshot lebih dulu, supaya
    /// akun yang berganti nama tidak menulis ulang siapa yang menulis catatan lama.
    /// </para>
    /// <para>
    /// <b>Daftar pantau menjawab siapa, kapan, dan seberapa terlambat - tidak lebih.</b> Nol isi
    /// klinis boleh bocor ke sana, dan itu dibuktikan uji, bukan sekadar dinyatakan.
    /// </para>
    /// </remarks>
    public class CpptVerificationWatchListAuthorTests
    {
        /// <summary>
        /// Teks samaran yang mudah dicari. Bila satu huruf pun darinya muncul pada butir daftar
        /// pantau, berarti isi klinis ikut bocor.
        /// </summary>
        private const string TeksKlinisSamaran = "RAHASIAKLINIS-JANGAN-BOCOR";

        private static CpptVerificationService BuatService(ApplicationDbContext c) =>
            new CpptVerificationService(c, new InpatientClinicalContextService(c));

        private static TrxPatientIntegratedProgressNote TulisCatatan(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k,
            Guid? penulisUserId,
            string? snapshotNama = null,
            CpptVerificationStatus status = CpptVerificationStatus.Pending,
            DateTime? batasVerifikasi = null,
            DateTime? waktuCatatan = null)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var catatan = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{pembeda}",
                PatientId = k.PatientId,
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                ProfessionType = "Nurse",
                ProviderUserId = penulisUserId,
                ProviderDisplayNameSnapshot = snapshotNama,
                NoteDateTime = waktuCatatan ?? DateTime.UtcNow.AddHours(-2),

                // Seluruh kolom isi diisi teks samaran supaya kebocoran apa pun ketahuan.
                SubjectiveSummary = TeksKlinisSamaran,
                ObjectiveSummary = TeksKlinisSamaran,
                AssessmentSummary = TeksKlinisSamaran,
                PlanSummary = TeksKlinisSamaran,
                Instruction = TeksKlinisSamaran,
                Evaluation = TeksKlinisSamaran,
                NoteText = TeksKlinisSamaran,
                PrivateNote = TeksKlinisSamaran,

                VerificationStatus = status,
                VerificationDueAt = batasVerifikasi,
                IsActive = true
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(catatan);
            context.SaveChanges();

            return catatan;
        }

        // =====================================================================
        // Kriteria 1 - setiap butir menyebut nama penulisnya
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-067 AC 1</c> - setiap butir daftar pantau membawa nama penulis catatan.
        /// </summary>
        [Fact]
        public async Task DaftarPantau_SetiapButirMembawaNamaPenulisnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var batas = DateTime.UtcNow.AddHours(1);
            TulisCatatan(context, k, perawat.Id, batasVerifikasi: batas);

            var hasil = await BuatService(context)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            var butir = Assert.Single(hasil.WatchList);

            Assert.Equal(perawat.Id, butir.ProviderUserId);
            Assert.Equal(perawat.DisplayName, butir.ProviderName);
            Assert.False(string.IsNullOrWhiteSpace(butir.ProviderName));
        }

        // =====================================================================
        // Kriteria 2 - snapshot lebih dulu; akun yang berganti nama tidak menulis ulang riwayat
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-067 AC 2</c> - akun yang berganti nama <b>setelah</b> catatannya ditulis
        /// tidak mengubah nama penulis pada daftar pantau.
        /// </summary>
        /// <remarks>
        /// Contoh pada kartu task: Ns. Sari menulis catatan pada 1 September, lalu namanya di
        /// sistem berubah menjadi Ns. Sari Wijaya pada 5 September. Daftar pantau tetap menyebut
        /// "Ns. Sari" untuk catatan 1 September itu.
        /// </remarks>
        [Fact]
        public async Task AkunBergantiNamaSetelahMenulis_DaftarPantauTetapMenyebutNamaLama()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            TulisCatatan(
                context, k, perawat.Id,
                snapshotNama: "Ns. Sari",
                batasVerifikasi: DateTime.UtcNow.AddHours(1));

            // Nama akunnya berubah sesudah catatannya ditulis.
            perawat.DisplayName = "Ns. Sari Wijaya";
            context.SaveChanges();

            using var pembaca = database.CreateContext();

            var hasil = await BuatService(pembaca)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            var butir = Assert.Single(hasil.WatchList);

            Assert.Equal("Ns. Sari", butir.ProviderName);
            Assert.NotEqual("Ns. Sari Wijaya", butir.ProviderName);
        }

        /// <summary>
        /// <c>BE-RWI-067 AC 2</c> - snapshot yang kosong jatuh ke nama akun, bukan menjadi kosong.
        /// </summary>
        [Fact]
        public async Task SnapshotKosong_NamanyaJatuhKeRelasiPengguna()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            TulisCatatan(
                context, k, perawat.Id,
                snapshotNama: "   ",
                batasVerifikasi: DateTime.UtcNow.AddHours(1));

            var hasil = await BuatService(context)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            var butir = Assert.Single(hasil.WatchList);

            Assert.Equal(perawat.DisplayName, butir.ProviderName);
        }

        // =====================================================================
        // Kriteria 3 - penulis yang tidak dikenali menghasilkan kolom kosong
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-067 AC 3</c> - penulis yang tidak dapat dikenali sama sekali menghasilkan
        /// nama <b>kosong</b>, bukan nomor pengguna mentah dan bukan nama tebakan.
        /// </summary>
        [Fact]
        public async Task PenulisTidakDikenali_NamanyaKosongDanBukanNomorPengguna()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            TulisCatatan(
                context, k, penulisUserId: null,
                batasVerifikasi: DateTime.UtcNow.AddHours(1));

            var hasil = await BuatService(context)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            var butir = Assert.Single(hasil.WatchList);

            Assert.Null(butir.ProviderName);
            Assert.Null(butir.ProviderUserId);
        }

        // =====================================================================
        // Kriteria 4 - nol isi klinis pada butir daftar pantau
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-067 AC 4</c> - butir daftar pantau tidak memuat satu pun isi klinis, dan itu
        /// dibuktikan dengan memeriksa <b>seluruh</b> nilai teksnya, bukan kolom yang dipilih.
        /// </summary>
        /// <remarks>
        /// Catatannya sengaja diisi teks samaran pada kedelapan kolom isinya. Bila satu pun butir
        /// membawa potongan teks itu - di kolom mana pun, termasuk kolom yang kelak ditambahkan
        /// orang lain - uji ini gagal.
        /// </remarks>
        [Fact]
        public async Task ButirDaftarPantau_TidakMemuatSatuPunIsiKlinis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            TulisCatatan(context, k, perawat.Id, batasVerifikasi: DateTime.UtcNow.AddHours(1));

            var hasil = await BuatService(context)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            var butir = Assert.Single(hasil.WatchList);

            // Setiap properti bertipe teks diperiksa, bukan hanya yang diingat penulis uji.
            var nilaiTeks = typeof(CpptVerificationWatchItem)
                .GetProperties()
                .Where(x => x.PropertyType == typeof(string))
                .Select(x => x.GetValue(butir) as string)
                .Where(x => x != null)
                .ToList();

            Assert.NotEmpty(nilaiTeks);

            foreach (var nilai in nilaiTeks)
            {
                Assert.DoesNotContain(TeksKlinisSamaran, nilai!, StringComparison.OrdinalIgnoreCase);
            }
        }

        // =====================================================================
        // Kriteria 5 - urutan, penyaringan, dan jumlah butir tidak berubah
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-067 AC 5</c> - hanya catatan menunggu dan lewat batas yang masuk daftar,
        /// urutannya tetap menurut waktu catatan, dan jumlahnya tidak bertambah.
        /// </summary>
        [Fact]
        public async Task UrutanPenyaringanDanJumlahButir_TidakBerubah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dasar = DateTime.UtcNow.AddHours(-6);

            var ketiga = TulisCatatan(
                context, k, perawat.Id,
                batasVerifikasi: DateTime.UtcNow.AddHours(1),
                waktuCatatan: dasar.AddMinutes(30));

            var pertama = TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.Overdue,
                batasVerifikasi: DateTime.UtcNow.AddHours(-1),
                waktuCatatan: dasar);

            // Dua catatan yang TIDAK boleh masuk daftar pantau.
            TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.Verified,
                waktuCatatan: dasar.AddMinutes(10));

            TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.NotRequired,
                waktuCatatan: dasar.AddMinutes(20));

            var hasil = await BuatService(context)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            Assert.Equal(4, hasil.TotalNoteCount);
            Assert.Equal(2, hasil.WatchList.Count);

            // Urutannya menurut waktu catatan, bukan waktu penyimpanan.
            Assert.Equal(pertama.Id, hasil.WatchList[0].NoteId);
            Assert.Equal(ketiga.Id, hasil.WatchList[1].NoteId);

            // Yang lewat batas tetap dikenali lewat batas.
            Assert.True(hasil.WatchList[0].IsOverdue);
            Assert.False(hasil.WatchList[1].IsOverdue);
            Assert.Equal(1, hasil.OverdueCount);

            // Rekapitulasinya tidak bergeser.
            Assert.Equal(1, hasil.VerifiedCount);
            Assert.Equal(1, hasil.NotRequiredCount);
        }

        // =====================================================================
        // Kriteria 6 - kebijakan yang tidak aktif tetap menghasilkan NotRequired
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-067 AC 6</c> - kebijakan verifikasi yang belum aktif tetap menghasilkan
        /// keadaan tidak-diwajibkan beserta penandanya, bukan daftar kosong yang tampak beres.
        /// </summary>
        [Fact]
        public async Task KebijakanBelumAktif_TetapNotRequiredDenganPenandanya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            // Nol batas waktu: inilah keadaan hari ini, karena RWI-RULE-021 belum disahkan.
            TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.NotRequired,
                batasVerifikasi: null);

            var hasil = await BuatService(context)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            Assert.True(hasil.IsVerificationPolicyEmpty);
            Assert.Equal(1, hasil.TotalNoteCount);
            Assert.Equal(1, hasil.NotRequiredCount);
            Assert.Equal(0, hasil.VerifiedCount);
            Assert.Empty(hasil.WatchList);
        }

        // =====================================================================
        // Risiko jumlah query - nama penulis tidak boleh melahirkan N+1
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-067</c> baris Risk - nama penulis dibaca pada query yang sama, bukan satu
        /// query tambahan per baris.
        /// </summary>
        [Fact]
        public async Task DaftarPantauBanyakCatatan_SatuQuerySajaUntukSeluruhNamanya()
        {
            using var database = TestDatabase.Create();
            using var penyiap = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(penyiap);

            var penulis = new List<ApplicationUser>();

            for (var i = 0; i < 5; i++)
            {
                var perawat = RekamMedisTestData.BuatPengguna(penyiap, $"perawat{i}");
                penulis.Add(perawat);

                TulisCatatan(
                    penyiap, k, perawat.Id,
                    batasVerifikasi: DateTime.UtcNow.AddHours(1),
                    waktuCatatan: DateTime.UtcNow.AddHours(-5).AddMinutes(i));
            }

            var perintah = new List<string>();
            using var terpantau = database.CreateContext(baris => perintah.Add(baris));

            var hasil = await BuatService(terpantau)
                .GetStatusByEpisodeAsync(k.EpisodeId, DateTime.UtcNow);

            Assert.Equal(5, hasil.WatchList.Count);

            foreach (var butir in hasil.WatchList)
            {
                var perawat = penulis.Single(x => x.Id == butir.ProviderUserId);
                Assert.Equal(perawat.DisplayName, butir.ProviderName);
            }

            // Satu perintah saja untuk seluruh daftar beserta namanya.
            var jumlahEksekusi = perintah.Count(x => x.Contains("Executed DbCommand"));

            Assert.Equal(1, jumlahEksekusi);
        }
    }
}
