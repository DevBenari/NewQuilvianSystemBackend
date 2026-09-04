using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-047</c> — catatan lama tetap dapat dibetulkan, termasuk
    /// setelah pasien pulang.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua hal yang tampak berlawanan, dan keduanya benar.</b> Perawatan yang sudah ditutup
    /// menolak catatan <b>baru</b> — pasien sudah pulang, dan catatan baru pada perawatan yang
    /// selesai akan menggeser riwayat. Perawatan yang sama tetap <b>menerima koreksi</b> atas
    /// catatan yang sudah ada, karena kesalahan tulis paling sering justru ditemukan setelah
    /// pasien pulang. Koreksi tidak membuka kembali perawatan; ia menempel pada catatannya.
    /// </para>
    /// <para>
    /// <b>Yang paling mudah terlewat ada pada kriteria 5.</b> Penetapan berhalangan menyatakan
    /// "dokter ini berhalangan" tanpa menyebut siapa penggantinya. Uji yang hanya memeriksa hak
    /// akses akan lulus semuanya dan tetap membiarkan dokter mana pun mengoreksi catatan pasien
    /// yang bukan tanggung jawabnya. Karena itu uji kriteria 5 sengaja memanggil endpoint
    /// pengganti <b>tanpa satu pun penolakan hak akses</b>: yang menolaknya harus aturan bisnis.
    /// </para>
    /// </remarks>
    public class InpatientDocumentCorrectionTests
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

        private static DoctorConsultationController BuatControllerCatatan(
            ApplicationDbContext c, Guid actorUserId) =>
            new DoctorConsultationController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ConsultationValidationService(c, new PrescriptionValidationService(c)),
                Finalisasi(c),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        /// <summary>
        /// Membuat controller koreksi dokumen.
        /// </summary>
        /// <remarks>
        /// <c>AccessPermissionService</c> sengaja tidak disuntikkan. Ia hanya dipakai endpoint
        /// <b>pembacaan</b> kewenangan, supaya layar tahu tombol mana yang ditampilkan; jalur
        /// pembuatan koreksi tidak pernah menyentuhnya. Membangunnya di sini menuntut seluruh
        /// mesin Identity dinyalakan tanpa satu pun baris uji yang memakainya.
        ///
        /// <para>
        /// Sebagai akibatnya, memanggil <c>CreateAsSubstitute</c> dari uji ini berarti
        /// <b>seluruh pemeriksaan hak akses dianggap lolos</b> — persis keadaan yang diminta
        /// kriteria 5.
        /// </para>
        /// </remarks>
        private static ClinicalNoteAddendumController BuatControllerKoreksi(
            ApplicationDbContext c, Guid actorUserId) =>
            new ClinicalNoteAddendumController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ClinicalNoteAddendumService(c, Keutuhan(c)),
                accessPermissionService: null!,
                new InpatientDocumentCorrectionAuthorityService(c),
                // BE-RWI-053. Koreksi mengembalikan catatan terpadu terverifikasi ke keadaan
                // menunggu verifikasi, sehingga service verifikasinya ikut disuntikkan.
                new CpptVerificationService(c, new InpatientClinicalContextService(c)))
                .DenganPengguna(actorUserId);

        /// <summary>
        /// Membuat controller penetapan penulis berhalangan.
        /// </summary>
        private static ClinicalNoteAuthorDelegationController BuatControllerPenetapan(
            ApplicationDbContext c, Guid actorUserId) =>
            new ClinicalNoteAuthorDelegationController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ClinicalNoteAuthorDelegationService(c))
                .DenganPengguna(actorUserId);

        /// <summary>
        /// Menyiapkan satu catatan dokter rawat inap yang sudah difinalkan, sehingga ia berada
        /// pada keadaan yang menerima koreksi.
        /// </summary>
        private static async Task<TrxDoctorConsultation> SiapkanCatatanFinalAsync(
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
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                IsActive = true,
                CreateBy = penulisUserId,

                Subjective = "Nyeri dada berkurang",
                Objective = "Tekanan darah 120/80",
                Assessment = "Perbaikan klinis",
                Plan = "Lanjutkan terapi",
                HasPrimaryDiagnosis = true,
                PrimaryDiagnosisText = "Angina stabil",
                DiagnosisCount = 1
            };

            context.Set<TrxDoctorConsultation>().Add(catatan);
            context.SaveChanges();

            var hasil = await Finalisasi(context).FinalizeAsync(
                catatan.Id, new FinalizeDoctorConsultationRequest(), penulisUserId);

            Assert.True(hasil.IsSuccess, hasil.ErrorMessage);

            return catatan;
        }

        /// <summary>
        /// Menutup perawatan beserta seluruh jejak waktunya, seperti keadaan setelah pasien
        /// benar-benar pulang.
        /// </summary>
        private static void TutupPerawatan(ApplicationDbContext context, Guid episodeId)
        {
            var episode = context.Set<InpEpisode>().Single(x => x.Id == episodeId);

            episode.EpisodeStatus = InpEpisodeStatus.Closed;
            episode.DischargeDecidedAt = DateTime.UtcNow.AddHours(-3);
            episode.PhysicallyLeftAt = DateTime.UtcNow.AddHours(-2);
            episode.ClosedAt = DateTime.UtcNow.AddHours(-1);

            context.SaveChanges();
        }

        /// <summary>
        /// Menempatkan pasien pada satu tempat tidur, supaya kriteria 2 benar-benar punya
        /// tempat tidur untuk dibuktikan tidak berubah.
        /// </summary>
        private static InpBedPlacement TempatkanDiTempatTidur(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var kamar = new MstRoom
            {
                ServiceUnitId = k.ServiceUnitId,
                RoomCode = $"KMR-{pembeda}",
                RoomName = "Kamar Uji"
            };
            context.Set<MstRoom>().Add(kamar);
            context.SaveChanges();

            var tempatTidur = new MstBed
            {
                RoomId = kamar.Id,
                BedCode = $"BED-{pembeda}",
                BedName = "Tempat Tidur Uji"
            };
            context.Set<MstBed>().Add(tempatTidur);
            context.SaveChanges();

            var episode = context.Set<InpEpisode>().AsNoTracking().Single(x => x.Id == k.EpisodeId);

            var penempatan = new InpBedPlacement
            {
                EpisodeId = k.EpisodeId,
                BedId = tempatTidur.Id,
                RoomId = kamar.Id,
                ServiceUnitId = k.ServiceUnitId,
                PatientClassId = episode.PatientClassId,
                SequenceNumber = 1,
                StartDateTime = DateTime.UtcNow.AddDays(-2),
                PlacedByUserId = k.DokterUserId
            };
            context.Set<InpBedPlacement>().Add(penempatan);
            context.SaveChanges();

            return penempatan;
        }

        private static CreateClinicalNoteAddendumRequest PermintaanKoreksi(string alasan) => new()
        {
            AddendumText = "Tekanan darah seharusnya 130/85.",
            CorrectionReason = alasan
        };

        // =====================================================================
        // Kriteria 1 — perawatan tertutup menolak catatan baru
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-047 AC 1</c>, <c>VAL-DOK-03</c> — perawatan yang sudah ditutup menolak
        /// catatan <b>baru</b> dengan <c>422</c>, beserta arahan bahwa koreksi tetap bisa.
        /// </summary>
        /// <remarks>
        /// Kode <c>422</c>, bukan <c>400</c>. Isian permintaannya tidak ada yang salah; yang
        /// salah adalah keadaan pasiennya. Layar perlu membedakan keduanya supaya tidak menyorot
        /// kolom isian yang sebenarnya sudah benar.
        /// </remarks>
        [Fact]
        public async Task PerawatanTertutup_MenolakCatatanBaru422()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, InpEpisodeStatus.Closed);

            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = k.EncounterId,
                    DoctorId = k.DoctorMasterId,
                    Subjective = "Catatan menyusul"
                });

            Assert.Equal(422, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("sudah ditutup", ControllerTestHarness.Pesan(hasil)!);
            Assert.Contains("koreksi", ControllerTestHarness.Pesan(hasil)!);

            Assert.Empty(context.Set<TrxDoctorConsultation>());
        }

        // =====================================================================
        // Kriteria 2 — perawatan tertutup menerima koreksi tanpa bergeser
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-047 AC 2</c> — perawatan yang sudah ditutup <b>menerima</b> koreksi, dan
        /// keadaannya tidak bergeser sedikit pun: status tetap tertutup, tempat tidurnya tetap
        /// sama, dan lama dirawatnya tidak berubah.
        /// </summary>
        /// <remarks>
        /// Inilah alasan koreksi memakai addendum, bukan penyuntingan. Menyunting catatan lama
        /// akan menuntut perawatan dibuka kembali, dan perawatan yang dibuka kembali menggeser
        /// lama rawat, tagihan, serta ketersediaan tempat tidur.
        /// </remarks>
        [Fact]
        public async Task PerawatanTertutup_MenerimaKoreksiTanpaMenggeserKeadaanPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var penempatan = TempatkanDiTempatTidur(context, k);
            var catatan = await SiapkanCatatanFinalAsync(context, k, k.DokterUserId);

            TutupPerawatan(context, k.EpisodeId);

            var sebelum = context.Set<InpEpisode>().AsNoTracking().Single(x => x.Id == k.EpisodeId);

            var hasil = await BuatControllerKoreksi(context, k.DokterUserId).Create(
                ClinicalDocumentKind.Consultation,
                catatan.Id,
                PermintaanKoreksi("Salah ketik tekanan darah"));

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();

            var sesudah = verifikasi.Set<InpEpisode>().Single(x => x.Id == k.EpisodeId);

            Assert.Equal(InpEpisodeStatus.Closed, sesudah.EpisodeStatus);
            Assert.Equal(sebelum.AdmittedAt, sesudah.AdmittedAt);
            Assert.Equal(sebelum.PhysicallyLeftAt, sesudah.PhysicallyLeftAt);
            Assert.Equal(sebelum.ClosedAt, sesudah.ClosedAt);

            var tempatTidurSesudah = verifikasi.Set<InpBedPlacement>()
                .Single(x => x.EpisodeId == k.EpisodeId);

            Assert.Equal(penempatan.BedId, tempatTidurSesudah.BedId);
            Assert.Equal(penempatan.StartDateTime, tempatTidurSesudah.StartDateTime);
            Assert.Null(tempatTidurSesudah.EndDateTime);

            Assert.Single(verifikasi.Set<MrcClinicalNoteAddendum>());
        }

        // =====================================================================
        // Kriteria 3 dan 4 — koreksi atas nama dokter yang berhalangan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-047 AC 3</c> dan <c>AC 4</c> — setelah penetapan berhalangan berlaku, DPJP
        /// aktif perawatan itu dapat mengoreksi catatan dokter yang berhalangan, dan penulis
        /// catatan aslinya <b>tidak berpindah</b>.
        /// </summary>
        /// <remarks>
        /// Penulis asli yang berpindah adalah pemalsuan rekam medis, bukan koreksi. Yang
        /// tersimpan adalah dua nama berbeda pada dua tempat berbeda: penulis catatan tetap
        /// dokter yang berhalangan, sedangkan penulis koreksi adalah DPJP yang menggantikannya.
        /// </remarks>
        [Fact]
        public async Task PenetapanBerlaku_DpjpAktifMengoreksiTanpaMemindahkanPenulisAsli()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            // Penulis catatan adalah dokter lain yang kemudian berhalangan. Ia bukan pengguna
            // dokter pada konteks, sehingga penulis dan pengoreksi benar-benar dua orang.
            var penulisBerhalangan = RekamMedisTestData.BuatPengguna(context, "dokterberhalangan");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepalaunit");

            var catatan = await SiapkanCatatanFinalAsync(context, k, penulisBerhalangan.Id);

            var penetapan = await BuatControllerPenetapan(context, kepalaUnit.Id)
                .Create(new CreateAuthorDelegationRequest
                {
                    OriginalAuthorUserId = penulisBerhalangan.Id,
                    GrantReason = "Cuti sakit dua minggu",
                    ValidUntil = DateTime.UtcNow.AddDays(14)
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(penetapan));

            var hasil = await BuatControllerKoreksi(context, k.DokterUserId).CreateAsSubstitute(
                ClinicalDocumentKind.Consultation,
                catatan.Id,
                PermintaanKoreksi("Membetulkan angka tekanan darah atas nama penulis"));

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();

            var keutuhan = verifikasi.Set<MrcClinicalDocumentIntegrity>()
                .Single(x => x.DocumentKind == ClinicalDocumentKind.Consultation
                             && x.DocumentId == catatan.Id);

            // Kriteria 4 — penulis catatan aslinya tetap dokter yang berhalangan.
            Assert.Equal(penulisBerhalangan.Id, keutuhan.AuthorUserId);
            Assert.Equal(penulisBerhalangan.Id, keutuhan.SignedByUserId);

            var koreksi = verifikasi.Set<MrcClinicalNoteAddendum>().Single();

            Assert.Equal(k.DokterUserId, koreksi.AuthorUserId);
            Assert.True(koreksi.IsSubstituteAuthor);
            Assert.NotNull(koreksi.DelegationId);
        }

        // =====================================================================
        // Kriteria 5 — bukan DPJP episode itu ditolak, walau hak aksesnya lolos
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-047 AC 5</c>, <c>VAL-DOK-35</c> — dokter yang <b>bukan</b> DPJP perawatan
        /// itu ditolak <c>403</c>, walaupun butir hak akses penggantinya ada dan penetapan
        /// berhalangannya berlaku.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Uji ini tidak dapat digantikan uji hak akses.</b> Endpoint dipanggil langsung,
        /// sehingga seluruh pemeriksaan hak akses memang lolos: pemanggil diperlakukan sebagai
        /// pemegang butir <c>ClinicalNoteAddendum : CreateAsSubstitute</c>. Penetapan
        /// berhalangannya pun sah dan masih berlaku. Yang tersisa hanya aturan bisnis, dan
        /// dari sanalah penolakannya datang.
        /// </para>
        /// <para>
        /// Dokter penyusup di sini adalah dokter sungguhan dengan akun yang tertaut — ia hanya
        /// tidak memiliki penugasan pada perawatan pasien ini. Itulah bedanya dengan penolakan
        /// "bukan dokter".
        /// </para>
        /// </remarks>
        [Fact]
        public async Task DokterBukanDpjpPerawatanItu_Ditolak403MeskipunHakAksesDanPenetapanLolos()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var penulisBerhalangan = RekamMedisTestData.BuatPengguna(context, "dokterberhalangan");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepalaunit");

            // Dokter dari bangsal lain: benar-benar dokter, benar-benar aktif, tetapi tidak
            // punya satu pun penugasan pada perawatan pasien ini.
            var dokterLuar = RekamMedisTestData.BuatPengguna(context, "dokterluar");
            var dokterLuarMaster = RawatInapTestData.BuatDokterMaster(context);
            dokterLuar.WorkforceProfileId = dokterLuarMaster.WorkforceProfileId;
            context.SaveChanges();

            var catatan = await SiapkanCatatanFinalAsync(context, k, penulisBerhalangan.Id);

            var penetapan = await BuatControllerPenetapan(context, kepalaUnit.Id)
                .Create(new CreateAuthorDelegationRequest
                {
                    OriginalAuthorUserId = penulisBerhalangan.Id,
                    GrantReason = "Cuti sakit dua minggu",
                    ValidUntil = DateTime.UtcNow.AddDays(14)
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(penetapan));

            var hasil = await BuatControllerKoreksi(context, dokterLuar.Id).CreateAsSubstitute(
                ClinicalDocumentKind.Consultation,
                catatan.Id,
                PermintaanKoreksi("Mencoba mengoreksi pasien bangsal lain"));

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains(
                "DPJP yang sedang bertanggung jawab",
                ControllerTestHarness.Pesan(hasil)!);

            using var verifikasi = database.CreateContext();
            Assert.Empty(verifikasi.Set<MrcClinicalNoteAddendum>());
        }

        /// <summary>
        /// <c>BE-RWI-047 AC 5</c> — DPJP perawatan itu tetap diterima setelah pasien pulang,
        /// walaupun penugasannya sudah diakhiri bersamaan dengan penutupan perawatan.
        /// </summary>
        /// <remarks>
        /// Tanpa keringanan ini, koreksi setelah pasien pulang menjadi mustahil bagi siapa pun,
        /// karena tidak ada satu dokter pun yang penugasannya masih berlaku hari ini — persis
        /// kebalikan dari yang diminta <c>FR-DOK-047</c>.
        /// </remarks>
        [Fact]
        public async Task SetelahPasienPulang_DpjpTerakhirMasihDapatMengoreksiAtasNamaPenulis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var penulisBerhalangan = RekamMedisTestData.BuatPengguna(context, "dokterberhalangan");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepalaunit");

            var catatan = await SiapkanCatatanFinalAsync(context, k, penulisBerhalangan.Id);

            var penetapan = await BuatControllerPenetapan(context, kepalaUnit.Id)
                .Create(new CreateAuthorDelegationRequest
                {
                    OriginalAuthorUserId = penulisBerhalangan.Id,
                    GrantReason = "Cuti sakit dua minggu",
                    ValidUntil = DateTime.UtcNow.AddDays(14)
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(penetapan));

            // Perawatan ditutup dan penugasan DPJP diakhiri, seperti pada pemulangan sungguhan.
            var penugasan = context.Set<InpDoctorAssignment>().Single(x => x.EpisodeId == k.EpisodeId);
            penugasan.EndDateTime = DateTime.UtcNow.AddHours(-2);
            context.SaveChanges();

            TutupPerawatan(context, k.EpisodeId);

            var hasil = await BuatControllerKoreksi(context, k.DokterUserId).CreateAsSubstitute(
                ClinicalDocumentKind.Consultation,
                catatan.Id,
                PermintaanKoreksi("Koreksi setelah pasien pulang"));

            Assert.Equal(201, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();
            Assert.Single(verifikasi.Set<MrcClinicalNoteAddendum>());
        }

        // =====================================================================
        // Kriteria 6 — penetapan tanpa masa berlaku ditolak
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-047 AC 6</c>, <c>VAL-DOK-34</c> — penetapan berhalangan tanpa masa berlaku
        /// ditolak <c>400</c>.
        /// </summary>
        /// <remarks>
        /// Penetapan permanen sama saja dengan pintu belakang tetap: sekali diterbitkan, jalur
        /// koreksi atas nama dokter itu terbuka selamanya dan tidak ada yang mengingat untuk
        /// menutupnya.
        /// </remarks>
        [Fact]
        public async Task PenetapanTanpaMasaBerlaku_Ditolak400()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var penulisBerhalangan = RekamMedisTestData.BuatPengguna(context, "dokterberhalangan");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepalaunit");

            var hasil = await BuatControllerPenetapan(context, kepalaUnit.Id)
                .Create(new CreateAuthorDelegationRequest
                {
                    OriginalAuthorUserId = penulisBerhalangan.Id,
                    GrantReason = "Cuti sakit tanpa batas waktu"
                    // ValidUntil sengaja tidak diisi.
                });

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Contains("Batas waktu penetapan", ControllerTestHarness.Pesan(hasil)!);

            Assert.Empty(context.Set<MrcClinicalNoteAuthorDelegation>());
            Assert.NotEqual(Guid.Empty, k.EpisodeId);
        }

        // =====================================================================
        // Regresi — dokumen di luar rawat inap tidak ikut terkunci
        // =====================================================================

        /// <summary>
        /// Regresi <c>BE-RWI-047</c> — penjaga per pasien hanya berlaku bagi dokumen yang berada
        /// di bawah perawatan rawat inap.
        /// </summary>
        /// <remarks>
        /// Catatan poliklinik dan IGD tidak punya perawatan rawat inap, sehingga aturan DPJP
        /// tidak dapat dinilai untuk keduanya. Menolaknya di sini akan mematikan jalur koreksi
        /// yang hari ini sudah berjalan pada modul lain — kerusakan yang jauh lebih besar
        /// daripada masalah yang sedang ditutup.
        /// </remarks>
        [Fact]
        public async Task DokumenDiLuarRawatInap_TidakTundukPenjagaDpjp()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokterLuar = RekamMedisTestData.BuatPengguna(context, "dokterluar");
            var penulis = RekamMedisTestData.BuatPengguna(context, "penulis");

            var catatan = new TrxPatientIntegratedProgressNote
            {
                PatientId = konteks.PatientId,
                EncounterId = konteks.EncounterId,
                ProfessionType = "Doctor",
                ProviderUserId = penulis.Id,
                NoteDateTime = DateTime.UtcNow
            };
            context.Set<TrxPatientIntegratedProgressNote>().Add(catatan);
            context.SaveChanges();

            await Keutuhan(context).RegisterSignedAsync(
                ClinicalDocumentKind.ProgressNote,
                catatan.Id,
                konteks.PatientId,
                konteks.EncounterId,
                penulis.Id,
                deviceInfo: "uji",
                ipAddress: "127.0.0.1",
                nowUtc: DateTime.UtcNow);
            await context.SaveChangesAsync();

            var hasil = await new InpatientDocumentCorrectionAuthorityService(context)
                .EnsureMaySubstituteAsync(
                    ClinicalDocumentKind.ProgressNote,
                    catatan.Id,
                    user: null,
                    dokterLuar.Id,
                    DateTime.UtcNow);

            Assert.True(hasil.IsAllowed);
            Assert.Equal(
                InpatientCorrectionAuthorityOutcome.NotInpatientDocument,
                hasil.Outcome);
        }
    }
}
