using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-010</c> — pratinjau dan penyusunan jurnal penutup
    /// tahun.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Acceptance (4) adalah alasan berkas ini ada.</b> Salah hitung tutup tahun
    /// <b>tidak menimbulkan error</b>: jurnalnya tetap seimbang, tetap dapat disetujui, tetap
    /// dapat disahkan, dan angkanya baru ketahuan salah bertahun-tahun kemudian lewat saldo awal
    /// laba ditahan. Karena itu uji di bawah tidak berhenti pada "debit sama dengan kredit"; ia
    /// mengesahkan jurnal penutupnya lalu memeriksa <b>saldo tiap akun pendapatan dan beban satu
    /// per satu</b> memakai <c>AccChartOfAccountService.HitungSaldoAsync</c> — penghitung saldo
    /// yang sama dengan yang dipakai buku besar, bukan penghitung khusus uji.
    /// </para>
    /// <para>
    /// Angkanya diambil apa adanya dari contoh berangka pada
    /// <c>flowcharts/04-tutup-tahun.md</c>: pendapatan Rp 1.300.000.000, beban Rp 900.000.000,
    /// laba Rp 400.000.000.
    /// </para>
    /// <para>
    /// <b>Catatan <c>ACC-TD-001</c>.</b> EF menyimpan <c>decimal</c> sebagai TEXT pada SQLite,
    /// sehingga check constraint <c>CK_AccJournalLine_TepatSatuSisiTerisi</c> mustahil dipenuhi:
    /// TEXT tidak pernah sama dengan angka. Uji yang menyimpan baris jurnal karena itu mematikan
    /// penegakan check constraint pada koneksi ujinya. Yang dikorbankan hanya constraint itu
    /// sendiri, dan ia bukan yang diuji di sini — ia dijamin PostgreSQL. Yang diuji di sini
    /// adalah perhitungan dan penolkannya.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccYearEndClosingTests
    {
        private static readonly Guid Pelaku = Guid.Parse("e1e1e1e1-0000-0000-0000-000000000001");
        private static readonly Guid Penyetuju = Guid.Parse("e2e2e2e2-0000-0000-0000-000000000002");
        private static readonly Guid BadanHukum = Guid.Parse("e3e3e3e3-0000-0000-0000-000000000003");
        private static readonly Guid JenisJurnalUmumId = Guid.Parse("e4e4e4e4-0000-0000-0000-000000000004");
        private static readonly Guid JenisJurnalTutupId = Guid.Parse("e5e5e5e5-0000-0000-0000-000000000005");
        private static readonly Guid JenisJurnalBalikId = Guid.Parse("e8e8e8e8-0000-0000-0000-000000000008");
        private static readonly Guid UnitBiayaId = Guid.Parse("e6e6e6e6-0000-0000-0000-000000000006");
        private static readonly Guid UnitBiayaLainId = Guid.Parse("e7e7e7e7-0000-0000-0000-000000000007");

        private const int TahunBuku = 2026;

        // =====================================================================
        // Penyiapan
        // =====================================================================

        /// <remarks>
        /// Lihat catatan <c>ACC-TD-001</c> pada dokumentasi kelas. <c>TestDatabase</c> memakai
        /// satu koneksi untuk seluruh konteksnya, sehingga pragma ini cukup sekali.
        /// </remarks>
        private static async Task LonggarkanCheckConstraintAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();
            await db.Database.ExecuteSqlRawAsync("PRAGMA ignore_check_constraints = ON;");
        }

        /// <summary>
        /// Badan hukum, dua jenis jurnal, dua unit biaya, dan dua belas periode tahun buku.
        /// </summary>
        private static async Task SiapkanAsync(
            TestDatabase database,
            AccountingPeriodStatus statusPeriode = AccountingPeriodStatus.SoftClosed,
            int tahun = TahunBuku)
        {
            await LonggarkanCheckConstraintAsync(database);

            await using var db = database.CreateContext();

            db.Set<MstLegalEntity>().Add(new MstLegalEntity
            {
                Id = BadanHukum,
                LegalEntityCode = "RS-UJI",
                LegalEntityName = "Rumah Sakit Uji",
                IsDefault = true,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            // JB ikut disiapkan karena acceptance (6) membalik jurnal penutup lewat jalur
            // pembalikan yang sudah ada, dan jalur itu mencari jenis jurnal JB di master.
            db.Set<AccJournalType>().AddRange(
                JenisJurnal(JenisJurnalUmumId, "JU", "Jurnal Umum"),
                JenisJurnal(JenisJurnalTutupId, "JT", "Jurnal Tutup Tahun"),
                JenisJurnal(JenisJurnalBalikId, "JB", "Jurnal Pembalik"));

            db.Set<MstCostCenter>().AddRange(
                UnitBiaya(UnitBiayaId, "CC-RJ", "Rawat Jalan"),
                UnitBiaya(UnitBiayaLainId, "CC-RI", "Rawat Inap"));

            db.Set<AccAccountingPeriod>().AddRange(PeriodeSetahun(tahun, statusPeriode));

            await db.SaveChangesAsync();
        }

        private static AccJournalType JenisJurnal(Guid id, string kode, string nama) => new()
        {
            Id = id,
            JournalTypeCode = kode,
            JournalTypeName = nama,
            NumberPrefix = kode,
            RequiresApproval = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Pelaku
        };

        private static MstCostCenter UnitBiaya(Guid id, string kode, string nama) => new()
        {
            Id = id,
            LegalEntityId = BadanHukum,
            CostCenterCode = kode,
            CostCenterName = nama,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Pelaku
        };

        private static List<AccAccountingPeriod> PeriodeSetahun(
            int tahun, AccountingPeriodStatus status)
        {
            return Enumerable.Range(1, 12).Select(bulan => new AccAccountingPeriod
            {
                Id = Guid.NewGuid(),
                LegalEntityId = BadanHukum,
                PeriodCode = $"{tahun}-{bulan:D2}",
                FiscalYear = tahun,
                PeriodMonth = bulan,
                StartDate = new DateTime(tahun, bulan, 1),
                EndDate = new DateTime(tahun, bulan, DateTime.DaysInMonth(tahun, bulan)),
                PeriodStatus = status,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            }).ToList();
        }

        private static AccChartOfAccount Akun(
            string kode,
            string nama,
            AccountType jenis,
            NormalBalance saldoNormal,
            bool menerimaTransaksi = true,
            bool aktif = true) => new()
            {
                Id = Guid.NewGuid(),
                LegalEntityId = BadanHukum,
                AccountCode = kode,
                AccountName = nama,
                AccountType = jenis,
                NormalBalance = saldoNormal,
                AccountLevel = 1,
                IsPostable = menerimaTransaksi,
                IsActive = aktif,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            };

        /// <summary>
        /// Daftar akun contoh berangka <c>flowcharts/04-tutup-tahun.md</c>, ditambah kas sebagai
        /// lawan jurnalnya dan akun laba ditahan.
        /// </summary>
        private sealed class DaftarAkun
        {
            public AccChartOfAccount RawatJalan { get; init; } = null!;
            public AccChartOfAccount RawatInap { get; init; } = null!;
            public AccChartOfAccount BebanObat { get; init; } = null!;
            public AccChartOfAccount BebanGaji { get; init; } = null!;
            public AccChartOfAccount Kas { get; init; } = null!;
            public AccChartOfAccount LabaDitahan { get; init; } = null!;
        }

        private static async Task<DaftarAkun> SimpanAkunAsync(
            TestDatabase database,
            bool labaDitahanMenerimaTransaksi = true,
            bool labaDitahanAktif = true,
            AccountType jenisLabaDitahan = AccountType.Equity)
        {
            var daftar = new DaftarAkun
            {
                RawatJalan = Akun("4-1001", "Pendapatan Rawat Jalan", AccountType.Revenue, NormalBalance.Credit),
                RawatInap = Akun("4-1002", "Pendapatan Rawat Inap", AccountType.Revenue, NormalBalance.Credit),
                BebanObat = Akun("5-1001", "Beban Obat", AccountType.Expense, NormalBalance.Debit),
                BebanGaji = Akun("5-2001", "Beban Gaji", AccountType.Expense, NormalBalance.Debit),
                Kas = Akun("1-1001", "Kas Besar", AccountType.Asset, NormalBalance.Debit),
                LabaDitahan = Akun(
                    "3-3001",
                    "Laba Ditahan",
                    jenisLabaDitahan,
                    NormalBalance.Credit,
                    labaDitahanMenerimaTransaksi,
                    labaDitahanAktif)
            };

            await using var db = database.CreateContext();

            db.Set<AccChartOfAccount>().AddRange(
                daftar.RawatJalan, daftar.RawatInap, daftar.BebanObat,
                daftar.BebanGaji, daftar.Kas, daftar.LabaDitahan);

            await db.SaveChangesAsync();

            return daftar;
        }

        private static async Task TetapkanLabaDitahanAsync(TestDatabase database, Guid akunId)
        {
            await using var db = database.CreateContext();

            db.Set<AccAccountingConfiguration>().Add(new AccAccountingConfiguration
            {
                Id = Guid.NewGuid(),
                LegalEntityId = BadanHukum,
                RetainedEarningsAccountId = akunId,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Menyimpan satu jurnal <b>yang sudah disahkan</b> beserta sepasang barisnya, langsung
        /// ke tabel.
        /// </summary>
        /// <remarks>
        /// Daur hidup jurnal bukan yang diuji berkas ini — ia sudah dibuktikan
        /// <c>BE-ACC-011</c>. Yang dibutuhkan di sini hanyalah buku besar yang berisi, dan
        /// menempuh ajukan-setujui-sahkan untuk tiap jurnal penyiapan hanya akan menambah jalan
        /// yang dapat rusak tanpa menambah satu pun bukti.
        /// </remarks>
        private static async Task JurnalDisahkanAsync(
            TestDatabase database,
            string nomor,
            Guid periodeId,
            DateTime tanggal,
            Guid akunDebit,
            Guid? unitBiayaDebit,
            Guid akunKredit,
            Guid? unitBiayaKredit,
            decimal nominal)
        {
            await using var db = database.CreateContext();

            var jurnal = new AccJournal
            {
                Id = Guid.NewGuid(),
                LegalEntityId = BadanHukum,
                JournalNumber = nomor,
                JournalTypeId = JenisJurnalUmumId,
                AccountingPeriodId = periodeId,
                AccountingDate = tanggal,
                Description = "Jurnal uji",
                JournalStatus = JournalStatus.Posted,
                TotalDebit = nominal,
                TotalCredit = nominal,
                PostedBy = Pelaku,
                PostedAt = DateTime.UtcNow,
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
                    CostCenterId = unitBiayaDebit,
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
                    CostCenterId = unitBiayaKredit,
                    DebitAmount = 0m,
                    CreditAmount = nominal,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                });

            await db.SaveChangesAsync();
        }

        private static async Task<Guid> IdPeriodeAsync(TestDatabase database, int tahun, int bulan)
        {
            await using var db = database.CreateContext();

            return await db.Set<AccAccountingPeriod>()
                .Where(x => x.FiscalYear == tahun && x.PeriodMonth == bulan)
                .Select(x => x.Id)
                .FirstAsync();
        }

        /// <summary>
        /// Buku besar contoh berangka <c>flowcharts/04-tutup-tahun.md</c>, seluruhnya disahkan
        /// pada tahun bukunya.
        /// </summary>
        /// <remarks>
        /// Beban dicatat dengan unit biaya karena <c>ACC-DEC-019</c> mewajibkannya. Rawat Jalan
        /// membawa Beban Obat, Rawat Inap membawa Beban Gaji.
        /// </remarks>
        private static async Task IsiBukuBesarContohAsync(TestDatabase database, DaftarAkun akun)
        {
            var maret = await IdPeriodeAsync(database, TahunBuku, 3);
            var juni = await IdPeriodeAsync(database, TahunBuku, 6);

            await JurnalDisahkanAsync(
                database, "JU/2026/03/00001", maret, new DateTime(TahunBuku, 3, 10),
                akun.Kas.Id, null, akun.RawatJalan.Id, null, 800_000_000m);

            await JurnalDisahkanAsync(
                database, "JU/2026/03/00002", maret, new DateTime(TahunBuku, 3, 11),
                akun.Kas.Id, null, akun.RawatInap.Id, null, 500_000_000m);

            await JurnalDisahkanAsync(
                database, "JU/2026/06/00001", juni, new DateTime(TahunBuku, 6, 10),
                akun.BebanObat.Id, UnitBiayaId, akun.Kas.Id, null, 300_000_000m);

            await JurnalDisahkanAsync(
                database, "JU/2026/06/00002", juni, new DateTime(TahunBuku, 6, 11),
                akun.BebanGaji.Id, UnitBiayaLainId, akun.Kas.Id, null, 600_000_000m);
        }

        private static AccYearEndClosingService Service(ApplicationDbContext db)
            => new(db, new AccJournalService(db));

        /// <summary>
        /// Menyiapkan seluruhnya sekaligus: master, akun, pengaturan, dan buku besar contoh.
        /// </summary>
        private static async Task<DaftarAkun> SiapkanContohLengkapAsync(
            TestDatabase database,
            AccountingPeriodStatus statusPeriode = AccountingPeriodStatus.SoftClosed)
        {
            await SiapkanAsync(database, statusPeriode);
            var akun = await SimpanAkunAsync(database);
            await TetapkanLabaDitahanAsync(database, akun.LabaDitahan.Id);
            await IsiBukuBesarContohAsync(database, akun);
            return akun;
        }

        // =====================================================================
        // Acceptance 1 — pratinjau tidak membuat apa pun
        // =====================================================================

        /// <summary>
        /// Dipanggil sepuluh kali, nol jurnal terbentuk — dan nol nomor jurnal terpakai.
        /// </summary>
        /// <remarks>
        /// <c>AccNumberSeries</c> ikut diperiksa karena itulah kebocoran yang paling mudah
        /// terlewat: pratinjau yang diam-diam mengalokasikan nomor tidak meninggalkan jurnal apa
        /// pun, tetapi menghabiskan nomor, dan jurnal sungguhan berikutnya melompat sepuluh
        /// nomor tanpa penjelasan.
        /// </remarks>
        [Fact]
        public async Task Pratinjau_DipanggilSepuluhKali_NolJurnalTerbentuk()
        {
            using var database = TestDatabase.Create();
            await SiapkanContohLengkapAsync(database);

            var jumlahJurnalAwal = await JumlahJurnalAsync(database);

            for (var ke = 1; ke <= 10; ke++)
            {
                await using var db = database.CreateContext();
                var hasil = await Service(db).PreviewAsync(BadanHukum, TahunBuku);

                Assert.True(hasil.Success);
                Assert.Equal(400_000_000m, hasil.Data!.NetIncome);
            }

            await using var pemeriksa = database.CreateContext();

            Assert.Equal(jumlahJurnalAwal, await pemeriksa.Set<AccJournal>().CountAsync());
            Assert.Equal(0, await pemeriksa.Set<AccNumberSeries>().CountAsync());
        }

        /// <summary>
        /// Pratinjau menghitung persis contoh berangka pada <c>flowcharts/04-tutup-tahun.md</c>.
        /// </summary>
        [Fact]
        public async Task Pratinjau_MengikutiContohBerangkaFlowchart()
        {
            using var database = TestDatabase.Create();
            await SiapkanContohLengkapAsync(database);

            await using var db = database.CreateContext();
            var hasil = await Service(db).PreviewAsync(BadanHukum, TahunBuku);

            Assert.True(hasil.Success);

            var pratinjau = hasil.Data!;

            Assert.Equal(1_300_000_000m, pratinjau.TotalRevenue);
            Assert.Equal(900_000_000m, pratinjau.TotalExpense);
            Assert.Equal(400_000_000m, pratinjau.NetIncome);

            // Laba masuk sisi KREDIT laba ditahan.
            Assert.Equal(400_000_000m, pratinjau.RetainedEarningsCredit);
            Assert.Equal(0m, pratinjau.RetainedEarningsDebit);

            // Lima baris, persis tabel jurnal penutup pada flowchart.
            Assert.Equal(5, pratinjau.LineCount);
            Assert.Equal(1_300_000_000m, pratinjau.TotalDebit);
            Assert.Equal(1_300_000_000m, pratinjau.TotalCredit);
            Assert.True(pratinjau.IsBalanced);

            // Pendapatan didebit, beban dikredit.
            Assert.All(pratinjau.RevenueLines, x => Assert.True(x.DebitAmount > 0m && x.CreditAmount == 0m));
            Assert.All(pratinjau.ExpenseLines, x => Assert.True(x.CreditAmount > 0m && x.DebitAmount == 0m));

            Assert.Equal(
                new[] { "4-1001", "4-1002" },
                pratinjau.RevenueLines.Select(x => x.AccountCode).ToArray());

            Assert.Equal(
                new[] { "5-1001", "5-2001" },
                pratinjau.ExpenseLines.Select(x => x.AccountCode).ToArray());

            // Tanggal akuntansinya hari terakhir periode terakhir tahun buku.
            Assert.Equal(new DateTime(TahunBuku, 12, 31), pratinjau.ClosingDate);
            Assert.Equal(12, pratinjau.PeriodCount);
            Assert.Equal("3-3001", pratinjau.RetainedEarningsAccountCode);
        }

        // =====================================================================
        // Acceptance 2 — periode belum tertutup ditolak 409 beserta daftarnya
        // =====================================================================

        [Theory]
        [InlineData(AccountingPeriodStatus.Open)]
        [InlineData(AccountingPeriodStatus.PendingClosingApproval)]
        public async Task AdaPeriodeBelumTertutup_Ditolak409(AccountingPeriodStatus status)
        {
            using var database = TestDatabase.Create();
            await SiapkanContohLengkapAsync(database);

            // Dua periode dikembalikan menjadi belum tertutup.
            await using (var db = database.CreateContext())
            {
                var periode = await db.Set<AccAccountingPeriod>()
                    .Where(x => x.PeriodMonth == 2 || x.PeriodMonth == 11)
                    .ToListAsync();

                foreach (var p in periode) p.PeriodStatus = status;

                await db.SaveChangesAsync();
            }

            await using var pemeriksa = database.CreateContext();
            var layanan = Service(pemeriksa);

            var pratinjau = await layanan.PreviewAsync(BadanHukum, TahunBuku);
            var susun = await layanan.GenerateAsync(Permintaan(), Pelaku);

            foreach (var hasil in new[] { pratinjau.StatusCode, susun.StatusCode })
            {
                Assert.Equal(409, hasil);
            }

            Assert.False(pratinjau.Success);
            Assert.False(susun.Success);

            // Daftar periodenya disebut, bukan sekadar jumlahnya — tanpa nama periode petugas
            // harus menebak bulan mana yang tertinggal.
            Assert.Contains("Februari 2026", susun.Message);
            Assert.Contains("November 2026", susun.Message);

            Assert.Equal(0, await JumlahJurnalPenutupAsync(database));
        }

        /// <summary>
        /// <c>SoftClosed</c> sudah cukup; tutup tahun tidak menuntut <c>Closed</c>.
        /// </summary>
        /// <remarks>
        /// Menuntut <c>Closed</c> akan mengunci tutup tahun selamanya: periode tutup permanen
        /// tidak menerima jurnal apa pun, termasuk jurnal penutupnya sendiri.
        /// </remarks>
        [Fact]
        public async Task PeriodeTutupSementara_SudahCukupUntukMenyusun()
        {
            using var database = TestDatabase.Create();
            await SiapkanContohLengkapAsync(database, AccountingPeriodStatus.SoftClosed);

            await using var db = database.CreateContext();
            var hasil = await Service(db).GenerateAsync(Permintaan(), Pelaku);

            Assert.True(hasil.Success);
            Assert.Equal(201, hasil.StatusCode);
        }

        /// <summary>
        /// Periode terakhir yang sudah <b>tutup permanen</b> ditolak <c>422</c> beserta nama
        /// periodenya, pada pratinjau maupun penyusunan.
        /// </summary>
        /// <remarks>
        /// Keadaan ini sah tetapi buntu: periode <c>Closed</c> tidak menerima jurnal apa pun,
        /// termasuk jurnal penutup tahunnya sendiri. Ditolak sejak pratinjau supaya petugas tidak
        /// melihat angka yang sempurna lalu ditolak dengan alasan yang tidak menyebut periodenya.
        /// </remarks>
        [Fact]
        public async Task PeriodeTerakhirTutupPermanen_Ditolak422()
        {
            using var database = TestDatabase.Create();
            await SiapkanContohLengkapAsync(database);

            await using (var db = database.CreateContext())
            {
                var desember = await db.Set<AccAccountingPeriod>()
                    .FirstAsync(x => x.FiscalYear == TahunBuku && x.PeriodMonth == 12);

                desember.PeriodStatus = AccountingPeriodStatus.Closed;
                await db.SaveChangesAsync();
            }

            await using var pemeriksa = database.CreateContext();
            var layanan = Service(pemeriksa);

            var pratinjau = await layanan.PreviewAsync(BadanHukum, TahunBuku);
            var susun = await layanan.GenerateAsync(Permintaan(), Pelaku);

            Assert.Equal(422, pratinjau.StatusCode);
            Assert.Equal(422, susun.StatusCode);
            Assert.Contains("Desember 2026", susun.Message);
            Assert.Contains("tutup permanen", susun.Message);

            Assert.Equal(0, await JumlahJurnalPenutupAsync(database));
        }

        // =====================================================================
        // Acceptance 3 — akun laba ditahan belum ditetapkan ditolak 422
        // =====================================================================

        [Fact]
        public async Task AkunLabaDitahanBelumDitetapkan_Ditolak422()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);
            var akun = await SimpanAkunAsync(database);
            await IsiBukuBesarContohAsync(database, akun);

            // Sengaja tanpa TetapkanLabaDitahanAsync.
            await using var db = database.CreateContext();
            var layanan = Service(db);

            var pratinjau = await layanan.PreviewAsync(BadanHukum, TahunBuku);
            var susun = await layanan.GenerateAsync(Permintaan(), Pelaku);

            Assert.Equal(422, pratinjau.StatusCode);
            Assert.Equal(422, susun.StatusCode);
            Assert.Contains("laba ditahan", susun.Message, StringComparison.OrdinalIgnoreCase);

            Assert.Equal(0, await JumlahJurnalPenutupAsync(database));
        }

        /// <summary>
        /// Akun laba ditahan yang <b>berubah</b> sesudah ditetapkan tetap ditolak <c>422</c>,
        /// dengan kalimat yang menunjuk pengaturan akuntansi.
        /// </summary>
        /// <remarks>
        /// <c>BE-ACC-P2-009</c> memeriksa akunnya saat ditetapkan, tetapi akun dapat
        /// dinonaktifkan atau diubah menjadi akun induk sesudahnya. Tanpa pemeriksaan ulang,
        /// penolakannya tetap terjadi — jauh di dalam validasi baris jurnal, dengan kalimat
        /// "Baris ke-5" yang tidak menyebut pengaturan akuntansi sama sekali.
        /// </remarks>
        [Theory]
        [InlineData(false, true, AccountType.Equity, "akun induk")]
        [InlineData(true, false, AccountType.Equity, "tidak aktif")]
        [InlineData(true, true, AccountType.Asset, "Ekuitas")]
        public async Task AkunLabaDitahanTidakLagiLayak_Ditolak422(
            bool menerimaTransaksi,
            bool aktif,
            AccountType jenis,
            string potonganPesan)
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var akun = await SimpanAkunAsync(database, menerimaTransaksi, aktif, jenis);
            await TetapkanLabaDitahanAsync(database, akun.LabaDitahan.Id);
            await IsiBukuBesarContohAsync(database, akun);

            await using var db = database.CreateContext();
            var hasil = await Service(db).GenerateAsync(Permintaan(), Pelaku);

            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains(potonganPesan, hasil.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pengaturan akuntansi", hasil.Message, StringComparison.OrdinalIgnoreCase);

            Assert.Equal(0, await JumlahJurnalPenutupAsync(database));
        }

        // =====================================================================
        // Acceptance 4 — saldo TIAP akun Revenue dan Expense menjadi NOL
        // =====================================================================

        /// <summary>
        /// Bukti utama task ini. Jurnal penutup disusun, diajukan, disetujui, dan
        /// <b>disahkan</b>, lalu saldo tiap akun pendapatan dan beban diperiksa satu per satu.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Yang diperiksa <b>bukan</b> keseimbangan jurnalnya. Jurnal penutup yang angkanya salah
        /// tetap seimbang — itulah sebabnya salah hitung tutup tahun tidak pernah menimbulkan
        /// error. Yang membuktikan penutupan berhasil hanyalah saldo akunnya sendiri.
        /// </para>
        /// <para>
        /// Penghitungnya adalah <c>AccChartOfAccountService.HitungSaldoAsync</c>, penghitung
        /// saldo milik buku besar, bukan penghitung khusus uji. Bila tutup tahun memakai
        /// perhitungan yang diam-diam berbeda dari buku besar, perbedaan itu muncul di sini.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task SesudahDisahkan_SaldoTiapAkunPendapatanDanBebanMenjadiNol()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanContohLengkapAsync(database);

            // Sebelum ditutup: keempat akun memang bersaldo.
            await using (var sebelum = database.CreateContext())
            {
                Assert.Equal(-800_000_000m, await Saldo(sebelum, akun.RawatJalan.Id));
                Assert.Equal(-500_000_000m, await Saldo(sebelum, akun.RawatInap.Id));
                Assert.Equal(300_000_000m, await Saldo(sebelum, akun.BebanObat.Id));
                Assert.Equal(600_000_000m, await Saldo(sebelum, akun.BebanGaji.Id));
                Assert.Equal(0m, await Saldo(sebelum, akun.LabaDitahan.Id));
            }

            var jurnalId = await SusunDanSahkanAsync(database);

            await using var sesudah = database.CreateContext();

            // ---- Inti acceptance (4): NOL, tiap akun, satu per satu. ----
            Assert.Equal(0m, await Saldo(sesudah, akun.RawatJalan.Id));
            Assert.Equal(0m, await Saldo(sesudah, akun.RawatInap.Id));
            Assert.Equal(0m, await Saldo(sesudah, akun.BebanObat.Id));
            Assert.Equal(0m, await Saldo(sesudah, akun.BebanGaji.Id));

            // Labanya mendarat di laba ditahan, sisi kredit — saldo condong kredit berarti
            // negatif pada perjanjian tanda debit-dikurangi-kredit.
            Assert.Equal(-400_000_000m, await Saldo(sesudah, akun.LabaDitahan.Id));

            // Kas tidak tersentuh sama sekali: tutup tahun memindahkan laba, bukan uang.
            Assert.Equal(400_000_000m, await Saldo(sesudah, akun.Kas.Id));

            // Jurnalnya memang berjenis JT, berstatus Posted, dan bertanggal akhir tahun buku.
            var jurnal = await sesudah.Set<AccJournal>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == jurnalId);

            Assert.Equal(JenisJurnalTutupId, jurnal.JournalTypeId);
            Assert.Equal(JournalStatus.Posted, jurnal.JournalStatus);
            Assert.Equal(new DateTime(TahunBuku, 12, 31), jurnal.AccountingDate);
            Assert.StartsWith("JT/2026/12/", jurnal.JournalNumber);
        }

        /// <summary>
        /// Akun beban yang dipakai <b>dua</b> unit biaya tetap menjadi nol, dan tiap unit
        /// biayanya ditutup pada barisnya sendiri.
        /// </summary>
        /// <remarks>
        /// Inilah alasan baris beban dipecah per unit biaya. Menggabungkannya menjadi satu baris
        /// per akun akan memaksa penutup tahun menebak satu unit biaya — jurnalnya tetap
        /// seimbang, akunnya tetap nol, tetapi laporan beban per unit biaya menjadi salah tanpa
        /// satu pun tanda.
        /// </remarks>
        [Fact]
        public async Task AkunBebanDuaUnitBiaya_TetapNolDanTerpisahPerUnitBiaya()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var akun = await SimpanAkunAsync(database);
            await TetapkanLabaDitahanAsync(database, akun.LabaDitahan.Id);

            var maret = await IdPeriodeAsync(database, TahunBuku, 3);

            await JurnalDisahkanAsync(
                database, "JU/2026/03/00001", maret, new DateTime(TahunBuku, 3, 10),
                akun.Kas.Id, null, akun.RawatJalan.Id, null, 500_000_000m);

            // Satu akun beban, dua unit biaya.
            await JurnalDisahkanAsync(
                database, "JU/2026/03/00002", maret, new DateTime(TahunBuku, 3, 11),
                akun.BebanObat.Id, UnitBiayaId, akun.Kas.Id, null, 120_000_000m);

            await JurnalDisahkanAsync(
                database, "JU/2026/03/00003", maret, new DateTime(TahunBuku, 3, 12),
                akun.BebanObat.Id, UnitBiayaLainId, akun.Kas.Id, null, 80_000_000m);

            await using (var db = database.CreateContext())
            {
                var pratinjau = await Service(db).PreviewAsync(BadanHukum, TahunBuku);

                Assert.True(pratinjau.Success);

                var barisBebanObat = pratinjau.Data!.ExpenseLines
                    .Where(x => x.AccountCode == "5-1001")
                    .ToList();

                Assert.Equal(2, barisBebanObat.Count);
                Assert.All(barisBebanObat, x => Assert.NotNull(x.CostCenterId));
                Assert.Equal(200_000_000m, barisBebanObat.Sum(x => x.CreditAmount));

                Assert.Equal(
                    new[] { "Rawat Inap", "Rawat Jalan" },
                    barisBebanObat.Select(x => x.CostCenterName).OrderBy(x => x).ToArray());
            }

            await SusunDanSahkanAsync(database);

            await using var sesudah = database.CreateContext();

            Assert.Equal(0m, await Saldo(sesudah, akun.BebanObat.Id));
            Assert.Equal(0m, await Saldo(sesudah, akun.RawatJalan.Id));
            Assert.Equal(-300_000_000m, await Saldo(sesudah, akun.LabaDitahan.Id));
        }

        /// <summary>
        /// Jurnal tahun <b>berikutnya</b> tidak ikut tersapu ke laba ditahan tahun yang ditutup.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Ini keadaan yang paling lazim, bukan keadaan pinggiran: tutup tahun 2026 dikerjakan
        /// pada Maret 2027, ketika Januari 2027 sudah berisi jurnal yang disahkan. Menghitung
        /// saldo <b>seumur hidup</b> akun — sebagaimana
        /// <c>AccChartOfAccountService.HitungSaldoAsync</c> — akan menyapu pendapatan 2027 ke
        /// laba ditahan 2026 dan membuat akun 2027 bersaldo negatif, tanpa satu pun error.
        /// </para>
        /// <para>
        /// Karena itu di sini saldo <b>tahun buku</b> yang wajib nol, sementara saldo seumur
        /// hidup akun justru wajib menyisakan angka 2027 apa adanya.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task JurnalTahunBerikutnya_TidakIkutDitutup()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanContohLengkapAsync(database);

            // Tahun buku 2027 dibuka, dan Januari 2027 sudah berisi jurnal yang disahkan.
            await using (var db = database.CreateContext())
            {
                db.Set<AccAccountingPeriod>().AddRange(
                    PeriodeSetahun(TahunBuku + 1, AccountingPeriodStatus.Open));
                await db.SaveChangesAsync();
            }

            var januari2027 = await IdPeriodeAsync(database, TahunBuku + 1, 1);

            await JurnalDisahkanAsync(
                database, "JU/2027/01/00001", januari2027, new DateTime(TahunBuku + 1, 1, 15),
                akun.Kas.Id, null, akun.RawatJalan.Id, null, 70_000_000m);

            // Pratinjau 2026 tetap menyebut angka 2026 saja.
            await using (var db = database.CreateContext())
            {
                var pratinjau = await Service(db).PreviewAsync(BadanHukum, TahunBuku);

                Assert.Equal(1_300_000_000m, pratinjau.Data!.TotalRevenue);
                Assert.Equal(400_000_000m, pratinjau.Data.NetIncome);
            }

            await SusunDanSahkanAsync(database);

            await using var sesudah = database.CreateContext();

            var idPeriode2026 = await sesudah.Set<AccAccountingPeriod>()
                .Where(x => x.FiscalYear == TahunBuku)
                .Select(x => x.Id)
                .ToListAsync();

            var saldo2026 = await AccYearEndClosingService.HitungSaldoTahunAsync(
                sesudah, BadanHukum, idPeriode2026);

            // Saldo TAHUN BUKU 2026 nol untuk setiap akun pendapatan dan beban.
            Assert.NotEmpty(saldo2026);
            Assert.All(saldo2026, x => Assert.Equal(0m, x.Balance));

            // Sementara pendapatan Januari 2027 masih utuh — tidak ikut tersapu.
            Assert.Equal(-70_000_000m, await Saldo(sesudah, akun.RawatJalan.Id));
            Assert.Equal(-400_000_000m, await Saldo(sesudah, akun.LabaDitahan.Id));
        }

        /// <summary>
        /// Tahun yang bebannya melebihi pendapatan menghasilkan rugi, dan ruginya masuk sisi
        /// <b>debit</b> laba ditahan.
        /// </summary>
        [Fact]
        public async Task TahunRugi_LabaDitahanDidebit()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var akun = await SimpanAkunAsync(database);
            await TetapkanLabaDitahanAsync(database, akun.LabaDitahan.Id);

            var maret = await IdPeriodeAsync(database, TahunBuku, 3);

            await JurnalDisahkanAsync(
                database, "JU/2026/03/00001", maret, new DateTime(TahunBuku, 3, 10),
                akun.Kas.Id, null, akun.RawatJalan.Id, null, 200_000_000m);

            await JurnalDisahkanAsync(
                database, "JU/2026/03/00002", maret, new DateTime(TahunBuku, 3, 11),
                akun.BebanGaji.Id, UnitBiayaId, akun.Kas.Id, null, 350_000_000m);

            await using (var db = database.CreateContext())
            {
                var pratinjau = await Service(db).PreviewAsync(BadanHukum, TahunBuku);

                Assert.Equal(-150_000_000m, pratinjau.Data!.NetIncome);
                Assert.Equal(150_000_000m, pratinjau.Data.RetainedEarningsDebit);
                Assert.Equal(0m, pratinjau.Data.RetainedEarningsCredit);
            }

            await SusunDanSahkanAsync(database);

            await using var sesudah = database.CreateContext();

            Assert.Equal(0m, await Saldo(sesudah, akun.RawatJalan.Id));
            Assert.Equal(0m, await Saldo(sesudah, akun.BebanGaji.Id));

            // Rugi membuat laba ditahan condong DEBIT.
            Assert.Equal(150_000_000m, await Saldo(sesudah, akun.LabaDitahan.Id));
        }

        /// <summary>
        /// Tidak ada saldo yang perlu ditutup ditolak <c>422</c>, bukan menghasilkan jurnal
        /// kosong.
        /// </summary>
        [Fact]
        public async Task TidakAdaSaldo_Ditolak422()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var akun = await SimpanAkunAsync(database);
            await TetapkanLabaDitahanAsync(database, akun.LabaDitahan.Id);

            await using var db = database.CreateContext();
            var hasil = await Service(db).GenerateAsync(Permintaan(), Pelaku);

            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains("Tidak ada saldo", hasil.Message);
            Assert.Equal(0, await JumlahJurnalPenutupAsync(database));
        }

        // =====================================================================
        // Acceptance 5 — penyusunan kedua untuk tahun yang sama ditolak 409
        // =====================================================================

        [Fact]
        public async Task PenyusunanKedua_Ditolak409()
        {
            using var database = TestDatabase.Create();
            await SiapkanContohLengkapAsync(database);

            string nomorPertama;

            await using (var db = database.CreateContext())
            {
                var pertama = await Service(db).GenerateAsync(Permintaan(), Pelaku);

                Assert.True(pertama.Success);
                nomorPertama = pertama.Data!.JournalNumber;
            }

            await using (var db = database.CreateContext())
            {
                var kedua = await Service(db).GenerateAsync(Permintaan(), Pelaku);

                Assert.False(kedua.Success);
                Assert.Equal(409, kedua.StatusCode);

                // Nomor jurnal yang sudah ada disebut, supaya petugas dapat langsung membukanya.
                Assert.Contains(nomorPertama, kedua.Message);
            }

            // Tetap satu jurnal penutup, bukan dua.
            Assert.Equal(1, await JumlahJurnalPenutupAsync(database));
        }

        /// <summary>
        /// Tahun buku yang <b>berbeda</b> tidak saling menahan.
        /// </summary>
        [Fact]
        public async Task TahunBukuLain_TidakIkutTertahan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanContohLengkapAsync(database);

            await using (var db = database.CreateContext())
            {
                db.Set<AccAccountingPeriod>().AddRange(
                    PeriodeSetahun(TahunBuku + 1, AccountingPeriodStatus.SoftClosed));
                await db.SaveChangesAsync();
            }

            var juli2027 = await IdPeriodeAsync(database, TahunBuku + 1, 7);

            await JurnalDisahkanAsync(
                database, "JU/2027/07/00001", juli2027, new DateTime(TahunBuku + 1, 7, 5),
                akun.Kas.Id, null, akun.RawatInap.Id, null, 90_000_000m);

            await using (var db = database.CreateContext())
            {
                Assert.True((await Service(db).GenerateAsync(Permintaan(), Pelaku)).Success);
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await Service(db).GenerateAsync(Permintaan(TahunBuku + 1), Pelaku);

                Assert.True(hasil.Success);
                Assert.StartsWith("JT/2027/12/", hasil.Data!.JournalNumber);
            }

            Assert.Equal(2, await JumlahJurnalPenutupAsync(database));
        }

        // =====================================================================
        // Acceptance 6 — jurnal penutup dapat dibalik lewat jalur yang sudah ada
        // =====================================================================

        /// <summary>
        /// Jurnal penutup dibalik memakai <c>AccJournalService.ReverseAsync</c> yang sudah ada,
        /// dan saldo akun pendapatan serta beban kembali seperti sebelum ditutup.
        /// </summary>
        /// <remarks>
        /// Tidak ada mekanisme "buka kembali tahun buku". Jurnal penutup adalah jurnal biasa,
        /// sehingga koreksinya adalah pembalikan jurnal biasa (<c>ACC-DEC-029</c>). Usulan
        /// selengkapnya menunggu ratifikasi <c>DEC-ACC-P2-006</c>.
        /// </remarks>
        [Fact]
        public async Task JurnalPenutup_DapatDibalikLewatJalurYangSudahAda()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanContohLengkapAsync(database);

            var jurnalId = await SusunDanSahkanAsync(database);

            Guid pembalikId;

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccJournalService(db).ReverseAsync(
                    jurnalId,
                    new ReverseJournalRequest
                    {
                        CorrectionType = JournalCorrectionType.FullReversal,
                        Reason = "Ada jurnal Desember yang terlewat."
                    },
                    Penyetuju);

                Assert.True(hasil.Success, hasil.Message);
                Assert.Equal(jurnalId, hasil.Data!.ReversalOfJournalId);

                pembalikId = hasil.Data.Id;
            }

            // Pembalik lahir PendingApproval; disetujui lalu disahkan lewat jalur yang sama.
            await using (var db = database.CreateContext())
            {
                Assert.True((await new AccJournalService(db).ApproveAsync(pembalikId, Pelaku)).Success);
            }

            await using (var db = database.CreateContext())
            {
                Assert.True((await new AccJournalService(db).PostAsync(pembalikId, Pelaku)).Success);
            }

            await using var sesudah = database.CreateContext();

            // Saldo kembali seperti sebelum tutup tahun.
            Assert.Equal(-800_000_000m, await Saldo(sesudah, akun.RawatJalan.Id));
            Assert.Equal(-500_000_000m, await Saldo(sesudah, akun.RawatInap.Id));
            Assert.Equal(300_000_000m, await Saldo(sesudah, akun.BebanObat.Id));
            Assert.Equal(600_000_000m, await Saldo(sesudah, akun.BebanGaji.Id));
            Assert.Equal(0m, await Saldo(sesudah, akun.LabaDitahan.Id));
        }

        // =====================================================================
        // Hak akses — QBE-PERM-001 dan role-access-rules
        // =====================================================================

        /// <summary>
        /// Argumen pertama <c>[AccessPermission]</c> wajib sama persis dengan
        /// <c>ControllerName</c>; bila menyimpang hasilnya <c>403</c> permanen yang tidak dapat
        /// diperbaiki dari layar Akses Role.
        /// </summary>
        [Theory]
        [InlineData(nameof(YearEndClosingController.Preview), "Read")]
        [InlineData(nameof(YearEndClosingController.Generate), "Generate")]
        public void KeduaEndpoint_MembawaHakAksesYangBenar(string namaMethod, string aksiDiharapkan)
        {
            var controller = typeof(YearEndClosingController);
            var namaController = controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;

            Assert.Equal("YearEndClosing", namaController);

            var method = controller.GetMethod(namaMethod)!;

            var aksi = method.GetCustomAttribute<AccessActionAttribute>();
            Assert.NotNull(aksi);

            var izin = method.GetCustomAttribute<AccessPermissionAttribute>();
            Assert.NotNull(izin);

            var argumen = izin!.Arguments!;

            Assert.Equal(namaController, (string)argumen[0]);
            Assert.Equal(aksiDiharapkan, (string)argumen[1]);

            // Argumen kedua wajib sama persis dengan ActionName pada [AccessAction] di method
            // yang sama, atau kemampuannya tidak akan pernah dapat dicentang.
            Assert.Equal(aksi!.ActionName, (string)argumen[1]);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private static GenerateYearEndClosingRequest Permintaan(int tahun = TahunBuku)
            => new() { LegalEntityId = BadanHukum, FiscalYear = tahun };

        private static Task<decimal> Saldo(ApplicationDbContext db, Guid akunId)
            => AccChartOfAccountService.HitungSaldoAsync(db, akunId);

        private static async Task<int> JumlahJurnalAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();
            return await db.Set<AccJournal>().CountAsync();
        }

        private static async Task<int> JumlahJurnalPenutupAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();

            return await db.Set<AccJournal>()
                .CountAsync(x => x.JournalTypeId == JenisJurnalTutupId && !x.IsDelete);
        }

        /// <summary>
        /// Menyusun jurnal penutup lalu menempuh jalur pengajuan, persetujuan, dan pengesahan
        /// yang sudah ada. Mengembalikan id jurnal penutupnya.
        /// </summary>
        /// <remarks>
        /// Penyetujunya sengaja <b>bukan</b> pembuatnya: <c>ACC-DEC-016</c> menolak penyetuju
        /// yang sama dengan pembuat tanpa pengecualian, termasuk untuk jurnal penutup tahun.
        /// </remarks>
        private static async Task<Guid> SusunDanSahkanAsync(
            TestDatabase database, int tahun = TahunBuku)
        {
            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                var hasil = await Service(db).GenerateAsync(Permintaan(tahun), Pelaku);

                Assert.True(hasil.Success, hasil.Message);
                Assert.Equal(JournalStatus.Draft, hasil.Data!.JournalStatus);

                jurnalId = hasil.Data.Id;
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccJournalService(db).SubmitAsync(jurnalId, Pelaku);
                Assert.True(hasil.Success, hasil.Message);
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccJournalService(db).ApproveAsync(jurnalId, Penyetuju);
                Assert.True(hasil.Success, hasil.Message);
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccJournalService(db).PostAsync(jurnalId, Penyetuju);
                Assert.True(hasil.Success, hasil.Message);
            }

            return jurnalId;
        }
    }
}
