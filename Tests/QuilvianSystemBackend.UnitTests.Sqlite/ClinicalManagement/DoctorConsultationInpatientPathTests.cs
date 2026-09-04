using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-037</c> — jalur tanpa antrean tidak lagi menggagalkan
    /// sistem — dan <c>BE-RWI-043</c> — pelonggaran batas satu catatan per kunjungan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kedua task menyentuh alur yang sedang melayani pasien poliklinik dan IGD, dan
    /// <c>RWI-RISK-002</c> mencatat belum ada jaring pengamannya. Karena itu <b>test regresi
    /// poliklinik dan IGD adalah bagian dari task</b>, bukan pekerjaan menyusul — <c>RWI-DEC-051</c>,
    /// <c>RWI-AC-143</c>.
    /// </para>
    /// <para>
    /// Kalimat penolakan rawat jalan dituliskan apa adanya di dalam uji ini. Bila kelak ada yang
    /// mengubahnya, uji ini gagal — dan itu memang tujuannya: <c>INV-DOK-05</c> menuntut kode dan
    /// kalimatnya sama persis seperti sebelum pelonggaran.
    /// </para>
    /// </remarks>
    public class DoctorConsultationInpatientPathTests
    {
        /// <summary>
        /// Kalimat penolakan catatan kedua, apa adanya sebelum <c>BE-RWI-043</c>. Rawat jalan
        /// dan medical check-up wajib tetap menerima kalimat ini.
        /// </summary>
        private const string PenolakanCatatanKedua = "Konsultasi dokter untuk encounter ini sudah ada.";

        private static ConsultationFinalizationService Finalisasi(ApplicationDbContext c) =>
            new(
                c,
                new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                new PrescriptionAggregateService(c, new PrescriptionSummaryService(c)),
                new PrescriptionWorkflowService(c),
                new ClinicalMilestoneFactProducer(
                    c,
                    new BillingFolioService(c),
                    ControllerTestHarness.BuatLoggerService()),
                // BE-RWI-038. Finalisasi kini sekaligus mendaftarkan catatan ke mesin keutuhan
                // rekam medis, sehingga service itu ikut disuntikkan pada uji.
                new ClinicalDocumentIntegrityService(c));

        private static DoctorConsultationController BuatController(
            ApplicationDbContext c, Guid actorUserId) =>
            new DoctorConsultationController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                Finalisasi(c),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        private static Task<int> JumlahAntreanAsync(ApplicationDbContext c) =>
            c.Set<TrxQueue>().CountAsync();

        // =====================================================================
        // Penyiapan data
        // =====================================================================

        private sealed record Kunjungan(
            Guid EncounterId,
            Guid PatientId,
            Guid ServiceUnitId,
            Guid DoctorMasterId,
            Guid AktorUserId);

        /// <summary>
        /// Kunjungan biasa beserta dokter master, tanpa antrean dan tanpa perawatan rawat inap.
        /// </summary>
        private static Kunjungan SiapkanKunjungan(
            ApplicationDbContext context,
            EncounterType encounterType)
        {
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context, EncounterStatus.Registered);
            var dokterMaster = RawatInapTestData.BuatDokterMaster(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var kunjungan = context.Set<TrxPatientEncounter>().First(x => x.Id == konteks.EncounterId);
            kunjungan.EncounterType = encounterType;
            context.SaveChanges();

            return new Kunjungan(
                konteks.EncounterId, konteks.PatientId, konteks.ServiceUnitId,
                dokterMaster.Id, aktor.Id);
        }

        /// <summary>
        /// Menandai satu kunjungan sebagai kunjungan IGD, syarat jalur tanpa antrean hari ini.
        /// </summary>
        private static void JadikanKunjunganIgd(ApplicationDbContext context, Kunjungan k)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            context.Set<EmgVisit>().Add(new EmgVisit
            {
                EmergencyVisitNumber = $"IGD-{pembeda}",
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                ArrivalDateTime = DateTime.UtcNow.AddHours(-1)
            });
            context.SaveChanges();
        }

        /// <summary>Membuat satu baris antrean yang siap dipakai konsultasi.</summary>
        private static TrxQueue BuatAntrean(ApplicationDbContext context, Kunjungan k, int nomor)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var antrean = new TrxQueue
            {
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                ServiceUnitId = k.ServiceUnitId,
                QueueCode = $"ANT-{pembeda}",
                QueueNumber = nomor,
                QueueDate = DateTime.UtcNow,
                QueueStatus = QueueStatus.WaitingForDoctor,
                DoctorId = k.DoctorMasterId,
                IsDoctorRequired = true
            };

            context.Set<TrxQueue>().Add(antrean);
            context.SaveChanges();
            return antrean;
        }

        private static CreateDoctorConsultationRequest Permintaan(
            Kunjungan k, Guid? queueId = null) => new()
            {
                EncounterId = k.EncounterId,
                QueueId = queueId,
                DoctorId = k.DoctorMasterId,
                Subjective = "Keluhan uji",
                Objective = "Pemeriksaan uji"
            };

        // =====================================================================
        // BE-RWI-037 - jalur tanpa antrean
        // =====================================================================

        /// <summary>
        /// `BE-RWI-037 AC 1, 2, 3` — catatan tanpa antrean tersimpan, jumlah baris antrean
        /// sebelum dan sesudah identik, dan jalur IGD lewat cara lamanya tetap berhasil.
        /// </summary>
        /// <remarks>
        /// Sebelum perbaikan, jalur ini menulis <c>queue.QueueStatus</c> pada antrean yang boleh
        /// kosong dan berujung kegagalan sistem. Uji ini gagal dengan
        /// <c>NullReferenceException</c> bila penjagaannya dicabut.
        /// </remarks>
        [Fact]
        public async Task TanpaAntrean_Tersimpan_DanJumlahAntreanTidakBerubah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = SiapkanKunjungan(context, EncounterType.Emergency);
            JadikanKunjunganIgd(context, k);

            var sebelum = await JumlahAntreanAsync(context);

            var hasil = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k));

            var sesudah = await JumlahAntreanAsync(context);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(sebelum, sesudah);
            Assert.Equal(0, sesudah);

            var tersimpan = await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.Null(tersimpan.QueueId);
            Assert.Equal(k.DoctorMasterId, tersimpan.DoctorId);
        }

        /// <summary>
        /// `BE-RWI-037 AC 3` — kunjungan ikut berpindah keadaan pada cabang tanpa antrean, sama
        /// seperti pada cabang berantre.
        /// </summary>
        [Fact]
        public async Task TanpaAntrean_KunjunganIkutBerpindahKeadaan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = SiapkanKunjungan(context, EncounterType.Emergency);
            JadikanKunjunganIgd(context, k);

            await BuatController(context, k.AktorUserId).CreateConsultation(Permintaan(k));

            var kunjungan = await context.Set<TrxPatientEncounter>().SingleAsync(x => x.Id == k.EncounterId);

            Assert.Equal(EncounterStatus.InConsultation, kunjungan.EncounterStatus);
        }

        /// <summary>
        /// `BE-RWI-037 AC 4` — jalur poliklinik lewat antrean tetap berhasil dan tetap
        /// memindahkan keadaan antreannya.
        /// </summary>
        [Fact]
        public async Task JalurAntreanPoliklinik_TetapBerhasil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = SiapkanKunjungan(context, EncounterType.Outpatient);
            var antrean = BuatAntrean(context, k, 1);

            var sebelum = await JumlahAntreanAsync(context);

            var hasil = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k, antrean.Id));

            var sesudah = await JumlahAntreanAsync(context);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(sebelum, sesudah);

            var antreanSesudah = await context.Set<TrxQueue>().SingleAsync(x => x.Id == antrean.Id);

            Assert.Equal(QueueStatus.InConsultation, antreanSesudah.QueueStatus);
            Assert.NotNull(antreanSesudah.ConsultationStartedAt);
        }

        /// <summary>
        /// `BE-RWI-037 AC 5` — kunjungan rawat jalan tanpa antrean tetap ditolak. Perbaikan itu
        /// hanya menutup kegagalan sistem, bukan membuka pintu baru.
        /// </summary>
        /// <remarks>
        /// <b>Kalimatnya diperbarui `BE-RWI-044`, kodenya tidak.</b> Sebelum task itu bunyinya
        /// "Konsultasi tanpa antrean hanya untuk pasien IGD"; sejak pintu rawat inap dibuka ada
        /// dua jalur sah tanpa antrean, sehingga kalimat lama berhenti benar. Penggantinya bunyi
        /// `VAL-DOK-04` apa adanya. Yang dijaga acceptance ini — penolakan `400` dan nol catatan
        /// yang lahir — tidak berubah sedikit pun.
        /// </remarks>
        [Fact]
        public async Task RawatJalanTanpaAntrean_TetapDitolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = SiapkanKunjungan(context, EncounterType.Outpatient);

            var hasil = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(
                "Konsultasi untuk pasien poliklinik tetap harus lewat antrean.",
                ControllerTestHarness.Pesan(hasil));
            Assert.Equal(0, await context.Set<TrxDoctorConsultation>().CountAsync());
        }

        // =====================================================================
        // BE-RWI-043 - pelonggaran batas satu catatan per kunjungan
        // =====================================================================

        /// <summary>
        /// `BE-RWI-043 AC 1` — batas satu catatan per kunjungan tidak lagi berlaku bagi catatan
        /// yang menempel pada perawatan rawat inap, <b>baik pada aturan aplikasi maupun pada
        /// index database</b>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dibuktikan pada lapisan penyimpanan, bukan ujung ke ujung lewat endpoint. Alasannya
        /// nyata dan tercatat: satu kunjungan hanya boleh memiliki satu baris antrean hidup —
        /// <c>IX_TrxQueue_EncounterId</c> unique — sehingga catatan kedua pada satu kunjungan
        /// hanya dapat lahir lewat cabang <b>tanpa antrean</b>. Cabang itu hari ini masih
        /// terbatas pada kunjungan IGD, dan membukanya untuk pasien rawat inap adalah pekerjaan
        /// <c>BE-RWI-044</c>.
        /// </para>
        /// <para>
        /// Yang dibuktikan di sini karena itu adalah penghalang yang selama ini menolak catatan
        /// kedua sudah benar-benar dilepas: dua catatan pada satu kunjungan rawat inap tersimpan
        /// keduanya, sedangkan pada kunjungan rawat jalan yang kedua tetap ditolak database.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task RawatInap_IndexTidakLagiMenolakCatatanKedua()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var rawatInap = RawatInapTestData.SiapkanPerawatan(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            TrxDoctorConsultation Catatan(Guid? episodeId, int nomor) => new()
            {
                ConsultationNumber = $"CON-{Guid.NewGuid():N}"[..20],
                EncounterId = rawatInap.EncounterId,
                InpEpisodeId = episodeId,
                PatientId = rawatInap.PatientId,
                DoctorId = rawatInap.DoctorMasterId,
                ServiceUnitId = rawatInap.ServiceUnitId,
                ConsultationDateTime = DateTime.UtcNow.AddHours(nomor),
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                StartedByUserId = aktor.Id,
                CreateBy = aktor.Id
            };

            context.Set<TrxDoctorConsultation>().Add(Catatan(rawatInap.EpisodeId, 1));
            await context.SaveChangesAsync();

            context.Set<TrxDoctorConsultation>().Add(Catatan(rawatInap.EpisodeId, 2));
            await context.SaveChangesAsync();

            Assert.Equal(2, await context.Set<TrxDoctorConsultation>()
                .CountAsync(x => x.EncounterId == rawatInap.EncounterId));

            // Catatan tanpa konteks perawatan — bentuk rawat jalan — tetap ditolak database.
            context.Set<TrxDoctorConsultation>().Add(Catatan(null, 3));
            await context.SaveChangesAsync();

            context.Set<TrxDoctorConsultation>().Add(Catatan(null, 4));

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        /// <summary>
        /// `BE-RWI-043` — konteks perawatan distempel pada catatan yang lahir di atas perawatan
        /// berjalan, sehingga penjagaan aplikasi dan index database memakai penanda yang sama.
        /// </summary>
        [Fact]
        public async Task RawatInap_KonteksPerawatanTerstempelPadaCatatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var rawatInap = RawatInapTestData.SiapkanPerawatan(context);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var k = new Kunjungan(
                rawatInap.EncounterId, rawatInap.PatientId, rawatInap.ServiceUnitId,
                rawatInap.DoctorMasterId, aktor.Id);

            var antrean = BuatAntrean(context, k, 1);
            await BuatController(context, k.AktorUserId).CreateConsultation(Permintaan(k, antrean.Id));

            var tersimpan = await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);

            Assert.Equal(rawatInap.EpisodeId, tersimpan.InpEpisodeId);
        }

        /// <summary>
        /// `BE-RWI-043 AC 3` — catatan kedua pada kunjungan <b>rawat jalan</b> tetap ditolak
        /// dengan kode dan kalimat yang sama persis seperti sebelum perubahan.
        /// </summary>
        /// <remarks>
        /// Ini penjaga <c>RWI-AC-143</c>. Kalimatnya dibandingkan dengan
        /// <see cref="PenolakanCatatanKedua"/>, bukan dengan potongan katanya, supaya perubahan
        /// sekecil apa pun terlihat.
        /// </remarks>
        [Theory]
        [InlineData(EncounterType.Outpatient)]
        [InlineData(EncounterType.MedicalCheckup)]
        public async Task RawatJalanDanMedicalCheckup_CatatanKeduaTetapDitolakDenganKalimatSama(
            EncounterType encounterType)
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = SiapkanKunjungan(context, encounterType);

            // Satu kunjungan hanya boleh memiliki satu baris antrean hidup, sehingga percobaan
            // kedua memakai antrean yang sama. Keadaan antrean sesudah konsultasi pertama masih
            // termasuk yang diizinkan, sehingga permintaan kedua benar-benar sampai pada aturan
            // satu catatan per kunjungan — bukan tertahan penjagaan lain sebelum itu.
            var antrean = BuatAntrean(context, k, 1);

            var pertama = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k, antrean.Id));
            var kedua = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k, antrean.Id));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pertama));
            Assert.Equal(400, ControllerTestHarness.KodeStatus(kedua));
            Assert.Equal(PenolakanCatatanKedua, ControllerTestHarness.Pesan(kedua));

            Assert.Equal(1, await context.Set<TrxDoctorConsultation>()
                .CountAsync(x => x.EncounterId == k.EncounterId));
        }

        /// <summary>
        /// `BE-RWI-043 AC 6` — perilaku IGD tetap berjalan: catatan pertama tetap diterima, dan
        /// batas satu catatan per kunjungan masih berlaku baginya.
        /// </summary>
        /// <remarks>
        /// Pelonggaran untuk kunjungan IGD yang diminta <c>INT-DOK-02</c> <b>tidak</b> dikerjakan
        /// task ini; alasannya dicatat pada laporan task. Uji ini mengunci perilaku IGD apa
        /// adanya supaya perubahan berikutnya terlihat.
        /// </remarks>
        [Fact]
        public async Task Igd_PerilakunyaTetapSepertiSebelumnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = SiapkanKunjungan(context, EncounterType.Emergency);
            JadikanKunjunganIgd(context, k);

            var pertama = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k));
            var kedua = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pertama));
            Assert.Equal(400, ControllerTestHarness.KodeStatus(kedua));
            Assert.Equal(PenolakanCatatanKedua, ControllerTestHarness.Pesan(kedua));
            Assert.Equal(0, await JumlahAntreanAsync(context));
        }

        /// <summary>
        /// Kunjungan bertipe rawat inap yang perawatannya <b>belum dimulai</b> tetap tunduk pada
        /// batas lama. Penyaringnya adalah perawatan yang berjalan, bukan nama tipe kunjungan.
        /// </summary>
        [Fact]
        public async Task RawatInapYangPerawatannyaBelumDimulai_TetapDibatasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var rawatInap = RawatInapTestData.SiapkanPerawatan(
                context,
                QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums.InpEpisodeStatus.Draft);
            var aktor = RekamMedisTestData.BuatPengguna(context, "dokter");

            var k = new Kunjungan(
                rawatInap.EncounterId, rawatInap.PatientId, rawatInap.ServiceUnitId,
                rawatInap.DoctorMasterId, aktor.Id);

            var antrean = BuatAntrean(context, k, 1);

            await BuatController(context, k.AktorUserId).CreateConsultation(Permintaan(k, antrean.Id));

            var kedua = await BuatController(context, k.AktorUserId)
                .CreateConsultation(Permintaan(k, antrean.Id));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(kedua));
            Assert.Equal(PenolakanCatatanKedua, ControllerTestHarness.Pesan(kedua));
        }
    }
}
