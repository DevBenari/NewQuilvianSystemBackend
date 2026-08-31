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
    /// Bukti acceptance untuk task `BE-12` — tinjauan jejak akses.
    ///
    /// Menutup uji penerimaan `AT-RM-08` dan `AT-RM-29`.
    /// </summary>
    public class MedicalRecordAccessReviewTests
    {
        private static readonly DateTime Sekarang = new(2026, 8, 26, 14, 0, 0, DateTimeKind.Utc);

        private static MedicalRecordAccessReviewService Service(ApplicationDbContext c) => new(c);

        private static TrxMedicalRecordAccessLog BuatJejak(
            ApplicationDbContext context,
            Guid patientId,
            Guid userId,
            bool perluDitinjau,
            MedicalRecordAccessScope scope = MedicalRecordAccessScope.Timeline)
        {
            var jejak = new TrxMedicalRecordAccessLog
            {
                PatientId = patientId,
                UserId = userId,
                UserDisplayNameSnapshot = "Pengguna Uji",
                AccessType = perluDitinjau
                    ? MedicalRecordAccessType.ReasonedAccess
                    : MedicalRecordAccessType.RoutineCare,
                AccessScope = scope,
                HasActiveEncounter = !perluDitinjau,
                IsFlaggedForReview = perluDitinjau,
                AccessedAt = Sekarang,
                CreateDateTime = Sekarang,
                CreateBy = userId
            };

            context.Set<TrxMedicalRecordAccessLog>().Add(jejak);
            context.SaveChanges();
            return jejak;
        }

        /// <summary>
        /// `AT-RM-29`: akses yang ditandai perlu ditinjau dapat ditandai sudah ditinjau,
        /// beserta catatan dan nama peninjaunya.
        /// </summary>
        [Fact]
        public async Task AksesYangPerluDitinjau_DapatDitandaiSudahDitinjau()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.rm");

            var jejak = BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: true);

            var (hasil, tersimpan) = await Service(context).MarkReviewedAsync(
                jejak.Id, petugas.Id, "Sudah diperiksa, alasannya wajar.", Sekarang.AddHours(2));

            Assert.True(hasil.IsAllowed);
            Assert.NotNull(tersimpan);
            Assert.Equal(Sekarang.AddHours(2), tersimpan!.ReviewedAt);
            Assert.Equal(petugas.Id, tersimpan.ReviewedByUserId);
            Assert.Equal("Sudah diperiksa, alasannya wajar.", tersimpan.ReviewNote);

            // Isi jejak aslinya tidak berubah sedikit pun.
            Assert.Equal(dokter.Id, tersimpan.UserId);
            Assert.Equal(Sekarang, tersimpan.AccessedAt);
        }

        /// <summary>
        /// Akses yang memang tidak perlu ditinjau tidak boleh ditandai.
        ///
        /// Bila diizinkan, angka pada laporan tinjauan menjadi tidak bermakna — tidak dapat
        /// dibedakan mana yang benar-benar ditelaah dan mana yang sekadar dibersihkan.
        /// </summary>
        [Fact]
        public async Task AksesYangTidakPerluDitinjau_TidakDapatDitandai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.rm");

            var jejak = BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: false);

            var (hasil, _) = await Service(context).MarkReviewedAsync(
                jejak.Id, petugas.Id, "Catatan.", Sekarang.AddHours(1));

            Assert.False(hasil.IsAllowed);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("tidak memerlukan tinjauan", hasil.ErrorMessage);
        }

        [Fact]
        public async Task AksesYangSudahDitinjau_TidakDapatDitandaiUlang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.rm");

            var jejak = BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: true);
            var service = Service(context);

            await service.MarkReviewedAsync(jejak.Id, petugas.Id, "Wajar.", Sekarang.AddHours(1));

            var (hasil, _) = await service.MarkReviewedAsync(
                jejak.Id, petugas.Id, "Diperiksa lagi.", Sekarang.AddHours(2));

            Assert.False(hasil.IsAllowed);
            Assert.Contains("sudah ditinjau", hasil.ErrorMessage);
        }

        /// <summary>
        /// Tinjauan tanpa catatan ditolak.
        ///
        /// Tinjauan tanpa catatan tidak dapat dibedakan dari sekadar membersihkan antrean.
        /// </summary>
        [Fact]
        public async Task TinjauanTanpaCatatan_Ditolak()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.rm");

            var jejak = BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: true);

            var (hasil, _) = await Service(context).MarkReviewedAsync(
                jejak.Id, petugas.Id, "   ", Sekarang.AddHours(1));

            Assert.False(hasil.IsAllowed);
            Assert.Contains("Catatan tinjauan wajib diisi", hasil.ErrorMessage);
        }

        /// <summary>
        /// `AT-RM-08`: tidak ada jalur mengubah isi jejak maupun menghapusnya.
        ///
        /// Dibuktikan lewat bentuk service: ia hanya menyediakan penandaan tinjauan dan rekap.
        /// </summary>
        [Fact]
        public void ServiceTinjauan_TidakMenyediakanJalurUbahIsiMaupunHapus()
        {
            var metode = typeof(MedicalRecordAccessReviewService)
                .GetMethods(System.Reflection.BindingFlags.Public
                            | System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(x => x.Name)
                .ToList();

            Assert.Equal(2, metode.Count);
            Assert.Contains("MarkReviewedAsync", metode);
            Assert.Contains("SummaryAsync", metode);

            Assert.DoesNotContain(metode, x => x.Contains("Delete", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(metode, x => x.Contains("Remove", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(metode, x => x.Contains("Create", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Rekap memisahkan akses rawatan dari akses beralasan.
        ///
        /// Angka inilah yang memberi tahu apakah aturan akses bekerja sebagaimana dimaksud.
        /// Bila hampir seluruhnya beralasan, berarti definisi pasien rawatan terlalu sempit dan
        /// justru menghambat pelayanan.
        /// </summary>
        [Fact]
        public async Task Rekap_MemisahkanAksesRawatanDariAksesBeralasan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);
            var dokter = RekamMedisTestData.BuatPengguna(context, "dokter");
            var petugas = RekamMedisTestData.BuatPengguna(context, "petugas.rm");

            BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: false);
            BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: false);
            var beralasan = BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: true);
            BuatJejak(context, konteks.PatientId, dokter.Id, perluDitinjau: true,
                      scope: MedicalRecordAccessScope.PrivateNote);

            await Service(context).MarkReviewedAsync(
                beralasan.Id, petugas.Id, "Wajar.", Sekarang.AddHours(1));

            var rekap = await Service(context).SummaryAsync(
                Sekarang.AddDays(-1), Sekarang.AddDays(1));

            Assert.Equal(4, rekap.TotalAkses);
            Assert.Equal(2, rekap.AksesRawatan);
            Assert.Equal(2, rekap.AksesBeralasan);
            Assert.Equal(1, rekap.AksesCatatanPribadi);
            Assert.Equal(2, rekap.PerluDitinjau);
            Assert.Equal(1, rekap.SudahDitinjau);
            Assert.Equal(1, rekap.BelumDitinjau);
            Assert.Equal(1, rekap.JumlahPenggunaBerbeda);
            Assert.Equal(1, rekap.JumlahPasienBerbeda);
        }
    }
}
