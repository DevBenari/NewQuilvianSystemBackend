using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance `BE-ACC-008` — API jenis jurnal.
    ///
    /// Memakai SQLite di dalam memori lewat <see cref="TestDatabase"/>; tidak menyentuh database
    /// mana pun di luar prosesnya sendiri.
    /// </summary>
    public class JournalTypeServiceTests
    {
        private static readonly Guid Actor = Guid.Parse("44444444-4444-4444-4444-444444444444");

        // ------------------------------------------------------------------
        // Acceptance (1) — kode jenis kembar ditolak 409
        // ------------------------------------------------------------------

        [Fact]
        public async Task KodeJenisKembar_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            Assert.True((await service.CreateAsync(Permintaan("JU", "Jurnal Umum", "JU"), Actor)).Success);

            var kedua = await service.CreateAsync(Permintaan("JU", "Jurnal Umum Lain", "JUX"), Actor);

            Assert.False(kedua.Success);
            Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
        }

        /// <summary>
        /// Kode unik <b>global</b>, bukan per badan hukum — jenis jurnal bersifat struktural dan
        /// berlaku sama untuk semua badan hukum. Perbandingannya mengabaikan besar kecil huruf.
        /// </summary>
        [Fact]
        public async Task KodeJenisKembar_TidakPeduliBesarKecilHuruf()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            await service.CreateAsync(Permintaan("JU", "Jurnal Umum", "JU"), Actor);

            var kedua = await service.CreateAsync(Permintaan("ju", "Jurnal Umum Kecil", "JUX"), Actor);

            Assert.False(kedua.Success);
            Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
        }

        // ------------------------------------------------------------------
        // Acceptance (2) — jenis sistem gagal diubah kode maupun awalan nomor
        // ------------------------------------------------------------------

        [Fact]
        public async Task JenisSistem_KodeGagalDiubah_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);
            var jb = await BuatJenisSistemAsync(db, "JB", "Jurnal Pembalik", "JB");

            var hasil = await service.UpdateAsync(jb, new UpdateJournalTypeRequest
            {
                JournalTypeCode = "JBX",
                JournalTypeName = "Jurnal Pembalik",
                NumberPrefix = "JB"
            }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("dipakai sistem", hasil.Message);
        }

        [Fact]
        public async Task JenisSistem_AwalanNomorGagalDiubah_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);
            var sa = await BuatJenisSistemAsync(db, "SA", "Saldo Awal", "SA");

            var hasil = await service.UpdateAsync(sa, new UpdateJournalTypeRequest
            {
                JournalTypeCode = "SA",
                JournalTypeName = "Saldo Awal",
                NumberPrefix = "SAL"
            }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("awalan", hasil.Message);
        }

        /// <summary>
        /// Yang terkunci hanya kode dan awalan nomor. Nama dan keaktifan tetap boleh diubah —
        /// keduanya tidak dipakai proses pembalikan maupun saldo awal untuk menemukan jenisnya.
        /// </summary>
        [Fact]
        public async Task JenisSistem_NamaMasihBolehDiubah()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);
            var jb = await BuatJenisSistemAsync(db, "JB", "Jurnal Pembalik", "JB");

            var hasil = await service.UpdateAsync(jb, new UpdateJournalTypeRequest
            {
                JournalTypeCode = "JB",
                JournalTypeName = "Jurnal Pembalik Otomatis",
                NumberPrefix = "JB"
            }, Actor);

            Assert.True(hasil.Success);
            Assert.Equal("Jurnal Pembalik Otomatis", hasil.Data!.JournalTypeName);
            Assert.True(hasil.Data!.IsSystemType);
        }

        /// <summary>
        /// Jenis biasa — `JU` dan `JP` — justru harus tetap dapat diubah kodenya. Kalau tidak,
        /// pemilik proses kehilangan kendali atas jenis yang paling sering disesuaikan.
        /// </summary>
        [Fact]
        public async Task JenisBiasa_KodeMasihBolehDiubah()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            var ju = (await service.CreateAsync(Permintaan("JU", "Jurnal Umum", "JU"), Actor)).Data!;

            var hasil = await service.UpdateAsync(ju.Id, new UpdateJournalTypeRequest
            {
                JournalTypeCode = "JUM",
                JournalTypeName = "Jurnal Umum",
                NumberPrefix = "JUM"
            }, Actor);

            Assert.True(hasil.Success);
            Assert.Equal("JUM", hasil.Data!.JournalTypeCode);
        }

        // ------------------------------------------------------------------
        // Acceptance (3) — awalan nomor kosong ditolak 400
        // ------------------------------------------------------------------

        [Fact]
        public async Task AwalanNomorKosong_Ditolak400()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            var hasil = await service.CreateAsync(Permintaan("JU", "Jurnal Umum", "   "), Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("Awalan nomor jurnal wajib diisi", hasil.Message);
        }

        [Fact]
        public async Task AwalanNomorKosongSaatUbah_Ditolak400()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            var ju = (await service.CreateAsync(Permintaan("JU", "Jurnal Umum", "JU"), Actor)).Data!;

            var hasil = await service.UpdateAsync(ju.Id, new UpdateJournalTypeRequest
            {
                JournalTypeCode = "JU",
                JournalTypeName = "Jurnal Umum",
                NumberPrefix = ""
            }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
        }

        [Fact]
        public async Task KodeKosongAtauTerlaluPanjang_Ditolak400()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            Assert.Equal(StatusCodes.Status400BadRequest,
                (await service.CreateAsync(Permintaan("", "Jurnal Umum", "JU"), Actor)).StatusCode);

            Assert.Equal(StatusCodes.Status400BadRequest,
                (await service.CreateAsync(Permintaan(new string('X', 11), "Jurnal Umum", "JU"), Actor)).StatusCode);
        }

        // ------------------------------------------------------------------
        // Tanda sistem tidak dapat diberikan pengguna
        // ------------------------------------------------------------------

        /// <summary>
        /// `CreateJournalTypeRequest` sengaja tanpa `IsSystemType`. Bila pengguna dapat
        /// menetapkannya, ia dapat membuat jenis yang lalu terkunci dari perubahan tanpa alasan
        /// yang sah — aturan "jenis sistem terkunci" berubah dari pengaman menjadi jebakan.
        /// </summary>
        [Fact]
        public async Task JenisBaru_TidakPernahBertandaSistem()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            var hasil = await service.CreateAsync(Permintaan("JK", "Jurnal Kas", "JK"), Actor);

            Assert.True(hasil.Success);
            Assert.False(hasil.Data!.IsSystemType);
            Assert.Null(typeof(CreateJournalTypeRequest).GetProperty("IsSystemType"));
            Assert.Null(typeof(UpdateJournalTypeRequest).GetProperty("IsSystemType"));
        }

        // ------------------------------------------------------------------
        // Call site seeder — menutup ACC-TD-004
        // ------------------------------------------------------------------

        [Fact]
        public async Task Seed_MengisiEmpatJenisJurnalBawaan()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            var hasil = await service.SeedAsync(Actor);

            Assert.True(hasil.Success);
            Assert.Equal(4, hasil.Data!.Inserted);
            Assert.Equal(
                new[] { "JB", "JP", "JU", "SA" },
                hasil.Data!.Items.Select(x => x.JournalTypeCode));

            // JB dan SA bertanda sistem, JU dan JP tidak.
            Assert.Equal(
                new[] { "JB", "SA" },
                hasil.Data!.Items.Where(x => x.IsSystemType).Select(x => x.JournalTypeCode));
        }

        [Fact]
        public async Task Seed_DijalankanDuaKali_TidakMenghasilkanDataGanda()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            await service.SeedAsync(Actor);
            var kedua = await service.SeedAsync(Actor);

            Assert.True(kedua.Success);
            Assert.Equal(0, kedua.Data!.Inserted);
            Assert.Equal(4, kedua.Data!.Skipped);
            Assert.Equal(4, await db.Set<AccJournalType>().CountAsync());
        }

        /// <summary>
        /// Sesudah di-seed, `/options` langsung memberi awalan nomor yang dibutuhkan
        /// `BE-ACC-010`. Inilah yang selama ini kosong dan tercatat sebagai `ACC-TD-004`.
        /// </summary>
        [Fact]
        public async Task SesudahSeed_OptionsMemberiAwalanNomor()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            await service.SeedAsync(Actor);
            var pilihan = await service.GetOptionsAsync();

            Assert.True(pilihan.Success);
            Assert.Equal(4, pilihan.Data!.Count);
            Assert.Equal("JU", pilihan.Data!.Single(x => x.JournalTypeCode == "JU").NumberPrefix);
            Assert.All(pilihan.Data!, x => Assert.True(x.RequiresApproval));
        }

        [Fact]
        public async Task CariMenurutKode_MenemukanJenisAktif()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);
            await service.SeedAsync(Actor);

            var jb = await AccJournalTypeService.CariMenurutKodeAsync(db, "jb");

            Assert.NotNull(jb);
            Assert.Equal("JB", jb!.JournalTypeCode);
            Assert.True(jb.IsSystemType);

            Assert.Null(await AccJournalTypeService.CariMenurutKodeAsync(db, "TIDAK-ADA"));
        }

        // ------------------------------------------------------------------
        // Penjaga badan hukum, ACC-DEC-043
        // ------------------------------------------------------------------

        [Fact]
        public async Task BadanHukumUtamaGanda_SeluruhJalurMenolak()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db, "PT-01");
            await BuatBadanHukumAsync(db, "PT-02");
            var service = new AccJournalTypeService(db);

            Assert.False((await service.GetPagedAsync(new JournalTypePagedQuery())).Success);
            Assert.False((await service.GetOptionsAsync()).Success);
            Assert.False((await service.SeedAsync(Actor)).Success);

            var tambah = await service.CreateAsync(Permintaan("JU", "Jurnal Umum", "JU"), Actor);
            Assert.False(tambah.Success);
            Assert.Equal(StatusCodes.Status409Conflict, tambah.StatusCode);

            Assert.Empty(db.AccJournalTypes);
        }

        // ------------------------------------------------------------------
        // Endpoint baca
        // ------------------------------------------------------------------

        [Fact]
        public async Task Options_HanyaJenisAktif()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            var ju = (await service.CreateAsync(Permintaan("JU", "Jurnal Umum", "JU"), Actor)).Data!;
            await service.CreateAsync(Permintaan("JP", "Jurnal Penyesuaian", "JP"), Actor);

            await service.UpdateAsync(ju.Id, new UpdateJournalTypeRequest
            {
                JournalTypeCode = "JU",
                JournalTypeName = "Jurnal Umum",
                NumberPrefix = "JU",
                IsActive = false
            }, Actor);

            var pilihan = await service.GetOptionsAsync();

            Assert.Equal("JP", Assert.Single(pilihan.Data!).JournalTypeCode);
        }

        [Fact]
        public async Task DaftarJenis_DapatDisaringDanDihalamankan()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);
            await service.SeedAsync(Actor);

            var sistem = await service.GetPagedAsync(new JournalTypePagedQuery { IsSystemType = true });
            Assert.Equal(2, sistem.Data!.TotalData);

            var cari = await service.GetPagedAsync(new JournalTypePagedQuery { Search = "pembalik" });
            Assert.Equal("JB", Assert.Single(cari.Data!.Items).JournalTypeCode);

            var halaman = await service.GetPagedAsync(
                new JournalTypePagedQuery { PageNumber = 1, PageSize = 3 });
            Assert.Equal(4, halaman.Data!.TotalData);
            Assert.Equal(2, halaman.Data!.TotalPage);
            Assert.Equal(3, halaman.Data!.Items.Count);
        }

        [Fact]
        public async Task JenisTidakDitemukan_Mengembalikan404()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccJournalTypeService(db);

            var hasil = await service.UpdateAsync(Guid.NewGuid(), new UpdateJournalTypeRequest
            {
                JournalTypeCode = "JU",
                JournalTypeName = "Jurnal Umum",
                NumberPrefix = "JU"
            }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status404NotFound, hasil.StatusCode);
        }

        // ------------------------------------------------------------------
        // Bahan uji
        // ------------------------------------------------------------------

        private static async Task<Guid> BuatBadanHukumAsync(
            ApplicationDbContext db, string kode = "PT-01")
        {
            var entity = new MstLegalEntity
            {
                Id = Guid.NewGuid(),
                LegalEntityCode = kode,
                LegalEntityName = "PT Uji " + kode,
                IsActive = true,
                IsDefault = true
            };

            db.Set<MstLegalEntity>().Add(entity);
            await db.SaveChangesAsync();
            return entity.Id;
        }

        /// <summary>
        /// Jenis bertanda sistem tidak dapat dibuat lewat service — itu memang batasnya. Untuk
        /// menguji aturan penguncian, barisnya ditanam langsung.
        /// </summary>
        private static async Task<Guid> BuatJenisSistemAsync(
            ApplicationDbContext db, string kode, string nama, string awalan)
        {
            var jenis = new AccJournalType
            {
                Id = Guid.NewGuid(),
                JournalTypeCode = kode,
                JournalTypeName = nama,
                NumberPrefix = awalan,
                RequiresApproval = true,
                IsSystemType = true,
                IsActive = true
            };

            db.Set<AccJournalType>().Add(jenis);
            await db.SaveChangesAsync();
            return jenis.Id;
        }

        private static CreateJournalTypeRequest Permintaan(string kode, string nama, string awalan)
            => new()
            {
                JournalTypeCode = kode,
                JournalTypeName = nama,
                NumberPrefix = awalan
            };
    }
}
