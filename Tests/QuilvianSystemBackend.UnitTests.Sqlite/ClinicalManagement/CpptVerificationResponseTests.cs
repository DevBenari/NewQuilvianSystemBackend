using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.ClinicalManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-RWI-066</c> - balasan catatan terpadu menyebutkan siapa yang
    /// memverifikasi dan kapan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Keempat kolom verifikasi sudah tersimpan sejak <c>BE-RWI-040</c> dan sudah terisi sejak
    /// <c>BE-RWI-053</c>, tetapi tidak satu pun ikut terkirim ke layar. Akibatnya layar hanya
    /// dapat berkata "status verifikasi belum dapat dipastikan", dan itulah yang menahan
    /// <c>FE-RWI-046</c>.
    /// </para>
    /// <para>
    /// <b>Penulis dan verifikator adalah dua orang.</b> Seluruh uji di berkas ini menaruh nama
    /// yang berbeda pada keduanya, lalu memeriksa keduanya terbaca pada dua kolom yang berbeda -
    /// <c>INV-DOK-11</c>. Balasan yang menampilkan nama penulis di kolom verifikator berarti
    /// memamerkan tanda tangan atas bacaan yang tidak pernah terjadi.
    /// </para>
    /// </remarks>
    public class CpptVerificationResponseTests
    {
        private static PatientIntegratedProgressNoteController BuatController(
            ApplicationDbContext c, Guid actorUserId) =>
            new PatientIntegratedProgressNoteController(
                c,
                ControllerTestHarness.BuatLoggerService(actorUserId),
                new ClinicalDocumentIntegrityService(c),
                new CpptVerificationService(c, new InpatientClinicalContextService(c)),
                new InpatientClinicalContextService(c))
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
        private static TrxPatientIntegratedProgressNote TulisCatatan(
            ApplicationDbContext context,
            RawatInapTestData.Konteks k,
            Guid penulisUserId,
            string profesi = "Nurse",
            CpptVerificationStatus status = CpptVerificationStatus.Pending,
            DateTime? batasVerifikasi = null,
            Guid? verifikatorUserId = null,
            DateTime? waktuVerifikasi = null,
            DateTime? waktuCatatan = null)
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
                VerifiedByUserId = verifikatorUserId,
                VerifiedAt = waktuVerifikasi,
                IsActive = true
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(catatan);
            context.SaveChanges();

            return catatan;
        }

        // =====================================================================
        // Kriteria 1, 2, dan 3 - balasan verifikasi membawa keempat kolom beserta namanya
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-066 AC 1, AC 2, AC 3</c> - balasan <c>PATCH /{id}/verify</c> menyebutkan
        /// verifikator beserta waktunya, dan penulis aslinya tetap berdiri di kolomnya sendiri.
        /// </summary>
        /// <remarks>
        /// Inilah contoh yang ditulis pada kartu task: catatan ditulis perawat lalu diverifikasi
        /// dokter. Balasannya harus menyebut <b>dua nama pada dua kolom</b>, sehingga layar tidak
        /// mungkin menimpa yang satu dengan yang lain.
        /// </remarks>
        [Fact]
        public async Task Verify_MembalasNamaVerifikatorDanNamaPenulisPadaDuaKolomBerbeda()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var catatan = TulisCatatan(context, k, perawat.Id);

            var dokter = context.Set<ApplicationUser>().Single(x => x.Id == k.DokterUserId);

            var hasil = await BuatController(context, k.DokterUserId)
                .VerifyProgressNote(catatan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var balasan = Isi<PatientIntegratedProgressNoteResponse>(hasil);

            // Kolom verifikasi ikut terkirim - AC 1.
            Assert.Equal(CpptVerificationStatus.Verified, balasan.VerificationStatus);
            Assert.NotNull(balasan.VerifiedAt);
            Assert.Equal(k.DokterUserId, balasan.VerifiedByUserId);

            // Verifikator disebut NAMANYA, bukan hanya nomor pengguna - AC 2.
            Assert.Equal(dokter.DisplayName, balasan.VerifiedByUserName);

            // Penulis aslinya tetap penulis - AC 3, INV-DOK-11.
            Assert.Equal(perawat.Id, balasan.ProviderUserId);
            Assert.Equal(perawat.DisplayName, balasan.ProviderUserName);

            // Dua kolom, dua nama, dan keduanya memang berbeda.
            Assert.NotEqual(balasan.ProviderUserName, balasan.VerifiedByUserName);
            Assert.NotEqual(balasan.ProviderUserId, balasan.VerifiedByUserId);
        }

        /// <summary>
        /// <c>BE-RWI-066 AC 3</c>, <c>INV-DOK-11</c> - verifikasi tidak menggeser satu pun kolom
        /// penulis, baik pada balasannya maupun pada barisnya.
        /// </summary>
        [Fact]
        public async Task Verify_TidakMengubahSatuPunKolomPenulisSebelumDanSesudah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var catatan = TulisCatatan(context, k, perawat.Id);

            catatan.ProviderDisplayNameSnapshot = "Ns. Sari";
            catatan.ProviderRoleSnapshot = "Perawat Pelaksana";
            context.SaveChanges();

            var sebelum = context.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Single(x => x.Id == catatan.Id);

            var hasil = await BuatController(context, k.DokterUserId)
                .VerifyProgressNote(catatan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            using var pemeriksa = database.CreateContext();

            var sesudah = pemeriksa.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Single(x => x.Id == catatan.Id);

            Assert.Equal(sebelum.ProviderUserId, sesudah.ProviderUserId);
            Assert.Equal(sebelum.ProviderDisplayNameSnapshot, sesudah.ProviderDisplayNameSnapshot);
            Assert.Equal(sebelum.ProviderRoleSnapshot, sesudah.ProviderRoleSnapshot);
            Assert.Equal(sebelum.ProfessionType, sesudah.ProfessionType);
            Assert.Equal(sebelum.ProfessionName, sesudah.ProfessionName);
            Assert.Equal(sebelum.DoctorId, sesudah.DoctorId);

            var balasan = Isi<PatientIntegratedProgressNoteResponse>(hasil);
            Assert.Equal("Ns. Sari", balasan.ProviderDisplayNameSnapshot);
            Assert.Equal(perawat.DisplayName, balasan.ProviderUserName);
        }

        /// <summary>
        /// <c>BE-RWI-066 AC 1</c> - lini masa satu perawatan membawa nilai verifikasi yang
        /// <b>sama persis</b> dengan yang tersimpan pada barisnya.
        /// </summary>
        [Fact]
        public async Task LiniMasaPerawatan_MembawaNilaiVerifikasiSamaPersisDenganBarisnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var verifikator = RekamMedisTestData.BuatPengguna(context, "dpjp");

            var waktuVerifikasi = new DateTime(2026, 9, 8, 7, 30, 0, DateTimeKind.Utc);
            var batas = new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc);

            var catatan = TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.Verified,
                batasVerifikasi: batas,
                verifikatorUserId: verifikator.Id,
                waktuVerifikasi: waktuVerifikasi);

            var hasil = await BuatController(context, k.DokterUserId)
                .GetByEpisode(k.EpisodeId);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var halaman = Isi<PagedResult<PatientIntegratedProgressNoteResponse>>(hasil);
            var baris = Assert.Single(halaman.Items);

            Assert.Equal(catatan.VerificationStatus, baris.VerificationStatus);
            Assert.Equal(catatan.VerifiedByUserId, baris.VerifiedByUserId);
            Assert.Equal(verifikator.DisplayName, baris.VerifiedByUserName);
            Assert.Equal(waktuVerifikasi, baris.VerifiedAt);
            Assert.Equal(batas, baris.VerificationDueAt);
        }

        /// <summary>
        /// <c>BE-RWI-066 AC 1</c> - <c>GET /timeline</c> ikut membawa keempat kolomnya.
        /// </summary>
        [Fact]
        public async Task Timeline_IkutMembawaKeadaanVerifikasiBesertaNamanya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");
            var verifikator = RekamMedisTestData.BuatPengguna(context, "dpjp");

            TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.Verified,
                verifikatorUserId: verifikator.Id,
                waktuVerifikasi: DateTime.UtcNow.AddMinutes(-30));

            var hasil = await BuatController(context, k.DokterUserId)
                .GetTimeline(k.PatientId, null, null, null, null);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var daftar = Isi<List<PatientIntegratedProgressNoteTimelineResponse>>(hasil);
            var baris = Assert.Single(daftar);

            Assert.Equal(CpptVerificationStatus.Verified, baris.VerificationStatus);
            Assert.Equal(verifikator.Id, baris.VerifiedByUserId);
            Assert.Equal(verifikator.DisplayName, baris.VerifiedByUserName);
            Assert.NotNull(baris.VerifiedAt);
        }

        // =====================================================================
        // Kriteria 4 dan 5 - kosong tetap kosong, dan tidak-diwajibkan tetap terbaca
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-066 AC 4</c> - catatan yang belum diverifikasi mengembalikan verifikator dan
        /// waktunya <b>kosong</b>.
        /// </summary>
        /// <remarks>
        /// Bukan <c>0001-01-01</c>, dan bukan teks kosong. Kolom bernama "Verifikator" yang berisi
        /// teks kosong terbaca sebagai nama yang gagal dimuat, dan layar kehilangan cara
        /// membedakannya dari catatan yang memang belum diverifikasi.
        /// </remarks>
        [Fact]
        public async Task BelumDiverifikasi_MengembalikanVerifikatorDanWaktunyaKosong()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var catatan = TulisCatatan(context, k, perawat.Id);

            var hasil = await BuatController(context, k.DokterUserId).GetById(catatan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var balasan = Isi<PatientIntegratedProgressNoteDetailResponse>(hasil);

            Assert.Equal(CpptVerificationStatus.Pending, balasan.VerificationStatus);
            Assert.Null(balasan.VerifiedAt);
            Assert.Null(balasan.VerifiedByUserId);
            Assert.Null(balasan.VerifiedByUserName);

            // Penulisnya tetap tersebut; yang kosong hanya verifikatornya.
            Assert.Equal(perawat.DisplayName, balasan.ProviderUserName);
        }

        /// <summary>
        /// <c>BE-RWI-066 AC 5</c> - catatan pada rumah sakit yang tidak mewajibkan verifikasi
        /// mengembalikan <c>NotRequired</c>, sehingga layar dapat membedakan "tidak diwajibkan"
        /// dari "sudah diverifikasi" tanpa menebak.
        /// </summary>
        [Fact]
        public async Task KebijakanTidakAktif_MengembalikanNotRequiredBukanDiverifikasi()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var catatan = TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.NotRequired);

            var hasil = await BuatController(context, k.DokterUserId).GetById(catatan.Id);

            var balasan = Isi<PatientIntegratedProgressNoteDetailResponse>(hasil);

            Assert.Equal(CpptVerificationStatus.NotRequired, balasan.VerificationStatus);
            Assert.Null(balasan.VerificationDueAt);
            Assert.Null(balasan.VerifiedByUserName);
            Assert.NotEqual(CpptVerificationStatus.Verified, balasan.VerificationStatus);
        }

        /// <summary>
        /// <c>BE-RWI-066 AC 2</c> - verifikator yang barisnya tidak dapat dikenali menghasilkan
        /// nama <b>kosong</b>, bukan nama penulis dan bukan nomor pengguna.
        /// </summary>
        /// <remarks>
        /// Tidak ada jenjang cadangan ke penulis. Menjatuhkan nama verifikator ke nama penulis
        /// akan menyatakan bahwa seseorang membaca dan menandatangani tulisannya sendiri.
        /// </remarks>
        [Fact]
        public async Task VerifikatorTidakDikenali_NamanyaKosongDanBukanNamaPenulis()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            // Catatan tercatat terverifikasi, tetapi barisnya tidak menunjuk pengguna mana pun.
            var catatan = TulisCatatan(
                context, k, perawat.Id,
                status: CpptVerificationStatus.Verified,
                verifikatorUserId: null,
                waktuVerifikasi: DateTime.UtcNow.AddMinutes(-10));

            var hasil = await BuatController(context, k.DokterUserId).GetById(catatan.Id);

            var balasan = Isi<PatientIntegratedProgressNoteDetailResponse>(hasil);

            Assert.Equal(CpptVerificationStatus.Verified, balasan.VerificationStatus);
            Assert.Null(balasan.VerifiedByUserName);
            Assert.NotEqual(balasan.ProviderUserName, balasan.VerifiedByUserName);
        }

        // =====================================================================
        // Kriteria 6 - jalur non-rawat-inap tidak berubah
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-066 AC 6</c>, <c>RWI-AC-143</c> - catatan di luar rawat inap tetap
        /// mengembalikan seluruh kolom lamanya persis seperti sebelumnya.
        /// </summary>
        /// <remarks>
        /// Kolom verifikasinya ikut terkirim dan berbunyi tidak-diwajibkan, karena memang begitu
        /// nilainya tersimpan. Yang dilarang adalah kolom lama yang berubah, hilang, atau
        /// berganti arti.
        /// </remarks>
        [Fact]
        public async Task CatatanDiLuarRawatInap_KolomLamanyaTidakBerubahSatuPun()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(context);
            var dokterPengguna = RekamMedisTestData.BuatPengguna(context, "dokterpoli");

            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var catatan = new TrxPatientIntegratedProgressNote
            {
                ProgressNoteNumber = $"CPPT-{pembeda}",
                PatientId = k.PatientId,
                EncounterId = k.EncounterId,

                // Tanpa penanda perawatan: inilah bentuk catatan poliklinik, MCU, dan IGD.
                InpEpisodeId = null,
                ProfessionType = "Doctor",
                ProfessionName = "Dokter",
                ProviderUserId = dokterPengguna.Id,
                ProviderDisplayNameSnapshot = "dr. Poli",
                NoteDateTime = DateTime.UtcNow.AddHours(-1),
                SubjectiveSummary = "Batuk dua hari",
                NoteText = "Batuk dua hari",
                IsActive = true
            };

            context.Set<TrxPatientIntegratedProgressNote>().Add(catatan);
            context.SaveChanges();

            var hasil = await BuatController(context, dokterPengguna.Id).GetById(catatan.Id);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var balasan = Isi<PatientIntegratedProgressNoteDetailResponse>(hasil);

            // Kolom lama tetap apa adanya.
            Assert.Equal(catatan.ProgressNoteNumber, balasan.ProgressNoteNumber);
            Assert.Equal(k.PatientId, balasan.PatientId);
            Assert.Equal(k.EncounterId, balasan.EncounterId);
            Assert.Null(balasan.InpEpisodeId);
            Assert.Equal("Doctor", balasan.ProfessionType);
            Assert.Equal("Dokter", balasan.ProfessionName);
            Assert.Equal(dokterPengguna.Id, balasan.ProviderUserId);
            Assert.Equal(dokterPengguna.DisplayName, balasan.ProviderUserName);
            Assert.Equal("dr. Poli", balasan.ProviderDisplayNameSnapshot);
            Assert.Equal("Batuk dua hari", balasan.SubjectiveSummary);
            Assert.True(balasan.IsActive);

            // Kolom verifikasinya tidak diwajibkan, dan tidak satu pun terbaca sebagai orang.
            Assert.Equal(CpptVerificationStatus.NotRequired, balasan.VerificationStatus);
            Assert.Null(balasan.VerifiedAt);
            Assert.Null(balasan.VerifiedByUserId);
            Assert.Null(balasan.VerifiedByUserName);
            Assert.Null(balasan.VerificationDueAt);
        }

        // =====================================================================
        // Risiko jumlah query - relasi verifikator tidak boleh melahirkan N+1
        // =====================================================================

        /// <summary>
        /// <c>BE-RWI-066</c> baris Risk - membaca banyak catatan sekaligus tidak melahirkan satu
        /// query tambahan per baris, dan setiap nama verifikatornya tetap terbaca.
        /// </summary>
        /// <remarks>
        /// Lini masa satu perawatan bisa memuat ratusan catatan. Bila relasi verifikator terbaca
        /// per baris, satu pembacaan lini masa berubah menjadi ratusan perjalanan ke basis data.
        /// Uji ini menghitung perintah SQL yang benar-benar dijalankan, bukan sekadar memeriksa
        /// namanya terisi.
        /// </remarks>
        [Fact]
        public async Task LiniMasaBanyakCatatan_SatuPembacaanTanpaQueryTambahanPerBaris()
        {
            using var database = TestDatabase.Create();
            using var penyiap = database.CreateContext();

            var k = RawatInapTestData.SiapkanPerawatan(penyiap);
            var perawat = RekamMedisTestData.BuatPengguna(penyiap, "perawat");

            var verifikator = new List<ApplicationUser>();

            for (var i = 0; i < 5; i++)
            {
                var dpjp = RekamMedisTestData.BuatPengguna(penyiap, $"dpjp{i}");
                verifikator.Add(dpjp);

                TulisCatatan(
                    penyiap, k, perawat.Id,
                    status: CpptVerificationStatus.Verified,
                    verifikatorUserId: dpjp.Id,
                    waktuVerifikasi: DateTime.UtcNow.AddMinutes(-10 - i),
                    waktuCatatan: DateTime.UtcNow.AddHours(-5).AddMinutes(i));
            }

            var perintah = new List<string>();
            using var terpantau = database.CreateContext(baris => perintah.Add(baris));

            var hasil = await BuatController(terpantau, k.DokterUserId)
                .GetByEpisode(k.EpisodeId);

            Assert.Equal(200, ControllerTestHarness.KodeStatus(hasil));

            var halaman = Isi<PagedResult<PatientIntegratedProgressNoteResponse>>(hasil);

            Assert.Equal(5, halaman.Items.Count);

            // Setiap baris menyebut verifikatornya sendiri, bukan verifikator baris lain.
            foreach (var baris in halaman.Items)
            {
                var dpjp = verifikator.Single(x => x.Id == baris.VerifiedByUserId);
                Assert.Equal(dpjp.DisplayName, baris.VerifiedByUserName);
            }

            // Dua perintah saja: satu menghitung total, satu mengambil halamannya.
            // Bila relasi verifikator terbaca per baris, jumlahnya menjadi tujuh.
            var jumlahEksekusi = perintah.Count(x => x.Contains("Executed DbCommand"));

            Assert.Equal(2, jumlahEksekusi);
        }
    }
}
