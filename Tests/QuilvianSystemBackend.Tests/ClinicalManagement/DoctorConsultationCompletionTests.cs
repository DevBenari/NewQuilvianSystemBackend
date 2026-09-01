using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk `RJ-DOC-BE-001` — penyatuan jalur penyelesaian konsultasi.
    ///
    /// Kontrak yang diuji: `RJ-DOC-COMPLETION-001@1.0.0`.
    ///
    /// Masalah yang dibuktikan tertutup: sebelum task ini, menyelesaikan konsultasi dari layar
    /// dokter menutup antrean dan kunjungan tanpa pernah menyentuh `TrxDoctorConsultation`.
    /// Konsultasi tertinggal `InProgress` selamanya, sehingga seluruh penguncian yang bergantung
    /// pada status itu tidak pernah aktif dan tidak satu pun fakta klinis diserahkan.
    ///
    /// Uji ini menguji **perilaku penyelesaian**, bukan satu controller tertentu, karena aturan
    /// yang sama harus berlaku pada setiap permukaan yang dapat menyelesaikan konsultasi.
    /// </summary>
    public class DoctorConsultationCompletionTests
    {
        private static ConsultationFinalizationService Finalisasi(ApplicationDbContext c) =>
            new(
                c,
                new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                new PrescriptionAggregateService(c, new PrescriptionSummaryService(c)),
                new PrescriptionWorkflowService(c),
                new ClinicalMilestoneFactProducer(
                    c,
                    new BillingFolioService(c),
                    ControllerTestHarness.BuatLoggerService()));

        private static DoctorConsultationController BuatControllerKonsultasi(
            ApplicationDbContext c,
            Guid actorUserId) =>
            new DoctorConsultationController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                Finalisasi(c))
                .DenganPengguna(actorUserId);

        /// <summary>
        /// Membuat satu dokter master beserta enam baris master yang wajib ada di belakangnya.
        ///
        /// Diperlukan karena `TrxDoctorConsultation.DoctorId` memiliki foreign key ke
        /// `MstDoctor`, dan basis data uji menegakkan foreign key. Seluruh nilainya karangan.
        /// </summary>
        private static MstDoctor BuatDokterMaster(ApplicationDbContext context)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var profil = new MstWorkforceProfile
            {
                ProfileCode = $"PRF-{pembeda}",
                UserType = UserType.PermanentDoctor,
                DisplayName = $"Dokter Uji {pembeda}"
            };
            var jenisTenaga = new MstWorkforceType
            {
                WorkforceTypeCode = $"WFT-{pembeda}",
                WorkforceTypeName = "Tenaga Medis"
            };
            var kategori = new MstEmployeeCategory
            {
                EmployeeCategoryCode = $"KAT-{pembeda}",
                EmployeeCategoryName = "Tetap"
            };
            var jenisKepegawaian = new MstEmploymentType
            {
                EmploymentTypeCode = $"EMT-{pembeda}",
                EmploymentTypeName = "Purnawaktu"
            };
            var statusKepegawaian = new MstEmploymentStatus
            {
                EmploymentStatusCode = $"EMS-{pembeda}",
                EmploymentStatusName = "Aktif"
            };
            var profesi = new MstProfession
            {
                ProfessionCode = $"PRO-{pembeda}",
                ProfessionName = "Dokter Umum",
                ProfessionGroup = "Medis"
            };

            context.AddRange(profil, jenisTenaga, kategori, jenisKepegawaian, statusKepegawaian, profesi);
            context.SaveChanges();

            var dokter = new MstDoctor
            {
                WorkforceProfileId = profil.Id,
                DoctorCode = $"DOK-{pembeda}",
                DoctorNumber = $"NO-{pembeda}",
                FullName = $"Dokter Uji {pembeda}",
                WorkforceTypeId = jenisTenaga.Id,
                EmployeeCategoryId = kategori.Id,
                EmploymentTypeId = jenisKepegawaian.Id,
                EmploymentStatusId = statusKepegawaian.Id,
                ProfessionId = profesi.Id
            };

            context.Set<MstDoctor>().Add(dokter);
            context.SaveChanges();

            return dokter;
        }

        /// <summary>
        /// Menyiapkan satu antrean Rawat Jalan yang sedang berkonsultasi beserta konsultasi
        /// dokternya, lengkap dengan dokumentasi klinis minimum agar validasi canonical lolos.
        /// </summary>
        private static async Task<(Guid ConsultationId, Guid QueueId, Guid EncounterId, Guid DokterUserId)>
            SiapkanKonsultasiBerjalan(ApplicationDbContext context, bool dokumentasiLengkap = true)
        {
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context,
                EncounterStatus.InConsultation);

            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var dokterMaster = BuatDokterMaster(context);
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var antrean = new TrxQueue
            {
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                ServiceUnitId = konteks.ServiceUnitId,
                QueueCode = $"ANT-{pembeda}",
                QueueNumber = 1,
                QueueDate = DateTime.UtcNow,
                QueueStatus = QueueStatus.InConsultation,
                DoctorId = dokterMaster.Id,
                IsDoctorRequired = true,
                ConsultationStartedAt = DateTime.UtcNow.AddMinutes(-10)
            };
            context.Set<TrxQueue>().Add(antrean);
            await context.SaveChangesAsync();

            var konsultasi = new TrxDoctorConsultation
            {
                ConsultationNumber = $"CON-{pembeda}",
                EncounterId = konteks.EncounterId,
                QueueId = antrean.Id,
                PatientId = konteks.PatientId,
                DoctorId = dokterMaster.Id,
                ServiceUnitId = konteks.ServiceUnitId,
                ConsultationDateTime = DateTime.UtcNow,
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                StartedByUserId = dokter.Id,
                IsActive = true,

                // Dokumentasi minimum agar ConsultationValidationService meloloskan finalisasi.
                Subjective = dokumentasiLengkap ? "Keluhan uji" : null,
                Objective = dokumentasiLengkap ? "Pemeriksaan uji" : null,
                Assessment = dokumentasiLengkap ? "Penilaian uji" : null,
                Plan = dokumentasiLengkap ? "Rencana uji" : null,
                HasPrimaryDiagnosis = dokumentasiLengkap,
                PrimaryDiagnosisText = dokumentasiLengkap ? "Diagnosis uji" : null,
                DiagnosisCount = dokumentasiLengkap ? 1 : 0
            };
            context.Set<TrxDoctorConsultation>().Add(konsultasi);
            await context.SaveChangesAsync();

            return (konsultasi.Id, antrean.Id, konteks.EncounterId, dokter.Id);
        }

        /// <summary>
        /// `A` — jalur canonical: konsultasi `InProgress` menjadi `Completed` beserta seluruh
        /// state yang diwajibkan kontrak bagian 1.7.
        /// </summary>
        [Fact]
        public async Task Canonical_MenyelesaikanKonsultasiBesertaAntreanDanKunjungan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (consultationId, queueId, encounterId, dokterId) =
                await SiapkanKonsultasiBerjalan(context);

            var hasil = await Finalisasi(context).FinalizeAsync(
                consultationId,
                new FinalizeDoctorConsultationRequest(),
                dokterId);

            Assert.True(hasil.IsSuccess, hasil.ErrorMessage);

            using var verifikasi = database.CreateContext();

            var konsultasi = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == consultationId);
            Assert.Equal(DoctorConsultationStatus.Completed, konsultasi.ConsultationStatus);
            Assert.NotNull(konsultasi.CompletedAt);
            Assert.NotNull(konsultasi.CompletedByUserId);
            Assert.NotEqual(Guid.Empty, konsultasi.CompletedByUserId.Value);
            Assert.Equal(dokterId, konsultasi.CompletedByUserId.Value);

            var antrean = verifikasi.Set<TrxQueue>().Single(x => x.Id == queueId);
            Assert.Equal(QueueStatus.Completed, antrean.QueueStatus);
            Assert.NotNull(antrean.ConsultationCompletedAt);
            Assert.NotNull(antrean.CompletedAt);
            Assert.Equal(dokterId, antrean.CompletedByUserId);

            // Kontrak bagian 1.8. Kunjungan berhenti di ConsultationCompleted; menaikkannya ke
            // Billing atau Completed bukan kewenangan modul dokter.
            var kunjungan = verifikasi.Set<TrxPatientEncounter>().Single(x => x.Id == encounterId);
            Assert.Equal(EncounterStatus.ConsultationCompleted, kunjungan.EncounterStatus);
        }

        /// <summary>
        /// `A2` — kontrak bagian 1.8 dinyatakan sebagai larangan eksplisit, bukan hanya sebagai
        /// nilai yang kebetulan benar.
        /// </summary>
        [Fact]
        public async Task Canonical_TidakMenaikkanKunjunganKeBillingAtauCompleted()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (consultationId, _, encounterId, dokterId) = await SiapkanKonsultasiBerjalan(context);

            await Finalisasi(context).FinalizeAsync(
                consultationId,
                new FinalizeDoctorConsultationRequest(),
                dokterId);

            using var verifikasi = database.CreateContext();
            var kunjungan = verifikasi.Set<TrxPatientEncounter>().Single(x => x.Id == encounterId);

            Assert.NotEqual(EncounterStatus.Billing, kunjungan.EncounterStatus);
            Assert.NotEqual(EncounterStatus.Completed, kunjungan.EncounterStatus);
        }

        /// <summary>
        /// `C` — validasi klinis yang gagal tidak boleh meninggalkan state separuh jadi.
        ///
        /// Inilah pembeda terpenting dari perilaku lama: dahulu antrean dan kunjungan ditutup
        /// tanpa syarat klinis apa pun.
        /// </summary>
        [Fact]
        public async Task ValidasiGagal_TidakMenutupKonsultasiAntreanMaupunKunjungan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (consultationId, queueId, encounterId, dokterId) =
                await SiapkanKonsultasiBerjalan(context, dokumentasiLengkap: false);

            var hasil = await Finalisasi(context).FinalizeAsync(
                consultationId,
                new FinalizeDoctorConsultationRequest(),
                dokterId);

            Assert.False(hasil.IsSuccess);
            Assert.NotNull(hasil.Validation);
            Assert.True(hasil.Validation!.ErrorCount > 0);

            using var verifikasi = database.CreateContext();

            var konsultasi = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == consultationId);
            Assert.Equal(DoctorConsultationStatus.InProgress, konsultasi.ConsultationStatus);
            Assert.Null(konsultasi.CompletedAt);
            Assert.Null(konsultasi.CompletedByUserId);

            var antrean = verifikasi.Set<TrxQueue>().Single(x => x.Id == queueId);
            Assert.Equal(QueueStatus.InConsultation, antrean.QueueStatus);
            Assert.Null(antrean.CompletedAt);

            var kunjungan = verifikasi.Set<TrxPatientEncounter>().Single(x => x.Id == encounterId);
            Assert.NotEqual(EncounterStatus.ConsultationCompleted, kunjungan.EncounterStatus);
            Assert.NotEqual(EncounterStatus.Completed, kunjungan.EncounterStatus);
        }

        /// <summary>
        /// Kontrak bagian 1.2 — aktor yang tidak dapat ditentukan ditolak, bukan disimpan
        /// sebagai `Guid.Empty`.
        /// </summary>
        [Fact]
        public async Task AktorKosong_DitolakDanTidakMenyimpanPenyelesaian()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (consultationId, _, _, _) = await SiapkanKonsultasiBerjalan(context);

            var hasil = await Finalisasi(context).FinalizeAsync(
                consultationId,
                new FinalizeDoctorConsultationRequest(),
                Guid.Empty);

            Assert.False(hasil.IsSuccess);

            using var verifikasi = database.CreateContext();
            var konsultasi = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == consultationId);

            Assert.Equal(DoctorConsultationStatus.InProgress, konsultasi.ConsultationStatus);
            Assert.Null(konsultasi.CompletedByUserId);
        }

        /// <summary>
        /// Kontrak bagian 1.9 — penyelesaian kedua atas konsultasi yang sama ditolak, dan tidak
        /// menimpa `CompletedAt`/`CompletedByUserId` yang sudah tercatat.
        /// </summary>
        [Fact]
        public async Task PenyelesaianKedua_DitolakDanTidakMenimpaJejakPenyelesaianPertama()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (consultationId, _, _, dokterId) = await SiapkanKonsultasiBerjalan(context);

            var pertama = await Finalisasi(context).FinalizeAsync(
                consultationId,
                new FinalizeDoctorConsultationRequest(),
                dokterId);
            Assert.True(pertama.IsSuccess, pertama.ErrorMessage);

            var completedAtPertama = pertama.Data!.CompletedAt;

            using var contextKedua = database.CreateContext();
            var kedua = await Finalisasi(contextKedua).FinalizeAsync(
                consultationId,
                new FinalizeDoctorConsultationRequest(),
                dokterId);

            Assert.False(kedua.IsSuccess);

            using var verifikasi = database.CreateContext();
            var konsultasi = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == consultationId);

            Assert.Equal(DoctorConsultationStatus.Completed, konsultasi.ConsultationStatus);
            Assert.Equal(completedAtPertama, konsultasi.CompletedAt);
            Assert.Equal(dokterId, konsultasi.CompletedByUserId);
        }

        /// <summary>
        /// `B` — resolusi konsultasi dari antrean, yang dipakai jalur kompatibilitas
        /// `POST /doctor-queues/{id}/finish-consultation`.
        ///
        /// Yang dibuktikan: resolusi menemukan konsultasi milik antrean itu, dan **berhenti
        /// menemukannya** setelah konsultasi selesai — sehingga jalur antrean tidak dapat
        /// memfinalisasi konsultasi dua kali.
        /// </summary>
        [Fact]
        public async Task ResolusiDariAntrean_MenemukanKonsultasiLaluBerhentiSetelahSelesai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (consultationId, queueId, encounterId, dokterId) =
                await SiapkanKonsultasiBerjalan(context);

            var lifecycle = new DoctorConsultationLifecycleService(context);

            var sebelum = await lifecycle.ResolveFinalizableForQueueAsync(queueId, encounterId);
            Assert.NotNull(sebelum);
            Assert.Equal(consultationId, sebelum!.Id);

            var hasil = await Finalisasi(context).FinalizeAsync(
                consultationId,
                new FinalizeDoctorConsultationRequest(),
                dokterId);
            Assert.True(hasil.IsSuccess, hasil.ErrorMessage);

            using var contextSesudah = database.CreateContext();
            var sesudah = await new DoctorConsultationLifecycleService(contextSesudah)
                .ResolveFinalizableForQueueAsync(queueId, encounterId);

            Assert.Null(sesudah);
        }

        /// <summary>
        /// `E` — kontrak bagian 1.11 dan keputusan `RJ-DOC-DEC-005`.
        ///
        /// `CompleteImmediately=true` tidak boleh menjadi jalur finalisasi alternatif untuk
        /// Rawat Jalan normal, karena ia menghasilkan konsultasi `Completed` tanpa validasi
        /// authoritative dan tanpa penyerahan fakta klinis.
        /// </summary>
        [Fact]
        public async Task CompleteImmediately_DitolakUntukPembuatanKonsultasiBerantrean()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, queueId, encounterId, dokterId) = await SiapkanKonsultasiBerjalan(context);

            var controller = BuatControllerKonsultasi(context, dokterId);

            var hasil = await controller.CreateConsultation(new CreateDoctorConsultationRequest
            {
                EncounterId = encounterId,
                QueueId = queueId,
                CompleteImmediately = true
            });

            var badRequest = Assert.IsType<BadRequestObjectResult>(hasil);
            var isi = Assert.IsType<ApiResponse<object>>(badRequest.Value);
            Assert.False(isi.Success);

            // Penolakan harus datang dari pembatasan CompleteImmediately, bukan dari validasi
            // lain yang kebetulan ikut gagal.
            Assert.Contains("tidak dapat langsung diselesaikan", isi.Message);

            // Tidak boleh ada konsultasi kedua yang lahir sudah selesai.
            using var verifikasi = database.CreateContext();
            var jumlahSelesai = verifikasi.Set<TrxDoctorConsultation>()
                .Count(x => x.EncounterId == encounterId &&
                            x.ConsultationStatus == DoctorConsultationStatus.Completed);

            Assert.Equal(0, jumlahSelesai);
        }

        /// <summary>
        /// `F` — pembatasan di atas hanya menyentuh `CompleteImmediately`, bukan pembuatan
        /// konsultasi itu sendiri. Pembuatan normal tetap berjalan.
        /// </summary>
        [Fact]
        public async Task PembuatanKonsultasiNormal_TetapBerjalanTanpaCompleteImmediately()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context,
                EncounterStatus.WaitingForDoctor);

            var dokterMaster = BuatDokterMaster(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var antrean = new TrxQueue
            {
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                ServiceUnitId = konteks.ServiceUnitId,
                QueueCode = $"ANT-{pembeda}",
                QueueNumber = 1,
                QueueDate = DateTime.UtcNow,
                QueueStatus = QueueStatus.WaitingForDoctor,
                DoctorId = dokterMaster.Id,
                IsDoctorRequired = true
            };
            context.Set<TrxQueue>().Add(antrean);
            await context.SaveChangesAsync();

            var controller = BuatControllerKonsultasi(context, dokter.Id);

            var hasil = await controller.CreateConsultation(new CreateDoctorConsultationRequest
            {
                EncounterId = konteks.EncounterId,
                QueueId = antrean.Id,
                CompleteImmediately = false
            });

            Assert.IsType<OkObjectResult>(hasil);

            using var verifikasi = database.CreateContext();
            var konsultasi = verifikasi.Set<TrxDoctorConsultation>()
                .Single(x => x.EncounterId == konteks.EncounterId);

            // Dibuat berjalan, bukan langsung selesai.
            Assert.Equal(DoctorConsultationStatus.InProgress, konsultasi.ConsultationStatus);
            Assert.Null(konsultasi.CompletedAt);
        }

        /// <summary>
        /// Resolusi tidak boleh mengambil konsultasi milik antrean atau kunjungan lain, walaupun
        /// keduanya sama-sama aktif.
        /// </summary>
        [Fact]
        public async Task ResolusiDariAntrean_TidakMengambilKonsultasiMilikAntreanLain()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pertama = await SiapkanKonsultasiBerjalan(context);
            var kedua = await SiapkanKonsultasiBerjalan(context);

            var lifecycle = new DoctorConsultationLifecycleService(context);

            var hasilPertama = await lifecycle.ResolveFinalizableForQueueAsync(
                pertama.QueueId, pertama.EncounterId);
            Assert.Equal(pertama.ConsultationId, hasilPertama!.Id);

            var hasilKedua = await lifecycle.ResolveFinalizableForQueueAsync(
                kedua.QueueId, kedua.EncounterId);
            Assert.Equal(kedua.ConsultationId, hasilKedua!.Id);

            // Pasangan antrean/kunjungan yang tidak cocok tidak menghasilkan apa pun.
            var silang = await lifecycle.ResolveFinalizableForQueueAsync(
                pertama.QueueId, kedua.EncounterId);
            Assert.Null(silang);
        }
    }
}
