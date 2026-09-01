using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk `RJ-DOC-BE-002` — validasi finalisasi yang mengikat.
    ///
    /// Kontrak yang diuji: `RJ-DOC-COMPLETION-001@1.0.0` bagian `1.5` dan `1.6`.
    ///
    /// Yang dibuktikan di sini:
    ///
    /// <list type="number">
    ///   <item>konsultasi tidak dapat menjadi selesai ketika validasi backend menolaknya;</item>
    ///   <item>jalur antrean memakai validator yang sama, bukan validasi yang lebih longgar;</item>
    ///   <item>peringatan wajib diakui secara sadar, tidak pernah diakui otomatis;</item>
    ///   <item>penolakan tidak meninggalkan satu pun perubahan yang tersimpan sebagian;</item>
    ///   <item>pemeriksaan penunjang yang belum selesai dikerjakan **tidak** menahan konsultasi.</item>
    /// </list>
    /// </summary>
    public class DoctorConsultationValidationTests
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

        private static ConsultationValidationService Validator(ApplicationDbContext c) =>
            new(c, new PrescriptionValidationService(c));

        // =====================================================================
        // Penyiapan data
        // =====================================================================

        private sealed record Konsultasi(
            Guid ConsultationId,
            Guid QueueId,
            Guid EncounterId,
            Guid PatientId,
            Guid ServiceUnitId,
            Guid DoctorMasterId,
            Guid DokterUserId);

        private static MstDoctor BuatDokterMaster(ApplicationDbContext context)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var profil = new MstWorkforceProfile
            {
                ProfileCode = $"PRF-{pembeda}",
                UserType = QuilvianSystemBackend.Enums.UserType.PermanentDoctor,
                DisplayName = $"Dokter Uji {pembeda}"
            };
            var jenisTenaga = new MstWorkforceType { WorkforceTypeCode = $"WFT-{pembeda}", WorkforceTypeName = "Tenaga Medis" };
            var kategori = new MstEmployeeCategory { EmployeeCategoryCode = $"KAT-{pembeda}", EmployeeCategoryName = "Tetap" };
            var jenisKepegawaian = new MstEmploymentType { EmploymentTypeCode = $"EMT-{pembeda}", EmploymentTypeName = "Purnawaktu" };
            var statusKepegawaian = new MstEmploymentStatus { EmploymentStatusCode = $"EMS-{pembeda}", EmploymentStatusName = "Aktif" };
            var profesi = new MstProfession { ProfessionCode = $"PRO-{pembeda}", ProfessionName = "Dokter Umum", ProfessionGroup = "Medis" };

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
        /// Konsultasi Rawat Jalan yang sedang berjalan dengan dokumentasi klinis lengkap,
        /// sehingga tanpa gangguan lain ia lolos validasi.
        /// </summary>
        private static async Task<Konsultasi> SiapkanKonsultasiLayak(
            ApplicationDbContext context,
            bool dokumentasiLengkap = true)
        {
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context, EncounterStatus.InConsultation);
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

            return new Konsultasi(
                konsultasi.Id, antrean.Id, konteks.EncounterId,
                konteks.PatientId, konteks.ServiceUnitId, dokterMaster.Id, dokter.Id);
        }

        private static MstProcedure BuatProcedureMaster(ApplicationDbContext context)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];
            var procedure = new MstProcedure
            {
                ProcedureCode = $"TIN-{pembeda}",
                ProcedureName = "Tindakan Uji",
                ProcedureType = "General"
            };
            context.Set<MstProcedure>().Add(procedure);
            context.SaveChanges();
            return procedure;
        }

        private static TrxPatientProcedure TambahTindakan(
            ApplicationDbContext context,
            Konsultasi k,
            Action<TrxPatientProcedure> ubah)
        {
            var master = BuatProcedureMaster(context);

            var tindakan = new TrxPatientProcedure
            {
                EncounterId = k.EncounterId,
                ConsultationId = k.ConsultationId,
                PatientId = k.PatientId,
                ProcedureId = master.Id,
                DoctorId = k.DoctorMasterId,
                ServiceUnitId = k.ServiceUnitId,
                ProcedureNameSnapshot = "Tindakan Uji",
                Quantity = 1,
                IsBillable = false,
                IsFreeOfCharge = true,
                ProcedureStatus = PatientProcedureStatus.Ordered,
                IsActive = true
            };

            ubah(tindakan);
            context.Set<TrxPatientProcedure>().Add(tindakan);
            context.SaveChanges();
            return tindakan;
        }

        private static async Task<List<ConsultationFinalizationIssueResponse>> IssuesAsync(
            ApplicationDbContext context, Guid consultationId)
        {
            var hasil = await Validator(context).ValidateAsync(consultationId);
            return hasil.Sections.SelectMany(x => x.Issues).ToList();
        }

        // =====================================================================
        // A. Dokumentasi klinis wajib
        // =====================================================================

        /// <summary>`A` — SOAP yang belum lengkap menahan finalisasi dan tidak mengubah state.</summary>
        [Fact]
        public async Task SoapBelumLengkap_MenahanFinalisasiTanpaMengubahState()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context, dokumentasiLengkap: false);

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.False(hasil.IsSuccess);
            Assert.NotNull(hasil.Validation);
            Assert.False(hasil.Validation!.CanFinalize);

            var kode = hasil.Validation.Sections.SelectMany(x => x.Issues).Select(x => x.Code).ToList();
            Assert.Contains("MISSING_SUBJECTIVE", kode);
            Assert.Contains("MISSING_OBJECTIVE", kode);
            Assert.Contains("MISSING_ASSESSMENT", kode);
            Assert.Contains("MISSING_PLAN", kode);

            await PastikanTidakAdaTransisi(database, k);
        }

        /// <summary>`B` — diagnosis utama yang belum ada menahan finalisasi.</summary>
        [Fact]
        public async Task DiagnosisUtamaBelumAda_MenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            var konsultasi = context.Set<TrxDoctorConsultation>().Single(x => x.Id == k.ConsultationId);
            konsultasi.HasPrimaryDiagnosis = false;
            konsultasi.DiagnosisCount = 0;
            await context.SaveChangesAsync();

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.False(hasil.IsSuccess);
            Assert.Contains("MISSING_PRIMARY_DIAGNOSIS",
                hasil.Validation!.Sections.SelectMany(x => x.Issues).Select(x => x.Code));

            await PastikanTidakAdaTransisi(database, k);
        }

        // =====================================================================
        // C. Tindakan
        // =====================================================================

        /// <summary>`C` — jumlah tindakan yang tidak masuk akal menahan finalisasi.</summary>
        [Fact]
        public async Task JumlahTindakanTidakValid_MenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            var tindakan = TambahTindakan(context, k, _ => { });

            // Dipasang lewat pembaruan, bukan saat penyisipan. Kolom `Quantity` memiliki nilai
            // bawaan `1` di database, dan EF menghilangkan properti bernilai default CLR (`0`)
            // dari perintah INSERT — sehingga menyisipkan `0` justru tersimpan sebagai `1`.
            // Pada UPDATE nilainya dikirim apa adanya.
            tindakan.Quantity = 0;
            await context.SaveChangesAsync();

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.False(hasil.IsSuccess);
            Assert.Contains("INVALID_PROCEDURE_QUANTITY",
                hasil.Validation!.Sections.SelectMany(x => x.Issues).Select(x => x.Code));

            await PastikanTidakAdaTransisi(database, k);
        }

        /// <summary>
        /// `RJ-DOC-BE-002` aturan baru — tindakan berstatus dibatalkan tetapi barisnya masih
        /// aktif adalah keadaan yang tidak dapat dipastikan, dan menahan finalisasi.
        /// </summary>
        [Fact]
        public async Task TindakanBerstatusDibatalkanTetapiMasihAktif_MenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            TambahTindakan(context, k, x => x.ProcedureStatus = PatientProcedureStatus.Cancelled);

            var issues = await IssuesAsync(context, k.ConsultationId);
            Assert.Contains("INCONSISTENT_PROCEDURE_STATUS", issues.Select(x => x.Code));

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);
            Assert.False(hasil.IsSuccess);

            await PastikanTidakAdaTransisi(database, k);
        }

        /// <summary>
        /// `RJ-DOC-BE-002` aturan baru — tindakan yang menempel pada kunjungan lain menahan
        /// finalisasi, karena ia bukan milik kunjungan yang sedang diselesaikan.
        /// </summary>
        [Fact]
        public async Task TindakanMenempelKunjunganLain_MenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            var lain = await SiapkanKonsultasiLayak(context);

            TambahTindakan(context, k, x => x.EncounterId = lain.EncounterId);

            var issues = await IssuesAsync(context, k.ConsultationId);
            Assert.Contains("PROCEDURE_ENCOUNTER_MISMATCH", issues.Select(x => x.Code));

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);
            Assert.False(hasil.IsSuccess);

            await PastikanTidakAdaTransisi(database, k);
        }

        // =====================================================================
        // D. Resep
        // =====================================================================

        /// <summary>`D` — resep kosong menahan finalisasi lewat `PrescriptionValidationService`.</summary>
        [Fact]
        public async Task ResepKosong_MenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);

            context.Set<TrxPrescription>().Add(new TrxPrescription
            {
                PrescriptionNumber = $"RSP-{Guid.NewGuid().ToString("N")[..8]}",
                EncounterId = k.EncounterId,
                ConsultationId = k.ConsultationId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                ServiceUnitId = k.ServiceUnitId,
                PrescriptionStatus = PrescriptionStatus.Draft,
                TotalItemCount = 0,
                IsActive = true
            });
            await context.SaveChangesAsync();

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.False(hasil.IsSuccess);
            Assert.Contains("EMPTY_PRESCRIPTION",
                hasil.Validation!.Sections.SelectMany(x => x.Issues).Select(x => x.Code));

            await PastikanTidakAdaTransisi(database, k);
        }

        /// <summary>
        /// `RJ-DOC-BE-002` aturan baru — resep yang menempel pada kunjungan lain menahan
        /// finalisasi. Bila dibiarkan, fakta klinisnya akan mendarat pada kunjungan yang salah.
        /// </summary>
        [Fact]
        public async Task ResepMenempelKunjunganLain_MenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            var lain = await SiapkanKonsultasiLayak(context);

            context.Set<TrxPrescription>().Add(new TrxPrescription
            {
                PrescriptionNumber = $"RSP-{Guid.NewGuid().ToString("N")[..8]}",
                EncounterId = lain.EncounterId,
                ConsultationId = k.ConsultationId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                ServiceUnitId = k.ServiceUnitId,
                PrescriptionStatus = PrescriptionStatus.Draft,
                TotalItemCount = 1,
                IsActive = true
            });
            await context.SaveChangesAsync();

            var issues = await IssuesAsync(context, k.ConsultationId);
            Assert.Contains("PRESCRIPTION_ENCOUNTER_MISMATCH", issues.Select(x => x.Code));

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);
            Assert.False(hasil.IsSuccess);

            await PastikanTidakAdaTransisi(database, k);
        }

        // =====================================================================
        // E. Peringatan wajib diakui secara sadar
        // =====================================================================

        /// <summary>
        /// Menyiapkan satu resep sah berisi satu obat high alert — sumber peringatan yang
        /// deterministik, tanpa satu pun error.
        /// </summary>
        private static async Task<string> SiapkanResepDenganPeringatanAsync(
            ApplicationDbContext context, Konsultasi k)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var kategoriObat = new MstDrugCategory
            {
                DrugCategoryCode = $"KTO-{pembeda}",
                DrugCategoryName = "Kategori Uji"
            };
            var satuan = new MstMeasurement
            {
                MeasurementCode = $"SAT-{pembeda}",
                MeasurementName = "Tablet",
                MeasurementType = "General"
            };
            var kategoriTarif = new MstTariffCategory
            {
                TariffCategoryCode = $"KTT-{pembeda}",
                TariffCategoryName = "Kategori Tarif Uji"
            };
            context.AddRange(kategoriObat, satuan, kategoriTarif);
            await context.SaveChangesAsync();

            var obat = new MstDrug
            {
                DrugCategoryId = kategoriObat.Id,
                DrugCode = $"OBT-{pembeda}",
                DrugName = "Obat Uji High Alert"
            };
            var tarif = new MstTariff
            {
                TariffCode = $"TRF-{pembeda}",
                TariffName = "Tarif Obat Uji",
                TariffCategoryId = kategoriTarif.Id
            };
            context.AddRange(obat, tarif);
            await context.SaveChangesAsync();

            var resep = new TrxPrescription
            {
                PrescriptionNumber = $"RSP-{pembeda}",
                EncounterId = k.EncounterId,
                ConsultationId = k.ConsultationId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                ServiceUnitId = k.ServiceUnitId,
                PrescriptionStatus = PrescriptionStatus.Draft,
                TotalItemCount = 1,
                IsActive = true
            };
            context.Set<TrxPrescription>().Add(resep);
            await context.SaveChangesAsync();

            var item = new TrxPrescriptionItem
            {
                PrescriptionId = resep.Id,
                DrugId = obat.Id,
                DrugNameSnapshot = "Obat Uji High Alert",
                TariffId = tarif.Id,
                Dose = 2,
                DoseUnitMeasurementId = satuan.Id,
                Quantity = 10,
                DispenseUnitMeasurementId = satuan.Id,
                Signa = "3 kali sehari 1 tablet",
                IsHighAlertSnapshot = true,
                IsActive = true
            };
            context.Set<TrxPrescriptionItem>().Add(item);
            await context.SaveChangesAsync();

            return $"HIGH_ALERT_DRUG:PrescriptionItem:{item.Id}";
        }

        /// <summary>
        /// `E` — kontrak bagian 1.5.
        ///
        /// Peringatan menahan finalisasi selama belum diakui, lalu meloloskannya setelah
        /// `IssueKey`-nya dikirim pada `AcknowledgedWarningKeys`. Peringatan **tidak pernah**
        /// diakui otomatis oleh server.
        /// </summary>
        [Fact]
        public async Task PeringatanBelumDiakui_MenahanFinalisasiLaluLolosSetelahDiakui()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            var issueKey = await SiapkanResepDenganPeringatanAsync(context, k);

            // Tanpa acknowledgement: tertahan, dan tidak ada error — murni karena peringatan.
            var tertahan = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.False(tertahan.IsSuccess);
            Assert.True(tertahan.RequiresWarningAcknowledgement);
            Assert.NotNull(tertahan.Validation);
            Assert.Equal(0, tertahan.Validation!.ErrorCount);
            Assert.True(tertahan.Validation.WarningCount > 0);

            var peringatan = tertahan.Validation.Sections
                .SelectMany(x => x.Issues)
                .Where(x => x.Severity == ConsultationValidationSeverity.Warning)
                .ToList();
            Assert.Contains(peringatan, x => x.IssueKey == issueKey);

            await PastikanTidakAdaTransisi(database, k);

            // Dengan acknowledgement atas IssueKey yang sama: lolos.
            using var contextKedua = database.CreateContext();
            var lolos = await Finalisasi(contextKedua).FinalizeAsync(
                k.ConsultationId,
                new FinalizeDoctorConsultationRequest
                {
                    AcknowledgedWarningKeys = peringatan.Select(x => x.IssueKey).ToList()
                },
                k.DokterUserId);

            Assert.True(lolos.IsSuccess, lolos.ErrorMessage);

            using var verifikasi = database.CreateContext();
            var konsultasi = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == k.ConsultationId);
            Assert.Equal(DoctorConsultationStatus.Completed, konsultasi.ConsultationStatus);
        }

        /// <summary>
        /// Mengakui `IssueKey` milik peringatan lain tidak meloloskan peringatan yang sedang
        /// menahan. Acknowledgement bersifat per-peringatan, bukan sapu jagat.
        /// </summary>
        [Fact]
        public async Task AcknowledgementIssueKeyLain_TidakMeloloskanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            await SiapkanResepDenganPeringatanAsync(context, k);

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId,
                new FinalizeDoctorConsultationRequest
                {
                    AcknowledgedWarningKeys = new List<string> { "HIGH_ALERT_DRUG:PrescriptionItem:" + Guid.NewGuid() }
                },
                k.DokterUserId);

            Assert.False(hasil.IsSuccess);
            Assert.True(hasil.RequiresWarningAcknowledgement);

            await PastikanTidakAdaTransisi(database, k);
        }

        // =====================================================================
        // F. Penunjang — order stabil, eksekusi belum selesai
        // =====================================================================

        /// <summary>
        /// `F` — keputusan pemilik `RJ-DOC-DEC-004`.
        ///
        /// Order laboratorium yang sudah tersimpan authoritative tetapi specimen-nya belum
        /// diambil **tidak** menahan penyelesaian konsultasi. Lifecycle eksekusi milik unit
        /// Laboratorium dan berlanjut setelah dokter selesai.
        /// </summary>
        [Fact]
        public async Task OrderLabBelumDikerjakan_TidakMenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            var master = BuatProcedureMaster(context);

            context.Set<LabOrder>().Add(new LabOrder
            {
                EncounterId = k.EncounterId,
                ProcedureId = master.Id,
                OrderStatus = LabOrderStatus.Requested,
                RequestedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.True(hasil.IsSuccess, hasil.ErrorMessage);

            using var verifikasi = database.CreateContext();
            var konsultasi = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == k.ConsultationId);
            Assert.Equal(DoctorConsultationStatus.Completed, konsultasi.ConsultationStatus);

            // Order laboratorium tetap apa adanya; dokter tidak menutup pekerjaan unit lain.
            var order = verifikasi.Set<LabOrder>().Single(x => x.EncounterId == k.EncounterId);
            Assert.Equal(LabOrderStatus.Requested, order.OrderStatus);
        }

        /// <summary>
        /// Ketiadaan order penunjang sama sekali juga tidak menahan finalisasi — Lab dan
        /// Radiologi berstatus `CONDITIONAL` menurut `RJ-DOC-DEC-002`.
        /// </summary>
        [Fact]
        public async Task TanpaOrderPenunjang_TidakMenahanFinalisasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);

            var hasil = await Finalisasi(context).FinalizeAsync(
                k.ConsultationId, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.True(hasil.IsSuccess, hasil.ErrorMessage);
        }

        // =====================================================================
        // G. Jalur antrean memakai validator yang sama, dan atomicity
        // =====================================================================

        private static DoctorQueueController BuatQueueController(ApplicationDbContext c, Guid actorUserId)
        {
            var logger = ControllerTestHarness.BuatLoggerService(actorUserId);

            var controller = new DoctorQueueController(
                c,
                logger,
                null!,
                new QueueRealtimeService(c, new HubContextKosong<QueueHub>(), logger),
                new DoctorConsultationLifecycleService(c),
                new ClinicalDocumentIntegrityService(c),
                Finalisasi(c));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = ControllerTestHarness.BuatHttpContextSuperAdmin(actorUserId)
            };

            return controller;
        }

        /// <summary>
        /// `12` — jalur kompatibilitas antrean memakai validator yang sama, bukan validasi yang
        /// lebih longgar. Sebelum `RJ-DOC-BE-001` jalur ini tidak memvalidasi apa pun.
        /// </summary>
        [Fact]
        public async Task JalurAntrean_MemakaiValidatorYangSamaDanMenolakKonsultasiTidakLayak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context, dokumentasiLengkap: false);
            var controller = BuatQueueController(context, k.DokterUserId);

            var hasil = await controller.FinishConsultation(k.QueueId, new DoctorQueueActionRequest());

            var badRequest = Assert.IsType<BadRequestObjectResult>(hasil);
            var isi = Assert.IsType<ApiResponse<ConsultationFinalizationValidationResponse>>(badRequest.Value);

            Assert.NotNull(isi.Data);
            Assert.False(isi.Data!.CanFinalize);
            Assert.True(isi.Data.ErrorCount > 0);
            Assert.Contains("MISSING_SUBJECTIVE", isi.Data.Sections.SelectMany(x => x.Issues).Select(x => x.Code));

            await PastikanTidakAdaTransisi(database, k);
        }

        /// <summary>
        /// `G` — atomicity. Penolakan validasi lewat jalur antrean tidak boleh meninggalkan
        /// catatan antrean maupun penguncian dokumen yang terlanjur tersimpan.
        ///
        /// Ini yang paling mudah luput: jalur antrean menulis catatan dan mengunci dokumen
        /// **sebelum** validasi berjalan, sehingga keduanya harus ikut dibatalkan.
        /// </summary>
        [Fact]
        public async Task JalurAntreanDitolak_TidakMeninggalkanCatatanMaupunPenguncianDokumen()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context, dokumentasiLengkap: false);

            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var integritas = new ClinicalDocumentIntegrityService(context);
            var dokumenId = Guid.NewGuid();
            await integritas.RegisterAsync(
                ClinicalDocumentKind.ProgressNote, dokumenId, k.PatientId, k.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            var controller = BuatQueueController(context, k.DokterUserId);
            var hasil = await controller.FinishConsultation(
                k.QueueId, new DoctorQueueActionRequest { Notes = "catatan penyelesaian uji" });

            Assert.IsType<BadRequestObjectResult>(hasil);

            using var verifikasi = database.CreateContext();

            // Dokumen tetap draf — tidak terkunci oleh penyelesaian yang gagal.
            var keutuhan = verifikasi.Set<TrxClinicalDocumentIntegrity>()
                .Single(x => x.DocumentId == dokumenId);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, keutuhan.IntegrityStatus);
            Assert.Null(keutuhan.LockedAt);

            // Catatan antrean tidak ikut tersimpan.
            var antrean = verifikasi.Set<TrxQueue>().Single(x => x.Id == k.QueueId);
            Assert.DoesNotContain("catatan penyelesaian uji", antrean.Notes ?? string.Empty);

            await PastikanTidakAdaTransisi(database, k);
        }

        /// <summary>
        /// Jalur antrean yang layak tetap berhasil, sehingga pengetatan validasi tidak menutup
        /// alur normal.
        /// </summary>
        [Fact]
        public async Task JalurAntrean_KonsultasiLayakTetapBerhasilDiselesaikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = await SiapkanKonsultasiLayak(context);
            var controller = BuatQueueController(context, k.DokterUserId);

            var hasil = await controller.FinishConsultation(k.QueueId, new DoctorQueueActionRequest());
            Assert.IsType<OkObjectResult>(hasil);

            using var verifikasi = database.CreateContext();
            var konsultasi = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == k.ConsultationId);
            Assert.Equal(DoctorConsultationStatus.Completed, konsultasi.ConsultationStatus);

            var kunjungan = verifikasi.Set<TrxPatientEncounter>().Single(x => x.Id == k.EncounterId);
            Assert.Equal(EncounterStatus.ConsultationCompleted, kunjungan.EncounterStatus);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Membuktikan tidak ada satu pun transisi penyelesaian yang tersimpan.
        /// </summary>
        private static async Task PastikanTidakAdaTransisi(TestDatabase database, Konsultasi k)
        {
            using var verifikasi = database.CreateContext();

            var konsultasi = await verifikasi.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.Id == k.ConsultationId);
            Assert.Equal(DoctorConsultationStatus.InProgress, konsultasi.ConsultationStatus);
            Assert.Null(konsultasi.CompletedAt);
            Assert.Null(konsultasi.CompletedByUserId);

            var antrean = await verifikasi.Set<TrxQueue>().SingleAsync(x => x.Id == k.QueueId);
            Assert.Equal(QueueStatus.InConsultation, antrean.QueueStatus);
            Assert.Null(antrean.CompletedAt);
            Assert.Null(antrean.ConsultationCompletedAt);

            var kunjungan = await verifikasi.Set<TrxPatientEncounter>().SingleAsync(x => x.Id == k.EncounterId);
            Assert.NotEqual(EncounterStatus.ConsultationCompleted, kunjungan.EncounterStatus);
            Assert.NotEqual(EncounterStatus.Completed, kunjungan.EncounterStatus);

            // Resep draf tidak boleh terfinalisasi sebagian.
            var resepSubmitted = await verifikasi.Set<TrxPrescription>()
                .CountAsync(x => x.ConsultationId == k.ConsultationId &&
                                 x.PrescriptionStatus != PrescriptionStatus.Draft);
            Assert.Equal(0, resepSubmitted);
        }
    }
}
