using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-044</c> — dokter membuka pasien rawat inap dan menulis
    /// tanpa nomor antrean.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sebelum task ini, jalur tanpa antrean hanya terbuka bagi kunjungan IGD, sehingga pasien
    /// menginap tidak punya pintu masuk sama sekali: ia tidak berantre, dan antrean semu
    /// dilarang <c>RWI-RULE-026</c> aturan 2.
    /// </para>
    /// <para>
    /// Berkas ini juga mengunci apa yang <b>tidak</b> berubah. Poliklinik, medical check-up, dan
    /// IGD masing-masing punya uji tersendiri di sini, karena pintu yang baru dibuka berada di
    /// jalur yang sama dengan ketiganya — <c>RWI-DEC-051</c>, <c>RWI-AC-143</c>.
    /// </para>
    /// </remarks>
    public class InpatientDoctorEntryPointTests
    {
        /// <summary>Kalimat penolakan <c>VAL-DOK-04</c>, ditulis apa adanya.</summary>
        private const string PenolakanPoliklinik =
            "Konsultasi untuk pasien poliklinik tetap harus lewat antrean.";

        /// <summary>Kalimat penolakan <c>VAL-DOK-26</c>, ditulis apa adanya.</summary>
        private const string PenolakanPenandaTidakCocok =
            "Perawatan rawat inap tidak sesuai dengan kunjungannya.";

        private static DoctorConsultationController BuatControllerCatatan(
            ApplicationDbContext c, Guid actorUserId) =>
            new DoctorConsultationController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                new ConsultationFinalizationService(
                    c,
                    new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                    new PrescriptionAggregateService(c, new PrescriptionSummaryService(c)),
                    new PrescriptionWorkflowService(c),
                    new ClinicalMilestoneFactProducer(
                        c,
                        new BillingFolioService(c),
                        ControllerTestHarness.BuatLoggerService()),
                    // BE-RWI-038. Finalisasi kini sekaligus mendaftarkan catatan ke mesin
                    // keutuhan rekam medis.
                    new ClinicalDocumentIntegrityService(c)),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        private static PatientAssessmentController BuatControllerPengkajian(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientAssessmentController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new InpatientClinicalContextService(c),
                new ClinicalDocumentIntegrityService(c))
                .DenganPengguna(actorUserId);

        private static CreateDoctorConsultationRequest PermintaanCatatan(
            RawatInapTestData.Konteks k,
            Guid? inpEpisodeId = null) => new()
            {
                EncounterId = k.EncounterId,
                DoctorId = k.DoctorMasterId,
                InpEpisodeId = inpEpisodeId,
                Subjective = "Pasien mengeluh nyeri ulu hati",
                Objective = "Abdomen supel, nyeri tekan epigastrium"
            };

        // =====================================================================
        // Kriteria 1 — pintu masuk rawat inap terbuka
        // =====================================================================

        /// <summary>
        /// `BE-RWI-044 AC 1` — catatan dokter dapat dibuat untuk perawatan berjalan tanpa
        /// antrean <b>dan</b> tanpa kunjungan IGD. Inilah kemampuan yang sebelumnya tidak ada.
        /// </summary>
        [Fact]
        public async Task RawatInapTanpaAntreanDanTanpaIgd_CatatanTersimpan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            Assert.False(await context.Set<EmgVisit>().AnyAsync(x => x.EncounterId == k.EncounterId));

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.Null(tersimpan.QueueId);
            Assert.Equal(k.EpisodeId, tersimpan.InpEpisodeId);
            Assert.Equal(k.DoctorMasterId, tersimpan.DoctorId);
            Assert.Equal(0, await context.Set<TrxQueue>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-044 AC 1` — pintu yang sama terbuka bagi pengkajian, sehingga kajian medis
        /// pada <c>BE-RWI-045</c> punya tempat berdiri.
        /// </summary>
        [Fact]
        public async Task RawatInapTanpaAntrean_PengkajianTersimpanBesertaKonteksnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerPengkajian(context, k.DokterUserId)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    ChiefComplaint = "Nyeri ulu hati"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var tersimpan = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.Null(tersimpan.QueueId);
            Assert.Equal(k.EpisodeId, tersimpan.InpEpisodeId);
        }

        /// <summary>
        /// `BE-RWI-044 AC 1` — pelonggaran `BE-RWI-043` kini terbukti lewat endpoint: catatan
        /// kedua sepanjang perawatan diterima, bukan hanya lolos index database.
        /// </summary>
        /// <remarks>
        /// Laporan <c>BE-RWI-043</c> menyatakan kriteria 1 dan 2 baru terbukti pada aturan
        /// aplikasi dan index, karena satu kunjungan hanya boleh punya satu baris antrean hidup.
        /// Pintu tanpa antrean yang dibuka di sini adalah jalan yang kurang itu.
        /// </remarks>
        [Fact]
        public async Task RawatInap_CatatanKeduaDiterimaLewatEndpoint()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var pertama = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));
            var kedua = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pertama));
            Assert.Equal(200, ControllerTestHarness.KodeStatus(kedua));

            Assert.Equal(2, await context.Set<TrxDoctorConsultation>()
                .CountAsync(x => x.EncounterId == k.EncounterId));
        }

        // =====================================================================
        // Kriteria 2 — penanda perawatan yang tidak cocok
        // =====================================================================

        /// <summary>
        /// `BE-RWI-044 AC 2` — penanda perawatan yang terisi tetapi menunjuk perawatan pasien
        /// lain ditolak `400`, dan tidak satu baris pun tersimpan.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-26</c>. Inilah keadaan paling berbahaya pada scope ini: kedua nilainya
        /// masuk akal bila dilihat sendiri-sendiri, sehingga catatan pasien A dapat mendarat
        /// pada perawatan pasien B tanpa satu pun galat yang terlihat.
        /// </remarks>
        [Fact]
        public async Task PenandaPerawatanMilikPasienLain_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pasienA = RawatInapTestData.SiapkanPerawatan(context);
            var pasienB = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerCatatan(context, pasienA.DokterUserId)
                .CreateConsultation(PermintaanCatatan(pasienA, inpEpisodeId: pasienB.EpisodeId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanPenandaTidakCocok, ControllerTestHarness.Pesan(hasil));
            Assert.Equal(0, await context.Set<TrxDoctorConsultation>().CountAsync());
        }

        /// <summary>
        /// `BE-RWI-044 AC 2` — penanda yang cocok diterima. Kendali positif, supaya uji di atas
        /// tidak lulus karena setiap penanda ditolak.
        /// </summary>
        [Fact]
        public async Task PenandaPerawatanYangCocok_Diterima()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k, inpEpisodeId: k.EpisodeId));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// `BE-RWI-044 AC 2` — penjagaan yang sama berlaku pada cabang <b>berantre</b>. Kunjungan
        /// yang berantre pun dapat menaungi perawatan rawat inap.
        /// </summary>
        [Fact]
        public async Task PenandaTidakCocokPadaJalurBerantre_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pasienA = RawatInapTestData.SiapkanPerawatan(context);
            var pasienB = RawatInapTestData.SiapkanPerawatan(context);

            var antrean = BuatAntrean(context, pasienA);

            var permintaan = PermintaanCatatan(pasienA, inpEpisodeId: pasienB.EpisodeId);
            permintaan.QueueId = antrean.Id;

            var hasil = await BuatControllerCatatan(context, pasienA.DokterUserId)
                .CreateConsultation(permintaan);

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanPenandaTidakCocok, ControllerTestHarness.Pesan(hasil));
        }

        /// <summary>
        /// `BE-RWI-044 AC 2` — penanda perawatan yang dikirim untuk kunjungan yang tidak
        /// menaungi perawatan mana pun juga ditolak `400`.
        /// </summary>
        [Fact]
        public async Task PenandaPerawatanPadaKunjunganTanpaPerawatan_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var rawatInap = RawatInapTestData.SiapkanPerawatan(context);
            var poliklinik = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokterMaster = RawatInapTestData.BuatDokterMaster(context);

            var hasil = await BuatControllerCatatan(context, rawatInap.DokterUserId)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = poliklinik.EncounterId,
                    DoctorId = dokterMaster.Id,
                    InpEpisodeId = rawatInap.EpisodeId,
                    Subjective = "Isi uji"
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanPenandaTidakCocok, ControllerTestHarness.Pesan(hasil));
        }

        // =====================================================================
        // Kriteria 3 — tidak menunggu pengkajian keperawatan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-044 AC 3` — catatan dapat dibuat walaupun pengkajian awal keperawatan belum
        /// selesai, bahkan ketika pengkajian itu belum ada sama sekali.
        /// </summary>
        /// <remarks>
        /// Dokter rawat inap tidak menunggu perawat. Uji ini menjaga agar penjagaan pengkajian
        /// tidak diam-diam dipasang sebagai gerbang pada jalur ini di kemudian hari.
        /// </remarks>
        [Fact]
        public async Task PengkajianKeperawatanBelumSelesai_CatatanTetapDapatDibuat()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            // Pengkajian keperawatan ada, tetapi masih dikerjakan.
            context.Set<TrxPatientAssessment>().Add(new TrxPatientAssessment
            {
                AssessmentNumber = $"ASM-{Guid.NewGuid():N}"[..20],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                InpEpisodeId = k.EpisodeId,
                AssessmentType = PatientAssessmentType.Initial,
                AssessmentStatus = PatientAssessmentStatus.InProgress
            });
            await context.SaveChangesAsync();

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(1, await context.Set<TrxDoctorConsultation>()
                .CountAsync(x => x.EncounterId == k.EncounterId));
        }

        // =====================================================================
        // Keadaan perawatan — kode yang membedakan sebab penolakan
        // =====================================================================

        /// <summary>
        /// `VAL-DOK-02` — perawatan yang masih `Draft` menolak dokumen baru dengan `422`, bukan
        /// `400`: yang salah bukan isian pengguna, melainkan keadaan pasien.
        /// </summary>
        [Fact]
        public async Task PerawatanBelumDimulai_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Draft);

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(
                "Perawatan rawat inap belum dimulai; pasien belum masuk kamar.",
                ControllerTestHarness.Pesan(hasil));
        }

        /// <summary>
        /// `VAL-DOK-03` — perawatan yang sudah ditutup menolak dokumen <b>baru</b> dengan `422`.
        /// </summary>
        [Fact]
        public async Task PerawatanSudahDitutup_Ditolak422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Closed);

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("sudah ditutup", ControllerTestHarness.Pesan(hasil));
        }

        /// <summary>
        /// Perawatan yang menunggu pemulangan tetap menerima catatan: pasien masih berada di
        /// kamar sampai ia benar-benar meninggalkan rumah sakit.
        /// </summary>
        [Fact]
        public async Task PerawatanMenungguPemulangan_CatatanTetapDapatDibuat()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.DischargePending);

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
        }

        // =====================================================================
        // Regresi — yang tidak berubah
        // =====================================================================

        /// <summary>
        /// Regresi poliklinik. Kunjungan rawat jalan tanpa antrean tetap ditolak `400`, dan
        /// tidak satu catatan pun lahir.
        /// </summary>
        /// <remarks>
        /// <b>Kalimatnya berubah, kodenya tidak.</b> Sebelum task ini bunyinya "Konsultasi tanpa
        /// antrean hanya untuk pasien IGD"; sejak pintu rawat inap dibuka kalimat itu tidak lagi
        /// benar, dan diganti bunyi <c>VAL-DOK-04</c> apa adanya. Selisih ini dicatat pada
        /// laporan task.
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
            var dokterMaster = RawatInapTestData.BuatDokterMaster(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var kunjungan = context.Set<TrxPatientEncounter>().First(x => x.Id == konteks.EncounterId);
            kunjungan.EncounterType = encounterType;
            await context.SaveChangesAsync();

            var hasil = await BuatControllerCatatan(context, aktor.Id)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = konteks.EncounterId,
                    DoctorId = dokterMaster.Id,
                    Subjective = "Isi uji"
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanPoliklinik, ControllerTestHarness.Pesan(hasil));
            Assert.Equal(0, await context.Set<TrxDoctorConsultation>().CountAsync());
        }

        /// <summary>
        /// Regresi IGD. Jalur tanpa antrean milik IGD tetap berhasil, dan tetap tidak melahirkan
        /// satu baris antrean pun.
        /// </summary>
        [Fact]
        public async Task IgdTanpaAntrean_TetapBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokterMaster = RawatInapTestData.BuatDokterMaster(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var kunjungan = context.Set<TrxPatientEncounter>().First(x => x.Id == konteks.EncounterId);
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

            var hasil = await BuatControllerCatatan(context, aktor.Id)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = konteks.EncounterId,
                    DoctorId = dokterMaster.Id,
                    Subjective = "Isi uji"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(0, await context.Set<TrxQueue>().CountAsync());
        }

        /// <summary>
        /// Regresi IGD. Pasien IGD yang perawatan rawat inapnya baru berstatus `Draft` tetap
        /// dilewatkan, karena kunjungan IGD diperiksa lebih dulu dan dilewatkan apa adanya.
        /// </summary>
        /// <remarks>
        /// Urutan pemeriksaan itu disengaja: menilai keadaan perawatan lebih dulu akan menutup
        /// pencatatan IGD pada pasien yang sedang dalam proses admisi — jalur yang hari ini
        /// berjalan dan tidak diminta berubah oleh task mana pun.
        /// </remarks>
        [Fact]
        public async Task IgdDenganPerawatanMasihDraft_TetapBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Draft);

            context.Set<EmgVisit>().Add(new EmgVisit
            {
                EmergencyVisitNumber = $"IGD-{Guid.NewGuid():N}"[..16],
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                ArrivalDateTime = DateTime.UtcNow.AddHours(-1)
            });
            await context.SaveChangesAsync();

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(PermintaanCatatan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// Regresi poliklinik. Jalur berantre tetap berhasil dan tetap memindahkan keadaan
        /// antreannya.
        /// </summary>
        [Fact]
        public async Task JalurBerantre_TetapBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokterMaster = RawatInapTestData.BuatDokterMaster(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var antrean = new TrxQueue
            {
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                ServiceUnitId = konteks.ServiceUnitId,
                QueueCode = $"ANT-{Guid.NewGuid():N}"[..16],
                QueueNumber = 1,
                QueueDate = DateTime.UtcNow,
                QueueStatus = QueueStatus.WaitingForDoctor,
                DoctorId = dokterMaster.Id,
                IsDoctorRequired = true
            };
            context.Set<TrxQueue>().Add(antrean);
            await context.SaveChangesAsync();

            var hasil = await BuatControllerCatatan(context, aktor.Id)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = konteks.EncounterId,
                    QueueId = antrean.Id,
                    Subjective = "Isi uji"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var sesudah = await context.Set<TrxQueue>().SingleAsync(x => x.Id == antrean.Id);
            Assert.Equal(QueueStatus.InConsultation, sesudah.QueueStatus);
        }

        // =====================================================================
        // Perkakas
        // =====================================================================

        private static TrxQueue BuatAntrean(ApplicationDbContext context, RawatInapTestData.Konteks k)
        {
            var antrean = new TrxQueue
            {
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                QueueCode = $"ANT-{Guid.NewGuid():N}"[..16],
                QueueNumber = 1,
                QueueDate = DateTime.UtcNow,
                QueueStatus = QueueStatus.WaitingForDoctor,
                DoctorId = k.DoctorMasterId,
                IsDoctorRequired = true
            };

            context.Set<TrxQueue>().Add(antrean);
            context.SaveChanges();
            return antrean;
        }
    }
}
