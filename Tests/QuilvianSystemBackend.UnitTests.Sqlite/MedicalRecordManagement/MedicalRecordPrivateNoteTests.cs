using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-15` — endpoint catatan pribadi klinisi.
    ///
    /// Menutup uji penerimaan `AT-RM-16` dan melengkapi `AT-RM-37`, beserta ketiga acceptance
    /// criteria `BE-15`:
    /// <list type="number">
    /// <item>alasan diminta **walaupun** pasien punya kunjungan aktif;</item>
    /// <item>jejak tercatat dengan `AccessScope = PrivateNote`;</item>
    /// <item>memakai izin terpisah, bukan izin baca biasa.</item>
    /// </list>
    ///
    /// Seluruh data di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </summary>
    public class MedicalRecordPrivateNoteTests
    {
        private const string IsiCatatanPribadi =
            "Dugaan kekerasan dalam rumah tangga. Belum dikonfirmasi, jangan disampaikan ke pengantar.";

        private static MedicalRecordController BuatController(
            ApplicationDbContext context,
            Guid userId)
        {
            var controller = new MedicalRecordController(
                context,
                ControllerTestHarness.BuatLoggerService(userId),
                new MedicalRecordAccessAuditService(context),
                new MedicalRecordTimelineService(context));

            return controller.DenganPengguna(userId);
        }

        private static TrxPatientIntegratedProgressNote BuatCppt(
            ApplicationDbContext context,
            Guid patientId,
            Guid encounterId,
            Guid penulisId,
            string? catatanPribadi)
        {
            var cppt = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                ProfessionType = "Doctor",
                ProfessionName = "Dokter",
                ProviderUserId = penulisId,
                ProviderDisplayNameSnapshot = "dr. Penulis Uji",
                SubjectiveSummary = "Pasien mengeluh nyeri kepala.",
                PrivateNote = catatanPribadi,
                NoteDateTime = DateTime.UtcNow.AddDays(-1)
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(cppt);
            context.SaveChanges();
            return cppt;
        }

        private static MstMedicalRecordAccessPurpose BuatKeperluan(ApplicationDbContext context)
        {
            var keperluan = new MstMedicalRecordAccessPurpose
            {
                PurposeCode = $"TELAAH-{Guid.NewGuid().ToString("N")[..6]}",
                PurposeName = "Penelaahan mutu rekam medis",
                IsFreeTextRequired = false,
                RequiresReview = true,
                IsActive = true
            };

            context.Set<MstMedicalRecordAccessPurpose>().Add(keperluan);
            context.SaveChanges();
            return keperluan;
        }

        private static T Isi<T>(IActionResult hasil)
        {
            var objek = Assert.IsType<OkObjectResult>(hasil);
            var body = Assert.IsType<ApiResponse<T>>(objek.Value);
            Assert.True(body.Success);
            Assert.NotNull(body.Data);
            return body.Data!;
        }

        // =====================================================================
        // Acceptance criteria 1 — alasan selalu wajib
        // =====================================================================

        /// <summary>
        /// `AT-RM-16`: membuka catatan pribadi pada pasien yang **punya** kunjungan aktif tetap
        /// diminta alasan.
        ///
        /// Inilah inti `RM-DEC-022`, dan ia sengaja berbeda dari seluruh isi rekam medis lain.
        /// Untuk isi biasa, dokter yang sedang merawat pasien tidak perlu memberi alasan. Untuk
        /// catatan pribadi, ia tetap perlu — karena kolom itu ditulis rekan sejawatnya dengan
        /// harapan bersifat pribadi.
        /// </summary>
        [Fact]
        public async Task CatatanPribadi_TetapMenuntutAlasanWalaupunPasienSedangDirawat()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            // Kunjungan berstatus Registered berarti pasien SEDANG dirawat.
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var pembaca = RekamMedisTestData.BuatPengguna(context, "pembaca");
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, penulis.Id, IsiCatatanPribadi);

            var controller = BuatController(context, pembaca.Id);

            // Pembanding: isi rekam medis biasa pada pasien yang sama TIDAK diminta alasan.
            var detailBiasa = await controller.GetDocumentDetail(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id);
            Assert.Equal(StatusCodes.Status200OK, ControllerTestHarness.KodeStatus(detailBiasa));

            // Catatan pribadi pada pasien yang sama: ditolak karena tanpa keperluan akses.
            var hasil = await controller.GetPrivateNote(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id);

            Assert.Equal(StatusCodes.Status400BadRequest, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("catatan pribadi", ControllerTestHarness.Pesan(hasil)!,
                StringComparison.OrdinalIgnoreCase);

            var objek = Assert.IsType<ObjectResult>(hasil);
            var body = Assert.IsType<ApiResponse<object>>(objek.Value);
            Assert.Null(body.Data);
        }

        /// <summary>
        /// Dengan keperluan akses yang sah, catatan pribadi benar-benar terbuka.
        ///
        /// Ini sisi lain `RM-DEC-022` yang sama pentingnya: kolom ini tidak boleh menjadi ruang
        /// gelap yang tidak dapat diperiksa siapa pun. Ia dapat dibuka, tetapi selalu dengan
        /// alasan dan selalu tercatat.
        /// </summary>
        [Fact]
        public async Task CatatanPribadi_TerbukaBilaKeperluanDiisi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var pembaca = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var keperluan = BuatKeperluan(context);
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, penulis.Id, IsiCatatanPribadi);

            var controller = BuatController(context, pembaca.Id);

            var hasil = await controller.GetPrivateNote(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id,
                accessPurposeId: keperluan.Id);

            var isi = Isi<MedicalRecordPrivateNoteResponse>(hasil);

            Assert.True(isi.HasPrivateNote);
            Assert.Equal(IsiCatatanPribadi, isi.PrivateNote);
            Assert.Equal(cppt.Id, isi.DocumentId);
            Assert.Equal(penulis.Id, isi.AuthorUserId);
            Assert.Equal("dr. Penulis Uji", isi.AuthorName);

            // Pembukaan catatan pribadi SELALU akses beralasan, tidak pernah akses rawatan.
            Assert.Equal(MedicalRecordAccessType.ReasonedAccess, isi.Access.AccessType);
            Assert.True(isi.Access.IsFlaggedForReview);
        }

        // =====================================================================
        // Acceptance criteria 2 — cakupan jejak
        // =====================================================================

        /// <summary>
        /// Jejak tercatat dengan `AccessScope = PrivateNote`.
        ///
        /// Cakupan yang benar bukan detail administratif: rekap tinjauan menghitung pembukaan
        /// catatan pribadi secara terpisah. Bila cakupannya tercatat sebagai detail dokumen
        /// biasa, angka itu menjadi tidak bermakna.
        /// </summary>
        [Fact]
        public async Task CatatanPribadi_JejakTercatatDenganCakupanPrivateNote()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var pembaca = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var keperluan = BuatKeperluan(context);
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, penulis.Id, IsiCatatanPribadi);

            var controller = BuatController(context, pembaca.Id);

            await controller.GetPrivateNote(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id,
                accessPurposeId: keperluan.Id);

            using var konteksBaca = database.CreateContext();
            var jejak = Assert.Single(
                await konteksBaca.Set<MrcAccessLog>().AsNoTracking().ToListAsync());

            Assert.Equal(MedicalRecordAccessScope.PrivateNote, jejak.AccessScope);
            Assert.Equal(MedicalRecordAccessType.ReasonedAccess, jejak.AccessType);
            Assert.Equal(pembaca.Id, jejak.UserId);
            Assert.Equal(keperluan.Id, jejak.AccessPurposeId);
            Assert.True(jejak.IsFlaggedForReview);

            // Pasien memang sedang dirawat, dan itu tetap tercatat apa adanya — yang berubah
            // hanya keharusan memberi alasan.
            Assert.True(jejak.HasActiveEncounter);
        }

        /// <summary>
        /// Rekap tinjauan menghitung pembukaan catatan pribadi secara terpisah.
        ///
        /// Diperiksa lewat `MedicalRecordAccessReviewService` milik `BE-12`, supaya terbukti
        /// bahwa cakupan jejak yang ditulis `BE-15` benar-benar terbaca di layar tinjauan.
        /// </summary>
        [Fact]
        public async Task PembukaanCatatanPribadi_TerhitungTerpisahPadaRekapTinjauan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var pembaca = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var keperluan = BuatKeperluan(context);
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, penulis.Id, IsiCatatanPribadi);

            var controller = BuatController(context, pembaca.Id);

            // Satu pembukaan berkas biasa, satu pembukaan catatan pribadi.
            await controller.GetSummary(konteks.PatientId);
            await controller.GetPrivateNote(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id,
                accessPurposeId: keperluan.Id);

            using var konteksBaca = database.CreateContext();
            var rekap = await new MedicalRecordAccessReviewService(konteksBaca).SummaryAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.Equal(2, rekap.TotalAkses);
            Assert.Equal(1, rekap.AksesRawatan);
            Assert.Equal(1, rekap.AksesBeralasan);
            Assert.Equal(1, rekap.AksesCatatanPribadi);
        }

        // =====================================================================
        // Acceptance criteria 3 — izin terpisah
        // =====================================================================

        /// <summary>
        /// Endpoint ini memakai izin terpisah `MedicalRecord : ReadPrivateNote`, bukan izin baca
        /// biasa `MedicalRecord : Read`.
        ///
        /// Diperiksa langsung pada atributnya, karena uji ini memanggil controller tanpa melewati
        /// lapisan hak akses. Yang dibuktikan di sini adalah **endpointnya benar-benar terdaftar
        /// dengan izin tersendiri** — sehingga seseorang dapat diberi hak membaca seluruh berkas
        /// rekam medis tanpa pernah dapat membuka catatan pribadi.
        ///
        /// Pelajaran `BE-06` berlaku di sini: satu endpoint hanya boleh punya satu
        /// `[AccessAction]`. Bila izin ini digabungkan ke endpoint detail dokumen, hak
        /// `ReadPrivateNote` tidak akan pernah terdaftar dan tidak dapat diberikan kepada siapa
        /// pun.
        /// </summary>
        [Fact]
        public void EndpointCatatanPribadi_MemakaiIzinTerpisah()
        {
            var metode = typeof(MedicalRecordController)
                .GetMethod(nameof(MedicalRecordController.GetPrivateNote))!;

            var tindakan = Assert.Single(
                metode.GetCustomAttributes(typeof(AccessActionAttribute), false)
                      .Cast<AccessActionAttribute>());

            Assert.Equal("ReadPrivateNote", tindakan.ActionName);

            var izin = Assert.Single(
                metode.GetCustomAttributes(typeof(AccessPermissionAttribute), false)
                      .Cast<AccessPermissionAttribute>());

            Assert.Equal(new object[] { "MedicalRecord", "ReadPrivateNote" }, izin.Arguments);

            // Endpoint detail dokumen tetap memakai izin baca biasa — keduanya harus berbeda.
            var metodeDetail = typeof(MedicalRecordController)
                .GetMethod(nameof(MedicalRecordController.GetDocumentDetail))!;

            var izinDetail = Assert.Single(
                metodeDetail.GetCustomAttributes(typeof(AccessPermissionAttribute), false)
                            .Cast<AccessPermissionAttribute>());

            Assert.Equal(new object[] { "MedicalRecord", "Read" }, izinDetail.Arguments);
        }

        // =====================================================================
        // Batas dan keadaan khusus
        // =====================================================================

        /// <summary>
        /// Jenis dokumen yang memang tidak punya kolom catatan pribadi dijawab `404` dengan
        /// keterangan yang jelas, dan **tanpa** menghasilkan jejak akses.
        ///
        /// Bedanya penting: pembaca harus tahu bahwa catatan itu memang tidak ada, bukan
        /// menyangka ada sesuatu yang disembunyikan darinya. Jejaknya tidak dicatat karena ini
        /// permintaan yang keliru bentuknya, bukan percobaan membuka berkas — mencatatnya akan
        /// mengotori angka tinjauan.
        /// </summary>
        [Fact]
        public async Task JenisDokumenTanpaCatatanPribadi_Dijawab404TanpaJejak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var pembaca = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var keperluan = BuatKeperluan(context);

            var controller = BuatController(context, pembaca.Id);

            var hasil = await controller.GetPrivateNote(
                konteks.PatientId, ClinicalDocumentKind.Allergy, Guid.NewGuid(),
                accessPurposeId: keperluan.Id);

            Assert.Equal(StatusCodes.Status404NotFound, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("tidak memiliki catatan pribadi", ControllerTestHarness.Pesan(hasil)!);

            using var konteksBaca = database.CreateContext();
            Assert.Empty(await konteksBaca.Set<MrcAccessLog>().AsNoTracking().ToListAsync());
        }

        /// <summary>
        /// Dokumen yang catatan pribadinya memang kosong dibedakan dari dokumen yang catatannya
        /// disembunyikan.
        ///
        /// Tanpa pembedaan ini, pembaca tidak punya cara tahu mana yang benar — dan itu justru
        /// mendorongnya mencari-cari lewat jalur lain.
        /// </summary>
        [Fact]
        public async Task DokumenTanpaIsiCatatanPribadi_DitandaiKosongBukanDisembunyikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var pembaca = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var keperluan = BuatKeperluan(context);
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, penulis.Id, catatanPribadi: null);

            var controller = BuatController(context, pembaca.Id);

            var hasil = await controller.GetPrivateNote(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id,
                accessPurposeId: keperluan.Id);

            var isi = Isi<MedicalRecordPrivateNoteResponse>(hasil);

            Assert.False(isi.HasPrivateNote);
            Assert.Null(isi.PrivateNote);
            Assert.Contains("tidak memuat catatan pribadi", ControllerTestHarness.Pesan(hasil)!);

            // Pembukaannya tetap tercatat, walaupun tidak ada isi yang dikembalikan.
            using var konteksBaca = database.CreateContext();
            var jejak = Assert.Single(
                await konteksBaca.Set<MrcAccessLog>().AsNoTracking().ToListAsync());
            Assert.Equal(MedicalRecordAccessScope.PrivateNote, jejak.AccessScope);
        }

        /// <summary>
        /// Catatan pribadi milik pasien lain dijawab `404`.
        /// </summary>
        [Fact]
        public async Task CatatanPribadiPasienLain_Dijawab404()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pasienA = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var pasienB = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var pembaca = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var keperluan = BuatKeperluan(context);

            var cpptMilikB = BuatCppt(
                context, pasienB.PatientId, pasienB.EncounterId, penulis.Id, IsiCatatanPribadi);

            var controller = BuatController(context, pembaca.Id);

            var hasil = await controller.GetPrivateNote(
                pasienA.PatientId, ClinicalDocumentKind.ProgressNote, cpptMilikB.Id,
                accessPurposeId: keperluan.Id);

            Assert.Equal(StatusCodes.Status404NotFound, ControllerTestHarness.KodeStatus(hasil));

            var objek = Assert.IsType<NotFoundObjectResult>(hasil);
            var body = Assert.IsType<ApiResponse<object>>(objek.Value);
            Assert.Null(body.Data);
        }

        /// <summary>
        /// Pasien yang kunjungannya sudah selesai tetap tunduk aturan yang sama: tanpa keperluan
        /// akses, ditolak.
        /// </summary>
        [Fact]
        public async Task PasienTanpaKunjunganBerjalan_TetapDitolakTanpaKeperluan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");
            var pembaca = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, penulis.Id, IsiCatatanPribadi);

            var controller = BuatController(context, pembaca.Id);

            var hasil = await controller.GetPrivateNote(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id);

            Assert.Equal(StatusCodes.Status400BadRequest, ControllerTestHarness.KodeStatus(hasil));

            using var konteksBaca = database.CreateContext();
            Assert.Empty(await konteksBaca.Set<MrcAccessLog>().AsNoTracking().ToListAsync());
        }
    }
}
