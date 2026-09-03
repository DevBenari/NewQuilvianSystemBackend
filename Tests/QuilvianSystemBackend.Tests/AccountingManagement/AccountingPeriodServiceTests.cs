using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance `BE-ACC-009` — API periode akuntansi.
    /// </summary>
    public class AccountingPeriodServiceTests
    {
        private static readonly Guid Actor = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // ------------------------------------------------------------------
        // Acceptance (1) — dua belas periode, tahun kabisat benar
        // ------------------------------------------------------------------

        [Fact]
        public async Task Generate_MenghasilkanTepatDuaBelasPeriode()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            var hasil = await service.GenerateAsync(Permintaan(pt, 2027), Actor);

            Assert.True(hasil.Success);
            Assert.Equal(StatusCodes.Status201Created, hasil.StatusCode);
            Assert.Equal(12, hasil.Data!.Count);

            Assert.Equal("2027-01", hasil.Data![0].PeriodCode);
            Assert.Equal("2027-12", hasil.Data![11].PeriodCode);

            // Kode periode tepat tujuh karakter — ACC-DEC-013.
            Assert.All(hasil.Data!, x => Assert.Equal(7, x.PeriodCode.Length));

            // Seluruhnya lahir dalam keadaan terbuka.
            Assert.All(hasil.Data!, x => Assert.Equal(AccountingPeriodStatus.Open, x.PeriodStatus));

            Assert.Equal(12, await db.Set<AccAccountingPeriod>().CountAsync());
        }

        /// <summary>
        /// Tahun kabisat ditangani perhitungan tanggal, bukan didaftar manual. 2028 kabisat,
        /// 2027 tidak, dan 2100 **bukan** kabisat walau habis dibagi empat — itu perangkap
        /// klasik yang hanya tertangkap bila aturan seratus-tahunan ikut benar.
        /// </summary>
        [Theory]
        [InlineData(2027, 28)]
        [InlineData(2028, 29)]
        [InlineData(2100, 28)]
        [InlineData(2000, 29)]
        public async Task Generate_TahunKabisatBenarPadaFebruari(int tahun, int hariTerakhirFebruari)
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            var hasil = await service.GenerateAsync(Permintaan(pt, tahun), Actor);

            var februari = hasil.Data!.Single(x => x.PeriodMonth == 2);
            Assert.Equal(hariTerakhirFebruari, februari.EndDate.Day);
            Assert.Equal(1, februari.StartDate.Day);
        }

        [Fact]
        public async Task Generate_SetiapPeriodeBerakhirDiHariTerakhirBulannya()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            var hasil = await service.GenerateAsync(Permintaan(pt, 2027), Actor);

            foreach (var p in hasil.Data!)
            {
                Assert.Equal(DateTime.DaysInMonth(2027, p.PeriodMonth), p.EndDate.Day);
                Assert.Equal(p.PeriodMonth, p.StartDate.Month);
                Assert.Equal(p.PeriodMonth, p.EndDate.Month);
            }
        }

        // ------------------------------------------------------------------
        // Acceptance (2) — tahun yang sama dua kali ditolak 409
        // ------------------------------------------------------------------

        [Fact]
        public async Task Generate_TahunYangSamaDuaKali_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            Assert.True((await service.GenerateAsync(Permintaan(pt, 2027), Actor)).Success);

            var kedua = await service.GenerateAsync(Permintaan(pt, 2027), Actor);

            Assert.False(kedua.Success);
            Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
            Assert.Contains("2027", kedua.Message);

            // Penolakan terjadi sebelum menulis — tetap 12, bukan 24.
            Assert.Equal(12, await db.Set<AccAccountingPeriod>().CountAsync());
        }

        [Fact]
        public async Task Generate_TahunBerbeda_Diterima()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            Assert.True((await service.GenerateAsync(Permintaan(pt, 2027), Actor)).Success);
            Assert.True((await service.GenerateAsync(Permintaan(pt, 2028), Actor)).Success);

            Assert.Equal(24, await db.Set<AccAccountingPeriod>().CountAsync());
        }

        // ------------------------------------------------------------------
        // Acceptance (3) — Closed dibuka kembali menjadi SoftClosed
        // ------------------------------------------------------------------

        /// <summary>
        /// Butir yang paling mudah salah di seluruh task ini. Mengembalikan periode tutup
        /// permanen ke <c>Open</c> melanggar `ACC-DEC-028` dan membuka pintu bagi jurnal
        /// operasional baru masuk ke tahun buku yang sudah ditutup.
        /// </summary>
        [Fact]
        public async Task Reopen_DariClosed_MenghasilkanSoftClosed_BukanOpen()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.Closed);

            var hasil = await service.ReopenAsync(periode, new ReopenPeriodRequest
            {
                Reason = "Koreksi penyusutan yang terlewat."
            }, Actor);

            Assert.True(hasil.Success);
            Assert.Equal(AccountingPeriodStatus.SoftClosed, hasil.Data!.PeriodStatus);
            Assert.NotEqual(AccountingPeriodStatus.Open, hasil.Data!.PeriodStatus);

            // Dan hanya penyesuaian serta pembalikan yang diterimanya.
            Assert.Equal(new[] { "JP", "JB" }, hasil.Data!.AcceptedJournalTypeCodes);
        }

        [Fact]
        public async Task Reopen_DariSoftClosed_MenghasilkanOpen()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.SoftClosed);

            var hasil = await service.ReopenAsync(periode, new ReopenPeriodRequest
            {
                Reason = "Masih ada jurnal operasional yang tertinggal."
            }, Actor);

            Assert.True(hasil.Success);
            Assert.Equal(AccountingPeriodStatus.Open, hasil.Data!.PeriodStatus);
        }

        [Fact]
        public async Task Reopen_PeriodeYangMasihTerbuka_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.Open);

            var hasil = await service.ReopenAsync(periode, new ReopenPeriodRequest
            {
                Reason = "Tidak ada gunanya."
            }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        }

        // ------------------------------------------------------------------
        // Acceptance (4) — buka kembali tanpa alasan ditolak 400
        // ------------------------------------------------------------------

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Reopen_TanpaAlasan_Ditolak400(string alasan)
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.Closed);

            var hasil = await service.ReopenAsync(periode, new ReopenPeriodRequest { Reason = alasan }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
            Assert.Contains("Alasan pembukaan kembali wajib diisi", hasil.Message);

            // Status tidak berubah.
            Assert.Equal(AccountingPeriodStatus.Closed,
                (await db.Set<AccAccountingPeriod>().SingleAsync()).PeriodStatus);
        }

        [Fact]
        public async Task Reopen_AlasanTercatatDiJejakAudit()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.Closed);

            await service.ReopenAsync(periode, new ReopenPeriodRequest
            {
                Reason = "Audit eksternal meminta koreksi penyusutan."
            }, Actor);

            var tersimpan = await db.Set<AccAccountingPeriod>().SingleAsync();

            Assert.Equal("Audit eksternal meminta koreksi penyusutan.", tersimpan.LastReasonNote);
            Assert.Equal(Actor, tersimpan.ReopenedBy);
            Assert.NotNull(tersimpan.ReopenedAt);
        }

        // ------------------------------------------------------------------
        // Penutupan
        // ------------------------------------------------------------------

        [Fact]
        public async Task Close_SementaraDanPermanen_MenghasilkanStatusYangBenar()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);

            var a = await BuatPeriodeAsync(db, AccountingPeriodStatus.Open, bulan: 1);
            var sementara = await service.CloseAsync(a, new ClosePeriodRequest { Permanent = false }, Actor);
            Assert.Equal(AccountingPeriodStatus.SoftClosed, sementara.Data!.PeriodStatus);

            var b = await BuatPeriodeAsync(db, AccountingPeriodStatus.Open, bulan: 2);
            var permanen = await service.CloseAsync(b, new ClosePeriodRequest { Permanent = true }, Actor);
            Assert.Equal(AccountingPeriodStatus.Closed, permanen.Data!.PeriodStatus);
        }

        [Fact]
        public async Task Close_SoftClosedMenjadiClosed_Diterima()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.SoftClosed);

            var hasil = await service.CloseAsync(periode, new ClosePeriodRequest { Permanent = true }, Actor);

            Assert.True(hasil.Success);
            Assert.Equal(AccountingPeriodStatus.Closed, hasil.Data!.PeriodStatus);
        }

        /// <summary>
        /// Menurunkan `Closed` menjadi `SoftClosed` lewat endpoint tutup adalah pembukaan
        /// kembali yang menyamar — dan pembukaan kembali mewajibkan alasan tertulis. Ditolak
        /// supaya kewajiban itu tidak dapat dilewati.
        /// </summary>
        [Fact]
        public async Task Close_ClosedMenjadiSoftClosed_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.Closed);

            var hasil = await service.CloseAsync(periode, new ClosePeriodRequest { Permanent = false }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("pembukaan kembali", hasil.Message);
        }

        [Fact]
        public async Task Close_StatusYangSama_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var service = new AccAccountingPeriodService(db);
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.SoftClosed);

            var hasil = await service.CloseAsync(periode, new ClosePeriodRequest { Permanent = false }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        }

        // ------------------------------------------------------------------
        // Aturan yang dipakai BE-ACC-010 dan BE-ACC-011
        // ------------------------------------------------------------------

        /// <summary>
        /// `ACC-STATE-0.1` bagian 2.2, baris demi baris. Inilah aturan yang kelak menentukan
        /// boleh atau tidaknya sebuah jurnal disahkan.
        /// </summary>
        [Theory]
        // Terbuka menerima semuanya.
        [InlineData(AccountingPeriodStatus.Open, "JU", true)]
        [InlineData(AccountingPeriodStatus.Open, "JP", true)]
        [InlineData(AccountingPeriodStatus.Open, "JB", true)]
        [InlineData(AccountingPeriodStatus.Open, "SA", true)]
        // Tutup sementara hanya penyesuaian dan pembalikan.
        [InlineData(AccountingPeriodStatus.SoftClosed, "JU", false)]
        [InlineData(AccountingPeriodStatus.SoftClosed, "JP", true)]
        [InlineData(AccountingPeriodStatus.SoftClosed, "JB", true)]
        [InlineData(AccountingPeriodStatus.SoftClosed, "SA", false)]
        // Tutup permanen menolak semuanya.
        [InlineData(AccountingPeriodStatus.Closed, "JU", false)]
        [InlineData(AccountingPeriodStatus.Closed, "JP", false)]
        [InlineData(AccountingPeriodStatus.Closed, "JB", false)]
        [InlineData(AccountingPeriodStatus.Closed, "SA", false)]
        public void JenisJurnalYangDiterima_SesuaiMatriksStatus(
            AccountingPeriodStatus status, string kode, bool diterima)
        {
            var alasan = AccAccountingPeriodService.AlasanPenolakanJenisJurnal(
                status, "September 2026", kode);

            if (diterima)
            {
                Assert.Null(alasan);
            }
            else
            {
                Assert.NotNull(alasan);
                // Pesannya menyebut nama periode, bukan istilah teknis.
                Assert.Contains("September 2026", alasan!);
            }
        }

        [Fact]
        public async Task AlasanPenolakan_DibacaDariPeriodeTersimpan()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var periode = await BuatPeriodeAsync(db, AccountingPeriodStatus.SoftClosed, bulan: 9);

            var ditolak = await AccAccountingPeriodService
                .AlasanPenolakanJenisJurnalAsync(db, periode, "JU");
            var diterima = await AccAccountingPeriodService
                .AlasanPenolakanJenisJurnalAsync(db, periode, "JP");

            Assert.NotNull(ditolak);
            Assert.Contains("September", ditolak!);
            Assert.Null(diterima);
        }

        // ------------------------------------------------------------------
        // Endpoint baca
        // ------------------------------------------------------------------

        [Fact]
        public async Task Current_MengembalikanPeriodeYangMencakupHariIni()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            await service.GenerateAsync(Permintaan(pt, DateTime.UtcNow.Year), Actor);

            var hasil = await service.GetCurrentAsync(pt);

            Assert.True(hasil.Success);
            Assert.Equal(DateTime.UtcNow.Month, hasil.Data!.PeriodMonth);
        }

        [Fact]
        public async Task Current_TanpaPeriode_Mengembalikan404()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            var hasil = await service.GetCurrentAsync(pt);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status404NotFound, hasil.StatusCode);
        }

        [Fact]
        public async Task DaftarPeriode_DapatDisaringPerTahunDanStatus()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            await service.GenerateAsync(Permintaan(pt, 2027), Actor);
            await service.GenerateAsync(Permintaan(pt, 2028), Actor);

            var perTahun = await service.GetPagedAsync(
                new AccountingPeriodPagedQuery { FiscalYear = 2027, PageSize = 100 });
            Assert.Equal(12, perTahun.Data!.TotalData);

            var terbuka = await service.GetPagedAsync(
                new AccountingPeriodPagedQuery { PeriodStatus = AccountingPeriodStatus.Open, PageSize = 100 });
            Assert.Equal(24, terbuka.Data!.TotalData);

            // Nama periode dibaca pengguna, bukan kode teknis.
            Assert.Equal("Januari 2027", perTahun.Data!.Items[0].PeriodName);
        }

        [Fact]
        public async Task BadanHukumTidakDitemukan_Ditolak400()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccAccountingPeriodService(db);

            var hasil = await service.GenerateAsync(Permintaan(Guid.NewGuid(), 2027), Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
        }

        [Fact]
        public async Task BadanHukumUtamaGanda_SeluruhJalurMenolak()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db, "PT-01");
            await BuatBadanHukumAsync(db, "PT-02");
            var service = new AccAccountingPeriodService(db);

            Assert.False((await service.GetPagedAsync(new AccountingPeriodPagedQuery())).Success);
            Assert.False((await service.GetCurrentAsync(pt)).Success);
            Assert.False((await service.GenerateAsync(Permintaan(pt, 2027), Actor)).Success);

            Assert.Empty(db.AccAccountingPeriods);
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

        private static async Task<Guid> BuatPeriodeAsync(
            ApplicationDbContext db,
            AccountingPeriodStatus status,
            int bulan = 9)
        {
            var pt = await db.Set<MstLegalEntity>().Select(x => x.Id).FirstOrDefaultAsync();
            if (pt == Guid.Empty) pt = await BuatBadanHukumAsync(db);

            var periode = new AccAccountingPeriod
            {
                Id = Guid.NewGuid(),
                LegalEntityId = pt,
                PeriodCode = $"2026-{bulan:D2}",
                FiscalYear = 2026,
                PeriodMonth = bulan,
                StartDate = new DateTime(2026, bulan, 1),
                EndDate = new DateTime(2026, bulan, DateTime.DaysInMonth(2026, bulan)),
                PeriodStatus = status
            };

            db.Set<AccAccountingPeriod>().Add(periode);
            await db.SaveChangesAsync();
            return periode.Id;
        }

        private static GenerateAccountingPeriodRequest Permintaan(Guid legalEntityId, int tahun)
            => new() { LegalEntityId = legalEntityId, FiscalYear = tahun };
    }
}
