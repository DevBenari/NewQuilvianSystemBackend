using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-053</c> — DPJP memverifikasi catatan profesi lain, dan
    /// keterlambatannya terpantau.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Verifikasi memantau, ia tidak menahan.</b> Catatan perawat yang belum diverifikasi
    /// tetap sah dan tetap terbaca, dan catatan berikutnya tetap dapat ditulis. Menjadikan
    /// verifikasi sebagai gerbang pelayanan akan menghentikan pendokumentasian setiap kali DPJP
    /// sedang di kamar operasi — bahaya yang jauh lebih besar daripada catatan yang belum
    /// terbaca.
    /// </para>
    /// <para>
    /// <b>Tidak satu angka batas waktu pun ditanam di kode.</b> Nilai batasnya
    /// <c>RWI-RULE-021</c> belum disahkan karena pemilik klinisnya belum ditunjuk. Mekanismenya
    /// dibangun penuh dan berjalan dengan kebijakan kosong, dan uji kriteria 4 adalah buktinya.
    /// </para>
    /// </remarks>
    public class CpptVerificationTests
    {
        private static PatientIntegratedProgressNoteController BuatControllerCppt(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientIntegratedProgressNoteController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ClinicalDocumentIntegrityService(c),
                new CpptVerificationService(c, new InpatientClinicalContextService(c)))
                .DenganPengguna(actorUserId);

        private static ClinicalNoteAddendumController BuatControllerKoreksi(
            ApplicationDbContext c, Guid actorUserId) =>
            new ClinicalNoteAddendumController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ClinicalNoteAddendumService(c, new ClinicalDocumentIntegrityService(c)),
                accessPermissionService: null!,
                new InpatientDocumentCorrectionAuthorityService(c),
                new CpptVerificationService(c, new InpatientClinicalContextService(c)))
                .DenganPengguna(actorUserId);

        private static T Isi<T>(IActionResult hasil)
        {
            var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
            var pembungkus = Assert.IsType<ApiResponse<T>>(objek.Value);

            Assert.NotNull(pembungkus.Data);
            return pembungkus.Data!;
        }

        /// <summary>
        /// Menulis satu catatan terpadu atas nama profesi tertentu.
        /// </summary>
        /// <remarks>
        /// Penanda perawatan diisi langsung, karena pengisian otomatisnya adalah pekerjaan
        /// task lain. Yang diuji di sini adalah verifikasinya.
        /// </remarks>
        private static TrxPatientIntegratedProgressNote TulisCatatan(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k,
            Guid penulisUserId,
            string profesi = "Nurse",
            DateTime? waktuCatatan = null,
            CpptVerificationStatus status = CpptVerificationStatus.Pending,
            DateTime? batasVerifikasi = null)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var catatan = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{pembeda}",
                PatientId = k.PatientId,
                EncounterId = k.EncounterId,
                InpEpisodeId = k.EpisodeId,
                ProfessionType = profesi,
                ProviderUserId = penulisUserId,
                NoteDateTime = waktuCatatan ?? DateTime.UtcNow.AddHours(-2),
                SubjectiveSummary = "Pasien mengeluh nyeri ringan",
                VerificationStatus = status,
                VerificationDueAt = batasVerifikasi,
                IsActive = true
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(catatan);
            context.SaveChanges();

            return catatan;
        }

        /// <summary>
        /// Mengganti DPJP perawatan: penugasan lama diakhiri, penugasan baru dimulai.
        /// </summary>
        private static (Guid DokterBaruUserId, Guid DokterBaruMasterId) GantiDpjp(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k)
        {
            var sekarang = DateTime.UtcNow;

            var lama = context.Set<InpDoctorAssignment>().Single(x => x.EpisodeId == k.EpisodeId);
            lama.EndDateTime = sekarang.AddMinutes(-1);

            var penggantiPengguna = RekamMedisTestData.BuatPengguna(context, "dpjpbaru");
            var penggantiMaster = RawatInapTestData.BuatDokterMaster(context);
            penggantiPengguna.WorkforceProfileId = penggantiMaster.WorkforceProfileId;

            context.Set<InpDoctorAssignment>().Add(new InpDoctorAssignment
            {
                EpisodeId = k.EpisodeId,
                DoctorId = penggantiMaster.Id,
                SequenceNumber = 2,
                StartDateTime = sekarang.AddMinutes(-1),
                EndDateTime = null,
                AssignedByUserId = k.DokterUserId,
                IsActive = true
            });

            context.SaveChanges();

            return (penggantiPengguna.Id, penggantiMaster.Id);
        }

        // =====================================================================
        // Kriteria 1 — verifikasi tidak mengubah penulis asli
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-053 AC 1</c>, <c>INV-DOK-11</c>, <c>AC-CAP021-03</c> — verifikasi tidak
        /// mengubah penulis asli, dan verifikatornya tersimpan pada kolom terpisah.
        /// </summary>
        /// <remarks>
        /// Penulis yang berpindah saat diverifikasi adalah pemalsuan rekam medis. Perawat yang
        /// menulis catatan tetap bertanggung jawab atas isinya; DPJP bertanggung jawab atas
        /// pernyataan bahwa ia sudah membacanya. Dua tanggung jawab berbeda, dua kolom berbeda.
        /// </remarks>
        [Fact]
        public async Task Verifikasi_TidakMengubahPenulisAsliDanMenyimpanVerifikatorTerpisah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var catatan = TulisCatatan(context, k, perawat.Id);

            var hasil = await BuatControllerCppt(context, k.DokterUserId)
                .VerifyProgressNote(catatan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();

            var sesudah = verifikasi.Set<TrxPatientIntegratedProgressNote>()
                .Single(x => x.Id == catatan.Id);

            Assert.Equal(CpptVerificationStatus.Verified, sesudah.VerificationStatus);
            Assert.Equal(k.DokterUserId, sesudah.VerifiedByUserId);
            Assert.NotNull(sesudah.VerifiedAt);

            // Penulis aslinya tidak bergeser satu huruf pun.
            Assert.Equal(perawat.Id, sesudah.ProviderUserId);
            Assert.NotEqual(sesudah.ProviderUserId, sesudah.VerifiedByUserId);
            Assert.Equal("Nurse", sesudah.ProfessionType);
            Assert.Equal("Pasien mengeluh nyeri ringan", sesudah.SubjectiveSummary);
        }

        // =====================================================================
        // Kriteria 2 — DPJP yang aktif saat verifikasi, bukan saat catatan ditulis
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-053 AC 2</c>, <c>RWI-RULE-030</c> — setelah pergantian DPJP, DPJP lama
        /// ditolak dan DPJP baru diterima, walaupun catatan itu ditulis pada masa DPJP lama.
        /// </summary>
        /// <remarks>
        /// Yang dinilai adalah kewenangan pada <b>saat verifikasi</b>. DPJP yang menerima alih
        /// rawat hari ini bertanggung jawab atas pasiennya, termasuk atas catatan yang ditulis
        /// sebelum ia mengambil alih; DPJP lama justru sudah tidak berwenang lagi.
        /// </remarks>
        [Fact]
        public async Task SetelahPergantianDpjp_DpjpLamaDitolakDanDpjpBaruDiterima()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            // Catatan ditulis pada masa DPJP lama.
            var catatan = TulisCatatan(context, k, perawat.Id, waktuCatatan: DateTime.UtcNow.AddHours(-6));

            var (dpjpBaruUserId, _) = GantiDpjp(context, k);

            var olehDpjpLama = await BuatControllerCppt(context, k.DokterUserId)
                .VerifyProgressNote(catatan.Id);

            Assert.Equal(403, ControllerTestHarness.KodeStatus(olehDpjpLama));
            Assert.Equal(
                "Verifikasi hanya dapat dilakukan DPJP pasien ini.",
                ControllerTestHarness.Pesan(olehDpjpLama));

            var olehDpjpBaru = await BuatControllerCppt(context, dpjpBaruUserId)
                .VerifyProgressNote(catatan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(olehDpjpBaru));

            using var verifikasi = database.CreateContext();

            var sesudah = verifikasi.Set<TrxPatientIntegratedProgressNote>()
                .Single(x => x.Id == catatan.Id);

            Assert.Equal(dpjpBaruUserId, sesudah.VerifiedByUserId);
        }

        // =====================================================================
        // Kriteria 3 — bukan DPJP ditolak
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-053 AC 3</c>, <c>VAL-DOK-07</c> — dokter jaga yang bukan DPJP perawatan itu
        /// ditolak <c>403</c>.
        /// </summary>
        /// <remarks>
        /// Memberikan verifikasi kepada dokter jaga membuat verifikasi kehilangan artinya: yang
        /// diverifikasi adalah catatan yang menjadi tanggung jawab DPJP. Penolakannya diturunkan
        /// dari penugasan pada perawatan, bukan dari nama peran.
        /// </remarks>
        [Fact]
        public async Task DokterJagaYangBukanDpjp_Ditolak403()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var catatan = TulisCatatan(context, k, perawat.Id);

            var dokterJaga = RekamMedisTestData.BuatPengguna(context, "dokterjaga");
            var dokterJagaMaster = RawatInapTestData.BuatDokterMaster(context);
            dokterJaga.WorkforceProfileId = dokterJagaMaster.WorkforceProfileId;
            context.SaveChanges();

            var hasil = await BuatControllerCppt(context, dokterJaga.Id)
                .VerifyProgressNote(catatan.Id);

            Assert.Equal(403, ControllerTestHarness.KodeStatus(hasil));

            using var verifikasi = database.CreateContext();

            var sesudah = verifikasi.Set<TrxPatientIntegratedProgressNote>()
                .Single(x => x.Id == catatan.Id);

            Assert.Equal(CpptVerificationStatus.Pending, sesudah.VerificationStatus);
            Assert.Null(sesudah.VerifiedByUserId);
        }

        // =====================================================================
        // Kriteria 4 — kebijakan verifikasi kosong
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-053 AC 4</c>, <c>VAL-DOK-24</c> — dengan kebijakan verifikasi kosong,
        /// seluruh catatan berstatus tidak-diwajibkan, daftar pantau kosong, dan pencatatan
        /// berjalan penuh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Inilah keadaan setiap rumah sakit hari ini: nilai batas waktunya belum disahkan.
        /// Bawaan <c>NotRequired</c> dipilih dengan sadar; bawaan <c>Pending</c> akan membuat
        /// setiap catatan perawat langsung terhitung menunggu verifikasi pada rumah sakit yang
        /// tidak mewajibkannya, dan daftar pantau penuh sejak hari pertama.
        /// </para>
        /// <para>
        /// Yang juga dibuktikan: catatan berikutnya tetap dapat ditulis. Kebijakan kosong tidak
        /// menahan apa pun.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task KebijakanVerifikasiKosong_DaftarPantauKosongDanPencatatanBerjalanPenuh()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            // Tiga catatan lahir dengan bawaan apa adanya: tidak diwajibkan, tanpa batas waktu.
            for (var i = 0; i < 3; i++)
            {
                var tulis = await BuatControllerCppt(context, perawat.Id)
                    .CreateProgressNote(new CreatePatientIntegratedProgressNoteRequest
                    {
                        PatientId = k.PatientId,
                        EncounterId = k.EncounterId,
                        ProfessionType = "Nurse",
                        ProviderUserId = perawat.Id,
                        SubjectiveSummary = $"Catatan ke-{i + 1}"
                    });

                Assert.Equal(200, ControllerTestHarness.KodeStatus(tulis));
            }

            Assert.Equal(3, context.Set<TrxPatientIntegratedProgressNote>().Count());

            foreach (var catatan in context.Set<TrxPatientIntegratedProgressNote>().ToList())
            {
                Assert.Equal(CpptVerificationStatus.NotRequired, catatan.VerificationStatus);
                Assert.Null(catatan.VerificationDueAt);

                // Penanda perawatan diisi supaya daftar pantau perawatan ini benar-benar
                // memeriksa ketiga catatan itu, bukan daftar yang kebetulan kosong.
                catatan.InpEpisodeId = k.EpisodeId;
            }

            context.SaveChanges();

            var keadaan = Isi<CpptVerificationStatusSummary>(
                await BuatControllerCppt(context, k.DokterUserId)
                    .GetVerificationStatusByEpisode(k.EpisodeId));

            Assert.Equal(3, keadaan.TotalNoteCount);
            Assert.Equal(3, keadaan.NotRequiredCount);
            Assert.Equal(0, keadaan.PendingCount);
            Assert.Equal(0, keadaan.OverdueCount);
            Assert.Empty(keadaan.WatchList);
            Assert.True(keadaan.IsVerificationPolicyEmpty);
        }

        // =====================================================================
        // Kriteria 5 — lewat batas terpantau, tidak menahan
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-053 AC 5</c>, <c>VAL-DOK-25</c> — catatan yang lewat batas muncul pada
        /// daftar pantau dan <b>tidak menahan</b> penulisan catatan berikutnya.
        /// </summary>
        /// <remarks>
        /// Batas waktunya diisi langsung pada data uji, bukan dihitung kode. Itu disengaja:
        /// nilainya belum disahkan, dan menanam angka bawaan berarti mengarang kebijakan klinis.
        /// Yang diuji adalah mekanismenya — bahwa keterlambatan terbaca ketika batasnya ada.
        /// </remarks>
        [Fact]
        public async Task CatatanLewatBatas_MunculPadaDaftarPantauTanpaMenahanCatatanBerikutnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var terlambat = TulisCatatan(
                context, k, perawat.Id,
                waktuCatatan: DateTime.UtcNow.AddHours(-30),
                status: CpptVerificationStatus.Pending,
                batasVerifikasi: DateTime.UtcNow.AddHours(-6));

            var keadaan = Isi<CpptVerificationStatusSummary>(
                await BuatControllerCppt(context, k.DokterUserId)
                    .GetVerificationStatusByEpisode(k.EpisodeId));

            Assert.Equal(1, keadaan.PendingCount);
            Assert.Equal(1, keadaan.OverdueCount);
            Assert.False(keadaan.IsVerificationPolicyEmpty);

            var baris = Assert.Single(keadaan.WatchList);
            Assert.Equal(terlambat.Id, baris.NoteId);
            Assert.True(baris.IsOverdue);

            // Keterlambatan TIDAK menahan catatan berikutnya.
            var berikutnya = await BuatControllerCppt(context, perawat.Id)
                .CreateProgressNote(new CreatePatientIntegratedProgressNoteRequest
                {
                    PatientId = k.PatientId,
                    EncounterId = k.EncounterId,
                    ProfessionType = "Nurse",
                    ProviderUserId = perawat.Id,
                    SubjectiveSummary = "Catatan berikutnya tetap dapat ditulis"
                });

            Assert.Equal(200, ControllerTestHarness.KodeStatus(berikutnya));
            Assert.Equal(2, context.Set<TrxPatientIntegratedProgressNote>().Count());
        }

        // =====================================================================
        // Kriteria 6 — koreksi mengembalikan ke menunggu verifikasi
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-053 AC 6</c>, <c>state-transition-matrix.md</c> bagian 3 — koreksi atas
        /// catatan yang sudah terverifikasi mengembalikannya ke keadaan menunggu verifikasi.
        /// </summary>
        /// <remarks>
        /// Verifikasi menyatakan "saya sudah membaca isi ini". Begitu isinya bertambah lewat
        /// koreksi, pernyataan itu berhenti berlaku. Membiarkannya tetap terverifikasi berarti
        /// menampilkan tanda tangan DPJP atas isi yang belum pernah ia baca.
        /// </remarks>
        [Fact]
        public async Task KoreksiCatatanTerverifikasi_MengembalikannyaKeMenungguVerifikasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var catatan = TulisCatatan(context, k, perawat.Id);

            // Catatan didaftarkan sebagai dokumen tertanda tangan supaya ia berada pada keadaan
            // yang menerima koreksi.
            await new ClinicalDocumentIntegrityService(context).RegisterSignedAsync(
                ClinicalDocumentKind.ProgressNote,
                catatan.Id,
                k.PatientId,
                k.EncounterId,
                perawat.Id,
                deviceInfo: "uji",
                ipAddress: "127.0.0.1",
                nowUtc: DateTime.UtcNow);
            await context.SaveChangesAsync();

            var verifikasiHasil = await BuatControllerCppt(context, k.DokterUserId)
                .VerifyProgressNote(catatan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(verifikasiHasil));

            var koreksi = await BuatControllerKoreksi(context, perawat.Id).Create(
                ClinicalDocumentKind.ProgressNote,
                catatan.Id,
                new CreateClinicalNoteAddendumRequest
                {
                    AddendumText = "Nyeri ternyata sedang, bukan ringan.",
                    CorrectionReason = "Salah menilai derajat nyeri"
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(koreksi));

            using var pemeriksaan = database.CreateContext();

            var sesudah = pemeriksaan.Set<TrxPatientIntegratedProgressNote>()
                .Single(x => x.Id == catatan.Id);

            Assert.Equal(CpptVerificationStatus.Pending, sesudah.VerificationStatus);
            Assert.Null(sesudah.VerifiedAt);
            Assert.Null(sesudah.VerifiedByUserId);

            // Penulis aslinya tetap perawat, bukan berpindah ke pembuat koreksi.
            Assert.Equal(perawat.Id, sesudah.ProviderUserId);

            var keadaan = Isi<CpptVerificationStatusSummary>(
                await BuatControllerCppt(pemeriksaan, k.DokterUserId)
                    .GetVerificationStatusByEpisode(k.EpisodeId));

            Assert.Equal(1, keadaan.PendingCount);
            Assert.Equal(0, keadaan.VerifiedCount);
        }

        /// <summary>
        /// Regresi <c>BE-RWI-053</c> — koreksi atas catatan yang <b>tidak diwajibkan</b>
        /// diverifikasi tidak menaikkannya menjadi menunggu verifikasi.
        /// </summary>
        /// <remarks>
        /// Rumah sakit yang tidak mewajibkan verifikasi tidak boleh tiba-tiba punya daftar
        /// pantau hanya karena seseorang membetulkan salah ketik.
        /// </remarks>
        [Fact]
        public async Task KoreksiCatatanTidakDiwajibkan_TidakMenaikkannyaKeMenungguVerifikasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var catatan = TulisCatatan(
                context, k, perawat.Id, status: CpptVerificationStatus.NotRequired);

            await new ClinicalDocumentIntegrityService(context).RegisterSignedAsync(
                ClinicalDocumentKind.ProgressNote,
                catatan.Id,
                k.PatientId,
                k.EncounterId,
                perawat.Id,
                deviceInfo: "uji",
                ipAddress: "127.0.0.1",
                nowUtc: DateTime.UtcNow);
            await context.SaveChangesAsync();

            var koreksi = await BuatControllerKoreksi(context, perawat.Id).Create(
                ClinicalDocumentKind.ProgressNote,
                catatan.Id,
                new CreateClinicalNoteAddendumRequest
                {
                    AddendumText = "Menambahkan hasil pemeriksaan susulan.",
                    CorrectionReason = "Melengkapi catatan"
                });

            Assert.Equal(201, ControllerTestHarness.KodeStatus(koreksi));

            using var pemeriksaan = database.CreateContext();

            var sesudah = pemeriksaan.Set<TrxPatientIntegratedProgressNote>()
                .Single(x => x.Id == catatan.Id);

            Assert.Equal(CpptVerificationStatus.NotRequired, sesudah.VerificationStatus);
        }

        // =====================================================================
        // Nama penanda aksi dan penanda hak akses
        // =====================================================================

        /// <summary>
        /// Pelajaran <c>BE-RWI-034</c> — nama pada penanda aksi dan penanda hak akses
        /// <c>Verify</c> <b>sama persis</b>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-034</c> pernah mengunci sembilan endpoint karena nama pada kedua penanda
        /// berbeda, sehingga seluruh peran selain SuperAdmin menerima <c>403</c> permanen yang
        /// tidak dapat diperbaiki dari layar Akses Role. Uji ini menutup jalan itu berulang.
        /// </remarks>
        [Fact]
        public void PenandaAksiDanPenandaHakAksesVerify_BernamaSamaPersis()
        {
            var metode = typeof(PatientIntegratedProgressNoteController)
                .GetMethod(nameof(PatientIntegratedProgressNoteController.VerifyProgressNote));

            Assert.NotNull(metode);

            var aksi = metode!
                .GetCustomAttributes(typeof(QuilvianSystemBackend.Attributes.AccessActionAttribute), false)
                .Cast<QuilvianSystemBackend.Attributes.AccessActionAttribute>()
                .Single();

            var izin = metode
                .GetCustomAttributes(typeof(QuilvianSystemBackend.Attributes.AccessPermissionAttribute), false)
                .Cast<QuilvianSystemBackend.Attributes.AccessPermissionAttribute>()
                .Single();

            // AccessPermissionAttribute menyimpan pasangan Resource-Action pada Arguments,
            // bukan sebagai properti bernama. Yang diperiksa tetap nilainya, apa adanya.
            Assert.Equal("Verify", aksi.ActionName);
            Assert.Equal("PatientIntegratedProgressNote", izin.Arguments![0]);
            Assert.Equal("Verify", izin.Arguments[1]);
        }
    }
}
