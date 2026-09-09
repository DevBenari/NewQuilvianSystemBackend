using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-013</c> — saldo control account dari buku besar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang paling berbahaya di sini adalah acceptance (2). Menghitung dari baris selain
    /// <c>Posted</c> menghasilkan saldo yang <b>tidak akan pernah cocok</b> dengan subledger, dan
    /// selisihnya akan disalahartikan sebagai cacat data — orang akan mencari kesalahan di tempat
    /// yang tidak ada kesalahannya.
    /// </para>
    /// <para>
    /// <b>Catatan <c>ACC-TD-001</c>.</b> EF menyimpan <c>decimal</c> sebagai TEXT pada SQLite,
    /// sehingga check constraint <c>CK_AccJournalLine_TepatSatuSisiTerisi</c> — yang berbunyi
    /// <c>"CreditAmount" = 0</c> — <b>mustahil dipenuhi</b>: TEXT tidak pernah sama dengan angka.
    /// Uji yang menyimpan baris jurnal karena itu mematikan penegakan check constraint pada
    /// koneksi ujinya lewat <c>PRAGMA ignore_check_constraints</c>.
    /// </para>
    /// <para>
    /// Yang dikorbankan hanya penegakan constraint itu sendiri, dan constraint itu <b>bukan</b>
    /// yang diuji berkas ini — ia dijamin PostgreSQL dan diuji di sana. Yang diuji di sini adalah
    /// perhitungan saldonya, dan perhitungan itu tidak bergantung pada constraint tersebut.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccControlAccountReconciliationTests
    {
        private static readonly Guid Pelaku = Guid.Parse("d1d1d1d1-0000-0000-0000-000000000001");
        private static readonly Guid BadanHukum = Guid.Parse("d2d2d2d2-0000-0000-0000-000000000002");
        private static readonly Guid BadanHukumLain = Guid.Parse("d3d3d3d3-0000-0000-0000-000000000003");
        private static readonly Guid PeriodeId = Guid.Parse("d4d4d4d4-0000-0000-0000-000000000004");
        private static readonly Guid JenisJurnalId = Guid.Parse("d5d5d5d5-0000-0000-0000-000000000005");

        /// <summary>
        /// Mematikan penegakan check constraint pada koneksi uji — lihat catatan
        /// <c>ACC-TD-001</c> pada dokumentasi kelas.
        /// </summary>
        /// <remarks>
        /// <c>TestDatabase</c> memakai satu koneksi untuk seluruh konteksnya, sehingga pragma ini
        /// cukup dijalankan sekali dan berlaku sampai basis datanya dibuang.
        /// </remarks>
        private static async Task LonggarkanCheckConstraintAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();
            await db.Database.ExecuteSqlRawAsync("PRAGMA ignore_check_constraints = ON;");
        }

        private static async Task SiapkanAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();

            db.Set<MstLegalEntity>().AddRange(
                Badan(BadanHukum, "RS-UJI", "Rumah Sakit Uji", utama: true),
                Badan(BadanHukumLain, "RS-LAIN", "Rumah Sakit Lain", utama: false));

            db.Set<AccJournalType>().Add(new AccJournalType
            {
                Id = JenisJurnalId,
                JournalTypeCode = "JU",
                JournalTypeName = "Jurnal Umum",
                NumberPrefix = "JU",
                RequiresApproval = true,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            db.Set<AccAccountingPeriod>().Add(new AccAccountingPeriod
            {
                Id = PeriodeId,
                LegalEntityId = BadanHukum,
                PeriodCode = "2026-09",
                FiscalYear = 2026,
                PeriodMonth = 9,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30),
                PeriodStatus = AccountingPeriodStatus.Open,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            await db.SaveChangesAsync();
        }

        private static MstLegalEntity Badan(Guid id, string kode, string nama, bool utama) => new()
        {
            Id = id,
            LegalEntityCode = kode,
            LegalEntityName = nama,
            IsDefault = utama,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Pelaku
        };

        private static AccChartOfAccount Akun(
            string kode,
            string nama,
            bool control,
            AccountType jenis = AccountType.Asset,
            NormalBalance saldoNormal = NormalBalance.Debit,
            Guid? badanHukum = null) => new()
            {
                Id = Guid.NewGuid(),
                LegalEntityId = badanHukum ?? BadanHukum,
                AccountCode = kode,
                AccountName = nama,
                AccountType = jenis,
                NormalBalance = saldoNormal,
                AccountLevel = 1,
                IsPostable = true,
                IsActive = true,
                IsControlAccount = control,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            };

        /// <summary>Menyimpan satu jurnal beserta sepasang barisnya.</summary>
        private static async Task JurnalAsync(
            TestDatabase database,
            string nomor,
            JournalStatus status,
            Guid akunDebit,
            Guid akunKredit,
            decimal nominal,
            DateTime tanggal)
        {
            await using var db = database.CreateContext();

            var jurnal = new AccJournal
            {
                Id = Guid.NewGuid(),
                LegalEntityId = BadanHukum,
                JournalNumber = nomor,
                JournalTypeId = JenisJurnalId,
                AccountingPeriodId = PeriodeId,
                AccountingDate = tanggal,
                Description = "Jurnal uji",
                JournalStatus = status,
                TotalDebit = nominal,
                TotalCredit = nominal,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            };

            db.Set<AccJournal>().Add(jurnal);

            db.Set<AccJournalLine>().AddRange(
                new AccJournalLine
                {
                    Id = Guid.NewGuid(),
                    JournalId = jurnal.Id,
                    LineNumber = 1,
                    AccountId = akunDebit,
                    DebitAmount = nominal,
                    CreditAmount = 0m,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                },
                new AccJournalLine
                {
                    Id = Guid.NewGuid(),
                    JournalId = jurnal.Id,
                    LineNumber = 2,
                    AccountId = akunKredit,
                    DebitAmount = 0m,
                    CreditAmount = nominal,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                });

            await db.SaveChangesAsync();
        }

        // =====================================================================
        // Acceptance 1 — hanya akun ber-IsControlAccount = true yang muncul
        // =====================================================================

        [Fact]
        public async Task HanyaControlAccount_YangMuncul()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var kasKasir = Akun("1-1002", "Kas Kasir", control: true);
            var piutang = Akun("1-1201", "Piutang Pasien", control: true);
            var bebanGaji = Akun("5-2001", "Beban Gaji", control: false, jenis: AccountType.Expense);

            await using (var db = database.CreateContext())
            {
                db.Set<AccChartOfAccount>().AddRange(kasKasir, piutang, bebanGaji);
                await db.SaveChangesAsync();
            }

            await using var konteks = database.CreateContext();
            var hasil = await new AccControlAccountReconciliationService(konteks)
                .GetGlBalancesAsync(new ControlAccountBalanceQuery { LegalEntityId = BadanHukum });

            Assert.True(hasil.Success);
            Assert.Equal(new[] { "1-1002", "1-1201" }, hasil.Data!.Accounts.Select(x => x.AccountCode));
            Assert.DoesNotContain(hasil.Data.Accounts, x => x.AccountCode == "5-2001");
        }

        /// <summary>
        /// Control account milik badan hukum lain tidak ikut — acceptance (3) sisi badan hukum.
        /// </summary>
        [Fact]
        public async Task ControlAccountBadanHukumLain_TidakIkut()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                db.Set<AccChartOfAccount>().AddRange(
                    Akun("1-1002", "Kas Kasir", control: true),
                    Akun("1-1002", "Kas Kasir Lain", control: true, badanHukum: BadanHukumLain));

                await db.SaveChangesAsync();
            }

            await using var konteks = database.CreateContext();
            var hasil = await new AccControlAccountReconciliationService(konteks)
                .GetGlBalancesAsync(new ControlAccountBalanceQuery { LegalEntityId = BadanHukum });

            Assert.Single(hasil.Data!.Accounts);
            Assert.Equal("Kas Kasir", hasil.Data.Accounts[0].AccountName);
        }

        [Fact]
        public async Task BelumAdaControlAccount_MenjawabBerhasilDenganDaftarKosong()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var konteks = database.CreateContext();
            var hasil = await new AccControlAccountReconciliationService(konteks)
                .GetGlBalancesAsync(new ControlAccountBalanceQuery { LegalEntityId = BadanHukum });

            Assert.True(hasil.Success);
            Assert.Empty(hasil.Data!.Accounts);
            Assert.Equal(0, hasil.Data.AccountCount);
        }

        // =====================================================================
        // Acceptance 2 — saldo HANYA dari baris berstatus Posted
        // =====================================================================

        /// <summary>
        /// Contoh berangka. Kas Kasir menerima empat jurnal, tetapi hanya dua yang
        /// <c>Posted</c> — saldonya wajib menghitung dua itu saja.
        /// </summary>
        /// <remarks>
        /// Posted   : debit Rp 12.500.000 dan debit Rp 2.000.000 → saldo Rp 14.500.000
        /// Diabaikan: Draft Rp 900.000.000 dan Approved Rp 700.000.000
        ///
        /// Bila status selain Posted ikut terhitung, saldonya menjadi Rp 1.614.500.000 —
        /// selisih Rp 1.600.000.000 dari subledger, dan tidak ada error apa pun yang muncul.
        /// </remarks>
        [Fact]
        public async Task SaldoDihitungHanyaDariBarisPosted()
        {
            using var database = TestDatabase.Create();
            await LonggarkanCheckConstraintAsync(database);
            await SiapkanAsync(database);

            var kasKasir = Akun("1-1002", "Kas Kasir", control: true);
            var pendapatan = Akun("4-1001", "Pendapatan Rawat Jalan", control: false,
                jenis: AccountType.Revenue, saldoNormal: NormalBalance.Credit);

            await using (var db = database.CreateContext())
            {
                db.Set<AccChartOfAccount>().AddRange(kasKasir, pendapatan);
                await db.SaveChangesAsync();
            }

            var tanggal = new DateTime(2026, 9, 15);

            await JurnalAsync(database, "JU-001", JournalStatus.Posted, kasKasir.Id, pendapatan.Id, 12_500_000m, tanggal);
            await JurnalAsync(database, "JU-002", JournalStatus.Posted, kasKasir.Id, pendapatan.Id, 2_000_000m, tanggal);
            await JurnalAsync(database, "JU-003", JournalStatus.Draft, kasKasir.Id, pendapatan.Id, 900_000_000m, tanggal);
            await JurnalAsync(database, "JU-004", JournalStatus.Approved, kasKasir.Id, pendapatan.Id, 700_000_000m, tanggal);

            await using var konteks = database.CreateContext();
            var hasil = await new AccControlAccountReconciliationService(konteks)
                .GetGlBalancesAsync(new ControlAccountBalanceQuery { LegalEntityId = BadanHukum });

            var baris = Assert.Single(hasil.Data!.Accounts);

            Assert.Equal(14_500_000m, baris.TotalDebit);
            Assert.Equal(0m, baris.TotalCredit);
            Assert.Equal(14_500_000m, baris.Balance);
            Assert.Equal(2, baris.PostedLineCount);
        }

        /// <summary>
        /// Angkanya <b>sama persis</b> dengan <c>AccChartOfAccountService.HitungSaldoAsync</c>.
        /// </summary>
        /// <remarks>
        /// Kartu roadmap menyebut pemakaian ulang fungsi itu. Laporan ini memakai satu query
        /// pengelompokan supaya tidak satu query per akun, jadi kesamaan angkanya wajib dibuktikan
        /// — bukan diasumsikan. Kalau keduanya berselisih, layar rekonsiliasi dan layar akun akan
        /// menampilkan saldo berbeda untuk akun yang sama.
        /// </remarks>
        [Fact]
        public async Task AngkanyaSamaDenganHitungSaldoAsync()
        {
            using var database = TestDatabase.Create();
            await LonggarkanCheckConstraintAsync(database);
            await SiapkanAsync(database);

            var kasKasir = Akun("1-1002", "Kas Kasir", control: true);
            var hutang = Akun("2-1001", "Hutang Pemasok", control: true,
                jenis: AccountType.Liability, saldoNormal: NormalBalance.Credit);

            await using (var db = database.CreateContext())
            {
                db.Set<AccChartOfAccount>().AddRange(kasKasir, hutang);
                await db.SaveChangesAsync();
            }

            var tanggal = new DateTime(2026, 9, 10);
            await JurnalAsync(database, "JU-101", JournalStatus.Posted, kasKasir.Id, hutang.Id, 5_000_000m, tanggal);
            await JurnalAsync(database, "JU-102", JournalStatus.Posted, kasKasir.Id, hutang.Id, 3_250_000m, tanggal);
            await JurnalAsync(database, "JU-103", JournalStatus.Draft, kasKasir.Id, hutang.Id, 99_000_000m, tanggal);

            await using var konteks = database.CreateContext();

            var laporan = await new AccControlAccountReconciliationService(konteks)
                .GetGlBalancesAsync(new ControlAccountBalanceQuery { LegalEntityId = BadanHukum });

            foreach (var baris in laporan.Data!.Accounts)
            {
                var pembanding = await AccChartOfAccountService
                    .HitungSaldoAsync(konteks, baris.AccountId);

                Assert.Equal(pembanding, baris.Balance);
            }
        }

        /// <summary>
        /// Akun bersaldo normal kredit ditampilkan positif pada
        /// <c>BalanceInNormalBalance</c>, tanpa mengubah <c>Balance</c> mentahnya.
        /// </summary>
        [Fact]
        public async Task AkunSaldoNormalKredit_DitampilkanPositifMenurutSaldoNormalnya()
        {
            using var database = TestDatabase.Create();
            await LonggarkanCheckConstraintAsync(database);
            await SiapkanAsync(database);

            var kas = Akun("1-1002", "Kas Kasir", control: false);
            var hutang = Akun("2-1001", "Hutang Pemasok", control: true,
                jenis: AccountType.Liability, saldoNormal: NormalBalance.Credit);

            await using (var db = database.CreateContext())
            {
                db.Set<AccChartOfAccount>().AddRange(kas, hutang);
                await db.SaveChangesAsync();
            }

            await JurnalAsync(database, "JU-201", JournalStatus.Posted, kas.Id, hutang.Id,
                4_000_000m, new DateTime(2026, 9, 20));

            await using var konteks = database.CreateContext();
            var hasil = await new AccControlAccountReconciliationService(konteks)
                .GetGlBalancesAsync(new ControlAccountBalanceQuery { LegalEntityId = BadanHukum });

            var baris = Assert.Single(hasil.Data!.Accounts);

            Assert.Equal(-4_000_000m, baris.Balance);
            Assert.Equal(4_000_000m, baris.BalanceInNormalBalance);
        }

        // =====================================================================
        // Acceptance 3 — disaring per tanggal
        // =====================================================================

        /// <summary>
        /// Batas tanggal memuat tanggalnya sendiri, dan jurnal sesudahnya tidak terhitung.
        /// </summary>
        [Fact]
        public async Task DisaringMenurutTanggal_TermasukTanggalBatasnya()
        {
            using var database = TestDatabase.Create();
            await LonggarkanCheckConstraintAsync(database);
            await SiapkanAsync(database);

            var kasKasir = Akun("1-1002", "Kas Kasir", control: true);
            var pendapatan = Akun("4-1001", "Pendapatan", control: false,
                jenis: AccountType.Revenue, saldoNormal: NormalBalance.Credit);

            await using (var db = database.CreateContext())
            {
                db.Set<AccChartOfAccount>().AddRange(kasKasir, pendapatan);
                await db.SaveChangesAsync();
            }

            await JurnalAsync(database, "JU-301", JournalStatus.Posted, kasKasir.Id, pendapatan.Id,
                1_000_000m, new DateTime(2026, 9, 29));
            await JurnalAsync(database, "JU-302", JournalStatus.Posted, kasKasir.Id, pendapatan.Id,
                2_000_000m, new DateTime(2026, 9, 30));
            await JurnalAsync(database, "JU-303", JournalStatus.Posted, kasKasir.Id, pendapatan.Id,
                5_000_000m, new DateTime(2026, 10, 1));

            await using var konteks = database.CreateContext();
            var layanan = new AccControlAccountReconciliationService(konteks);

            // Sampai 30 September — dua jurnal pertama saja.
            var sampai30 = await layanan.GetGlBalancesAsync(new ControlAccountBalanceQuery
            {
                LegalEntityId = BadanHukum,
                AsOfDate = new DateTime(2026, 9, 30)
            });

            Assert.Equal(3_000_000m, sampai30.Data!.Accounts.Single().Balance);

            // Tanpa batas — ketiganya.
            var seluruhnya = await layanan.GetGlBalancesAsync(new ControlAccountBalanceQuery
            {
                LegalEntityId = BadanHukum
            });

            Assert.Equal(8_000_000m, seluruhnya.Data!.Accounts.Single().Balance);
        }

        [Fact]
        public async Task BadanHukumKosong_Ditolak400()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var konteks = database.CreateContext();
            var hasil = await new AccControlAccountReconciliationService(konteks)
                .GetGlBalancesAsync(new ControlAccountBalanceQuery { LegalEntityId = Guid.Empty });

            Assert.False(hasil.Success);
            Assert.Equal(400, hasil.StatusCode);
        }

        // =====================================================================
        // Acceptance 4 — [AccessPermission] terpasang
        // =====================================================================

        [Fact]
        public void Endpoint_MembawaHakAksesYangCocokDenganAccessController()
        {
            var controller = typeof(ReconciliationController);
            var namaController = controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;

            var method = controller.GetMethod(nameof(ReconciliationController.GetGlBalances))!;

            Assert.NotNull(method.GetCustomAttribute<AccessActionAttribute>());

            var izin = method.GetCustomAttribute<AccessPermissionAttribute>();
            Assert.NotNull(izin);

            var argumen = izin!.Arguments!;
            Assert.Equal(namaController, (string)argumen[0]);
            Assert.Equal("Read", (string)argumen[1]);
        }
    }
}
