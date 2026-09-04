using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-07` — penguncian catatan saat kunjungan selesai.
    ///
    /// Menutup uji penerimaan `AT-RM-03`.
    ///
    /// Uji ini menguji **perilaku penguncian saat kunjungan berpindah ke selesai**, bukan
    /// controller tertentu. Alasannya: penelusuran menemukan kunjungan dapat berpindah ke
    /// selesai lewat TIGA jalur berbeda, dan aturan yang sama harus berlaku pada ketiganya.
    /// </summary>
    public class EncounterClosureLockTests
    {
        private static readonly DateTime Sekarang = new(2026, 8, 26, 11, 0, 0, DateTimeKind.Utc);

        private static ClinicalDocumentIntegrityService Service(ApplicationDbContext c) => new(c);

        /// <summary>
        /// Menyiapkan satu kunjungan berisi tiga CPPT: dua masih draf, satu sudah
        /// ditandatangani.
        /// </summary>
        private static async Task<(RekamMedisTestData.Konteks Konteks, Guid PenulisId, Guid DokumenDitandatangani)>
            SiapkanKunjunganDenganTigaCatatan(ApplicationDbContext context)
        {
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var service = Service(context);

            var ditandatangani = Guid.NewGuid();

            await service.RegisterAsync(ClinicalDocumentKind.ProgressNote, ditandatangani,
                konteks.PatientId, konteks.EncounterId, penulis.Id);
            await service.RegisterAsync(ClinicalDocumentKind.ProgressNote, Guid.NewGuid(),
                konteks.PatientId, konteks.EncounterId, penulis.Id);
            await service.RegisterAsync(ClinicalDocumentKind.ProgressNote, Guid.NewGuid(),
                konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            await service.SignAsync(ClinicalDocumentKind.ProgressNote, ditandatangani,
                penulis.Id, null, null, Sekarang.AddMinutes(-30));

            return (konteks, penulis.Id, ditandatangani);
        }

        /// <summary>
        /// `AT-RM-03`: kunjungan selesai mengunci seluruh catatan yang masih draf, dan tidak
        /// menyentuh catatan yang sudah ditandatangani.
        /// </summary>
        [Fact]
        public async Task KunjunganSelesai_MenguncSeluruhCatatanDrafDanTidakMenyentuhYangDitandatangani()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, penulisId, ditandatangani) =
                await SiapkanKunjunganDenganTigaCatatan(context);

            // Kunjungan berpindah ke selesai, beserta penguncian dalam satu SaveChanges.
            var kunjungan = context.Set<TrxPatientEncounter>().Single(x => x.Id == konteks.EncounterId);
            kunjungan.EncounterStatus = EncounterStatus.Completed;
            kunjungan.CompletedAt = Sekarang;

            var jumlah = await Service(context).LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, penulisId, Sekarang, Sekarang);

            await context.SaveChangesAsync();

            Assert.Equal(2, jumlah);

            var seluruhnya = context.Set<MrcClinicalDocumentIntegrity>().AsNoTracking().ToList();

            var yangDikunci = seluruhnya
                .Where(x => x.LockTrigger == ClinicalDocumentLockTrigger.EncounterClosed)
                .ToList();
            Assert.Equal(2, yangDikunci.Count);
            Assert.All(yangDikunci, x =>
            {
                Assert.Equal(ClinicalDocumentIntegrityStatus.LockedUnsigned, x.IntegrityStatus);
                Assert.Equal(Sekarang, x.LockedEncounterClosedAt);
            });

            // Catatan yang sudah ditandatangani tidak tersentuh: jejak bahwa penulisnya
            // menyatakan catatan final tetap utuh.
            var yangDitandatangani = seluruhnya.Single(x => x.DocumentId == ditandatangani);
            Assert.Equal(ClinicalDocumentIntegrityStatus.Signed, yangDitandatangani.IntegrityStatus);
            Assert.Equal(ClinicalDocumentLockTrigger.AuthorSigned, yangDitandatangani.LockTrigger);
        }

        /// <summary>
        /// Setelah kunjungan selesai, seluruh catatannya menolak perubahan.
        ///
        /// Inilah tujuan sebenarnya dari penguncian: bukan sekadar status berubah, melainkan
        /// isinya benar-benar tidak dapat diubah lagi.
        /// </summary>
        [Fact]
        public async Task SetelahKunjunganSelesai_SeluruhCatatannyaMenolakPerubahan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var (konteks, penulisId, _) = await SiapkanKunjunganDenganTigaCatatan(context);
            var service = Service(context);

            await service.LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, penulisId, Sekarang, Sekarang);
            await context.SaveChangesAsync();

            var seluruhDokumen = context.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Select(x => x.DocumentId)
                .ToList();

            foreach (var documentId in seluruhDokumen)
            {
                var hasil = await service.EnsureMutableAsync(
                    ClinicalDocumentKind.ProgressNote, documentId);

                Assert.False(hasil.IsAllowed);
                Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            }
        }

        /// <summary>
        /// Pembatalan kunjungan TIDAK mengunci catatan.
        ///
        /// Membatalkan kunjungan tidak sama dengan menyelesaikan pelayanan. Catatan tetap pada
        /// statusnya, sehingga masih dapat dirapikan bila pembatalannya keliru.
        /// </summary>
        [Fact]
        public async Task KunjunganDibatalkan_TidakMenguncCatatan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var documentId = Guid.NewGuid();

            await Service(context).RegisterAsync(ClinicalDocumentKind.ProgressNote, documentId,
                konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            // Pembatalan tidak memanggil penguncian sama sekali.
            var kunjungan = context.Set<TrxPatientEncounter>().Single(x => x.Id == konteks.EncounterId);
            kunjungan.EncounterStatus = EncounterStatus.Cancelled;
            kunjungan.IsCancel = true;
            context.SaveChanges();

            var keutuhan = context.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking().Single(x => x.DocumentId == documentId);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, keutuhan.IntegrityStatus);
        }

        /// <summary>
        /// Catatan yang dibuat setelah kunjungan sudah selesai tetap berstatus draf.
        ///
        /// Keterbatasan ini dinyatakan terbuka: penguncian hanya berlaku pada saat kunjungan
        /// berpindah ke selesai. Catatan susulan perlu ditandatangani penulisnya sendiri, atau
        /// akan menggantung terbuka.
        /// </summary>
        [Fact]
        public async Task CatatanYangDibuatSetelahKunjunganSelesai_TetapBerstatusDraf()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var penulis = RekamMedisTestData.BuatPengguna(context, "dokter");
            var service = Service(context);

            await service.LockOpenDocumentsForEncounterAsync(
                konteks.EncounterId, penulis.Id, Sekarang, Sekarang);
            await context.SaveChangesAsync();

            var susulan = Guid.NewGuid();
            await service.RegisterAsync(ClinicalDocumentKind.ProgressNote, susulan,
                konteks.PatientId, konteks.EncounterId, penulis.Id);
            await context.SaveChangesAsync();

            var keutuhan = context.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking().Single(x => x.DocumentId == susulan);

            Assert.Equal(ClinicalDocumentIntegrityStatus.Draft, keutuhan.IntegrityStatus);
        }
    }
}
