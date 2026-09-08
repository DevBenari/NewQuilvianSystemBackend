using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-056</c> — pengkajian awal dan pengkajian ulang tidak lagi
    /// saling menimpa.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang dijaga bukan sekadar dua baris tersimpan, melainkan bahwa <b>nilai pengkajian
    /// pertama tetap utuh</b> sesudah pengkajian kedua dibuat. Itulah yang benar-benar berarti
    /// bagi perawat: skor nyeri kemarin tidak boleh hilang ketika pengkajian hari ini diisi —
    /// <c>FR-KEP-005</c>, PRD 16.2 aturan 3.
    /// </para>
    /// </remarks>
    public class NursingAssessmentSeparationTests
    {
        /// <summary>Kalimat penolakan <c>VAL-KEP-11</c>, ditulis apa adanya.</summary>
        private const string PenolakanPengkajianAwalKedua =
            "Pengkajian awal untuk pasien ini sudah ada. Gunakan pengkajian ulang.";

        private static CreatePatientAssessmentRequest Permintaan(
            RawatInapTestData.Konteks k,
            PatientAssessmentType jenis,
            int? skalaNyeri = null) => new()
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = jenis,
                ChiefComplaint = "Nyeri perut kanan bawah",
                HasPain = skalaNyeri.HasValue,
                PainScale = skalaNyeri,
                FallRiskStatus = FallRiskStatus.NoRisk,
                NutritionRiskStatus = NutritionRiskStatus.LowRisk
            };

        // =====================================================================
        // Kriteria 1 — dua record terpisah, nilai record pertama utuh
        // =====================================================================

        /// <summary>
        /// `BE-RWI-056 AC 1` — pengkajian awal dan pengkajian ulang tersimpan sebagai record
        /// terpisah, dan nilai record pertama <b>sama persis</b> sebelum dan sesudah record kedua
        /// dibuat.
        /// </summary>
        [Fact]
        public async Task PengkajianAwalDanUlang_DuaRecordDanNilaiPertamaUtuh()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var pertama = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(Permintaan(k, PatientAssessmentType.Initial, skalaNyeri: 7));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pertama));

            var awal = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.AssessmentType == PatientAssessmentType.Initial);

            var nyeriSebelum = awal.PainScale;
            var nomorSebelum = awal.AssessmentNumber;

            var kedua = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(Permintaan(k, PatientAssessmentType.DailyReassessment, skalaNyeri: 3));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            using var pembaca = database.CreateContext();

            var seluruh = await pembaca.Set<TrxPatientAssessment>()
                .Where(x => x.InpEpisodeId == k.EpisodeId)
                .OrderBy(x => x.AssessmentType)
                .ToListAsync();

            Assert.Equal(2, seluruh.Count);

            var awalSesudah = seluruh.Single(x => x.AssessmentType == PatientAssessmentType.Initial);
            var ulang = seluruh.Single(x => x.AssessmentType == PatientAssessmentType.DailyReassessment);

            Assert.NotEqual(awalSesudah.Id, ulang.Id);
            Assert.Equal(nyeriSebelum, awalSesudah.PainScale);
            Assert.Equal(nomorSebelum, awalSesudah.AssessmentNumber);
            Assert.Equal(7, awalSesudah.PainScale);
            Assert.Equal(3, ulang.PainScale);
        }

        // =====================================================================
        // Kriteria 2 — pengkajian awal kedua ditolak 409
        // =====================================================================

        /// <summary>
        /// `BE-RWI-056 AC 2` — pengkajian awal kedua pada satu perawatan ditolak <c>409</c>, dan
        /// pesannya mengarahkan memakai pengkajian ulang.
        /// </summary>
        [Fact]
        public async Task PengkajianAwalKedua_Ditolak409DenganArahan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var controller = NursingAssessmentContextTests.BuatController(context, perawat.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(
                await controller.CreateAssessment(Permintaan(k, PatientAssessmentType.Initial))));

            var kedua = await controller.CreateAssessment(
                Permintaan(k, PatientAssessmentType.Initial));

            Assert.Equal(409, ControllerTestHarness.KodeStatus(kedua));
            Assert.Equal(PenolakanPengkajianAwalKedua, ControllerTestHarness.Pesan(kedua));
            Assert.Equal(1, await context.Set<TrxPatientAssessment>().CountAsync());
        }

        /// <summary>
        /// Aturan ini <b>tidak</b> menyentuh jalur poliklinik. Dua pengkajian awal pada dua
        /// kunjungan rawat jalan yang berbeda tetap boleh, karena keduanya memang tidak punya
        /// perawatan rawat inap.
        /// </summary>
        [Fact]
        public async Task PoliklinikBerantre_TidakTersentuhAturanSatuPengkajianAwal()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            foreach (var _ in Enumerable.Range(0, 2))
            {
                var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
                var antrean = NursingAssessmentContextTests.BuatAntreanScreening(context, konteks);

                var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                    .CreateAssessment(new CreatePatientAssessmentRequest
                    {
                        EncounterId = konteks.EncounterId,
                        QueueId = antrean.Id
                    });

                Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            }

            Assert.Equal(2, await context.Set<TrxPatientAssessment>().CountAsync());
        }

        // =====================================================================
        // Kriteria 3 — pengkajian awal yang dibatalkan tidak menghalangi
        // =====================================================================

        /// <summary>
        /// `BE-RWI-056 AC 3` — pengkajian awal yang dibatalkan tidak menghalangi pembuatan
        /// pengkajian awal berikutnya.
        /// </summary>
        /// <remarks>
        /// Kejadian nyatanya sederhana: perawat salah memilih pasien, membatalkan pengkajiannya,
        /// lalu membuat yang benar. Menghitung baris yang dibatalkan akan mengunci perawatan itu
        /// selamanya — dan itulah alasan aturan ini dijaga service, bukan unique index.
        /// </remarks>
        [Fact]
        public async Task PengkajianAwalDibatalkan_PengkajianAwalBerikutnyaDiterima()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var controller = NursingAssessmentContextTests.BuatController(context, perawat.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(
                await controller.CreateAssessment(Permintaan(k, PatientAssessmentType.Initial))));

            var pertama = await context.Set<TrxPatientAssessment>().SingleAsync();

            var dibatalkan = await controller.CancelAssessment(
                pertama.Id, new CancelPatientAssessmentRequest { CancelReason = "Salah pasien" });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibatalkan));

            var kedua = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(Permintaan(k, PatientAssessmentType.Initial));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            using var pembaca = database.CreateContext();
            var seluruh = await pembaca.Set<TrxPatientAssessment>()
                .Where(x => x.InpEpisodeId == k.EpisodeId)
                .ToListAsync();

            Assert.Equal(2, seluruh.Count);
            Assert.Single(seluruh.Where(x => x.AssessmentStatus == PatientAssessmentStatus.Cancelled));
        }

        // =====================================================================
        // Kriteria 4 — isian wajib kosong ditolak dan disebut satu per satu
        // =====================================================================

        /// <summary>
        /// `BE-RWI-056 AC 4` — menyelesaikan pengkajian keperawatan rawat inap yang skrining
        /// gizinya belum diisi ditolak <c>400</c>, dan pesannya menyebut bagian yang kosong.
        /// </summary>
        /// <remarks>
        /// <c>VAL-KEP-08</c>. Skrining gizi disebut karena memang dibiarkan kosong. Sejak
        /// penilaian risiko jatuh ikut dapat bernilai "belum diisi", bagian itu juga muncul pada
        /// kalimat penolakan; uji berikutnya yang menjaga keduanya disebut satu per satu.
        /// </remarks>
        [Fact]
        public async Task MenyelesaikanPengkajianTanpaSkriningGizi_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var controller = NursingAssessmentContextTests.BuatController(context, perawat.Id);

            var dibuat = await controller.CreateAssessment(new CreatePatientAssessmentRequest
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = PatientAssessmentType.Initial,
                ChiefComplaint = "Lemas"
                // NutritionRiskStatus sengaja dibiarkan Unknown.
            });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>().SingleAsync();

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            var pesan = ControllerTestHarness.Pesan(hasil)!;
            Assert.Contains("belum dapat diselesaikan", pesan);
            Assert.Contains("skrining gizi", pesan);

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync();

            Assert.NotEqual(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
        }

        /// <summary>
        /// Aturan isian wajib itu <b>tidak</b> menyentuh pengkajian poliklinik: kunjungan tanpa
        /// perawatan rawat inap tetap dapat diselesaikan seperti sebelumnya.
        /// </summary>
        [Fact]
        public async Task PengkajianPoliklinik_TetapDapatDiselesaikanTanpaSkriningGizi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var antrean = NursingAssessmentContextTests.BuatAntreanScreening(context, konteks);

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = konteks.EncounterId,
                    QueueId = antrean.Id,
                    ChiefComplaint = "Batuk"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>().SingleAsync();

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync();

            Assert.Equal(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
        }

        // =====================================================================
        // Kriteria 4 - penilaian risiko jatuh yang belum diisi
        // =====================================================================

        /// <summary>
        /// `BE-RWI-056 AC 4` / <c>UAT-KEP-07</c> - menyelesaikan pengkajian dengan
        /// <b>penilaian risiko jatuh</b> belum terisi ditolak <c>400</c>, dan bagian itulah yang
        /// disebut.
        /// </summary>
        /// <remarks>
        /// Inilah skenario yang sebelumnya tidak dapat ditegakkan. Perhitungan lama menyamakan
        /// "belum diisi" dengan "tidak berisiko", sehingga perawat yang tidak pernah membuka
        /// bagian risiko jatuh tersimpan sebagai perawat yang menyatakan pasiennya aman. Skrining
        /// gizi sengaja <b>diisi</b> di sini supaya kalimat penolakannya membuktikan risiko jatuh
        /// benar-benar berdiri sendiri sebagai bagian yang kosong.
        /// </remarks>
        [Fact]
        public async Task MenyelesaikanPengkajianTanpaRisikoJatuh_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.Initial,
                    ChiefComplaint = "Lemas",
                    NutritionRiskStatus = NutritionRiskStatus.LowRisk
                    // FallRiskStatus sengaja dibiarkan Unknown.
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>().SingleAsync();

            // Yang tersimpan memang "belum diisi", bukan "tidak berisiko".
            Assert.Equal(FallRiskStatus.Unknown, pengkajian.FallRiskStatus);

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            var pesan = ControllerTestHarness.Pesan(hasil)!;
            Assert.Contains("belum dapat diselesaikan", pesan);
            Assert.Contains("penilaian risiko jatuh", pesan);
            Assert.DoesNotContain("skrining gizi", pesan);

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync();

            Assert.NotEqual(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
        }

        /// <summary>
        /// `BE-RWI-056 AC 4` - ketika <b>dua</b> bagian kosong, keduanya disebut
        /// <b>satu per satu</b>, bukan diringkas menjadi "data tidak lengkap".
        /// </summary>
        [Fact]
        public async Task MenyelesaikanPengkajianDuaBagianKosong_KeduanyaDisebutSatuPerSatu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.Initial,
                    ChiefComplaint = "Lemas"
                    // FallRiskStatus dan NutritionRiskStatus dua-duanya Unknown.
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>().SingleAsync();

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));

            var pesan = ControllerTestHarness.Pesan(hasil)!;

            Assert.Contains("penilaian risiko jatuh", pesan);
            Assert.Contains("skrining gizi", pesan);
            Assert.Contains("penilaian risiko jatuh, skrining gizi", pesan);
        }

        /// <summary>
        /// `BE-RWI-056 AC 4` - perawat yang benar-benar <b>menyatakan</b> pasiennya tidak
        /// berisiko jatuh tetap dapat menyelesaikan pengkajiannya.
        /// </summary>
        /// <remarks>
        /// Penjagaan ini menolak bagian yang <b>kosong</b>, bukan pasien yang memang aman.
        /// Pernyataan "tidak berisiko" tersimpan apa adanya sebagai
        /// <see cref="FallRiskStatus.NoRisk"/>.
        /// </remarks>
        [Fact]
        public async Task RisikoJatuhDinyatakanTidakBerisiko_PengkajianDapatDiselesaikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.Initial,
                    ChiefComplaint = "Lemas",
                    FallRiskStatus = FallRiskStatus.NoRisk,
                    NutritionRiskStatus = NutritionRiskStatus.LowRisk
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var pengkajian = await context.Set<TrxPatientAssessment>().SingleAsync();

            Assert.Equal(FallRiskStatus.NoRisk, pengkajian.FallRiskStatus);

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CompleteAssessment(pengkajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var sesudah = await pembaca.Set<TrxPatientAssessment>().SingleAsync();

            Assert.Equal(PatientAssessmentStatus.Completed, sesudah.AssessmentStatus);
            Assert.Equal(FallRiskStatus.NoRisk, sesudah.FallRiskStatus);
        }

        /// <summary>
        /// `BE-RWI-056 AC 4` - pintu kedua penyelesaian ikut dijaga: pembuatan yang langsung
        /// meminta selesai lewat <c>completeImmediately</c> juga ditolak <c>400</c>, dan
        /// <b>tidak satu baris pun</b> tersimpan.
        /// </summary>
        /// <remarks>
        /// Tanpa penjagaan ini aturan isian wajib dapat dilewati hanya dengan menyalakan satu
        /// flag pada permintaan pembuatan, dan pengkajian rawat inap yang kosong tetap mendarat
        /// sebagai <c>Completed</c>.
        /// </remarks>
        [Fact]
        public async Task PengkajianLangsungSelesai_TanpaRisikoJatuh_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.Initial,
                    ChiefComplaint = "Lemas",
                    NutritionRiskStatus = NutritionRiskStatus.LowRisk,
                    CompleteImmediately = true
                    // FallRiskStatus sengaja dibiarkan Unknown.
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("penilaian risiko jatuh", ControllerTestHarness.Pesan(hasil)!);

            using var pembaca = database.CreateContext();

            Assert.Empty(await pembaca.Set<TrxPatientAssessment>().ToListAsync());
        }

        /// <summary>
        /// Regresi jalur bersama - pengkajian <b>poliklinik</b> yang tidak menyebut risiko jatuh
        /// tetap tersimpan sebagai <see cref="FallRiskStatus.NoRisk"/>, persis seperti sebelum
        /// task ini.
        /// </summary>
        /// <remarks>
        /// Pembedaan "belum diisi" hanya berlaku bagi pengkajian yang menempel pada perawatan
        /// rawat inap. Poliklinik, medical check-up, dan IGD memakai request yang sama; nilai
        /// yang tersimpan bagi mereka tidak bergeser satu langkah pun.
        /// </remarks>
        [Fact]
        public async Task PengkajianPoliklinik_TanpaRisikoJatuh_TetapTersimpanNoRisk()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var antrean = NursingAssessmentContextTests.BuatAntreanScreening(context, konteks);

            var dibuat = await NursingAssessmentContextTests.BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = konteks.EncounterId,
                    QueueId = antrean.Id,
                    ChiefComplaint = "Batuk"
                    // FallRiskStatus tidak disebut, persis seperti kiriman poliklinik hari ini.
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            using var pembaca = database.CreateContext();
            var pengkajian = await pembaca.Set<TrxPatientAssessment>().SingleAsync();

            Assert.Equal(FallRiskStatus.NoRisk, pengkajian.FallRiskStatus);
            Assert.Null(pengkajian.InpEpisodeId);
        }

        // =====================================================================
        // Bentuk index
        // =====================================================================

        /// <summary>
        /// `BE-RWI-056` — index parsial perawatan-jenis terbentuk, dan ia <b>non-unique</b>.
        /// </summary>
        /// <remarks>
        /// Revision `1` roadmap sempat meminta unique index; <c>02-backend-architecture.md</c>
        /// `0.3` bagian 4.1 mencabutnya. Unique index akan ikut menghitung pengkajian awal yang
        /// dibatalkan, sehingga perawatan yang pengkajian awalnya salah lalu dibatalkan tidak
        /// akan pernah bisa punya pengkajian awal lagi.
        /// </remarks>
        [Fact]
        public void IndexParsialPerawatanJenis_TerbentukDanNonUnique()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = context.Model.FindEntityType(typeof(TrxPatientAssessment))!;

            var index = entity.GetIndexes().SingleOrDefault(x =>
                x.Name == "IX_TrxPatientAssessment_Episode_Type_Active");

            Assert.True(index != null,
                "Index IX_TrxPatientAssessment_Episode_Type_Active tidak ditemukan.");
            Assert.Equal(["InpEpisodeId", "AssessmentType"],
                index!.Properties.Select(p => p.Name));
            Assert.False(index.IsUnique,
                "Index perawatan-jenis seharusnya non-unique.");

            var penyaring = index.GetFilter();

            Assert.False(string.IsNullOrWhiteSpace(penyaring),
                "Index perawatan-jenis seharusnya parsial.");
            Assert.Contains("AssessmentType", penyaring!);
            Assert.Contains("IsDelete", penyaring!);

            // Index pencarian kajian per perawatan milik BE-RWI-040 tetap ada apa adanya.
            var indexLama = entity.GetIndexes().Where(x =>
                x.Properties.Select(p => p.Name).SequenceEqual(["InpEpisodeId", "AssessmentType"]) &&
                x.GetFilter() == null);

            Assert.Single(indexLama);
        }
    }
}
