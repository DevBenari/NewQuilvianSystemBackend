using Microsoft.EntityFrameworkCore;
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
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-012</c> — penolakan jurnal manual ke control account,
    /// beserta <c>ACC-DEC-072</c> (koreksi) dan <c>ACC-DEC-073</c> (template berulang).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Acceptance (2) adalah alasan berkas ini ada.</b> Menolak jurnal manual itu mudah;
    /// yang berbahaya adalah ikut menolak jalur otomatis. <see cref="AccJournal"/> tidak
    /// menyimpan asal-usulnya, dan jurnal penutup tahun, draft template, serta jurnal pembalik
    /// diajukan lewat endpoint yang sama dengan jurnal manual. Bila pengecualiannya keliru,
    /// seluruh posting otomatis tertolak <c>422</c>.
    /// </para>
    /// <para>
    /// Karena itu setiap pengecualian diuji berpasangan dengan kebalikannya. Jurnal <c>JT</c>
    /// hasil tutup tahun lolos, tetapi jurnal berjenis <c>JT</c> yang menyentuh Kas ditolak —
    /// membuktikan pengecualiannya tidak diturunkan dari kode jenis. Jurnal pembalik penuh lolos,
    /// tetapi jurnal pembalik yang barisnya diubah ditolak.
    /// </para>
    /// <para>
    /// Lihat catatan <c>ACC-TD-001</c> pada <c>AccYearEndClosingTests</c> mengenai
    /// <c>PRAGMA ignore_check_constraints</c>. Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccControlAccountJournalGuardTests
    {
        private static readonly Guid Pelaku = Guid.Parse("c1c1c1c1-0000-0000-0000-000000000001");
        private static readonly Guid BadanHukum = Guid.Parse("c2c2c2c2-0000-0000-0000-000000000002");
        private static readonly Guid JenisUmumId = Guid.Parse("c3c3c3c3-0000-0000-0000-000000000003");
        private static readonly Guid JenisTutupId = Guid.Parse("c4c4c4c4-0000-0000-0000-000000000004");
        private static readonly Guid JenisBalikId = Guid.Parse("c5c5c5c5-0000-0000-0000-000000000005");
        private static readonly Guid JenisSesuaiId = Guid.Parse("c6c6c6c6-0000-0000-0000-000000000006");
        private static readonly Guid UnitBiayaId = Guid.Parse("c7c7c7c7-0000-0000-0000-000000000007");

        private const int Tahun = 2026;

        private static readonly DateTime TanggalUji = new(Tahun, 9, 10);

        // =====================================================================
        // Penyiapan
        // =====================================================================

        /// <summary>
        /// Kas Kasir sudah bertanda control account. Kas Kecil sengaja belum — ia ditandai di
        /// tengah uji untuk meniru akun yang ditandai sesudah draft atau template tersimpan.
        /// </summary>
        private sealed class DaftarAkun
        {
            public AccChartOfAccount KasKasir { get; init; } = null!;
            public AccChartOfAccount KasKecil { get; init; } = null!;
            public AccChartOfAccount KasBesar { get; init; } = null!;
            public AccChartOfAccount Pendapatan { get; init; } = null!;
            public AccChartOfAccount PendapatanLain { get; init; } = null!;
            public AccChartOfAccount LabaDitahan { get; init; } = null!;
        }

        private static async Task<DaftarAkun> SiapkanAsync(
            TestDatabase database,
            AccountingPeriodStatus statusPeriode = AccountingPeriodStatus.Open)
        {
            await using (var pragma = database.CreateContext())
            {
                await pragma.Database.ExecuteSqlRawAsync("PRAGMA ignore_check_constraints = ON;");
            }

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

            db.Set<AccJournalType>().AddRange(
                JenisJurnal(JenisUmumId, "JU", "Jurnal Umum"),
                JenisJurnal(JenisTutupId, "JT", "Jurnal Tutup Tahun"),
                JenisJurnal(JenisBalikId, "JB", "Jurnal Pembalik"),
                JenisJurnal(JenisSesuaiId, "JP", "Jurnal Penyesuaian"));

            db.Set<MstCostCenter>().Add(new MstCostCenter
            {
                Id = UnitBiayaId,
                LegalEntityId = BadanHukum,
                CostCenterCode = "CC-UM",
                CostCenterName = "Umum",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            db.Set<AccAccountingPeriod>().AddRange(
                Enumerable.Range(1, 12).Select(bulan => new AccAccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = BadanHukum,
                    PeriodCode = $"{Tahun}-{bulan:D2}",
                    FiscalYear = Tahun,
                    PeriodMonth = bulan,
                    StartDate = new DateTime(Tahun, bulan, 1),
                    EndDate = new DateTime(Tahun, bulan, DateTime.DaysInMonth(Tahun, bulan)),
                    PeriodStatus = statusPeriode,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                }));

            var daftar = new DaftarAkun
            {
                KasKasir = Akun("1-1002", "Kas Kasir", AccountType.Asset, NormalBalance.Debit, control: true),
                KasKecil = Akun("1-1003", "Kas Kecil", AccountType.Asset, NormalBalance.Debit),
                KasBesar = Akun("1-1001", "Kas Besar", AccountType.Asset, NormalBalance.Debit),
                Pendapatan = Akun("4-1001", "Pendapatan Rawat Jalan", AccountType.Revenue, NormalBalance.Credit),
                PendapatanLain = Akun("4-1002", "Pendapatan Rawat Inap", AccountType.Revenue, NormalBalance.Credit),
                LabaDitahan = Akun("3-3001", "Laba Ditahan", AccountType.Equity, NormalBalance.Credit)
            };

            db.Set<AccChartOfAccount>().AddRange(
                daftar.KasKasir, daftar.KasKecil, daftar.KasBesar,
                daftar.Pendapatan, daftar.PendapatanLain, daftar.LabaDitahan);

            await db.SaveChangesAsync();

            return daftar;
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

        private static AccChartOfAccount Akun(
            string kode,
            string nama,
            AccountType jenis,
            NormalBalance saldoNormal,
            bool control = false) => new()
            {
                Id = Guid.NewGuid(),
                LegalEntityId = BadanHukum,
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

        private static CreateJournalLineRequest Baris(int nomor, Guid akunId, decimal debit, decimal kredit) => new()
        {
            LineNumber = nomor,
            AccountId = akunId,
            Description = "Baris uji",
            DebitAmount = debit,
            CreditAmount = kredit
        };

        private static CreateJournalRequest Jurnal(Guid jenisId, params CreateJournalLineRequest[] baris) => new()
        {
            LegalEntityId = BadanHukum,
            JournalTypeId = jenisId,
            AccountingDate = TanggalUji,
            Description = "Jurnal uji",
            Lines = baris.ToList()
        };

        private static UpdateJournalRequest Ubah(Guid jenisId, params CreateJournalLineRequest[] baris) => new()
        {
            JournalTypeId = jenisId,
            AccountingDate = TanggalUji,
            Description = "Jurnal uji diubah",
            Lines = baris.ToList()
        };

        private static async Task TandaiControlAsync(TestDatabase database, Guid akunId)
        {
            await using var db = database.CreateContext();
            var akun = await db.Set<AccChartOfAccount>().SingleAsync(x => x.Id == akunId);
            akun.IsControlAccount = true;
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Jurnal Disahkan disimpan langsung ke tabel. Daur hidupnya bukan yang diuji di sini;
        /// yang dibutuhkan hanyalah jurnal asal untuk dibalik atau disesuaikan.
        /// </summary>
        private static async Task<Guid> JurnalDisahkanAsync(
            TestDatabase database,
            Guid akunDebit,
            Guid akunKredit,
            decimal nominal,
            int bulan = 9,
            string nomor = "JU/2026/09/00900")
        {
            await using var db = database.CreateContext();

            var periodeId = await db.Set<AccAccountingPeriod>()
                .Where(x => x.FiscalYear == Tahun && x.PeriodMonth == bulan)
                .Select(x => x.Id)
                .FirstAsync();

            var jurnal = new AccJournal
            {
                Id = Guid.NewGuid(),
                LegalEntityId = BadanHukum,
                JournalNumber = nomor,
                JournalTypeId = JenisUmumId,
                AccountingPeriodId = periodeId,
                AccountingDate = new DateTime(Tahun, bulan, 10),
                Description = "Jurnal asal uji",
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

            return jurnal.Id;
        }

        private static async Task JadikanDitolakAsync(TestDatabase database, Guid jurnalId)
        {
            await using var db = database.CreateContext();
            var jurnal = await db.Set<AccJournal>().SingleAsync(x => x.Id == jurnalId);
            jurnal.JournalStatus = JournalStatus.Rejected;
            jurnal.RejectionReason = "Ditolak untuk uji.";
            await db.SaveChangesAsync();
        }

        private static AccRecurringJournalService Template(ApplicationDbContext db)
            => new(db, new AccJournalService(db));

        private static CreateRecurringJournalRequest PermintaanTemplate(Guid akunDebit, Guid akunKredit) => new()
        {
            LegalEntityId = BadanHukum,
            TemplateCode = "SETOR-KAS",
            TemplateName = "Setoran kas bulanan",
            JournalTypeId = JenisUmumId,
            DayOfMonth = 25,
            StartDate = new DateTime(Tahun, 1, 1),
            Lines = new List<CreateRecurringJournalLineRequest>
            {
                new() { LineNumber = 1, AccountId = akunDebit, DebitAmount = 1_000_000m },
                new() { LineNumber = 2, AccountId = akunKredit, CreditAmount = 1_000_000m }
            }
        };

        // =====================================================================
        // Acceptance 1 — jurnal manual ke control account ditolak 422, pesan menyebut akunnya
        // =====================================================================

        [Fact]
        public async Task JurnalManual_KeControlAccount_Ditolak422_DanMenyebutAkunnya()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await new AccJournalService(db).CreateManualAsync(
                Jurnal(JenisUmumId,
                    Baris(1, akun.KasKasir.Id, 500_000m, 0m),
                    Baris(2, akun.Pendapatan.Id, 0m, 500_000m)),
                Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains("1-1002 Kas Kasir", hasil.Message);
            Assert.Contains("bukan jurnal manual", hasil.Message);

            // Nol jurnal tersimpan, dan nomor jurnal tidak ikut terpakai.
            Assert.Equal(0, await db.Set<AccJournal>().CountAsync());
        }

        [Fact]
        public async Task JurnalManual_DraftSebelumAkunDitandai_DitolakSaatDiajukan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                var simpan = await new AccJournalService(db).CreateManualAsync(
                    Jurnal(JenisUmumId,
                        Baris(1, akun.KasKecil.Id, 200_000m, 0m),
                        Baris(2, akun.Pendapatan.Id, 0m, 200_000m)),
                    Pelaku);

                Assert.True(simpan.Success, simpan.Message);
                jurnalId = simpan.Data!.Id;
            }

            await TandaiControlAsync(database, akun.KasKecil.Id);

            await using (var db = database.CreateContext())
            {
                var ajukan = await new AccJournalService(db).SubmitAsync(jurnalId, Pelaku);

                Assert.Equal(422, ajukan.StatusCode);
                Assert.Contains("1-1003 Kas Kecil", ajukan.Message);
            }

            await using var sesudah = database.CreateContext();
            Assert.Equal(
                JournalStatus.Draft,
                (await sesudah.Set<AccJournal>().SingleAsync(x => x.Id == jurnalId)).JournalStatus);
        }

        // =====================================================================
        // Acceptance 3 — akun non-control tetap dapat dijurnal manual
        // =====================================================================

        [Fact]
        public async Task JurnalManual_KeAkunBiasa_TetapDiterimaDanDapatDiajukan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccJournalService(db).CreateManualAsync(
                    Jurnal(JenisUmumId,
                        Baris(1, akun.KasBesar.Id, 500_000m, 0m),
                        Baris(2, akun.Pendapatan.Id, 0m, 500_000m)),
                    Pelaku);

                Assert.True(hasil.Success, hasil.Message);
                Assert.Equal(201, hasil.StatusCode);
                jurnalId = hasil.Data!.Id;
            }

            await using (var db = database.CreateContext())
            {
                var ajukan = await new AccJournalService(db).SubmitAsync(jurnalId, Pelaku);
                Assert.True(ajukan.Success, ajukan.Message);
            }
        }

        // =====================================================================
        // Acceptance 4 — jurnal lama tidak ikut ditolak saat diubah, kecuali menyentuh control
        // =====================================================================

        [Fact]
        public async Task UbahDraft_TanpaControlAccount_TetapDiterima()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var layanan = new AccJournalService(db);

            var simpan = await layanan.CreateManualAsync(
                Jurnal(JenisUmumId,
                    Baris(1, akun.KasBesar.Id, 500_000m, 0m),
                    Baris(2, akun.Pendapatan.Id, 0m, 500_000m)),
                Pelaku);

            Assert.True(simpan.Success, simpan.Message);

            var ubah = await layanan.UpdateAsync(
                simpan.Data!.Id,
                Ubah(JenisUmumId,
                    Baris(1, akun.KasBesar.Id, 750_000m, 0m),
                    Baris(2, akun.PendapatanLain.Id, 0m, 750_000m)),
                Pelaku);

            Assert.True(ubah.Success, ubah.Message);
        }

        [Fact]
        public async Task UbahDraft_MenambahControlAccount_Ditolak422_BarisLamaUtuh()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                var simpan = await new AccJournalService(db).CreateManualAsync(
                    Jurnal(JenisUmumId,
                        Baris(1, akun.KasBesar.Id, 500_000m, 0m),
                        Baris(2, akun.Pendapatan.Id, 0m, 500_000m)),
                    Pelaku);

                Assert.True(simpan.Success, simpan.Message);
                jurnalId = simpan.Data!.Id;
            }

            await using (var db = database.CreateContext())
            {
                var ubah = await new AccJournalService(db).UpdateAsync(
                    jurnalId,
                    Ubah(JenisUmumId,
                        Baris(1, akun.KasKasir.Id, 500_000m, 0m),
                        Baris(2, akun.Pendapatan.Id, 0m, 500_000m)),
                    Pelaku);

                Assert.Equal(422, ubah.StatusCode);
                Assert.Contains("1-1002 Kas Kasir", ubah.Message);
            }

            await using var sesudah = database.CreateContext();

            var akunTersimpan = await sesudah.Set<AccJournalLine>()
                .Where(x => x.JournalId == jurnalId && !x.IsDelete)
                .Select(x => x.AccountId)
                .ToListAsync();

            Assert.Contains(akun.KasBesar.Id, akunTersimpan);
            Assert.DoesNotContain(akun.KasKasir.Id, akunTersimpan);
        }

        // =====================================================================
        // Acceptance 2 — jalur otomatis TIDAK terkena aturan
        // =====================================================================

        /// <summary>
        /// Jurnal penutup hasil <c>AccYearEndClosingService</c> lolos diajukan, bahkan ketika
        /// salah satu akun pendapatannya keliru ditandai control account.
        /// </summary>
        [Fact]
        public async Task JurnalPenutupTahun_TidakTerkenaLarangan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database, AccountingPeriodStatus.SoftClosed);

            await JurnalDisahkanAsync(database, akun.KasBesar.Id, akun.Pendapatan.Id, 1_000_000m, bulan: 3, nomor: "JU/2026/03/00001");

            await using (var db = database.CreateContext())
            {
                db.Set<AccAccountingConfiguration>().Add(new AccAccountingConfiguration
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = BadanHukum,
                    RetainedEarningsAccountId = akun.LabaDitahan.Id,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                });

                await db.SaveChangesAsync();
            }

            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                var susun = await new AccYearEndClosingService(db, new AccJournalService(db)).GenerateAsync(
                    new GenerateYearEndClosingRequest { LegalEntityId = BadanHukum, FiscalYear = Tahun },
                    Pelaku);

                Assert.True(susun.Success, susun.Message);
                jurnalId = susun.Data!.Id;
            }

            await TandaiControlAsync(database, akun.Pendapatan.Id);

            await using (var db = database.CreateContext())
            {
                var ajukan = await new AccJournalService(db).SubmitAsync(jurnalId, Pelaku);

                Assert.True(ajukan.Success, ajukan.Message);
                Assert.Equal(JournalStatus.PendingApproval, ajukan.Data!.JournalStatus);
            }
        }

        /// <summary>
        /// Pasangan uji di atas: jurnal berjenis <c>JT</c> yang menyentuh Kas Kasir bukan hasil
        /// tutup tahun, apa pun jenisnya. Pengecualian tidak diturunkan dari kode jenis.
        /// </summary>
        [Fact]
        public async Task JenisJT_YangMenyentuhKas_BukanTutupTahun_DitolakSaatDiajukan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                // Lewat CreateAsync — meniru draft yang tersimpan sebelum aturan ini berlaku.
                var simpan = await new AccJournalService(db).CreateAsync(
                    Jurnal(JenisTutupId,
                        Baris(1, akun.KasKasir.Id, 300_000m, 0m),
                        Baris(2, akun.Pendapatan.Id, 0m, 300_000m)),
                    Pelaku);

                Assert.True(simpan.Success, simpan.Message);
                jurnalId = simpan.Data!.Id;
            }

            await using (var db = database.CreateContext())
            {
                var ajukan = await new AccJournalService(db).SubmitAsync(jurnalId, Pelaku);
                Assert.Equal(422, ajukan.StatusCode);
            }
        }

        /// <summary>
        /// <c>ACC-DEC-073</c> butir (3): template aktif yang akunnya baru ditandai control tetap
        /// terbit, dan draftnya lolos diajukan. Ini sisa risiko yang diterima owner, dan uji ini
        /// menjaga supaya penerbitan otomatis tidak mati diam-diam.
        /// </summary>
        [Fact]
        public async Task TemplateAktif_AkunnyaBaruDitandai_TetapTerbitDanDraftnyaLolosDiajukan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            Guid templateId;

            await using (var db = database.CreateContext())
            {
                var simpan = await Template(db).CreateAsync(
                    PermintaanTemplate(akun.KasKecil.Id, akun.Pendapatan.Id), Pelaku);
                Assert.True(simpan.Success, simpan.Message);
                templateId = simpan.Data!.Id;
            }

            await using (var db = database.CreateContext())
            {
                var aktif = await Template(db).ActivateAsync(templateId, Pelaku);
                Assert.True(aktif.Success, aktif.Message);
            }

            await TandaiControlAsync(database, akun.KasKecil.Id);

            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                var terbit = await Template(db).GenerateAsync(
                    templateId,
                    new GenerateRecurringJournalRequest { AccountingDate = new DateTime(Tahun, 9, 25) },
                    Pelaku);

                Assert.True(terbit.Success, terbit.Message);
                jurnalId = terbit.Data!.JournalId;
            }

            await using (var db = database.CreateContext())
            {
                var ajukan = await new AccJournalService(db).SubmitAsync(jurnalId, Pelaku);
                Assert.True(ajukan.Success, ajukan.Message);
            }
        }

        // =====================================================================
        // ACC-DEC-072 — koreksi jurnal yang menyentuh control account
        // =====================================================================

        [Fact]
        public async Task PembalikanPenuh_JurnalControl_DiterimaDanLolosDiajukanUlang()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            var asalId = await JurnalDisahkanAsync(database, akun.KasKasir.Id, akun.Pendapatan.Id, 400_000m);

            Guid pembalikId;

            await using (var db = database.CreateContext())
            {
                var balik = await new AccJournalService(db).ReverseAsync(
                    asalId,
                    new ReverseJournalRequest
                    {
                        CorrectionType = JournalCorrectionType.FullReversal,
                        Reason = "Salah input nominal."
                    },
                    Pelaku);

                Assert.True(balik.Success, balik.Message);
                Assert.Equal(JournalStatus.PendingApproval, balik.Data!.JournalStatus);
                pembalikId = balik.Data.Id;
            }

            // Pembalik ditolak penyetuju, lalu diajukan ulang tanpa diubah.
            await JadikanDitolakAsync(database, pembalikId);

            await using (var db = database.CreateContext())
            {
                var ajukan = await new AccJournalService(db).SubmitAsync(pembalikId, Pelaku);
                Assert.True(ajukan.Success, ajukan.Message);
            }
        }

        [Fact]
        public async Task PembalikYangBarisnyaDiubah_DiperiksaSepertiJurnalManual()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            var asalId = await JurnalDisahkanAsync(database, akun.KasKasir.Id, akun.Pendapatan.Id, 400_000m);

            Guid pembalikId;

            await using (var db = database.CreateContext())
            {
                var balik = await new AccJournalService(db).ReverseAsync(
                    asalId,
                    new ReverseJournalRequest
                    {
                        CorrectionType = JournalCorrectionType.FullReversal,
                        Reason = "Salah input nominal."
                    },
                    Pelaku);

                Assert.True(balik.Success, balik.Message);
                pembalikId = balik.Data!.Id;
            }

            await JadikanDitolakAsync(database, pembalikId);

            await using (var db = database.CreateContext())
            {
                var ubah = await new AccJournalService(db).UpdateAsync(
                    pembalikId,
                    Ubah(JenisBalikId,
                        Baris(1, akun.KasKasir.Id, 0m, 900_000m),
                        Baris(2, akun.Pendapatan.Id, 900_000m, 0m)),
                    Pelaku);

                Assert.Equal(422, ubah.StatusCode);
                Assert.Contains("bukan jurnal manual", ubah.Message);
            }
        }

        [Fact]
        public async Task Penyesuaian_BarisKeControlAccount_Ditolak422()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            // Jurnal asalnya sama sekali tidak menyentuh control account.
            var asalId = await JurnalDisahkanAsync(database, akun.KasBesar.Id, akun.Pendapatan.Id, 400_000m);

            await using var db = database.CreateContext();

            var sesuai = await new AccJournalService(db).ReverseAsync(
                asalId,
                new ReverseJournalRequest
                {
                    CorrectionType = JournalCorrectionType.Adjustment,
                    Reason = "Penyesuaian uji.",
                    AdjustmentLines = new List<CreateJournalLineRequest>
                    {
                        Baris(1, akun.KasKasir.Id, 100_000m, 0m),
                        Baris(2, akun.Pendapatan.Id, 0m, 100_000m)
                    }
                },
                Pelaku);

            Assert.Equal(422, sesuai.StatusCode);
            Assert.Contains("bukan jurnal penyesuaian", sesuai.Message);
            Assert.Equal(0, await db.Set<AccJournal>().CountAsync(x => x.ReversalOfJournalId == asalId));
        }

        [Fact]
        public async Task Penyesuaian_AtasJurnalControl_BarisnyaNonControl_Diterima()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            var asalId = await JurnalDisahkanAsync(database, akun.KasKasir.Id, akun.Pendapatan.Id, 400_000m);

            await using var db = database.CreateContext();

            // Memindahkan pendapatan yang salah akun — tidak menyentuh Kas Kasir.
            var sesuai = await new AccJournalService(db).ReverseAsync(
                asalId,
                new ReverseJournalRequest
                {
                    CorrectionType = JournalCorrectionType.Adjustment,
                    Reason = "Pendapatan salah akun.",
                    AdjustmentLines = new List<CreateJournalLineRequest>
                    {
                        Baris(1, akun.Pendapatan.Id, 400_000m, 0m),
                        Baris(2, akun.PendapatanLain.Id, 0m, 400_000m)
                    }
                },
                Pelaku);

            Assert.True(sesuai.Success, sesuai.Message);
        }

        // =====================================================================
        // ACC-DEC-073 — template berulang
        // =====================================================================

        [Fact]
        public async Task Template_KeControlAccount_Ditolak422SaatDisimpan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var simpan = await Template(db).CreateAsync(
                PermintaanTemplate(akun.KasKasir.Id, akun.Pendapatan.Id), Pelaku);

            Assert.Equal(422, simpan.StatusCode);
            Assert.Contains("1-1002 Kas Kasir", simpan.Message);
            Assert.Contains("bukan template jurnal berulang", simpan.Message);
            Assert.Equal(0, await db.Set<AccRecurringJournalTemplate>().CountAsync());
        }

        [Fact]
        public async Task Template_AkunnyaBaruDitandai_Ditolak422SaatDiaktifkan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            Guid templateId;

            await using (var db = database.CreateContext())
            {
                var simpan = await Template(db).CreateAsync(
                    PermintaanTemplate(akun.KasKecil.Id, akun.Pendapatan.Id), Pelaku);
                Assert.True(simpan.Success, simpan.Message);
                templateId = simpan.Data!.Id;
            }

            await TandaiControlAsync(database, akun.KasKecil.Id);

            await using (var db = database.CreateContext())
            {
                var aktif = await Template(db).ActivateAsync(templateId, Pelaku);

                Assert.Equal(422, aktif.StatusCode);
                Assert.Contains("1-1003 Kas Kecil", aktif.Message);
            }

            await using var sesudah = database.CreateContext();
            Assert.False((await sesudah.Set<AccRecurringJournalTemplate>().SingleAsync(x => x.Id == templateId)).IsActive);
        }

        // =====================================================================
        // /options — penanda dikirim, akun control tidak disaring
        // =====================================================================

        [Fact]
        public async Task Options_MembawaPenandaControlAccount_TanpaMenyaringnya()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await new AccChartOfAccountService(db).GetOptionsAsync(BadanHukum, null);

            Assert.True(hasil.Success, hasil.Message);

            var kasKasir = Assert.Single(hasil.Data!, x => x.Id == akun.KasKasir.Id);
            var kasBesar = Assert.Single(hasil.Data!, x => x.Id == akun.KasBesar.Id);

            Assert.True(kasKasir.IsControlAccount);
            Assert.False(kasBesar.IsControlAccount);
        }
    }
}
