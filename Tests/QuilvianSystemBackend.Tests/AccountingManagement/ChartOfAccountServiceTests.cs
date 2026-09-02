using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance `BE-ACC-007` — API daftar akun.
    ///
    /// Seluruh uji memakai SQLite di dalam memori lewat <see cref="TestDatabase"/>, jadi
    /// **tidak** menyentuh database mana pun di luar prosesnya sendiri.
    /// </summary>
    public class ChartOfAccountServiceTests
    {
        private static readonly Guid Actor = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // ------------------------------------------------------------------
        // Acceptance (1) — kode akun kembar
        // ------------------------------------------------------------------

        [Fact]
        public async Task KodeAkunKembarPadaBadanHukumSama_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var pertama = await service.CreateAsync(Permintaan(badanHukum, "1-1001", "Kas Besar"), Actor);
            Assert.True(pertama.Success);

            var kedua = await service.CreateAsync(Permintaan(badanHukum, "1-1001", "Kas Kecil"), Actor);

            Assert.False(kedua.Success);
            Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
            Assert.Contains("1-1001", kedua.Message);
        }

        /// <summary>
        /// Kode yang sama pada badan hukum **berbeda** justru harus diterima — itu inti
        /// `ACC-DEC-037`, dan pemisahan data itulah yang tetap berlaku walau `ACC-DEC-041`
        /// menunda penyaringan per pengguna.
        /// </summary>
        [Fact]
        public async Task KodeAkunSamaPadaBadanHukumBerbeda_Diterima()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt1 = await BuatBadanHukumAsync(db, "PT-01", "PT Sehat Sentosa");
            // Badan hukum kedua AKTIF tetapi bukan yang utama — persis keadaan database
            // sungguhan (ACC-DEC-043). Penjaga tidak menyala karena default-nya tetap satu.
            var pt2 = await BuatBadanHukumAsync(db, "PT-02", "PT Sehat Mandiri", utama: false);
            var service = new AccChartOfAccountService(db);

            var a = await service.CreateAsync(Permintaan(pt1, "1-1001", "Kas Besar"), Actor);
            var b = await service.CreateAsync(Permintaan(pt2, "1-1001", "Kas Besar"), Actor);

            Assert.True(a.Success);
            Assert.True(b.Success);
            Assert.Equal(2, await db.Set<AccChartOfAccount>().CountAsync());
        }

        // ------------------------------------------------------------------
        // Acceptance (2) — akun beranak tidak menerima transaksi
        // ------------------------------------------------------------------

        [Fact]
        public async Task AkunBeranak_TidakDapatMenerimaTransaksi_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var induk = (await service.CreateAsync(
                Permintaan(badanHukum, "1", "Aset", postable: false), Actor)).Data!;

            var anak = Permintaan(badanHukum, "1-1001", "Kas Besar", postable: true);
            anak.ParentAccountId = induk.Id;
            anak.AccountLevel = 2;
            Assert.True((await service.CreateAsync(anak, Actor)).Success);

            var ubah = await service.UpdateAsync(induk.Id, new UpdateChartOfAccountRequest
            {
                AccountCode = "1",
                AccountName = "Aset",
                AccountLevel = 1,
                IsPostable = true
            }, Actor);

            Assert.False(ubah.Success);
            Assert.Equal(StatusCodes.Status409Conflict, ubah.StatusCode);
            Assert.Contains("Gunakan akun turunannya", ubah.Message);
        }

        // ------------------------------------------------------------------
        // Acceptance (3) — akun bersaldo gagal dinonaktifkan
        // ------------------------------------------------------------------

        [Fact]
        public async Task AkunBersaldo_GagalDinonaktifkan_PesanMenyebutJumlah()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var akun = (await service.CreateAsync(
                Permintaan(badanHukum, "1-1201", "Piutang Asuransi X", postable: true), Actor)).Data!;

            await TanamBarisJurnalAsync(db, badanHukum, akun.Id, debit: 15_000_000m,
                status: JournalStatus.Posted);

            var hasil = await service.DeactivateAsync(akun.Id, new DeactivateChartOfAccountRequest(), Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("15.000.000", hasil.Message);
            Assert.True((await db.Set<AccChartOfAccount>().SingleAsync(x => x.Id == akun.Id)).IsActive);
        }

        [Fact]
        public async Task AkunBersaldoNol_BerhasilDinonaktifkan()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var akun = (await service.CreateAsync(
                Permintaan(badanHukum, "1-1209", "Piutang Lain-lain", postable: true), Actor)).Data!;

            // Debit dan kredit sama besar — saldonya nol, jadi boleh dinonaktifkan.
            await TanamBarisJurnalAsync(db, badanHukum, akun.Id, debit: 5_000_000m, status: JournalStatus.Posted);
            await TanamBarisJurnalAsync(db, badanHukum, akun.Id, kredit: 5_000_000m, status: JournalStatus.Posted);

            var hasil = await service.DeactivateAsync(akun.Id, new DeactivateChartOfAccountRequest(), Actor);

            Assert.True(hasil.Success);
            Assert.False((await db.Set<AccChartOfAccount>().SingleAsync(x => x.Id == akun.Id)).IsActive);
        }

        /// <summary>
        /// Jebakan yang paling mudah terlewat: jurnal `Draft` **bukan** transaksi.
        ///
        /// Kalau saldo dihitung tanpa menyaring `JournalStatus == Posted`, akun yang sebenarnya
        /// masih bebas akan terkunci hanya karena ada seseorang menyimpan draft. Kesalahan ini
        /// tidak membuat test lain merah, dan baru terasa sebagai "akun tidak bisa dinonaktifkan
        /// padahal belum dipakai".
        /// </summary>
        [Fact]
        public async Task JurnalDraft_TidakMenguncikanAkun()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var akun = (await service.CreateAsync(
                Permintaan(badanHukum, "5-1001", "Beban Obat", postable: true), Actor)).Data!;

            await TanamBarisJurnalAsync(db, badanHukum, akun.Id, debit: 9_000_000m,
                status: JournalStatus.Draft);

            // Saldo dianggap nol, jadi penonaktifan lolos.
            var nonaktif = await service.DeactivateAsync(akun.Id, new DeactivateChartOfAccountRequest(), Actor);
            Assert.True(nonaktif.Success);

            // Dan kodenya masih boleh diubah, karena belum ada transaksi yang disahkan.
            var ubah = await service.UpdateAsync(akun.Id, new UpdateChartOfAccountRequest
            {
                AccountCode = "5-1002",
                AccountName = "Beban Obat",
                AccountLevel = 1,
                IsPostable = true
            }, Actor);

            Assert.True(ubah.Success);
            Assert.Equal("5-1002", ubah.Data!.AccountCode);
        }

        // ------------------------------------------------------------------
        // Acceptance (4) — kode akun bertransaksi gagal diubah
        // ------------------------------------------------------------------

        [Fact]
        public async Task KodeAkunBertransaksi_GagalDiubah_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var akun = (await service.CreateAsync(
                Permintaan(badanHukum, "4-1001", "Pendapatan Rawat Inap", postable: true), Actor)).Data!;

            await TanamBarisJurnalAsync(db, badanHukum, akun.Id, kredit: 1_000_000m,
                status: JournalStatus.Posted);

            var hasil = await service.UpdateAsync(akun.Id, new UpdateChartOfAccountRequest
            {
                AccountCode = "4-1002",
                AccountName = "Pendapatan Rawat Inap",
                AccountLevel = 1,
                IsPostable = true
            }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("tidak dapat diubah", hasil.Message);
        }

        /// <summary>
        /// Akun bertransaksi tetap boleh disunting selama kodenya **tidak** berubah — mengganti
        /// nama atau keterangan tidak mengancam integritas jurnal yang sudah disahkan.
        /// </summary>
        [Fact]
        public async Task AkunBertransaksi_NamanyaMasihBolehDiubah()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var akun = (await service.CreateAsync(
                Permintaan(badanHukum, "4-1001", "Pendapatan Ranap", postable: true), Actor)).Data!;

            await TanamBarisJurnalAsync(db, badanHukum, akun.Id, kredit: 1_000_000m,
                status: JournalStatus.Posted);

            var hasil = await service.UpdateAsync(akun.Id, new UpdateChartOfAccountRequest
            {
                AccountCode = "4-1001",
                AccountName = "Pendapatan Rawat Inap",
                AccountLevel = 1,
                IsPostable = true
            }, Actor);

            Assert.True(hasil.Success);
            Assert.Equal("Pendapatan Rawat Inap", hasil.Data!.AccountName);
        }

        // ------------------------------------------------------------------
        // Acceptance (5b) — penjaga jumlah badan hukum, ACC-DEC-041
        // ------------------------------------------------------------------

        [Fact]
        public async Task LebihDariSatuBadanHukumUtama_SeluruhEndpointMenolak()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt1 = await BuatBadanHukumAsync(db, "PT-01", "PT Sehat Sentosa");
            await BuatBadanHukumAsync(db, "PT-02", "PT Sehat Mandiri");
            var service = new AccChartOfAccountService(db);

            var daftar = await service.GetPagedAsync(new ChartOfAccountPagedQuery());
            var pilihan = await service.GetOptionsAsync(pt1, null);
            var susunan = await service.GetTreeAsync(pt1);
            var tambah = await service.CreateAsync(Permintaan(pt1, "1-1001", "Kas Besar"), Actor);

            Assert.False(daftar.Success);
            Assert.False(pilihan.Success);
            Assert.False(susunan.Success);
            Assert.False(tambah.Success);

            Assert.Equal(StatusCodes.Status409Conflict, tambah.StatusCode);
            Assert.Contains("lebih dari satu badan hukum bertanda utama", tambah.Message);

            // Penjaga menolak SEBELUM menulis apa pun.
            Assert.Empty(db.AccChartOfAccounts);
        }

        /// <summary>
        /// `ACC-DEC-043` — nol badan hukum utama juga ditolak. Tanpa penanda utama, modul tidak
        /// dapat menentukan buku besar mana yang dipakai, dan menebaknya jauh lebih berbahaya
        /// daripada berhenti.
        /// </summary>
        [Fact]
        public async Task TanpaBadanHukumUtama_SeluruhEndpointMenolak()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt = await BuatBadanHukumAsync(db, "PT-01", "PT Sehat Sentosa", utama: false);
            var service = new AccChartOfAccountService(db);

            var hasil = await service.CreateAsync(Permintaan(pt, "1-1001", "Kas Besar"), Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("bertanda utama", hasil.Message);
            Assert.Empty(db.AccChartOfAccounts);
        }

        /// <summary>
        /// Keadaan database sungguhan per 2 September 2026: tiga badan hukum aktif, hanya satu
        /// bertanda utama. Penjaga **tidak** menyala, dan Accounting berjalan normal.
        /// </summary>
        [Fact]
        public async Task TigaBadanHukumAktifDenganSatuUtama_AccountingTetapBerjalan()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var mmc = await BuatBadanHukumAsync(db, "LE-MMC-001", "PT Metropolitan Medical Centre");
            await BuatBadanHukumAsync(db, "LE-MDC-001", "PT Metropolitan Diagnostic Centre", utama: false);
            await BuatBadanHukumAsync(db, "LE-MHS-001", "PT Metropolitan Healthcare Services", utama: false);
            var service = new AccChartOfAccountService(db);

            var hasil = await service.CreateAsync(Permintaan(mmc, "1-1001", "Kas Besar"), Actor);

            Assert.True(hasil.Success);
            Assert.Equal(mmc, hasil.Data!.LegalEntityId);
        }

        /// <summary>
        /// Badan hukum yang sudah nonaktif atau terhapus lunak tidak dihitung — keduanya tidak
        /// dapat menerima pembukuan baru, jadi tidak menimbulkan risiko yang dijaga.
        /// </summary>
        [Fact]
        public async Task BadanHukumUtamaYangNonaktifAtauTerhapus_TidakDihitung()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt1 = await BuatBadanHukumAsync(db, "PT-01", "PT Sehat Sentosa");
            await BuatBadanHukumAsync(db, "PT-02", "PT Lama", aktif: false);
            await BuatBadanHukumAsync(db, "PT-03", "PT Dihapus", terhapus: true);
            var service = new AccChartOfAccountService(db);

            var hasil = await service.CreateAsync(Permintaan(pt1, "1-1001", "Kas Besar"), Actor);

            Assert.True(hasil.Success);
        }

        // ------------------------------------------------------------------
        // Aturan lain ACC-VALIDATION-0.2 bagian 1
        // ------------------------------------------------------------------

        [Fact]
        public async Task TingkatAkunDiLuarSatuSampaiLima_Ditolak400()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var permintaan = Permintaan(badanHukum, "1-1001", "Kas Besar");
            permintaan.AccountLevel = 6;

            var hasil = await service.CreateAsync(permintaan, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
        }

        [Fact]
        public async Task IndukDariBadanHukumBerbeda_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var pt1 = await BuatBadanHukumAsync(db, "PT-01", "PT Sehat Sentosa");
            var pt2 = await BuatBadanHukumAsync(db, "PT-02", "PT Sehat Mandiri", utama: false);
            var service = new AccChartOfAccountService(db);

            var indukPt2 = (await service.CreateAsync(Permintaan(pt2, "1", "Aset"), Actor)).Data!;

            var anak = Permintaan(pt1, "1-1001", "Kas Besar");
            anak.ParentAccountId = indukPt2.Id;
            anak.AccountLevel = 2;

            var hasil = await service.CreateAsync(anak, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("badan hukum yang sama", hasil.Message);
        }

        [Fact]
        public async Task AkunTidakDapatMenjadiIndukBagiDirinyaSendiri_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var akun = (await service.CreateAsync(Permintaan(badanHukum, "1", "Aset"), Actor)).Data!;

            var hasil = await service.UpdateAsync(akun.Id, new UpdateChartOfAccountRequest
            {
                AccountCode = "1",
                AccountName = "Aset",
                AccountLevel = 1,
                ParentAccountId = akun.Id
            }, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        }

        [Fact]
        public async Task AkunBertransaksi_TidakDapatDiberiTurunan_Ditolak409()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var induk = (await service.CreateAsync(
                Permintaan(badanHukum, "1-1001", "Kas Besar", postable: true), Actor)).Data!;

            await TanamBarisJurnalAsync(db, badanHukum, induk.Id, debit: 1_000_000m,
                status: JournalStatus.Posted);

            var anak = Permintaan(badanHukum, "1-1001-01", "Kas Besar Unit A");
            anak.ParentAccountId = induk.Id;
            anak.AccountLevel = 2;

            var hasil = await service.CreateAsync(anak, Actor);

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
            Assert.Contains("sudah memiliki transaksi", hasil.Message);
        }

        // ------------------------------------------------------------------
        // Endpoint baca
        // ------------------------------------------------------------------

        /// <summary>
        /// `/options` hanya mengembalikan akun yang menerima transaksi dan aktif, sehingga
        /// `ACC-DEC-022` terjaga sejak di layar. `RequiresCostCenter` diturunkan dari jenis
        /// akun, bukan dibaca dari kolom — roadmap `BE-ACC-003` melarang kolomnya.
        /// </summary>
        [Fact]
        public async Task Options_HanyaAkunMenerimaTransaksiDanAktif_DenganRequiresCostCenterDiturunkan()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            await service.CreateAsync(Permintaan(badanHukum, "1", "Aset", postable: false), Actor);
            await service.CreateAsync(Permintaan(badanHukum, "1-1001", "Kas Besar", postable: true), Actor);
            var beban = (await service.CreateAsync(
                Permintaan(badanHukum, "5-1001", "Beban Obat", AccountType.Expense, NormalBalance.Debit, true),
                Actor)).Data!;
            var nonaktif = (await service.CreateAsync(
                Permintaan(badanHukum, "1-1002", "Kas Kecil", postable: true), Actor)).Data!;
            await service.DeactivateAsync(nonaktif.Id, new DeactivateChartOfAccountRequest(), Actor);

            var hasil = await service.GetOptionsAsync(badanHukum, null);

            Assert.True(hasil.Success);
            Assert.Equal(new[] { "1-1001", "5-1001" }, hasil.Data!.Select(x => x.AccountCode));

            Assert.True(hasil.Data!.Single(x => x.Id == beban.Id).RequiresCostCenter);
            Assert.False(hasil.Data!.Single(x => x.AccountCode == "1-1001").RequiresCostCenter);
        }

        [Fact]
        public async Task Tree_MenyusunIndukDanAnak()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var induk = (await service.CreateAsync(Permintaan(badanHukum, "1", "Aset"), Actor)).Data!;

            var anak = Permintaan(badanHukum, "1-1001", "Kas Besar", postable: true);
            anak.ParentAccountId = induk.Id;
            anak.AccountLevel = 2;
            await service.CreateAsync(anak, Actor);

            var hasil = await service.GetTreeAsync(badanHukum);

            Assert.True(hasil.Success);
            var akar = Assert.Single(hasil.Data!);
            Assert.Equal("1", akar.AccountCode);
            Assert.Equal("1-1001", Assert.Single(akar.Children).AccountCode);
        }

        [Fact]
        public async Task DaftarAkun_DapatDisaringDanDihalamankan()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            var badanHukum = await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            await service.CreateAsync(Permintaan(badanHukum, "1-1001", "Kas Besar", postable: true), Actor);
            await service.CreateAsync(Permintaan(badanHukum, "1-1002", "Kas Kecil", postable: true), Actor);
            await service.CreateAsync(
                Permintaan(badanHukum, "5-1001", "Beban Obat", AccountType.Expense, NormalBalance.Debit, true),
                Actor);

            var pencarian = await service.GetPagedAsync(new ChartOfAccountPagedQuery { Search = "kas" });
            Assert.Equal(2, pencarian.Data!.TotalData);

            var perJenis = await service.GetPagedAsync(
                new ChartOfAccountPagedQuery { AccountType = AccountType.Expense });
            Assert.Equal("5-1001", Assert.Single(perJenis.Data!.Items).AccountCode);

            var halaman = await service.GetPagedAsync(
                new ChartOfAccountPagedQuery { PageNumber = 1, PageSize = 2 });
            Assert.Equal(3, halaman.Data!.TotalData);
            Assert.Equal(2, halaman.Data!.TotalPage);
            Assert.Equal(2, halaman.Data!.Items.Count);
        }

        [Fact]
        public async Task AkunTidakDitemukan_Mengembalikan404()
        {
            using var uji = TestDatabase.Create();
            await using var db = uji.CreateContext();
            await BuatBadanHukumAsync(db);
            var service = new AccChartOfAccountService(db);

            var hasil = await service.GetByIdAsync(Guid.NewGuid());

            Assert.False(hasil.Success);
            Assert.Equal(StatusCodes.Status404NotFound, hasil.StatusCode);
        }

        // ------------------------------------------------------------------
        // Bahan uji
        // ------------------------------------------------------------------

        private static async Task<Guid> BuatBadanHukumAsync(
            ApplicationDbContext db,
            string kode = "PT-01",
            string nama = "PT Sehat Sentosa",
            bool aktif = true,
            bool terhapus = false,
            bool utama = true)
        {
            var entity = new MstLegalEntity
            {
                Id = Guid.NewGuid(),
                LegalEntityCode = kode,
                LegalEntityName = nama,
                IsActive = aktif,
                IsDelete = terhapus,
                IsDefault = utama
            };

            db.Set<MstLegalEntity>().Add(entity);
            await db.SaveChangesAsync();
            return entity.Id;
        }

        private static CreateChartOfAccountRequest Permintaan(
            Guid legalEntityId,
            string kode,
            string nama,
            AccountType jenis = AccountType.Asset,
            NormalBalance saldoNormal = NormalBalance.Debit,
            bool postable = false)
        {
            return new CreateChartOfAccountRequest
            {
                LegalEntityId = legalEntityId,
                AccountCode = kode,
                AccountName = nama,
                AccountType = jenis,
                NormalBalance = saldoNormal,
                AccountLevel = 1,
                IsPostable = postable
            };
        }

        private static CreateChartOfAccountRequest Permintaan(
            Guid legalEntityId, string kode, string nama, bool postable)
            => Permintaan(legalEntityId, kode, nama, AccountType.Asset, NormalBalance.Debit, postable);

        /// <summary>
        /// Menanam baris jurnal langsung ke basis data uji.
        ///
        /// Sengaja tidak lewat endpoint: `BE-ACC-010` yang membuat jurnal belum ada. Kalau
        /// menunggu endpoint itu, acceptance (3) dan (4) `BE-ACC-007` akan lolos palsu karena
        /// saldonya selalu nol dan tidak pernah ada transaksi yang menguji aturannya.
        /// </summary>
        private static async Task TanamBarisJurnalAsync(
            ApplicationDbContext db,
            Guid legalEntityId,
            Guid accountId,
            decimal debit = 0m,
            decimal kredit = 0m,
            JournalStatus status = JournalStatus.Posted)
        {
            var jenis = await db.Set<AccJournalType>().FirstOrDefaultAsync();
            if (jenis is null)
            {
                jenis = new AccJournalType
                {
                    Id = Guid.NewGuid(),
                    JournalTypeCode = "JU",
                    JournalTypeName = "Jurnal Umum",
                    NumberPrefix = "JU"
                };
                db.Set<AccJournalType>().Add(jenis);
            }

            var periode = await db.Set<AccAccountingPeriod>().FirstOrDefaultAsync();
            if (periode is null)
            {
                periode = new AccAccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = legalEntityId,
                    PeriodCode = "2027-01",
                    FiscalYear = 2027,
                    PeriodMonth = 1,
                    StartDate = new DateTime(2027, 1, 1),
                    EndDate = new DateTime(2027, 1, 31),
                    PeriodStatus = AccountingPeriodStatus.Open
                };
                db.Set<AccAccountingPeriod>().Add(periode);
            }

            await db.SaveChangesAsync();

            var jurnal = new AccJournal
            {
                Id = Guid.NewGuid(),
                LegalEntityId = legalEntityId,
                JournalNumber = $"JU/{Guid.NewGuid():N}"[..25],
                JournalTypeId = jenis.Id,
                AccountingPeriodId = periode.Id,
                AccountingDate = new DateTime(2027, 1, 15),
                Description = "Jurnal bahan uji",
                JournalStatus = status,
                TotalDebit = debit,
                TotalCredit = kredit
            };

            db.Set<AccJournal>().Add(jurnal);
            await db.SaveChangesAsync();

            await SisipkanBarisJurnalLewatSqlAsync(db, jurnal.Id, accountId, debit, kredit);
        }

        /// <summary>
        /// Menyisipkan satu <c>AccJournalLine</c> lewat SQL mentah, bukan lewat EF.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Ini bukan pilihan gaya, melainkan jalan keluar dari batas test harness yang nyata.
        /// </para>
        /// <para>
        /// Di PostgreSQL, <c>DebitAmount</c> dan <c>CreditAmount</c> bertipe
        /// <c>numeric(18,2)</c>, sehingga check constraint
        /// <c>CK_AccJournalLine_TepatSatuSisiTerisi</c> membandingkan angka dengan angka dan
        /// berperilaku benar. Di SQLite, EF Core menyimpan <c>decimal</c> sebagai <b>TEXT</b>.
        /// SQLite membandingkan lintas tipe menurut urutan tipe — nilai TEXT apa pun selalu
        /// lebih besar daripada angka apa pun — sehingga <c>"CreditAmount" = 0</c> selalu salah
        /// dan constraint itu menjadi <b>mustahil dipenuhi</b>, berapa pun nilainya.
        /// </para>
        /// <para>
        /// Menyisipkan lewat SQL mentah dengan literal angka membuat SQLite menyimpannya sebagai
        /// angka, sehingga constraint-nya berperilaku sama dengan di PostgreSQL. Nilai yang
        /// dimasukkan berasal dari test ini sendiri, bukan dari masukan luar.
        /// </para>
        /// <para>
        /// Batas ini dicatat sebagai utang teknis <c>ACC-TD-001</c>. Ia <b>tidak</b> menandakan
        /// cacat pada migration maupun pada configuration — keduanya benar untuk PostgreSQL.
        /// </para>
        /// </remarks>
        private static Task SisipkanBarisJurnalLewatSqlAsync(
            ApplicationDbContext db,
            Guid journalId,
            Guid accountId,
            decimal debit,
            decimal kredit)
        {
            var d = debit.ToString(CultureInfo.InvariantCulture);
            var k = kredit.ToString(CultureInfo.InvariantCulture);

            var sql =
                "INSERT INTO \"AccJournalLine\" " +
                "(\"Id\",\"JournalId\",\"LineNumber\",\"AccountId\",\"DebitAmount\",\"CreditAmount\"," +
                "\"CreateDateTime\",\"CreateBy\",\"UpdateBy\",\"DeleteBy\",\"CancelBy\",\"IsCancel\",\"IsDelete\") " +
                $"VALUES ({{0}},{{1}},{{2}},{{3}},{d},{k},{{4}},{{5}},{{5}},{{5}},{{5}},0,0)";

            return db.Database.ExecuteSqlRawAsync(
                sql,
                Guid.NewGuid(),
                journalId,
                NomorBarisBerikutnya(),
                accountId,
                DateTime.UtcNow,
                Guid.Empty);
        }

        private static int _nomorBaris;

        private static int NomorBarisBerikutnya() => Interlocked.Increment(ref _nomorBaris);
    }
}
