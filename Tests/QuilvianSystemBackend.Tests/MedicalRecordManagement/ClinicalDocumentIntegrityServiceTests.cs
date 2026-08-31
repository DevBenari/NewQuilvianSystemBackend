using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-02` — service keutuhan dokumen klinis.
    ///
    /// Menutup uji penerimaan `AT-RM-02`, `AT-RM-10`, dan `AT-RM-11`.
    /// </summary>
    public class ClinicalDocumentIntegrityServiceTests
    {
        private static readonly DateTime Sekarang = new(2026, 8, 26, 9, 0, 0, DateTimeKind.Utc);

        private static ClinicalDocumentIntegrityService Service(ApplicationDbContext context)
            => new(context);

        /// <summary>
        /// Mendaftarkan satu CPPT beserta pasien dan kunjungannya, lalu menyimpannya.
        /// </summary>
        private static async Task<(RekamMedisTestData.Konteks Konteks, Guid DocumentId, Guid AuthorId)>
            DaftarkanCppt(ApplicationDbContext context)
        {
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var documentId = Guid.NewGuid();

            await Service(context).RegisterAsync(
                ClinicalDocumentKind.ProgressNote,
                documentId,
                konteks.PatientId,
                konteks.EncounterId,
                penulis.Id);

            await context.SaveChangesAsync();

            return (konteks, documentId, penulis.Id);
        }

        // =====================================================================
        // Pendaftaran
        // =====================================================================

        /// <summary>
        /// Acceptance criteria 1: pendaftaran kedua untuk dokumen yang sama tidak membuat baris
        /// kedua.
        ///
        /// Penting karena pendaftaran dipanggil dari controller yang bisa saja dijalankan ulang.
        /// Dua baris keutuhan untuk satu dokumen berarti dua status yang bertentangan.
        /// </summary>
        [Fact]
        public async Task PendaftaranKedua_TidakMembuatBarisKedua()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, documentId, authorId) = await DaftarkanCppt(context);

            var ulang = await Service(context).RegisterAsync(
                ClinicalDocumentKind.ProgressNote,
                documentId,
                konteks.PatientId,
                konteks.EncounterId,
                authorId);

            await context.SaveChangesAsync();

            Assert.Equal(1, context.Set<TrxClinicalDocumentIntegrity>().Count());
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, ulang.IntegrityStatus);
        }

        [Fact]
        public async Task PendaftaranDenganIdDokumenKosong_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Service(context).RegisterAsync(
                    ClinicalDocumentKind.ProgressNote,
                    Guid.Empty,
                    konteks.PatientId,
                    konteks.EncounterId,
                    Guid.NewGuid()));
        }

        // =====================================================================
        // Menandatangani — AT-RM-02
        // =====================================================================

        /// <summary>
        /// `AT-RM-02`: penulis menandatangani catatannya sendiri.
        ///
        /// Membuktikan pula `RM-DEC-021`: tidak ada permintaan kata sandi maupun sidik jari.
        /// Yang dicatat cukup siapa, kapan, dan dari perangkat apa.
        /// </summary>
        [Fact]
        public async Task Penulis_DapatMenandatanganiCatatannyaSendiri()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, documentId, authorId) = await DaftarkanCppt(context);

            var (hasil, keutuhan) = await Service(context).SignAsync(
                ClinicalDocumentKind.ProgressNote,
                documentId,
                authorId,
                deviceInfo: "Chrome 140 di Windows 11",
                ipAddress: "10.20.30.40",
                nowUtc: Sekarang);

            Assert.True(hasil.IsAllowed);
            Assert.NotNull(keutuhan);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, keutuhan!.IntegrityStatus);
            Assert.Equal(Sekarang, keutuhan.SignedAt);
            Assert.Equal(authorId, keutuhan.SignedByUserId);
            Assert.Equal("Chrome 140 di Windows 11", keutuhan.SignatureDeviceInfo);
            Assert.Equal("10.20.30.40", keutuhan.SignatureIpAddress);
            Assert.Equal(Sekarang, keutuhan.LockedAt);
            Assert.Equal(ClinicalDocumentLockTrigger.AuthorSigned, keutuhan.LockTrigger);
        }

        /// <summary>
        /// Acceptance criteria 2: bukan penulis ditolak dengan kode 403, bukan 400.
        ///
        /// Bedanya bermakna bagi pengguna: 400 berarti permintaannya salah, 403 berarti
        /// permintaannya benar tetapi ia tidak berhak.
        /// </summary>
        [Fact]
        public async Task BukanPenulis_TidakDapatMenandatangani()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, documentId, _) = await DaftarkanCppt(context);
            var oranglain = RekamMedisTestData.BuatPengguna(context, "perawat");

            var (hasil, keutuhan) = await Service(context).SignAsync(
                ClinicalDocumentKind.ProgressNote,
                documentId,
                oranglain.Id,
                deviceInfo: null,
                ipAddress: null,
                nowUtc: Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status403Forbidden, hasil.StatusCode);
            Assert.Contains("Hanya penulis catatan", hasil.ErrorMessage);
            Assert.Null(keutuhan);

            // Status dokumen tidak berubah sedikit pun.
            var tersimpan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Single(x => x.DocumentId == documentId);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, tersimpan.IntegrityStatus);
        }

        /// <summary>
        /// `AT-RM-11`: menandatangani dokumen yang sudah terkunci ditolak.
        ///
        /// Tanda tangan yang diberikan setelah dokumen terkunci berarti menyatakan dokumen
        /// final pada waktu yang sudah lewat — dan tanda tangan yang mundur ke belakang bukan
        /// tanda tangan.
        /// </summary>
        [Fact]
        public async Task DokumenYangSudahTerkunci_TidakDapatDitandatanganiUlang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, documentId, authorId) = await DaftarkanCppt(context);
            var service = Service(context);

            await service.SignAsync(
                ClinicalDocumentKind.ProgressNote, documentId, authorId, null, null, Sekarang);

            var (hasil, _) = await service.SignAsync(
                ClinicalDocumentKind.ProgressNote, documentId, authorId, null, null,
                Sekarang.AddHours(1));

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("sudah terkunci", hasil.ErrorMessage);
        }

        [Fact]
        public async Task DokumenYangBelumTerdaftar_TidakDapatDitandatangani()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (hasil, _) = await Service(context).SignAsync(
                ClinicalDocumentKind.ProgressNote,
                Guid.NewGuid(),
                Guid.NewGuid(),
                null, null, Sekarang);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status404NotFound, hasil.StatusCode);
        }

        // =====================================================================
        // Pemeriksaan boleh diubah — AT-RM-10
        // =====================================================================

        [Fact]
        public async Task DokumenBerstatusDraf_MasihBolehDiubah()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, documentId, _) = await DaftarkanCppt(context);

            var hasil = await Service(context).EnsureMutableAsync(
                ClinicalDocumentKind.ProgressNote, documentId);

            Assert.True(hasil.IsAllowed);
        }

        /// <summary>
        /// Acceptance criteria 4 dan `AT-RM-10`: dokumen yang sudah ditandatangani menolak
        /// perubahan, dengan pesan yang mengarahkan ke addendum.
        ///
        /// Pesan yang mengarahkan itu penting. Menolak tanpa memberi tahu jalan keluarnya akan
        /// membuat tenaga klinis mencari cara lain, dan biasanya cara lain itu lebih buruk.
        /// </summary>
        [Fact]
        public async Task DokumenYangDitandatangani_MenolakPerubahanDanMengarahkanKeAddendum()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, documentId, authorId) = await DaftarkanCppt(context);
            var service = Service(context);

            await service.SignAsync(
                ClinicalDocumentKind.ProgressNote, documentId, authorId, null, null, Sekarang);

            var hasil = await service.EnsureMutableAsync(
                ClinicalDocumentKind.ProgressNote, documentId);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("addendum", hasil.ErrorMessage);
        }

        [Fact]
        public async Task DokumenYangTerkunciTanpaTandaTangan_MenolakPerubahan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, documentId, authorId) = await DaftarkanCppt(context);

            await Service(context).LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, authorId, Sekarang);
            await context.SaveChangesAsync();

            var hasil = await Service(context).EnsureMutableAsync(
                ClinicalDocumentKind.ProgressNote, documentId);

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
        }

        [Fact]
        public async Task DokumenYangDibatalkan_MenolakPerubahan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, documentId, _) = await DaftarkanCppt(context);

            var keutuhan = context.Set<TrxClinicalDocumentIntegrity>()
                .Single(x => x.DocumentId == documentId);
            keutuhan.IntegrityStatus = ClinicalDocumentIntegrityStatus.Cancelled;
            context.SaveChanges();

            var hasil = await Service(context).EnsureMutableAsync(
                ClinicalDocumentKind.ProgressNote, documentId);

            Assert.False(hasil.IsAllowed);
            Assert.Contains("dibatalkan", hasil.ErrorMessage);
        }

        /// <summary>
        /// Jenis dokumen yang belum tunduk aturan keutuhan dibiarkan lewat.
        ///
        /// Ini yang membuat cakupan rilis pertama dapat dibatasi ke satu jenis dokumen tanpa
        /// memblokir alur yang berjalan. Keadaannya dinyatakan terbuka di layar, bukan
        /// disembunyikan.
        /// </summary>
        [Fact]
        public async Task JenisDokumenYangBelumDitegakkan_DibiarkanLewat()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            Assert.True(ClinicalDocumentIntegrityService.DitegakkanUntuk(
                ClinicalDocumentKind.ProgressNote));
            Assert.False(ClinicalDocumentIntegrityService.DitegakkanUntuk(
                ClinicalDocumentKind.Consultation));

            var hasil = await Service(context).EnsureMutableAsync(
                ClinicalDocumentKind.Consultation, Guid.NewGuid());

            Assert.True(hasil.IsAllowed);
        }

        // =====================================================================
        // Penguncian saat kunjungan ditutup
        // =====================================================================

        /// <summary>
        /// Acceptance criteria untuk lapis kedua `RM-DEC-003`: seluruh dokumen draf pada satu
        /// kunjungan terkunci sekaligus.
        /// </summary>
        [Fact]
        public async Task PenutupanKunjungan_MenguncSeluruhDokumenDrafPadaKunjunganItu()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var service = Service(context);

            for (var i = 0; i < 3; i++)
            {
                await service.RegisterAsync(
                    ClinicalDocumentKind.ProgressNote,
                    Guid.NewGuid(),
                    konteks.PatientId,
                    konteks.EncounterId,
                    penulis.Id);
            }
            await context.SaveChangesAsync();

            var jumlah = await service.LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, penulis.Id, Sekarang);
            await context.SaveChangesAsync();

            Assert.Equal(3, jumlah);

            var seluruhnya = context.Set<TrxClinicalDocumentIntegrity>().AsNoTracking().ToList();
            Assert.All(seluruhnya, x =>
            {
                Assert.Equal(ClinicalDocumentIntegrityStatus.LockedUnsigned, x.IntegrityStatus);
                Assert.Equal(ClinicalDocumentLockTrigger.EncounterClosed, x.LockTrigger);
                Assert.Equal(Sekarang, x.LockedAt);
            });
        }

        /// <summary>
        /// Dokumen yang sudah ditandatangani tidak diubah penguncian kunjungan.
        ///
        /// Bila ikut diubah, penanda "ditandatangani penulis" akan hilang dan berganti menjadi
        /// "terkunci karena kunjungan ditutup" — menghapus jejak bahwa penulisnya memang sudah
        /// menyatakan catatannya final.
        /// </summary>
        [Fact]
        public async Task PenutupanKunjungan_TidakMengubahDokumenYangSudahDitandatangani()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, documentId, authorId) = await DaftarkanCppt(context);
            var service = Service(context);

            await service.SignAsync(
                ClinicalDocumentKind.ProgressNote, documentId, authorId, null, null, Sekarang);

            var jumlah = await service.LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, authorId, Sekarang.AddHours(2));
            await context.SaveChangesAsync();

            Assert.Equal(0, jumlah);

            var tersimpan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Single(x => x.DocumentId == documentId);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, tersimpan.IntegrityStatus);
            Assert.Equal(ClinicalDocumentLockTrigger.AuthorSigned, tersimpan.LockTrigger);
            Assert.Equal(Sekarang, tersimpan.LockedAt);
        }

        /// <summary>
        /// Aman dipanggil berulang — pemanggilan kedua tidak mengunci apa pun lagi.
        ///
        /// Diperlukan karena endpoint perubahan status kunjungan tidak memvalidasi perpindahan
        /// (`RM-CAP-019`), sehingga status dapat berpindah menuju selesai lebih dari sekali.
        /// </summary>
        [Fact]
        public async Task PenguncianKunjungan_AmanDipanggilBerulang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, _, authorId) = await DaftarkanCppt(context);
            var service = Service(context);

            var pertama = await service.LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, authorId, Sekarang);
            await context.SaveChangesAsync();

            var kedua = await service.LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, authorId, Sekarang.AddHours(1));
            await context.SaveChangesAsync();

            Assert.Equal(1, pertama);
            Assert.Equal(0, kedua);
        }

        /// <summary>
        /// Dokumen milik kunjungan lain tidak ikut terkunci.
        /// </summary>
        [Fact]
        public async Task PenutupanKunjungan_TidakMenyentuhDokumenKunjunganLain()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var service = Service(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");

            var kunjunganA = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var kunjunganB = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var dokumenA = Guid.NewGuid();
            var dokumenB = Guid.NewGuid();

            await service.RegisterAsync(ClinicalDocumentKind.ProgressNote, dokumenA,
                kunjunganA.PatientId, kunjunganA.EncounterId, penulis.Id);
            await service.RegisterAsync(ClinicalDocumentKind.ProgressNote, dokumenB,
                kunjunganB.PatientId, kunjunganB.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            await service.LockOpenDocumentsForEncounterAsync(
                kunjunganA.EncounterId, penulis.Id, Sekarang);
            await context.SaveChangesAsync();

            var a = context.Set<TrxClinicalDocumentIntegrity>().AsNoTracking()
                .Single(x => x.DocumentId == dokumenA);
            var b = context.Set<TrxClinicalDocumentIntegrity>().AsNoTracking()
                .Single(x => x.DocumentId == dokumenB);

            Assert.Equal(ClinicalDocumentIntegrityStatus.LockedUnsigned, a.IntegrityStatus);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, b.IntegrityStatus);
        }

        /// <summary>
        /// Penguncian bertahap per potongan bekerja untuk kunjungan dengan banyak dokumen.
        ///
        /// Kunjungan rawat inap yang panjang dapat memuat sangat banyak dokumen. Mengambilnya
        /// sekaligus membuat transaksi menahan tabel terlalu lama.
        /// </summary>
        [Fact]
        public async Task PenguncianBertahap_MenguncSeluruhnyaWalauPotongannyaKecil()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var service = Service(context);

            for (var i = 0; i < 7; i++)
            {
                await service.RegisterAsync(
                    ClinicalDocumentKind.ProgressNote,
                    Guid.NewGuid(),
                    konteks.PatientId,
                    konteks.EncounterId,
                    penulis.Id);
            }
            await context.SaveChangesAsync();

            var jumlah = await service.LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, penulis.Id, Sekarang, batchSize: 2);
            await context.SaveChangesAsync();

            Assert.Equal(7, jumlah);
            Assert.Equal(0, context.Set<TrxClinicalDocumentIntegrity>()
                .Count(x => x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft));
        }

        /// <summary>
        /// Acceptance criteria 5: penulis tidak dapat diubah lewat jalur mana pun pada service
        /// ini.
        ///
        /// Dibuktikan dengan memastikan penandatanganan tidak menyentuh `AuthorUserId`. Inilah
        /// lapis kedua yang menutup `RM-CAP-012`: walaupun kolom penulis pada tabel klinis
        /// masih dapat berubah, penentu penulis yang sah ada di sini.
        /// </summary>
        [Fact]
        public async Task PenulisDokumen_TidakBerubahSetelahDitandatangani()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (_, documentId, authorId) = await DaftarkanCppt(context);

            await Service(context).SignAsync(
                ClinicalDocumentKind.ProgressNote, documentId, authorId, null, null, Sekarang);

            var tersimpan = context.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Single(x => x.DocumentId == documentId);

            Assert.Equal(authorId, tersimpan.AuthorUserId);
        }
    }
}
