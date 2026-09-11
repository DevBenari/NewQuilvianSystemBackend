using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-054</c> — pengkajian rawat inap punya tempat menyimpan
    /// tenggat beserta kebijakannya, dan jalur lama tidak bergeser satu langkah pun.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>RWI-DEC-051</c> mencatat bahwa cabang rawat inap pada endpoint pengkajian sudah menyala
    /// <b>tanpa</b> satu pun test regresi yang menjaga jalur poliklinik, medical check-up, dan
    /// IGD. Berkas ini menutup utang itu: setiap jalur lama punya barisnya sendiri di sini.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan. Tidak ada data pasien sungguhan.
    /// </para>
    /// </remarks>
    public class NursingAssessmentContextTests
    {
        /// <summary>Kalimat penolakan <c>VAL-KEP-04</c>, ditulis apa adanya.</summary>
        private const string PenolakanPoliklinik =
            "Pengkajian untuk pasien poliklinik tetap harus lewat antrean.";

        /// <summary>Kalimat penolakan <c>VAL-KEP-01</c>, ditulis apa adanya.</summary>
        private const string PenolakanTanpaPerawatan =
            "Pasien ini tidak sedang dirawat inap. Pengkajian rawat inap hanya untuk pasien " +
            "yang sudah masuk kamar.";

        internal static PatientAssessmentController BuatController(
            ApplicationDbContext c, Guid actorUserId)
        {
            var keutuhan = new ClinicalDocumentIntegrityService(c);
            var kebijakan = new ClinicalAssessmentPolicyService(c);

            return new PatientAssessmentController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new InpatientClinicalContextService(c),
                keutuhan,
                kebijakan,
                new ClinicalNoteAddendumService(c, keutuhan),
                new NursingAssessmentMonitoringService(c, kebijakan))
                .DenganPengguna(actorUserId);
        }

        internal static CreatePatientAssessmentRequest PermintaanPengkajian(
            RawatInapTestData.Konteks k,
            PatientAssessmentType jenis = PatientAssessmentType.Initial) => new()
            {
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = jenis,
                ChiefComplaint = "Lemas dan pusing sejak pagi",
                FallRiskStatus = FallRiskStatus.NoRisk,
                NutritionRiskStatus = NutritionRiskStatus.LowRisk
            };

        // =====================================================================
        // Kriteria 1 — dua kolom baru, keduanya nullable
        // =====================================================================

        /// <summary>
        /// `BE-RWI-054 AC 1` — <c>DueAt</c> dan <c>PolicyId</c> terbentuk dan keduanya nullable,
        /// sehingga baris lama tidak perlu disentuh sama sekali.
        /// </summary>
        [Fact]
        public void KolomTenggatDanKebijakan_TerbentukNullable()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = context.Model.FindEntityType(typeof(TrxPatientAssessment))!;

            var dueAt = entity.FindProperty("DueAt");
            var policyId = entity.FindProperty("PolicyId");

            Assert.NotNull(dueAt);
            Assert.NotNull(policyId);
            Assert.True(dueAt!.IsNullable, "DueAt seharusnya nullable.");
            Assert.True(policyId!.IsNullable, "PolicyId seharusnya nullable.");
        }

        /// <summary>
        /// `BE-RWI-054 AC 1` — pengkajian yang dibuat tanpa kebijakan apa pun tersimpan dengan
        /// kedua kolom kosong, dan itu <b>bukan</b> kegagalan.
        /// </summary>
        /// <remarks>
        /// <c>VAL-KEP-17</c>. Master kebijakan kosong berarti tidak ada tenggat yang dapat
        /// dihitung; pencatatan tetap berjalan penuh.
        /// </remarks>
        [Fact]
        public async Task PengkajianTanpaKebijakan_TersimpanDenganTenggatKosong()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(PermintaanPengkajian(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pembaca = database.CreateContext();
            var tersimpan = await pembaca.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.Null(tersimpan.DueAt);
            Assert.Null(tersimpan.PolicyId);
            Assert.Equal(k.EpisodeId, tersimpan.InpEpisodeId);
        }

        // =====================================================================
        // Kriteria 2 — pengkajian rawat inap tanpa antrean dan tanpa IGD
        // =====================================================================

        /// <summary>
        /// `BE-RWI-054 AC 2` — pengkajian dapat dibuat untuk perawatan <c>Admitted</c> tanpa
        /// nomor antrean <b>dan</b> tanpa kunjungan IGD.
        /// </summary>
        /// <remarks>
        /// Kontrak menyatakan pengkajian yang tersimpan dijawab <c>200</c> atau <c>201</c>
        /// (<c>api-contract.md</c> bagian 1). Source menjawab <c>200</c> sejak sebelum task ini,
        /// dan mengubahnya menjadi <c>201</c> akan merusak pemanggil poliklinik dan IGD yang
        /// sudah ada; selisih itu dicatat pada laporan task.
        /// </remarks>
        [Fact]
        public async Task RawatInapTanpaAntreanDanTanpaIgd_PengkajianTersimpan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            Assert.False(await context.Set<EmgVisit>().AnyAsync(x => x.EncounterId == k.EncounterId));

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(PermintaanPengkajian(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.Null(tersimpan.QueueId);
            Assert.Equal(k.EpisodeId, tersimpan.InpEpisodeId);
            Assert.Equal(PatientAssessmentType.Initial, tersimpan.AssessmentType);
            Assert.Equal(0, await context.Set<TrxQueue>().CountAsync());
        }

        // =====================================================================
        // Kriteria 3 — tiga sebab penolakan 422
        // =====================================================================

        /// <summary>
        /// `BE-RWI-054 AC 3` — <c>VAL-KEP-01</c>: kunjungan rawat inap yang belum punya perawatan
        /// ditolak <c>422</c> beserta kalimat yang mengarahkan menyelesaikan admisi.
        /// </summary>
        [Fact]
        public async Task KunjunganRawatInapTanpaPerawatan_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var kunjungan = context.Set<RegPatientEncounter>().First(x => x.Id == konteks.EncounterId);
            kunjungan.EncounterType = EncounterType.Inpatient;
            await context.SaveChangesAsync();

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = konteks.EncounterId
                });

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanTanpaPerawatan, ControllerTestHarness.Pesan(hasil));
            Assert.Equal(0, await context.Set<TrxPatientAssessment>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-054 AC 3` — <c>VAL-KEP-02</c>: perawatan yang masih <c>Draft</c> ditolak
        /// <c>422</c>. Pasien belum benar-benar berada di kamar.
        /// </summary>
        [Fact]
        public async Task PerawatanMasihDraft_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Draft);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId
                });

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(0, await context.Set<TrxPatientAssessment>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-054 AC 3` — <c>VAL-KEP-03</c>: perawatan yang sudah ditutup menolak dokumen
        /// baru dengan <c>422</c>.
        /// </summary>
        [Fact]
        public async Task PerawatanSudahDitutup_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Closed);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId
                });

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(0, await context.Set<TrxPatientAssessment>().CountAsync());
        }

        // =====================================================================
        // Kriteria 4 dan 5 — regresi tiga jalur lama
        // =====================================================================

        /// <summary>
        /// `BE-RWI-054 AC 4 dan 5` — <c>VAL-KEP-04</c>: kunjungan poliklinik dan medical check-up
        /// tanpa antrean <b>tetap</b> ditolak <c>400</c> beserta kalimat lamanya.
        /// </summary>
        /// <remarks>
        /// <c>RWI-DEC-070</c>. Pelonggaran pintu masuk hanya untuk rawat inap dan IGD; rawat
        /// jalan tidak boleh berubah sedikit pun.
        /// </remarks>
        [Theory]
        [InlineData(EncounterType.Outpatient)]
        [InlineData(EncounterType.MedicalCheckup)]
        public async Task PoliklinikDanMedicalCheckupTanpaAntrean_TetapDitolak400(
            EncounterType encounterType)
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var kunjungan = context.Set<RegPatientEncounter>().First(x => x.Id == konteks.EncounterId);
            kunjungan.EncounterType = encounterType;
            await context.SaveChangesAsync();

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = konteks.EncounterId
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanPoliklinik, ControllerTestHarness.Pesan(hasil));
            Assert.Equal(0, await context.Set<TrxPatientAssessment>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-054 AC 5` — regresi IGD. Jalur tanpa antrean milik IGD tetap berhasil dan
        /// tetap tidak melahirkan satu baris antrean pun.
        /// </summary>
        [Fact]
        public async Task IgdTanpaAntrean_PengkajianTetapBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var kunjungan = context.Set<RegPatientEncounter>().First(x => x.Id == konteks.EncounterId);
            kunjungan.EncounterType = EncounterType.Emergency;
            context.Set<EmgVisit>().Add(new EmgVisit
            {
                EmergencyVisitNumber = $"IGD-{Guid.NewGuid():N}"[..16],
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                ServiceUnitId = konteks.ServiceUnitId,
                ArrivalDateTime = DateTime.UtcNow.AddHours(-1)
            });
            await context.SaveChangesAsync();

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = konteks.EncounterId
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.EncounterId == konteks.EncounterId);

            Assert.Null(tersimpan.InpEpisodeId);
            Assert.Null(tersimpan.DueAt);
            Assert.Equal(0, await context.Set<TrxQueue>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-054 AC 5` — regresi poliklinik berantre. Jalur lama tetap berhasil, dan
        /// pengkajiannya tetap menempel pada antreannya.
        /// </summary>
        [Fact]
        public async Task JalurBerantre_PengkajianTetapBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var antrean = BuatAntreanScreening(context, konteks);

            var hasil = await BuatController(context, perawat.Id)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = konteks.EncounterId,
                    QueueId = antrean.Id
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.EncounterId == konteks.EncounterId);

            Assert.Equal(antrean.Id, tersimpan.QueueId);
            Assert.Null(tersimpan.InpEpisodeId);
        }

        // =====================================================================
        // Kriteria 6 — index perawatan dan penjaga penghapusan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-054 AC 6` — index perawatan terbentuk, dan penghapusan perawatan yang masih
        /// punya pengkajian ditolak database.
        /// </summary>
        /// <remarks>
        /// Index <c>IX_TrxPatientAssessment_InpEpisodeId</c> beserta
        /// <c>DeleteBehavior.Restrict</c>-nya sudah mendarat lebih dulu lewat <c>BE-RWI-040</c>
        /// milik sub-modul <c>dokter-rawat-inap</c> — <c>INT-DOK-09</c>. Uji ini mengunci
        /// keduanya supaya tidak hilang tanpa disadari.
        /// </remarks>
        [Fact]
        public async Task IndexPerawatanDanPenjagaPenghapusan_Terpasang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = context.Model.FindEntityType(typeof(TrxPatientAssessment))!;

            var adaIndex = entity.GetIndexes().Any(x =>
                x.Properties.Count == 1 && x.Properties[0].Name == "InpEpisodeId");

            Assert.True(adaIndex, "Index tunggal atas InpEpisodeId tidak ditemukan.");

            var relasi = entity.GetForeignKeys()
                .Single(x => x.Properties.Count == 1 && x.Properties[0].Name == "InpEpisodeId");

            Assert.Equal(DeleteBehavior.Restrict, relasi.DeleteBehavior);

            // Penghapusan perawatan yang masih punya pengkajian ditolak di tingkat database.
            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var dibuat = await BuatController(context, perawat.Id)
                .CreateAssessment(PermintaanPengkajian(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            using var penghapus = database.CreateContext();
            var episode = await penghapus.Set<InpEpisode>().SingleAsync(x => x.Id == k.EpisodeId);
            penghapus.Set<InpEpisode>().Remove(episode);

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => penghapus.SaveChangesAsync());
        }

        /// <summary>Membuat satu antrean yang membutuhkan screening perawat.</summary>
        internal static TrxQueue BuatAntreanScreening(
            ApplicationDbContext context,
            RekamMedisTestData.Konteks konteks)
        {
            var antrean = new TrxQueue
            {
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                ServiceUnitId = konteks.ServiceUnitId,
                QueueCode = $"ANT-{Guid.NewGuid():N}"[..16],
                QueueNumber = 1,
                QueueDate = DateTime.UtcNow,
                QueueStatus = QueueStatus.WaitingForNurse,
                IsScreeningRequired = true
            };

            context.Set<TrxQueue>().Add(antrean);
            context.SaveChanges();

            return antrean;
        }
    }
}
