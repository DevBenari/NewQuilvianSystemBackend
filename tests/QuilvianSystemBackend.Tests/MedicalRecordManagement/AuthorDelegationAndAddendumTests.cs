using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-05` (penetapan penulis berhalangan) dan `BE-06`
    /// (addendum).
    ///
    /// Menutup uji penerimaan `AT-RM-04`, `AT-RM-05`, `AT-RM-14`, `AT-RM-17`, `AT-RM-26`,
    /// `AT-RM-27`, dan `AT-RM-28`.
    /// </summary>
    public class AuthorDelegationAndAddendumTests
    {
        private static readonly DateTime Sekarang = new(2026, 8, 26, 10, 0, 0, DateTimeKind.Utc);

        private static ClinicalDocumentIntegrityService Keutuhan(ApplicationDbContext c) => new(c);
        private static ClinicalNoteAddendumService Addendum(ApplicationDbContext c)
            => new(c, Keutuhan(c));
        private static ClinicalNoteAuthorDelegationService Penetapan(ApplicationDbContext c) => new(c);

        /// <summary>
        /// Menyiapkan satu CPPT yang sudah ditandatangani penulisnya, siap dikoreksi.
        /// </summary>
        private static async Task<(Guid DocumentId, ApplicationUser Penulis)> SiapkanCpptTerkunci(
            ApplicationDbContext context)
        {
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var documentId = Guid.NewGuid();

            await Keutuhan(context).RegisterAsync(
                ClinicalDocumentKind.ProgressNote, documentId,
                konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            await Keutuhan(context).SignAsync(
                ClinicalDocumentKind.ProgressNote, documentId, penulis.Id, null, null, Sekarang);

            return (documentId, penulis);
        }

        // =====================================================================
        // AT-RM-04 — penulis mengoreksi catatannya sendiri
        // =====================================================================

        /// <summary>
        /// `AT-RM-04`: penulis menambah addendum pada catatannya yang sudah ditandatangani.
        ///
        /// Tiga hal yang dibuktikan sekaligus: addendum tersimpan dengan urutan 1, isi dokumen
        /// induk tidak berubah, dan status dokumen tetap `Signed`.
        /// </summary>
        [Fact]
        public async Task Penulis_DapatMenambahAddendumPadaCatatannyaYangTerkunci()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);

            var (hasil, addendum) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, penulis.Id,
                actorHasSubstituteAuthority: false,
                addendumText: "Dosis yang benar adalah 500 mg, bukan 5000 mg.",
                correctionReason: "Salah tulis dosis pada catatan awal.",
                deviceInfo: null, ipAddress: null, nowUtc: Sekarang.AddDays(2));

            Assert.True(hasil.IsAllowed);
            Assert.NotNull(addendum);
            Assert.Equal(1, addendum!.Sequence);
            Assert.Equal(penulis.Id, addendum.AuthorUserId);
            Assert.False(addendum.IsSubstituteAuthor);
            Assert.Null(addendum.DelegationId);

            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking().Single(x => x.DocumentId == documentId);

            // Status dokumen TIDAK berubah — addendum adalah lampiran, bukan perubahan keadaan.
            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(1, keutuhan.AddendumCount);
        }

        /// <summary>
        /// `AT-RM-17`: tiga addendum berturut-turut mendapat urutan 1, 2, 3, dan status dokumen
        /// tetap sama sepanjang ketiganya.
        /// </summary>
        [Fact]
        public async Task TigaAddendumBerturutTurut_MendapatUrutanBerurut()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);
            var service = Addendum(context);

            for (var i = 1; i <= 3; i++)
            {
                var (hasil, addendum) = await service.CreateAsync(
                    ClinicalDocumentKind.ProgressNote, documentId, penulis.Id, false,
                    $"Koreksi ke-{i}.", $"Alasan koreksi ke-{i}.",
                    null, null, Sekarang.AddDays(i));

                Assert.True(hasil.IsAllowed);
                Assert.Equal(i, addendum!.Sequence);
            }

            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking().Single(x => x.DocumentId == documentId);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan.IntegrityStatus);
            Assert.Equal(3, keutuhan.AddendumCount);

            var daftar = await service.ListByDocumentAsync(
                ClinicalDocumentKind.ProgressNote, documentId);
            Assert.Equal([1, 2, 3], daftar.Select(x => x.Sequence));
        }

        // =====================================================================
        // AT-RM-05 — bukan penulis ditolak
        // =====================================================================

        /// <summary>
        /// `AT-RM-05`: perawat mencoba mengoreksi catatan dokter yang akunnya masih aktif.
        /// </summary>
        [Fact]
        public async Task BukanPenulis_TanpaKewenanganPengganti_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, _) = await SiapkanCpptTerkunci(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var (hasil, addendum) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, perawat.Id,
                actorHasSubstituteAuthority: false,
                "Koreksi dari perawat.", "Menemukan kesalahan.",
                null, null, Sekarang.AddDays(1));

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status403Forbidden, hasil.StatusCode);
            Assert.Contains("Hanya penulis catatan", hasil.ErrorMessage);
            Assert.Null(addendum);
        }

        /// <summary>
        /// Kepala unit pun ditolak bila penulis masih aktif dan tidak ada penetapan.
        ///
        /// Ini yang membedakan `RM-DEC-004` dari pilihan yang lebih longgar: kewenangan
        /// pengganti tidak berlaku terus-menerus, hanya saat penulisnya benar-benar berhalangan.
        /// </summary>
        [Fact]
        public async Task KepalaUnit_DitolakBilaPenulisMasihAktifDanTanpaPenetapan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, _) = await SiapkanCpptTerkunci(context);
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            var (hasil, _) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, kepalaUnit.Id,
                actorHasSubstituteAuthority: true,
                "Koreksi.", "Alasan.", null, null, Sekarang.AddDays(1));

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status403Forbidden, hasil.StatusCode);
        }

        // =====================================================================
        // AT-RM-14 — pengganti saat akun penulis nonaktif
        // =====================================================================

        /// <summary>
        /// `AT-RM-14`: kepala unit mengoreksi catatan dokter yang akunnya sudah nonaktif.
        ///
        /// Yang paling penting dibuktikan: addendum tercatat atas nama **kepala unit**, bukan
        /// atas nama dokter yang sudah keluar.
        /// </summary>
        [Fact]
        public async Task AkunPenulisNonaktif_KepalaUnitDapatMengoreksiAtasNamanyaSendiri()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            // Dokter keluar dari rumah sakit; akunnya dinonaktifkan.
            var akun = context.Set<ApplicationUser>().Single(x => x.Id == penulis.Id);
            akun.IsActive = false;
            context.SaveChanges();

            var (hasil, addendum) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, kepalaUnit.Id,
                actorHasSubstituteAuthority: true,
                "Pembetulan atas catatan dokter yang sudah tidak bertugas.",
                "Ditemukan kekeliruan setelah dokter keluar.",
                null, null, Sekarang.AddDays(30));

            Assert.True(hasil.IsAllowed);
            Assert.NotNull(addendum);
            Assert.Equal(kepalaUnit.Id, addendum!.AuthorUserId);
            Assert.True(addendum.IsSubstituteAuthor);

            // Tidak perlu penetapan manual — sistem menyimpulkannya dari keadaan akun.
            Assert.Null(addendum.DelegationId);
        }

        /// <summary>
        /// Perawat tetap ditolak walaupun akun penulis nonaktif, karena ia bukan kepala unit
        /// maupun DPJP.
        /// </summary>
        [Fact]
        public async Task AkunPenulisNonaktif_PerawatTetapDitolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);
            var perawat = RekamMedisTestData.BuatPengguna(context, "perawat");

            var akun = context.Set<ApplicationUser>().Single(x => x.Id == penulis.Id);
            akun.IsActive = false;
            context.SaveChanges();

            var (hasil, _) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, perawat.Id,
                actorHasSubstituteAuthority: false,
                "Koreksi.", "Alasan.", null, null, Sekarang.AddDays(1));

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status403Forbidden, hasil.StatusCode);
        }

        // =====================================================================
        // AT-RM-26 dan AT-RM-27 — penetapan berhalangan
        // =====================================================================

        /// <summary>
        /// `AT-RM-26`: penetapan tanpa batas waktu ditolak.
        ///
        /// Penetapan tanpa batas waktu adalah pintu belakang permanen. Bila kepala unit membuka
        /// jalur pengganti sekali lalu lupa menutupnya, catatan penulis itu selamanya dapat
        /// dikoreksi orang lain.
        /// </summary>
        [Fact]
        public async Task PenetapanDenganBatasWaktuYangSudahLewat_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            var (hasil, penetapan) = await Penetapan(context).CreateAsync(
                penulis.Id, kepalaUnit.Id, "Dokter cuti panjang.",
                validUntilUtc: Sekarang.AddDays(-1), nowUtc: Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("setelah hari ini", hasil.ErrorMessage);
            Assert.Null(penetapan);
        }

        [Fact]
        public async Task PenetapanTanpaAlasan_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            var (hasil, _) = await Penetapan(context).CreateAsync(
                penulis.Id, kepalaUnit.Id, "   ", Sekarang.AddDays(14), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Contains("Alasan penetapan wajib diisi", hasil.ErrorMessage);
        }

        [Fact]
        public async Task MenetapkanDiriSendiriBerhalangan_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            var (hasil, _) = await Penetapan(context).CreateAsync(
                kepalaUnit.Id, kepalaUnit.Id, "Saya cuti.", Sekarang.AddDays(14), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Contains("diri sendiri", hasil.ErrorMessage);
        }

        [Fact]
        public async Task PenetapanGandaUntukPenulisYangSama_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");
            var service = Penetapan(context);

            await service.CreateAsync(penulis.Id, kepalaUnit.Id, "Cuti panjang.",
                Sekarang.AddDays(14), Sekarang);

            var (hasil, _) = await service.CreateAsync(penulis.Id, kepalaUnit.Id, "Cuti lagi.",
                Sekarang.AddDays(30), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        }

        /// <summary>
        /// Akun yang sudah nonaktif tidak perlu penetapan — jalurnya sudah terbuka otomatis.
        ///
        /// Membiarkan penetapan dibuat justru menyesatkan, karena kepala unit akan mengira
        /// kewenangan itu berasal dari penetapannya.
        /// </summary>
        [Fact]
        public async Task PenetapanUntukAkunYangSudahNonaktif_DitolakDenganPenjelasan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            var akun = context.Set<ApplicationUser>().Single(x => x.Id == penulis.Id);
            akun.IsActive = false;
            context.SaveChanges();

            var (hasil, _) = await Penetapan(context).CreateAsync(
                penulis.Id, kepalaUnit.Id, "Dokter keluar.", Sekarang.AddDays(14), Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Contains("terbuka", hasil.ErrorMessage);
        }

        /// <summary>
        /// Penetapan yang sah membuka jalur koreksi bagi kepala unit.
        /// </summary>
        [Fact]
        public async Task PenetapanYangSah_MembukaJalurKoreksiBagiKepalaUnit()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            var (hasilPenetapan, penetapan) = await Penetapan(context).CreateAsync(
                penulis.Id, kepalaUnit.Id, "Dokter cuti dua minggu.",
                Sekarang.AddDays(14), Sekarang);

            Assert.True(hasilPenetapan.IsAllowed);

            var (hasil, addendum) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, kepalaUnit.Id,
                actorHasSubstituteAuthority: true,
                "Pembetulan selama dokter berhalangan.", "Ditemukan kekeliruan.",
                null, null, Sekarang.AddDays(1));

            Assert.True(hasil.IsAllowed);
            Assert.Equal(kepalaUnit.Id, addendum!.AuthorUserId);
            Assert.True(addendum.IsSubstituteAuthor);
            Assert.Equal(penetapan!.Id, addendum.DelegationId);
        }

        /// <summary>
        /// `AT-RM-27`: penetapan yang batas waktunya sudah lewat tidak lagi membuka jalur, dan
        /// pesannya membedakan diri dari "memang tidak berhak".
        /// </summary>
        [Fact]
        public async Task PenetapanYangSudahLewatBatasWaktu_TidakLagiMembukaJalur()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);
            var kepalaUnit = RekamMedisTestData.BuatPengguna(context, "kepala.unit");

            await Penetapan(context).CreateAsync(
                penulis.Id, kepalaUnit.Id, "Cuti singkat.",
                validUntilUtc: Sekarang.AddDays(3), nowUtc: Sekarang);

            // Pada hari kedua, penetapan masih berlaku.
            var saatMasihBerlaku = await Addendum(context).ResolveAuthorityAsync(
                ClinicalDocumentKind.ProgressNote, documentId, kepalaUnit.Id,
                actorHasSubstituteAuthority: true,
                nowUtc: Sekarang.AddDays(2));

            // Pada hari kesepuluh, penetapan sudah lewat batas waktunya.
            var setelahLewat = await Addendum(context).ResolveAuthorityAsync(
                ClinicalDocumentKind.ProgressNote, documentId, kepalaUnit.Id,
                actorHasSubstituteAuthority: true,
                nowUtc: Sekarang.AddDays(10));

            Assert.True(saatMasihBerlaku.IsAllowed);
            Assert.False(setelahLewat.IsAllowed);

            // Pesannya membedakan diri dari "memang tidak berhak", supaya kepala unit tahu
            // harus meminta perpanjangan.
            Assert.Contains("sudah berakhir", setelahLewat.Explanation);
        }

        // =====================================================================
        // Aturan dasar addendum
        // =====================================================================

        /// <summary>
        /// Addendum pada dokumen yang masih draf ditolak, disertai arahan yang benar: perbaiki
        /// langsung pada catatannya.
        ///
        /// Addendum bukan cara mengoreksi catatan yang masih bisa diedit.
        /// </summary>
        [Fact]
        public async Task AddendumPadaDokumenYangMasihDraf_DitolakDenganArahanYangBenar()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var documentId = Guid.NewGuid();

            await Keutuhan(context).RegisterAsync(
                ClinicalDocumentKind.ProgressNote, documentId,
                konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            var (hasil, _) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, penulis.Id, false,
                "Koreksi.", "Alasan.", null, null, Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("belum terkunci", hasil.ErrorMessage);
        }

        [Fact]
        public async Task AddendumTanpaAlasanKoreksi_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);

            var (hasil, _) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, penulis.Id, false,
                "Koreksi.", "   ", null, null, Sekarang.AddDays(1));

            Assert.False(hasil.IsAllowed);
            Assert.Contains("Alasan koreksi wajib diisi", hasil.ErrorMessage);
        }

        [Fact]
        public async Task AddendumTanpaIsi_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (documentId, penulis) = await SiapkanCpptTerkunci(context);

            var (hasil, _) = await Addendum(context).CreateAsync(
                ClinicalDocumentKind.ProgressNote, documentId, penulis.Id, false,
                "  ", "Alasan.", null, null, Sekarang.AddDays(1));

            Assert.False(hasil.IsAllowed);
            Assert.Contains("Isi koreksi wajib diisi", hasil.ErrorMessage);
        }

        /// <summary>
        /// `AT-RM-28`: tidak ada jalur mengubah maupun menghapus addendum.
        ///
        /// Dibuktikan lewat bentuk service: ia hanya menyediakan pembuatan dan pembacaan.
        /// </summary>
        [Fact]
        public void ServiceAddendum_TidakMenyediakanJalurUbahMaupunHapus()
        {
            var metode = typeof(ClinicalNoteAddendumService)
                .GetMethods(System.Reflection.BindingFlags.Public
                            | System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(x => x.Name)
                .ToList();

            Assert.DoesNotContain(metode, x => x.Contains("Update", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(metode, x => x.Contains("Delete", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(metode, x => x.Contains("Remove", StringComparison.OrdinalIgnoreCase));
        }
    }
}
