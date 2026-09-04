using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-20` — master keperluan akses rekam medis.
    ///
    /// Master ini menentukan apakah modul rekam medis berguna: selama isinya kosong, pembukaan
    /// berkas pasien di luar rawatan pengguna selalu ditolak.
    /// </summary>
    public class MedicalRecordAccessPurposeMasterTests
    {
        private static readonly Guid Petugas = Guid.NewGuid();

        private static MedicalRecordAccessPurposeService Service(ApplicationDbContext c) => new(c);

        private static CreateMedicalRecordAccessPurposeRequest Permintaan(
            string kode = "rujukan",
            string nama = "Permintaan rujukan",
            bool wajibAlasan = false,
            bool perluDitinjau = true,
            int urutan = 1) => new()
            {
                PurposeCode = kode,
                PurposeName = nama,
                Description = "Keterangan uji.",
                IsFreeTextRequired = wajibAlasan,
                RequiresReview = perluDitinjau,
                SortOrder = urutan,
                IsActive = true
            };

        [Fact]
        public async Task Menambah_MenormalkanKodeDanMengisiJejakAudit()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var hasil = await Service(context).CreateAsync(Permintaan(), Petugas);

            Assert.Equal(MedicalRecordAccessPurposeStatus.Success, hasil.Status);

            // Kode disimpan dalam huruf besar supaya "rujukan" dan "RUJUKAN" tidak pernah
            // menjadi dua baris yang berbeda.
            Assert.Equal("RUJUKAN", hasil.Entity!.PurposeCode);

            // QBE-AUD-001: jejak audit database terisi, terpisah dari logging aplikasi.
            Assert.Equal(Petugas, hasil.Entity.CreateBy);
            Assert.NotEqual(default, hasil.Entity.CreateDateTime);
        }

        [Fact]
        public async Task Menambah_KodeKembarDitolakSebagaiKonflik()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var service = Service(context);

            await service.CreateAsync(Permintaan(kode: "RUJUKAN"), Petugas);

            // Beda besar-kecil huruf tetap dianggap kembar.
            var kedua = await service.CreateAsync(Permintaan(kode: "rujukan"), Petugas);

            Assert.Equal(MedicalRecordAccessPurposeStatus.DuplicateCode, kedua.Status);
            Assert.Contains("sudah dipakai", kedua.Message);

            var jumlah = await context.Set<MstMedicalRecordAccessPurpose>().CountAsync();
            Assert.Equal(1, jumlah);
        }

        [Theory]
        [InlineData("", "Nama sah", "Kode keperluan wajib diisi.")]
        [InlineData("KODE", "", "Nama keperluan wajib diisi.")]
        public async Task Menambah_IsianTidakLengkapDitolak(
            string kode,
            string nama,
            string pesan)
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var hasil = await Service(context).CreateAsync(
                Permintaan(kode: kode, nama: nama),
                Petugas);

            Assert.Equal(MedicalRecordAccessPurposeStatus.Invalid, hasil.Status);
            Assert.Equal(pesan, hasil.Message);
            Assert.Empty(await context.Set<MstMedicalRecordAccessPurpose>().ToListAsync());
        }

        [Fact]
        public async Task Mengubah_KodeDapatDibetulkanSelamaBelumDipakaiKeperluanLain()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var service = Service(context);

            var pertama = await service.CreateAsync(Permintaan(kode: "SALAHKETIK"), Petugas);
            await service.CreateAsync(Permintaan(kode: "AUDIT", nama: "Audit mutu"), Petugas);

            var dibetulkan = await service.UpdateAsync(
                pertama.Entity!.Id,
                new UpdateMedicalRecordAccessPurposeRequest
                {
                    PurposeCode = "RUJUKAN",
                    PurposeName = "Permintaan rujukan",
                    RequiresReview = true,
                    SortOrder = 1,
                    IsActive = true
                },
                Petugas);

            Assert.Equal(MedicalRecordAccessPurposeStatus.Success, dibetulkan.Status);
            Assert.Equal("RUJUKAN", dibetulkan.Entity!.PurposeCode);
            Assert.Equal(Petugas, dibetulkan.Entity.UpdateBy);

            // Diubah menjadi kode milik keperluan lain tetap ditolak.
            var bentrok = await service.UpdateAsync(
                pertama.Entity.Id,
                new UpdateMedicalRecordAccessPurposeRequest
                {
                    PurposeCode = "AUDIT",
                    PurposeName = "Permintaan rujukan",
                    RequiresReview = true,
                    SortOrder = 1,
                    IsActive = true
                },
                Petugas);

            Assert.Equal(MedicalRecordAccessPurposeStatus.DuplicateCode, bentrok.Status);
        }

        [Fact]
        public async Task Mengubah_KeperluanYangTidakAdaDijawabTidakDitemukan()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var hasil = await Service(context).UpdateAsync(
                Guid.NewGuid(),
                new UpdateMedicalRecordAccessPurposeRequest
                {
                    PurposeCode = "APAPUN",
                    PurposeName = "Apa pun",
                    SortOrder = 0,
                    IsActive = true
                },
                Petugas);

            Assert.Equal(MedicalRecordAccessPurposeStatus.NotFound, hasil.Status);
        }

        [Fact]
        public async Task Pilihan_HanyaMemuatKeperluanAktifDanUrutSesuaiSortOrder()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var service = Service(context);

            await service.CreateAsync(Permintaan(kode: "B", nama: "Kedua", urutan: 2), Petugas);
            await service.CreateAsync(Permintaan(kode: "A", nama: "Pertama", urutan: 1), Petugas);

            var nonaktif = await service.CreateAsync(
                Permintaan(kode: "C", nama: "Ketiga", urutan: 3),
                Petugas);

            await service.UpdateStatusAsync(nonaktif.Entity!.Id, isActive: false, Petugas);

            var pilihan = await service.GetOptionsAsync();

            Assert.Equal(2, pilihan.Count);
            Assert.Equal(new[] { "A", "B" }, pilihan.Select(x => x.PurposeCode).ToArray());

            // Kedua penanda yang menentukan perilaku layar lain ikut terbawa.
            Assert.All(pilihan, x => Assert.True(x.RequiresReview));
        }

        /// <summary>
        /// Menonaktifkan keperluan TIDAK menyentuh jejak akses yang sudah memakainya.
        ///
        /// Jejak adalah catatan bahwa seseorang pernah membuka berkas dengan alasan tertentu
        /// pada suatu waktu. Ia harus tetap terbaca utuh puluhan tahun kemudian, apa pun yang
        /// terjadi pada masternya.
        /// </summary>
        [Fact]
        public async Task Menonaktifkan_TidakMenyentuhJejakAksesYangSudahMemakainya()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var service = Service(context);
            var keperluan = await service.CreateAsync(Permintaan(kode: "RUJUKAN"), Petugas);

            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var jejak = new MrcAccessLog
            {
                PatientId = konteks.PatientId,
                UserId = konteks.UserId,
                UserDisplayNameSnapshot = "Pengguna Uji",
                AccessType = MedicalRecordAccessType.ReasonedAccess,
                AccessScope = MedicalRecordAccessScope.Timeline,
                AccessPurposeId = keperluan.Entity!.Id,
                AccessReason = "Permintaan rujukan dari RS lain",
                IsFlaggedForReview = true,
                AccessedAt = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = konteks.UserId
            };

            context.Set<MrcAccessLog>().Add(jejak);
            await context.SaveChangesAsync();

            await service.UpdateStatusAsync(keperluan.Entity.Id, isActive: false, Petugas);

            var jejakSetelahnya = await context.Set<MrcAccessLog>()
                .AsNoTracking()
                .SingleAsync(x => x.Id == jejak.Id);

            Assert.Equal(keperluan.Entity.Id, jejakSetelahnya.AccessPurposeId);
            Assert.Equal("Permintaan rujukan dari RS lain", jejakSetelahnya.AccessReason);
            Assert.True(jejakSetelahnya.IsFlaggedForReview);
        }

        [Fact]
        public async Task Daftar_MenyaringPencarianDanStatusAktif()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var service = Service(context);

            await service.CreateAsync(Permintaan(kode: "RUJUKAN", nama: "Permintaan rujukan"), Petugas);
            await service.CreateAsync(Permintaan(kode: "AUDIT", nama: "Audit mutu"), Petugas);

            var hasilCari = await service.GetPagedAsync("audit", null, 1, 25);

            Assert.Equal(1, hasilCari.TotalData);
            Assert.Equal("AUDIT", hasilCari.Items.Single().PurposeCode);

            var semua = await service.GetPagedAsync(null, isActive: true, 1, 25);

            Assert.Equal(2, semua.TotalData);
            Assert.Equal(1, semua.PageNumber);
        }

        [Fact]
        public async Task Daftar_HalamanTidakSahDinormalkan()
        {
            using var db = TestDatabase.Create();
            using var context = db.CreateContext();

            var hasil = await Service(context).GetPagedAsync(null, null, pageNumber: 0, pageSize: 0);

            Assert.Equal(1, hasil.PageNumber);
            Assert.Equal(25, hasil.PageSize);
        }
    }
}
