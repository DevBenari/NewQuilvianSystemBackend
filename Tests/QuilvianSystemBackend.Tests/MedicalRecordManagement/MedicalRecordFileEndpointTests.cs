using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Text.Json;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-14` — endpoint berkas rekam medis.
    ///
    /// Menutup uji penerimaan `AT-RM-09`, `AT-RM-32`, dan `AT-RM-37`, beserta seluruh
    /// acceptance criteria `BE-14`:
    /// <list type="number">
    /// <item>setiap permintaan melewati pencatatan jejak lebih dulu;</item>
    /// <item>status keutuhan ikut dikembalikan untuk jenis dokumen yang sudah tunduk aturan;</item>
    /// <item>jenis dokumen yang belum tunduk ditandai jelas;</item>
    /// <item>`PrivateNote` TIDAK ADA pada respons mana pun di endpoint ini.</item>
    /// </list>
    ///
    /// Seluruh data di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </summary>
    public class MedicalRecordFileEndpointTests
    {
        private const string CatatanPribadiUji = "RAHASIA-UJI-jangan-pernah-bocor-ke-respons";

        // =====================================================================
        // Penyiapan
        // =====================================================================

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
            string? catatanPribadi = null)
        {
            var cppt = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                ProfessionType = "Doctor",
                ProfessionName = "Dokter",
                ProviderDisplayNameSnapshot = "dr. Uji",
                SubjectiveSummary = "Pasien mengeluh nyeri kepala.",
                PlanSummary = "Observasi dan analgesik.",
                PrivateNote = catatanPribadi,
                NoteDateTime = DateTime.UtcNow.AddDays(-1)
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(cppt);
            context.SaveChanges();
            return cppt;
        }

        private static TrxPatientAllergy BuatAlergi(
            ApplicationDbContext context,
            Guid patientId,
            Guid encounterId)
        {
            var alergi = new TrxPatientAllergy
            {
                AllergyRecordNumber = $"ALG-{Guid.NewGuid():N}"[..20],
                PatientId = patientId,
                EncounterId = encounterId,
                AllergenName = "Amoksisilin",
                AllergenGroupName = "Antibiotik",
                ReactionType = "Ruam",
                Severity = PatientAllergySeverity.Severe,
                IsLifeThreatening = true,
                AllergyStatus = PatientAllergyStatus.Active,
                ReportedDateTime = DateTime.UtcNow.AddDays(-2)
            };

            context.Set<TrxPatientAllergy>().Add(alergi);
            context.SaveChanges();
            return alergi;
        }

        private static MstMedicalRecordAccessPurpose BuatKeperluan(ApplicationDbContext context)
        {
            var keperluan = new MstMedicalRecordAccessPurpose
            {
                PurposeCode = $"KLAIM-{Guid.NewGuid().ToString("N")[..6]}",
                PurposeName = "Penyelesaian klaim",
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
        // Acceptance criteria 1 — jejak lebih dulu
        // =====================================================================

        /// <summary>
        /// Setiap permintaan melewati pencatatan jejak lebih dulu.
        ///
        /// Ketiga endpoint yang membuka berkas dipanggil sekali, dan hasilnya tepat tiga baris
        /// jejak dengan cakupan yang berbeda-beda. Cakupan yang benar penting karena pembukaan
        /// catatan pribadi harus dapat dihitung terpisah saat ditinjau (RM-DEC-022).
        /// </summary>
        [Fact]
        public async Task SetiapPembukaan_MencatatJejakLebihDulu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId);

            var controller = BuatController(context, dokter.Id);

            Assert.IsType<OkObjectResult>(await controller.GetSummary(konteks.PatientId));
            Assert.IsType<OkObjectResult>(await controller.GetTimeline(konteks.PatientId));
            Assert.IsType<OkObjectResult>(await controller.GetDocumentDetail(
                konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id));

            using var konteksBaca = database.CreateContext();
            var jejak = await konteksBaca.Set<TrxMedicalRecordAccessLog>()
                .AsNoTracking()
                .Where(x => x.PatientId == konteks.PatientId)
                .ToListAsync();

            Assert.Equal(3, jejak.Count);
            Assert.All(jejak, x => Assert.Equal(dokter.Id, x.UserId));
            Assert.Contains(jejak, x => x.AccessScope == MedicalRecordAccessScope.Summary);
            Assert.Contains(jejak, x => x.AccessScope == MedicalRecordAccessScope.Timeline);
            Assert.Contains(jejak, x => x.AccessScope == MedicalRecordAccessScope.DocumentDetail);

            // Pasien sedang dirawat, jadi seluruhnya akses rawatan dan tidak perlu ditinjau.
            Assert.All(jejak, x => Assert.Equal(MedicalRecordAccessType.RoutineCare, x.AccessType));
            Assert.All(jejak, x => Assert.False(x.IsFlaggedForReview));
        }

        /// <summary>
        /// Akses yang ditolak tidak mengembalikan isi rekam medis sama sekali.
        ///
        /// Pasien tanpa kunjungan berjalan menuntut keperluan akses. Tanpa itu permintaan
        /// ditolak, dan yang penting: tidak ada isi yang ikut terbawa pada balasan penolakan.
        /// </summary>
        [Fact]
        public async Task AksesDitolak_IsiRekamMedisTidakDikembalikan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            BuatCppt(context, konteks.PatientId, konteks.EncounterId);

            var controller = BuatController(context, dokter.Id);

            var hasil = await controller.GetTimeline(konteks.PatientId);

            Assert.Equal(StatusCodes.Status400BadRequest, ControllerTestHarness.KodeStatus(hasil));

            var objek = Assert.IsType<ObjectResult>(hasil);
            var body = Assert.IsType<ApiResponse<object>>(objek.Value);
            Assert.False(body.Success);
            Assert.Null(body.Data);

            // Ditolak berarti tidak ada baris jejak keberhasilan yang terbentuk.
            using var konteksBaca = database.CreateContext();
            Assert.Empty(await konteksBaca.Set<TrxMedicalRecordAccessLog>().AsNoTracking().ToListAsync());
        }

        /// <summary>
        /// Akses beralasan yang sah tetap dilayani, dan pengguna diberi tahu bahwa aksesnya
        /// akan ditelaah.
        ///
        /// Keterangan itu sengaja dikembalikan pada balasan: pengguna berhak mengetahuinya
        /// sekarang, bukan baru saat ditanya unit rekam medis.
        /// </summary>
        [Fact]
        public async Task AksesBeralasan_DilayaniDanPenggunaDiberiTahuAkanDitelaah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(
                context, EncounterStatus.Completed);
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.klaim");
            var keperluan = BuatKeperluan(context);
            BuatCppt(context, konteks.PatientId, konteks.EncounterId);

            var controller = BuatController(context, petugas.Id);

            var hasil = await controller.GetTimeline(
                konteks.PatientId, accessPurposeId: keperluan.Id);

            var isi = Isi<MedicalRecordTimelineResponse>(hasil);

            Assert.Equal(MedicalRecordAccessType.ReasonedAccess, isi.Access.AccessType);
            Assert.Equal("Akses Beralasan", isi.Access.AccessTypeName);
            Assert.False(isi.Access.HasActiveEncounter);
            Assert.True(isi.Access.IsFlaggedForReview);
            Assert.NotNull(isi.Access.AccessLogId);
        }

        // =====================================================================
        // Acceptance criteria 2 dan 3 — status keutuhan
        // =====================================================================

        /// <summary>
        /// Status keutuhan ikut dikembalikan untuk jenis dokumen yang sudah tunduk aturan.
        /// </summary>
        [Fact]
        public async Task StatusKeutuhan_IkutDikembalikanUntukJenisYangSudahTunduk()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId);

            await new ClinicalDocumentIntegrityService(context).RegisterAsync(
                ClinicalDocumentKind.ProgressNote, cppt.Id,
                konteks.PatientId, konteks.EncounterId, dokter.Id);
            await context.SaveChangesAsync();

            var controller = BuatController(context, dokter.Id);

            var detail = Isi<MedicalRecordDocumentDetailResponse>(
                await controller.GetDocumentDetail(
                    konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id));

            Assert.True(detail.IsIntegrityEnforced);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, detail.IntegrityStatus);
            Assert.Equal("Draf", detail.IntegrityStatusName);
            Assert.Equal(dokter.Id, detail.AuthorUserId);
        }

        /// <summary>
        /// `AT-RM-32`: jenis dokumen yang belum tunduk aturan keutuhan ditandai jelas, baik pada
        /// riwayat, detail dokumen, ringkasan, maupun daftar pilihan penyaring.
        ///
        /// Ini bukan hal kecil. Menampilkan alergi seolah-olah sudah terlindungi aturan keutuhan
        /// akan membuat pembacanya mempercayai dokumen yang sebenarnya masih dapat diubah bebas.
        /// </summary>
        [Fact]
        public async Task JenisYangBelumTundukAturanKeutuhan_DitandaiJelas()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            BuatCppt(context, konteks.PatientId, konteks.EncounterId);
            var alergi = BuatAlergi(context, konteks.PatientId, konteks.EncounterId);

            var controller = BuatController(context, dokter.Id);

            // 1) Pada riwayat.
            var riwayat = Isi<MedicalRecordTimelineResponse>(
                await controller.GetTimeline(konteks.PatientId));

            var barisAlergi = riwayat.Page.Items.Single(
                x => x.DocumentKind == ClinicalDocumentKind.Allergy);
            Assert.False(barisAlergi.IsIntegrityEnforced);
            Assert.Null(barisAlergi.IntegrityStatus);

            var barisCppt = riwayat.Page.Items.Single(
                x => x.DocumentKind == ClinicalDocumentKind.ProgressNote);
            Assert.True(barisCppt.IsIntegrityEnforced);

            // 2) Pada detail dokumen, termasuk pada pesan balasannya.
            var hasilDetail = await controller.GetDocumentDetail(
                konteks.PatientId, ClinicalDocumentKind.Allergy, alergi.Id);

            var detail = Isi<MedicalRecordDocumentDetailResponse>(hasilDetail);
            Assert.False(detail.IsIntegrityEnforced);
            Assert.Null(detail.IntegrityStatus);
            Assert.Contains("belum tunduk", ControllerTestHarness.Pesan(hasilDetail)!);

            // 3) Pada ringkasan berkas.
            var ringkasan = Isi<MedicalRecordSummaryResponse>(
                await controller.GetSummary(konteks.PatientId));

            Assert.Equal(13, ringkasan.DocumentCounts.Count);
            Assert.Single(ringkasan.DocumentCounts, x => x.IsIntegrityEnforced);
            Assert.True(ringkasan.DocumentCounts
                .Single(x => x.DocumentKind == ClinicalDocumentKind.ProgressNote)
                .IsIntegrityEnforced);
            Assert.False(ringkasan.DocumentCounts
                .Single(x => x.DocumentKind == ClinicalDocumentKind.Allergy)
                .IsIntegrityEnforced);

            // 4) Pada daftar pilihan penyaring.
            var metadata = Isi<MedicalRecordFilterMetadataResponse>(
                await controller.GetFilterMetadata());

            Assert.Equal(13, metadata.DocumentKinds.Count);
            Assert.Single(metadata.DocumentKinds, x => x.IsIntegrityEnforced);
        }

        // =====================================================================
        // Acceptance criteria 4 — PrivateNote tidak pernah ada
        // =====================================================================

        /// <summary>
        /// `AT-RM-37`: `PrivateNote` TIDAK ADA pada respons mana pun di endpoint ini.
        ///
        /// Dibuktikan dengan cara yang paling sulit dielakkan: seluruh balasan diubah menjadi
        /// teks JSON, lalu dicari isinya. Bila kolom itu bocor lewat jalur mana pun — bagian
        /// isi dokumen, judul, keterangan pendek, atau kolom yang tidak sengaja ikut — uji ini
        /// gagal.
        ///
        /// Yang tetap dikembalikan hanya PENANDA bahwa catatan pribadi itu ada. Penanda perlu,
        /// karena tanpanya tidak ada yang tahu ada sesuatu yang dapat dibuka lewat jalur sah
        /// (`BE-15`).
        /// </summary>
        [Fact]
        public async Task CatatanPribadi_TidakAdaPadaResponsManaPun()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var cppt = BuatCppt(context, konteks.PatientId, konteks.EncounterId, CatatanPribadiUji);

            var controller = BuatController(context, dokter.Id);

            var ringkasan = Isi<MedicalRecordSummaryResponse>(
                await controller.GetSummary(konteks.PatientId));
            var riwayat = Isi<MedicalRecordTimelineResponse>(
                await controller.GetTimeline(konteks.PatientId));
            var detail = Isi<MedicalRecordDocumentDetailResponse>(
                await controller.GetDocumentDetail(
                    konteks.PatientId, ClinicalDocumentKind.ProgressNote, cppt.Id));

            foreach (var balasan in new object[] { ringkasan, riwayat, detail })
            {
                var json = JsonSerializer.Serialize(balasan);
                Assert.DoesNotContain(CatatanPribadiUji, json, StringComparison.OrdinalIgnoreCase);
            }

            // Isi dokumen lain tetap terbaca — yang disembunyikan hanya catatan pribadinya.
            Assert.Contains(detail.Sections, x => x.Value.Contains("nyeri kepala"));

            // Penandanya ada, isinya tidak.
            Assert.True(detail.HasPrivateNote);
        }

        // =====================================================================
        // Batas kepemilikan dokumen dan pasien hasil penggabungan
        // =====================================================================

        /// <summary>
        /// Dokumen milik pasien lain dijawab `404`, bukan ditampilkan.
        ///
        /// Tanpa pemeriksaan ini, siapa pun yang berhak membuka rekam medis satu pasien dapat
        /// membaca dokumen pasien lain hanya dengan menebak id-nya.
        /// </summary>
        [Fact]
        public async Task DokumenMilikPasienLain_Dijawab404()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var pasienA = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var pasienB = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            var cpptMilikB = BuatCppt(context, pasienB.PatientId, pasienB.EncounterId);

            var controller = BuatController(context, dokter.Id);

            var hasil = await controller.GetDocumentDetail(
                pasienA.PatientId, ClinicalDocumentKind.ProgressNote, cpptMilikB.Id);

            Assert.Equal(StatusCodes.Status404NotFound, ControllerTestHarness.KodeStatus(hasil));
        }

        /// <summary>
        /// Pasien hasil penggabungan nomor rekam medis dijawab `409` disertai nomor
        /// penggantinya, dan riwayat sebagiannya TIDAK ditampilkan (`RM-CAP-007`).
        ///
        /// Perilaku ini berasal dari `BE-11` dan diperiksa lagi di sini karena kontrak endpoint
        /// menjanjikannya.
        /// </summary>
        [Fact]
        public async Task PasienHasilPenggabungan_Dijawab409TanpaRiwayatSebagian()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var lama = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var baru = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCppt(context, lama.PatientId, lama.EncounterId);

            var pasienLama = await context.Set<MstPatient>().FirstAsync(x => x.Id == lama.PatientId);
            var pasienBaru = await context.Set<MstPatient>().FirstAsync(x => x.Id == baru.PatientId);
            pasienLama.MergedToPatientId = pasienBaru.Id;
            await context.SaveChangesAsync();

            var controller = BuatController(context, dokter.Id);

            var hasil = await controller.GetTimeline(lama.PatientId);

            Assert.Equal(StatusCodes.Status409Conflict, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains(pasienBaru.MedicalRecordNumber, ControllerTestHarness.Pesan(hasil)!);

            var objek = Assert.IsType<ObjectResult>(hasil);
            var body = Assert.IsType<ApiResponse<object>>(objek.Value);
            Assert.Null(body.Data);
        }

        // =====================================================================
        // Ringkasan berkas dan daftar pilihan
        // =====================================================================

        /// <summary>
        /// `AT-RM-09` pada tingkat endpoint: ringkasan berkas memuat identitas pasien, alergi
        /// aktif, dan jumlah dokumen per jenis.
        ///
        /// Alergi yang mengancam jiwa didahulukan, karena harus terbaca lebih dulu oleh siapa
        /// pun yang membuka berkas.
        /// </summary>
        [Fact]
        public async Task RingkasanBerkas_MemuatIdentitasAlergiAktifDanJumlahDokumen()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");

            BuatCppt(context, konteks.PatientId, konteks.EncounterId);
            BuatCppt(context, konteks.PatientId, konteks.EncounterId);
            BuatAlergi(context, konteks.PatientId, konteks.EncounterId);

            var controller = BuatController(context, dokter.Id);

            var ringkasan = Isi<MedicalRecordSummaryResponse>(
                await controller.GetSummary(konteks.PatientId));

            Assert.Equal(konteks.PatientId, ringkasan.Patient.PatientId);
            Assert.False(string.IsNullOrWhiteSpace(ringkasan.Patient.MedicalRecordNumber));
            Assert.Equal("Pasien Uji", ringkasan.Patient.FullName);

            var alergi = Assert.Single(ringkasan.ActiveAllergies);
            Assert.Equal("Amoksisilin", alergi.AllergenName);
            Assert.Equal("Berat", alergi.SeverityName);
            Assert.True(alergi.IsLifeThreatening);

            Assert.Equal(2, ringkasan.DocumentCounts
                .Single(x => x.DocumentKind == ClinicalDocumentKind.ProgressNote).Total);
            Assert.Equal(3, ringkasan.TotalDocument);
            Assert.True(ringkasan.IsComplete);
        }

        /// <summary>
        /// Daftar pilihan memperingatkan bila master keperluan akses masih kosong.
        ///
        /// Ini penting untuk keadaan sekarang: `BE-09` belum terisi karena menunggu SOP rekam
        /// medis. Tanpa peringatan ini, pengguna akan mengira penolakan akses adalah
        /// kesalahannya sendiri.
        /// </summary>
        [Fact]
        public async Task DaftarPilihan_MemperingatkanBilaMasterKeperluanKosong()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var controller = BuatController(context, petugas.Id);

            var hasilKosong = await controller.GetFilterMetadata();
            var metadataKosong = Isi<MedicalRecordFilterMetadataResponse>(hasilKosong);

            Assert.True(metadataKosong.IsAccessPurposeMasterEmpty);
            Assert.Empty(metadataKosong.AccessPurposes);
            Assert.Contains("PERHATIAN", ControllerTestHarness.Pesan(hasilKosong)!);
            Assert.Equal(
                MedicalRecordTimelineService.UkuranHalamanMaksimal,
                metadataKosong.PageSizeMax);

            BuatKeperluan(context);

            var metadataTerisi = Isi<MedicalRecordFilterMetadataResponse>(
                await controller.GetFilterMetadata());

            Assert.False(metadataTerisi.IsAccessPurposeMasterEmpty);
            Assert.Single(metadataTerisi.AccessPurposes);
        }

        /// <summary>
        /// Daftar pilihan tidak menghasilkan jejak akses.
        ///
        /// Endpoint ini tidak menyentuh data pasien mana pun. Mencatatnya sebagai pembukaan
        /// rekam medis akan mengotori angka tinjauan dengan pembukaan yang tidak pernah terjadi.
        /// </summary>
        [Fact]
        public async Task DaftarPilihan_TidakMenghasilkanJejakAkses()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.rm");
            var controller = BuatController(context, petugas.Id);

            await controller.GetFilterMetadata();

            using var konteksBaca = database.CreateContext();
            Assert.Empty(await konteksBaca.Set<TrxMedicalRecordAccessLog>().AsNoTracking().ToListAsync());
        }
    }
}
