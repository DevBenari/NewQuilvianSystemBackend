using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-038</c> — catatan yang sudah diselesaikan dapat
    /// dikoreksi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Masalah yang ditutup.</b> Sebelum task ini hanya catatan terpadu yang terdaftar pada
    /// mesin keutuhan rekam medis — <c>RWI-FACT-014</c>. Akibatnya catatan dokter, kajian
    /// medis, dan tindakan yang sudah diselesaikan berada di keadaan buntu: tidak dapat
    /// disunting karena statusnya sudah selesai, dan tidak dapat dikoreksi karena mesin koreksi
    /// tidak mengenalnya. Satu-satunya jalan membetulkan salah ketik adalah menulis catatan
    /// baru yang membantah catatan lama, dan rekam medis yang saling membantah lebih berbahaya
    /// daripada rekam medis yang salah ketik.
    /// </para>
    /// <para>
    /// <b>Yang paling penting dibuktikan di sini bukan pendaftarannya</b>, melainkan bahwa
    /// pendaftaran dan finalisasi benar-benar satu transaksi. Bila keduanya terpisah dan
    /// pendaftaran gagal, yang lahir adalah catatan final tanpa baris keutuhan — persis
    /// keadaan buntu yang sedang ditutup.
    /// </para>
    /// </remarks>
    public class ClinicalDocumentFinalizationIntegrityTests
    {
        private static ClinicalDocumentIntegrityService Keutuhan(ApplicationDbContext c) => new(c);

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
                Keutuhan(c));

        private static PatientAssessmentController BuatControllerKajian(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientAssessmentController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new InpatientClinicalContextService(c),
                Keutuhan(c))
                .DenganPengguna(actorUserId);

        private static PatientProcedureController BuatControllerTindakan(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientProcedureController(
                c,
                new EncounterInsuranceService(c),
                new InsuranceCoverageService(c, new EncounterInsuranceService(c)),
                new ClinicalMilestoneFactProducer(
                    c,
                    new BillingFolioService(c),
                    ControllerTestHarness.BuatLoggerService(actorUserId)),
                Keutuhan(c),
                // BE-RWI-051. Controller tindakan kini menjaga penanda pasien, perawatan, dan
                // kejadian visite lewat service konteks klinis.
                new InpatientClinicalContextService(c),
                ControllerTestHarness.BuatLoggerService(actorUserId))
                .DenganPengguna(actorUserId);

        private static PatientIntegratedProgressNoteController BuatControllerCatatanTerpadu(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientIntegratedProgressNoteController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                Keutuhan(c),
                // BE-RWI-053. Controller CPPT kini juga memegang verifikasi DPJP, sehingga
                // service verifikasinya ikut disuntikkan pada uji.
                new CpptVerificationService(c, new InpatientClinicalContextService(c)))
                .DenganPengguna(actorUserId);

        private static ClinicalNoteAddendumService Koreksi(ApplicationDbContext c) =>
            new(c, Keutuhan(c));

        /// <summary>
        /// Menyiapkan satu catatan dokter rawat inap yang sudah lengkap dan siap difinalkan.
        /// </summary>
        /// <param name="context">Konteks basis data uji.</param>
        /// <param name="k">Rangkaian data rawat inap yang menaungi catatan.</param>
        /// <param name="penulisUserId">
        /// Penulis catatan. Sengaja dapat dibedakan dari aktor yang menekan tombol selesai,
        /// karena <c>RWI-AC-157</c> menuntut penanda tangan adalah penulisnya.
        /// </param>
        private static TrxDoctorConsultation SiapkanCatatanSiapFinal(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k,
            Guid penulisUserId)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var catatan = new TrxDoctorConsultation
            {
                ConsultationNumber = $"CON-{pembeda}",
                EncounterId = k.EncounterId,
                PatientId = k.PatientId,
                DoctorId = k.DoctorMasterId,
                ServiceUnitId = k.ServiceUnitId,
                InpEpisodeId = k.EpisodeId,
                ConsultationDateTime = DateTime.UtcNow,
                ClinicalDateTime = DateTime.UtcNow.AddHours(-1),
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                StartedAt = DateTime.UtcNow.AddMinutes(-30),
                StartedByUserId = penulisUserId,
                IsActive = true,
                CreateBy = penulisUserId,

                Subjective = "Sesak berkurang",
                Objective = "Napas 20 kali per menit",
                Assessment = "Perbaikan klinis",
                Plan = "Lanjutkan terapi",
                HasPrimaryDiagnosis = true,
                PrimaryDiagnosisText = "Bronkopneumonia",
                DiagnosisCount = 1
            };

            context.Set<TrxDoctorConsultation>().Add(catatan);
            context.SaveChanges();

            return catatan;
        }

        // =====================================================================
        // Kriteria 1 — finalisasi mendaftarkan dokumen sebagai tertanda tangan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-038 AC 1</c> — memfinalkan catatan dokter mendaftarkannya sebagai dokumen
        /// tertanda tangan, dengan <b>penulis catatan</b> sebagai penanda tangannya.
        /// </summary>
        /// <remarks>
        /// Penulis dan aktor sengaja dibuat berbeda. Bila penanda tangan diambil dari aktor,
        /// catatan yang diselesaikan supervisor akan tercatat ditandatangani supervisor, dan
        /// tanggung jawab atas isi catatan berpindah diam-diam.
        /// </remarks>
        [Fact]
        public async Task FinalisasiCatatanDokter_MendaftarkanDokumenTertandaTanganAtasNamaPenulis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var supervisor = RekamMedisTestData.BuatPengguna(context, "supervisor");
            var catatan = SiapkanCatatanSiapFinal(context, k, k.DokterUserId);

            var hasil = await Finalisasi(context).FinalizeAsync(
                catatan.Id,
                new FinalizeDoctorConsultationRequest(),
                supervisor.Id);

            Assert.True(hasil.IsSuccess, hasil.ErrorMessage);

            using var verifikasi = database.CreateContext();

            var keutuhan = verifikasi.Set<MrcClinicalDocumentIntegrity>()
                .Single(x => x.DocumentKind == ClinicalDocumentKind.Consultation
                             && x.DocumentId == catatan.Id);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(k.DokterUserId, keutuhan.AuthorUserId);
            Assert.Equal(k.DokterUserId, keutuhan.SignedByUserId);
            Assert.NotEqual(supervisor.Id, keutuhan.SignedByUserId);
            Assert.NotNull(keutuhan.SignedAt);
            Assert.NotNull(keutuhan.LockedAt);
            Assert.Equal(ClinicalDocumentLockTrigger.AuthorSigned, keutuhan.LockTrigger);
        }

        // =====================================================================
        // Kriteria 2 — pendaftaran gagal, finalisasi ikut batal
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-038 AC 2</c> — ketika pendaftaran keutuhan gagal, finalisasi ikut
        /// dibatalkan dan catatan tetap berstatus belum selesai.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kegagalannya dipaksa dengan cara yang paling mendekati kejadian nyata: sudah ada
        /// baris keutuhan untuk dokumen itu yang <b>ditandai terhapus</b>. Baris terhapus tidak
        /// terlihat oleh pencarian aplikasi, sehingga pendaftaran mengira dokumen belum
        /// terdaftar dan menyisipkan baris baru — lalu unique index basis data menolaknya.
        /// </para>
        /// <para>
        /// Yang dibuktikan bukan pesan galatnya, melainkan <b>keadaan sesudahnya</b>: catatan
        /// tetap <c>InProgress</c>, tidak punya waktu selesai, dan tidak punya baris keutuhan
        /// yang berlaku. Bila pendaftaran berada di luar transaksi, catatan akan tertinggal
        /// <c>Completed</c> dan uji ini gagal.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task PendaftaranKeutuhanGagal_FinalisasiCatatanDokterIkutDibatalkan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = SiapkanCatatanSiapFinal(context, k, k.DokterUserId);

            context.Set<MrcClinicalDocumentIntegrity>().Add(new MrcClinicalDocumentIntegrity
            {
                DocumentKind = ClinicalDocumentKind.Consultation,
                DocumentId = catatan.Id,
                PatientId = k.PatientId,
                EncounterId = k.EncounterId,
                AuthorUserId = k.DokterUserId,
                IntegrityStatus = ClinicalDocumentIntegrityStatus.Draft,
                IsDelete = true
            });
            context.SaveChanges();

            await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
                Finalisasi(context).FinalizeAsync(
                    catatan.Id,
                    new FinalizeDoctorConsultationRequest(),
                    k.DokterUserId));

            using var verifikasi = database.CreateContext();

            var sesudah = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == catatan.Id);

            Assert.Equal(DoctorConsultationStatus.InProgress, sesudah.ConsultationStatus);
            Assert.Null(sesudah.CompletedAt);
            Assert.Null(sesudah.CompletedByUserId);

            Assert.Empty(verifikasi.Set<MrcClinicalDocumentIntegrity>()
                .Where(x => x.DocumentKind == ClinicalDocumentKind.Consultation
                            && x.DocumentId == catatan.Id
                            && !x.IsDelete));
        }

        // =====================================================================
        // Kriteria 3 — kajian medis dan tindakan berperilaku sama
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-038 AC 3</c> — menyelesaikan kajian medis mendaftarkannya sebagai dokumen
        /// tertanda tangan, bukan sekadar sebagai konsep.
        /// </summary>
        /// <remarks>
        /// Bedanya menentukan. Dokumen berstatus konsep justru <b>ditolak</b> mesin koreksi
        /// dengan arahan menyunting langsung, padahal kajian yang sudah selesai memang tidak
        /// dapat disunting lagi. Mendaftarkannya sebagai konsep berarti membiarkan keadaan
        /// buntu yang sama.
        /// </remarks>
        [Fact]
        public async Task PenyelesaianKajianMedis_MendaftarkanDokumenTertandaTangan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var buat = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.MedicalInitial,
                    ChiefComplaint = "Demam empat hari",
                    CurrentIllnessHistory = "Demam naik turun disertai menggigil"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(buat));

            var kajian = context.Set<TrxPatientAssessment>()
                .Single(x => x.AssessmentType == PatientAssessmentType.MedicalInitial);

            var selesai = await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(selesai));

            using var verifikasi = database.CreateContext();

            var keutuhan = verifikasi.Set<MrcClinicalDocumentIntegrity>()
                .Single(x => x.DocumentKind == ClinicalDocumentKind.Assessment
                             && x.DocumentId == kajian.Id);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(ClinicalDocumentLockTrigger.AuthorSigned, keutuhan.LockTrigger);
            Assert.NotNull(keutuhan.SignedAt);
            Assert.NotNull(keutuhan.SignedByUserId);
        }

        /// <summary>
        /// <c>BE-RWI-038 AC 3</c> — menandai tindakan sudah dikerjakan mendaftarkannya sebagai
        /// dokumen tertanda tangan.
        /// </summary>
        [Fact]
        public async Task PenandaanTindakanDikerjakan_MendaftarkanDokumenTertandaTangan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = TindakanTestData.BuatCatatanInduk(
                context, k.EncounterId, k.PatientId, k.DoctorMasterId, k.ServiceUnitId, k.EpisodeId);

            var tindakan = TindakanTestData.BuatTindakan(
                context, k.EncounterId, catatan.Id, k.PatientId, k.DoctorMasterId,
                k.ServiceUnitId, k.EpisodeId);

            var hasil = await BuatControllerTindakan(context, k.DokterUserId)
                .ExecuteProcedure(tindakan.Id, new ExecutePatientProcedureRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();

            var keutuhan = verifikasi.Set<MrcClinicalDocumentIntegrity>()
                .Single(x => x.DocumentKind == ClinicalDocumentKind.Procedure
                             && x.DocumentId == tindakan.Id);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(k.DokterUserId, keutuhan.SignedByUserId);

            var sesudah = verifikasi.Set<TrxPatientProcedure>().Single(x => x.Id == tindakan.Id);
            Assert.Equal(PatientProcedureStatus.Completed, sesudah.ProcedureStatus);
        }

        // =====================================================================
        // Kriteria 4 dan 5 — koreksi diterima pada dokumen final, ditolak pada konsep
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-038 AC 4</c> — koreksi pada catatan dokter yang sudah final diterima, dan
        /// isi catatan aslinya tidak berubah satu huruf pun.
        /// </summary>
        [Fact]
        public async Task KoreksiPadaCatatanDokterYangSudahFinal_Diterima()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = SiapkanCatatanSiapFinal(context, k, k.DokterUserId);

            var finalisasi = await Finalisasi(context).FinalizeAsync(
                catatan.Id, new FinalizeDoctorConsultationRequest(), k.DokterUserId);

            Assert.True(finalisasi.IsSuccess, finalisasi.ErrorMessage);

            var (hasil, addendum) = await Koreksi(context).CreateAsync(
                ClinicalDocumentKind.Consultation,
                catatan.Id,
                k.DokterUserId,
                actorHasSubstituteAuthority: false,
                addendumText: "Frekuensi napas seharusnya 24 kali per menit.",
                correctionReason: "Salah ketik angka",
                deviceInfo: "uji",
                ipAddress: "127.0.0.1",
                nowUtc: DateTime.UtcNow);

            Assert.True(hasil.IsAllowed, hasil.ErrorMessage);
            Assert.NotNull(addendum);
            Assert.Equal(1, addendum!.Sequence);
            Assert.False(addendum.IsSubstituteAuthor);

            using var verifikasi = database.CreateContext();

            // Isi catatan aslinya tidak tersentuh. Itulah inti addendum.
            var sesudah = verifikasi.Set<TrxDoctorConsultation>().Single(x => x.Id == catatan.Id);
            Assert.Equal("Napas 20 kali per menit", sesudah.Objective);

            var keutuhan = verifikasi.Set<MrcClinicalDocumentIntegrity>()
                .Single(x => x.DocumentKind == ClinicalDocumentKind.Consultation
                             && x.DocumentId == catatan.Id);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(1, keutuhan.AddendumCount);
        }

        /// <summary>
        /// <c>BE-RWI-038 AC 5</c>, <c>VAL-DOK-32</c> — koreksi pada catatan yang <b>belum</b>
        /// final ditolak <c>400</c> beserta arahan menyunting langsung.
        /// </summary>
        /// <remarks>
        /// Kode <c>400</c>, bukan <c>403</c>. Pengguna ini memang berwenang mengoreksi
        /// catatannya sendiri; yang salah adalah waktunya. Menjawab <c>403</c> akan membuatnya
        /// mengira dirinya tidak berhak, lalu menghubungi kepala unit tanpa perlu.
        /// </remarks>
        [Fact]
        public async Task KoreksiPadaCatatanDokterYangBelumFinal_Ditolak400BesertaArahanMenyunting()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = SiapkanCatatanSiapFinal(context, k, k.DokterUserId);

            var (hasil, addendum) = await Koreksi(context).CreateAsync(
                ClinicalDocumentKind.Consultation,
                catatan.Id,
                k.DokterUserId,
                actorHasSubstituteAuthority: false,
                addendumText: "Koreksi terlalu dini.",
                correctionReason: "Salah ketik angka",
                deviceInfo: "uji",
                ipAddress: "127.0.0.1",
                nowUtc: DateTime.UtcNow);

            Assert.False(hasil.IsAllowed);
            Assert.Null(addendum);
            Assert.Equal(400, hasil.StatusCode);
            Assert.Contains("belum final", hasil.ErrorMessage);
            Assert.Contains("Perbaiki langsung pada catatannya", hasil.ErrorMessage);
        }

        // =====================================================================
        // Kriteria 6 — catatan terpadu tidak berubah perilakunya
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-038 AC 6</c> — catatan terpadu tetap berperilaku persis seperti
        /// sebelumnya: terdaftar sebagai <b>konsep</b> saat dibuat, dan masih boleh disunting.
        /// </summary>
        /// <remarks>
        /// Uji regresi. Task ini menyentuh service yang dipakai bersama catatan terpadu, dan
        /// catatan terpadu adalah satu-satunya jenis yang aturan keutuhannya sudah berjalan di
        /// produksi. Perubahan diam-diam pada perilakunya akan mengunci catatan perawat yang
        /// baru diketik.
        /// </remarks>
        [Fact]
        public async Task CatatanTerpadu_TetapTerdaftarSebagaiKonsepDanMasihBolehDisunting()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerCatatanTerpadu(context, k.DokterUserId)
                .CreateProgressNote(new CreatePatientIntegratedProgressNoteRequest
                {
                    PatientId = k.PatientId,
                    EncounterId = k.EncounterId,
                    ProfessionType = "Doctor",
                    ProviderUserId = k.DokterUserId,
                    SubjectiveSummary = "Pasien mengeluh nyeri ringan"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();

            var catatan = verifikasi.Set<TrxPatientIntegratedProgressNote>().Single();

            var keutuhan = verifikasi.Set<MrcClinicalDocumentIntegrity>()
                .Single(x => x.DocumentKind == ClinicalDocumentKind.ProgressNote
                             && x.DocumentId == catatan.Id);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, keutuhan.IntegrityStatus);
            Assert.Null(keutuhan.SignedAt);
            Assert.Null(keutuhan.LockTrigger);

            var penjaga = await Keutuhan(verifikasi).EnsureMutableAsync(
                ClinicalDocumentKind.ProgressNote, catatan.Id);

            Assert.True(penjaga.IsAllowed);
        }
    }
}
