using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-068</c> — diagnosis kerja dapat dicatat langsung dari
    /// kajian medis awal, tanpa nomor konsultasi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kesembilan skenario di bawah dikunci <c>testing/acceptance-test-matrix.md</c> bagian 11.
    /// Lima di antaranya jalur gagal dan <b>dua</b> di antaranya regresi, karena yang paling
    /// mungkin rusak dari pelonggaran sebuah kolom wajib bukanlah jalur barunya melainkan jalur
    /// lama yang selama ini bergantung pada kewajiban itu. Poliklinik memakai grup diagnosis
    /// ini setiap hari.
    /// </para>
    /// <para>
    /// Tiga uji sengaja lebih galak daripada sekadar memeriksa kode balasan:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// Jumlah baris <c>TrxDoctorConsultation</c> dihitung sebelum dan sesudah permintaan gagal.
    /// Cara termudah membuat jalur ini terlihat berhasil adalah diam-diam membuatkan konsultasi
    /// bayangan, dan uji yang hanya melihat kode balasan tidak akan pernah menangkapnya.
    /// </item>
    /// <item>
    /// Kalimat penolakan rawat jalan dibandingkan <b>utuh</b> dengan
    /// <see cref="PenolakanKonsultasiTidakDitemukan"/>, bukan dengan potongan katanya — cara
    /// yang sama dipakai <c>BE-RWI-043</c> membuktikan <c>RWI-AC-143</c>.
    /// </item>
    /// <item>
    /// <c>VAL-DOK-39</c> diuji dengan pengguna biasa tanpa peran <c>SuperAdmin</c>, sehingga
    /// penolakannya benar-benar datang dari aturan bisnis dan bukan dari mesin hak akses —
    /// pelajaran <c>BE-RWI-034</c>.
    /// </item>
    /// </list>
    /// </remarks>
    public class InpatientStructuredDiagnosisTests
    {
        /// <summary>
        /// Kalimat penolakan jalur lama, apa adanya sebelum kontrak <c>0.4.0</c>. Rawat jalan
        /// dan medical check-up wajib tetap menerima kalimat ini — <c>VAL-DOK-38</c>.
        /// </summary>
        private const string PenolakanKonsultasiTidakDitemukan =
            "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter.";

        /// <summary><c>VAL-DOK-36</c>.</summary>
        private const string PenolakanTanpaKonteks =
            "Diagnosis harus melekat pada catatan dokter atau pada perawatan pasien yang " +
            "sedang berjalan.";

        /// <summary><c>VAL-DOK-37</c> dan <c>VAL-DOK-40</c>, berbagi satu kalimat.</summary>
        private const string PenolakanKonteksPasienBerbeda =
            "Diagnosis ini tidak cocok dengan perawatan pasien. Periksa kembali pasien yang " +
            "sedang Anda buka.";

        /// <summary><c>VAL-DOK-39</c>, memakai penjaga yang sama dengan <c>VAL-DOK-06</c>.</summary>
        private const string PenolakanBukanDpjpPasien =
            "Anda bukan DPJP pasien ini. Hubungi DPJP atau supervisor klinis.";

        private static PatientDiagnosisController BuatControllerDiagnosis(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientDiagnosisController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        private static PatientAssessmentController BuatControllerKajian(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientAssessmentController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new InpatientClinicalContextService(c),
                new ClinicalDocumentIntegrityService(c),
                new ClinicalAssessmentPolicyService(c),
                new ClinicalNoteAddendumService(c, new ClinicalDocumentIntegrityService(c)),
                new NursingAssessmentMonitoringService(c, new ClinicalAssessmentPolicyService(c)))
                .DenganPengguna(actorUserId);

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
                    new ClinicalDocumentIntegrityService(c)),
                new InpatientClinicalContextService(c))
                .DenganPengguna(actorUserId);

        /// <summary>
        /// Permintaan diagnosis berkode ICD. Kedua kolom konteksnya sengaja dibiarkan kosong
        /// supaya setiap uji mengisinya sendiri — itulah yang sedang diuji.
        /// </summary>
        private static CreatePatientDiagnosisRequest PermintaanDiagnosis(
            Guid encounterId,
            Guid? consultationId = null,
            Guid? inpEpisodeId = null,
            string kode = "J18.9") => new()
            {
                EncounterId = encounterId,
                ConsultationId = consultationId,
                InpEpisodeId = inpEpisodeId,
                DiagnosisCode = kode,
                DiagnosisName = "Pneumonia, organisme tidak spesifik",
                DiagnosisMasterType = "ICD10",
                IcdVersion = "ICD-10 2019"
            };

        private static Task<int> JumlahKonsultasiAsync(ApplicationDbContext c) =>
            c.Set<TrxDoctorConsultation>().CountAsync();

        private static Task<int> JumlahDiagnosisAsync(ApplicationDbContext c) =>
            c.Set<TrxPatientDiagnosis>().CountAsync();

        /// <summary>
        /// Membuat satu catatan dokter di atas perawatan rawat inap, dipakai uji regresi jalur
        /// lama.
        /// </summary>
        private static async Task<TrxDoctorConsultation> BuatCatatanDokterAsync(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k)
        {
            var hasil = await BuatControllerCatatan(context, k.DokterUserId)
                .CreateConsultation(new CreateDoctorConsultationRequest
                {
                    EncounterId = k.EncounterId,
                    DoctorId = k.DoctorMasterId,
                    Subjective = "Sesak berkurang"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            return await context.Set<TrxDoctorConsultation>()
                .SingleAsync(x => x.EncounterId == k.EncounterId);
        }

        // =====================================================================
        // Kriteria 1 - diagnosis lahir dari kajian medis, tanpa nomor konsultasi
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 1` — diagnosis terstruktur dapat dibuat dengan menyebut perawatan
        /// rawat inap sebagai konteks, tanpa nomor konsultasi, ketika pasien itu belum punya
        /// satu pun catatan harian.
        /// </summary>
        /// <remarks>
        /// <c>CAP-022</c> aturan 5. Yang dibuktikan bukan hanya kode balasannya, melainkan juga
        /// bentuk baris yang tersimpan: kolom konsultasi <b>kosong</b>, kolom perawatan
        /// <b>terisi</b>. Jumlah konsultasi dihitung untuk membuktikan jalur ini tidak
        /// membuatkan konsultasi bayangan.
        /// </remarks>
        [Fact]
        public async Task DiagnosisDariKajianMedis_TanpaNomorKonsultasi_Diterima()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var konsultasiSebelum = await JumlahKonsultasiAsync(context);
            Assert.Equal(0, konsultasiSebelum);

            var hasil = await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(
                    k.EncounterId,
                    inpEpisodeId: k.EpisodeId));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var diagnosis = await context.Set<TrxPatientDiagnosis>().SingleAsync();

            Assert.Null(diagnosis.ConsultationId);
            Assert.Equal(k.EpisodeId, diagnosis.InpEpisodeId);
            Assert.Equal(k.EncounterId, diagnosis.EncounterId);
            Assert.Equal(k.PatientId, diagnosis.PatientId);
            Assert.Equal(k.DoctorMasterId, diagnosis.DoctorId);
            Assert.Equal("J18.9", diagnosis.DiagnosisCode);

            // Nol konsultasi bayangan.
            Assert.Equal(konsultasiSebelum, await JumlahKonsultasiAsync(context));
        }

        // =====================================================================
        // Kriteria 2 - terbaca pada daftar masalah kajian medis
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 2` — diagnosis yang baru dibuat terbaca pada daftar masalah kajian
        /// medis pasien itu lewat penyaring <c>inpEpisodeId</c>.
        /// </summary>
        /// <remarks>
        /// <c>CAP-022</c> aturan 2. Kedua permukaan baca diuji: daftar bernomor halaman yang
        /// dipakai layar riwayat, dan feed <c>options</c> yang dipakai daftar masalah pada
        /// layar kajian medis.
        /// </remarks>
        [Fact]
        public async Task DiagnosisPerawatan_TerbacaPadaDaftarMasalahLewatPenyaringPerawatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var lain = RawatInapTestData.SiapkanPerawatan(context);

            await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(k.EncounterId, inpEpisodeId: k.EpisodeId));

            await BuatControllerDiagnosis(context, lain.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(
                    lain.EncounterId,
                    inpEpisodeId: lain.EpisodeId,
                    kode: "E11.9"));

            var daftar = await BuatControllerDiagnosis(context, k.DokterUserId)
                .GetDiagnoses(
                    search: null,
                    encounterId: null,
                    consultationId: null,
                    inpEpisodeId: k.EpisodeId,
                    patientId: null,
                    doctorId: null,
                    serviceUnitId: null,
                    clinicId: null,
                    diagnosisId: null,
                    diagnosisType: null,
                    diagnosisStatus: null,
                    isPrimary: null,
                    isConfirmed: null,
                    isFromMasterDiagnosis: null,
                    startDate: null,
                    endDate: null);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(daftar));

            var isi = AmbilData<PagedResult<PatientDiagnosisResponse>>(daftar);

            // Penyaringnya benar-benar menyaring: diagnosis perawatan lain tidak ikut terbawa.
            Assert.Equal(1, isi.TotalData);
            Assert.Equal("J18.9", Assert.Single(isi.Items).DiagnosisCode);
            Assert.Equal(k.EpisodeId, isi.Items[0].InpEpisodeId);
            Assert.Null(isi.Items[0].ConsultationId);

            var pilihan = await BuatControllerDiagnosis(context, k.DokterUserId)
                .GetDiagnosisOptions(
                    consultationId: null,
                    encounterId: null,
                    inpEpisodeId: k.EpisodeId,
                    patientId: null);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(pilihan));

            var daftarPilihan = AmbilData<List<PatientDiagnosisOptionResponse>>(pilihan);

            Assert.Equal("J18.9", Assert.Single(daftarPilihan).DiagnosisCode);
        }

        // =====================================================================
        // Kriteria 8 - regresi jalur lama
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 8` — permintaan lama yang menyebut nomor konsultasi tetap berperilaku
        /// identik seperti sebelum <c>0.4.0</c>.
        /// </summary>
        /// <remarks>
        /// <c>INT-DOK-10</c>, uji <b>regresi</b>. Kolom konsultasi tetap terisi dan kolom
        /// perawatan tetap kosong — jalur lama tidak diam-diam ikut memakai konteks baru.
        /// </remarks>
        [Fact]
        public async Task DiagnosisDenganNomorKonsultasi_TetapBerperilakuSepertiSebelumnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var catatan = await BuatCatatanDokterAsync(context, k);

            var hasil = await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(
                    k.EncounterId,
                    consultationId: catatan.Id));

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var diagnosis = await context.Set<TrxPatientDiagnosis>().SingleAsync();

            Assert.Equal(catatan.Id, diagnosis.ConsultationId);
            Assert.Null(diagnosis.InpEpisodeId);
            Assert.Equal(catatan.PatientId, diagnosis.PatientId);
            Assert.Equal(catatan.DoctorId, diagnosis.DoctorId);

            // Ringkasan diagnosis pada catatan dokter tetap diperbarui seperti sebelumnya.
            var catatanTersimpan = await context.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .SingleAsync(x => x.Id == catatan.Id);

            Assert.Equal(1, catatanTersimpan.DiagnosisCount);
        }

        // =====================================================================
        // Kriteria 6 - regresi rawat jalan dan medical check-up
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 6` — pada kunjungan rawat jalan dan medical check-up, permintaan
        /// tanpa nomor konsultasi tetap ditolak dengan kode <b>dan kalimat</b> yang sama persis
        /// seperti sebelum <c>0.4.0</c>.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-38</c>, <c>RWI-AC-143</c>, uji <b>regresi</b>. Inilah aturan terpenting
        /// bagian 9 validation-matrix, dan ia tidak menambah kemampuan apa pun: ia menuliskan
        /// hitam di atas putih bahwa pelonggaran ini tidak menetes ke poliklinik. Kalimatnya
        /// dibandingkan utuh supaya pengujiannya tidak dapat lolos hanya dengan menolak.
        /// </remarks>
        [Theory]
        [InlineData(EncounterType.Outpatient)]
        [InlineData(EncounterType.MedicalCheckup)]
        public async Task RawatJalanDanMedicalCheckup_TanpaNomorKonsultasi_DitolakKalimatSama(
            EncounterType encounterType)
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context, encounterType: encounterType);

            var hasil = await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(k.EncounterId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanKonsultasiTidakDitemukan, ControllerTestHarness.Pesan(hasil));

            Assert.Equal(0, await JumlahDiagnosisAsync(context));
        }

        /// <summary>
        /// `BE-RWI-068 AC 7` — jalur IGD tidak ikut dibuka.
        /// </summary>
        /// <remarks>
        /// <c>api-contract.md</c> bagian 11 dan <c>integration-contract.md</c> bagian 10.2.
        /// Sebabnya kepemilikan, bukan teknis: <c>RWI-DEC-069</c> mencabut bagian IGD dari
        /// persetujuan lintas modul <c>RWI-DEC-062</c>. Uji ini mengunci keadaan itu supaya
        /// pelonggarannya tidak menetes ke sana tanpa keputusan pemiliknya.
        /// </remarks>
        [Fact]
        public async Task KunjunganIgd_TidakIkutDilonggarkan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(
                context, encounterType: EncounterType.Emergency);

            var hasil = await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(
                    k.EncounterId,
                    inpEpisodeId: k.EpisodeId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanKonsultasiTidakDitemukan, ControllerTestHarness.Pesan(hasil));

            Assert.Equal(0, await JumlahDiagnosisAsync(context));
        }

        // =====================================================================
        // Kriteria 3 - kedua konteks kosong
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 3` — permintaan yang tidak menyebut satu pun konteks ditolak
        /// <c>400</c>, dan <b>nol baris konsultasi terbentuk</b>.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-36</c>. Kasus ini diuji khusus karena satu jaring pengaman basis data
        /// memang dilepas: aturan salah-satu-wajib tidak dapat ditegakkan <c>NOT NULL</c>,
        /// sehingga basis data tidak lagi menolak diagnosis tanpa konteks dan penjagaannya
        /// sepenuhnya aturan bisnis.
        /// </remarks>
        [Fact]
        public async Task KeduaKonteksKosong_Ditolak_DanNolKonsultasiTerbentuk()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var konsultasiSebelum = await JumlahKonsultasiAsync(context);

            var hasil = await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(k.EncounterId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanTanpaKonteks, ControllerTestHarness.Pesan(hasil));

            Assert.Equal(0, await JumlahDiagnosisAsync(context));
            Assert.Equal(konsultasiSebelum, await JumlahKonsultasiAsync(context));
        }

        // =====================================================================
        // Kriteria 4 - konteks milik pasien lain
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 4` — nomor konsultasi dan perawatan sama-sama terisi tetapi menunjuk
        /// pasien yang berbeda ditolak <c>400</c>.
        /// </summary>
        /// <remarks><c>VAL-DOK-37</c>.</remarks>
        [Fact]
        public async Task KeduaKonteksTerisiTetapiPasienBerbeda_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var lain = RawatInapTestData.SiapkanPerawatan(context);

            var catatan = await BuatCatatanDokterAsync(context, k);

            var hasil = await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(
                    k.EncounterId,
                    consultationId: catatan.Id,
                    inpEpisodeId: lain.EpisodeId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanKonteksPasienBerbeda, ControllerTestHarness.Pesan(hasil));

            Assert.Equal(0, await JumlahDiagnosisAsync(context));
        }

        /// <summary>
        /// `BE-RWI-068 AC 4` — perawatan yang disebut bukan milik pasien pada permintaan itu
        /// ditolak <c>400</c>.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-40</c>. Kalimatnya sengaja sama dengan <c>VAL-DOK-37</c>: bagi dokter
        /// yang sedang membuka layar, keduanya adalah kesalahan yang sama.
        /// </remarks>
        [Fact]
        public async Task PerawatanMilikPasienLain_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var lain = RawatInapTestData.SiapkanPerawatan(context);

            var hasil = await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(
                    k.EncounterId,
                    inpEpisodeId: lain.EpisodeId));

            Assert.Equal(400, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanKonteksPasienBerbeda, ControllerTestHarness.Pesan(hasil));

            Assert.Equal(0, await JumlahDiagnosisAsync(context));
        }

        // =====================================================================
        // Kriteria 5 - kewenangan per pasien, bukan per peran
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 5` — dokter yang memegang butir hak akses
        /// <c>PatientDiagnosis : Create</c> tetapi bukan DPJP pasien itu ditolak <c>403</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>VAL-DOK-39</c>, memakai pemeriksaan dokter aktif per episode yang sama dengan
        /// <c>VAL-DOK-06</c>. Mesin hak akses hanya melihat sumber daya
        /// <c>PatientDiagnosis</c> dan <b>tidak tahu</b> pasien siapa, sehingga ia akan
        /// meloloskan permintaan ini.
        /// </para>
        /// <para>
        /// Pengguna pada uji ini <b>bukan</b> <c>SuperAdmin</c> dan memang seorang dokter
        /// aktif; satu-satunya yang tidak ia miliki adalah penugasan pada perawatan tersebut.
        /// Itulah yang membuat penolakannya benar-benar datang dari aturan bisnis —
        /// pelajaran <c>BE-RWI-034</c>.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task DokterYangBukanDpjpPasienItu_Ditolak403()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            // Dokter kedua: benar-benar dokter aktif, tetapi tanpa penugasan pada perawatan itu.
            var dokterLain = RawatInapTestData.BuatDokterMaster(context);
            var penggunaDokterLain = RekamMedisTestData.BuatPengguna(context, "dokter-lain");
            penggunaDokterLain.WorkforceProfileId = dokterLain.WorkforceProfileId;
            await context.SaveChangesAsync();

            var hasil = await BuatControllerDiagnosis(context, penggunaDokterLain.Id)
                .CreateDiagnosis(PermintaanDiagnosis(
                    k.EncounterId,
                    inpEpisodeId: k.EpisodeId));

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));
            Assert.Equal(PenolakanBukanDpjpPasien, ControllerTestHarness.Pesan(hasil));

            Assert.Equal(0, await JumlahDiagnosisAsync(context));
        }

        // =====================================================================
        // Kriteria 9 - VAL-DOK-11 dipertajam, bukan diperketat
        // =====================================================================

        /// <summary>
        /// `BE-RWI-068 AC 9` — kajian medis tetap dapat diselesaikan ketika diagnosis kerja
        /// berupa teks <b>kosong</b> tetapi daftar masalah terstruktur <b>terisi</b>.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-11</c>, dipertajam pada <c>0.4.0</c>. Daftar masalah kini punya dua
        /// bentuk sah dan kajian lolos bila salah satu terisi; menuntut keduanya akan memaksa
        /// dokter mengetik hal yang sama dua kali. Uji ini juga membuktikan arah sebaliknya:
        /// tanpa keduanya, kajian tetap ditolak dan penolakannya tetap menyebut bagian yang
        /// kosong.
        /// </remarks>
        [Fact]
        public async Task KajianMedis_SelesaiDenganDaftarMasalahTerstrukturSaja()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);

            var dibuat = await BuatControllerKajian(context, k.DokterUserId)
                .CreateAssessment(new CreatePatientAssessmentRequest
                {
                    EncounterId = k.EncounterId,
                    InpEpisodeId = k.EpisodeId,
                    AssessmentType = PatientAssessmentType.MedicalInitial,
                    ChiefComplaint = "Sesak napas sejak dua hari",
                    CurrentIllnessHistory = "Sesak memberat saat berbaring",
                    PhysicalExamination = "Ronki basah halus di kedua basal paru",

                    // Sengaja kosong: inilah yang sedang diuji.
                    WorkingDiagnosis = null,
                    TherapyPlan = "Furosemid intravena, evaluasi ulang 24 jam"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(dibuat));

            var kajian = await context.Set<TrxPatientAssessment>()
                .SingleAsync(x => x.AssessmentType == PatientAssessmentType.MedicalInitial);

            // Tanpa kedua bentuk daftar masalah, kajian tetap ditolak - aturan lama tidak
            // dilemahkan, hanya diperluas bentuknya.
            var sebelumAdaDiagnosis = await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(400, ControllerTestHarness.KodeStatus(sebelumAdaDiagnosis));
            Assert.Contains("diagnosis kerja", ControllerTestHarness.Pesan(sebelumAdaDiagnosis)!);

            await BuatControllerDiagnosis(context, k.DokterUserId)
                .CreateDiagnosis(PermintaanDiagnosis(k.EncounterId, inpEpisodeId: k.EpisodeId));

            var sesudahAdaDiagnosis = await BuatControllerKajian(context, k.DokterUserId)
                .CompleteAssessment(kajian.Id, new CompletePatientAssessmentRequest());

            Assert.Equal(200, ControllerTestHarness.KodeStatus(sesudahAdaDiagnosis));

            var kajianSelesai = await context.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .SingleAsync(x => x.Id == kajian.Id);

            Assert.Equal(PatientAssessmentStatus.Completed, kajianSelesai.AssessmentStatus);
            Assert.Null(kajianSelesai.WorkingDiagnosis);
        }

        /// <summary>
        /// Mengambil isi <c>ApiResponse&lt;T&gt;.Data</c> dari balasan controller.
        /// </summary>
        private static T AmbilData<T>(Microsoft.AspNetCore.Mvc.IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);

            Assert.NotNull(pembungkus.Data);

            return pembungkus.Data!;
        }
    }
}
