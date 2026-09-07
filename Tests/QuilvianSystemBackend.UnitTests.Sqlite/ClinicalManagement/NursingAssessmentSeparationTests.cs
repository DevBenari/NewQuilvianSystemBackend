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
        /// <c>VAL-KEP-08</c>. Bagian yang benar-benar dapat "kosong" pada bentuk data hari ini
        /// adalah skrining gizi; penilaian risiko jatuh tidak pernah tersimpan sebagai
        /// <c>Unknown</c> karena perhitungan lama mengubah "tidak berisiko" dan "belum diisi"
        /// menjadi nilai yang sama. Keterbatasan itu dicatat pada laporan task.
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
