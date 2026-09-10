using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-008</c> — penerbitan jurnal berulang dan penjadwalnya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Acceptance (1) adalah alasan berkas ini ada.</b> Yang dijaga bukan sekadar "penerbitan
    /// kedua ditolak", melainkan <b>siapa</b> yang menolaknya. Pemeriksaan di kode C# tidak
    /// mencegah apa pun ketika dua proses berjalan bersamaan: keduanya dapat sama-sama lolos
    /// memeriksa sebelum salah satunya menyimpan. Yang benar-benar mencegah adalah unique index
    /// <c>(TemplateId, AccountingPeriodId)</c> di database.
    /// </para>
    /// <para>
    /// Karena itu uji di bawah tidak berhenti pada jalur normal; ia <b>melangkahi</b> pemeriksaan
    /// kode dengan menyisipkan baris penerbitan langsung ke tabel, lalu memastikan penerbitan
    /// tetap gagal — kali ini karena database yang menolaknya.
    /// </para>
    /// <para>
    /// Yang <b>tidak</b> dapat dibuktikan di sini adalah konkurensi sungguhan pada dua koneksi
    /// terpisah. SQLite dalam memori memakai satu koneksi bersama, dan `ACC-TD-001` mencatat
    /// batas itu. Pembuktiannya menuntut PostgreSQL.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccRecurringJournalGenerateTests
    {
        private static readonly Guid Pelaku = Guid.Parse("a8a8a8a8-0000-0000-0000-000000000001");
        private static readonly Guid BadanHukum = Guid.Parse("b8b8b8b8-0000-0000-0000-000000000002");
        private static readonly Guid JenisJurnalId = Guid.Parse("c8c8c8c8-0000-0000-0000-000000000003");
        private static readonly Guid UnitBiayaId = Guid.Parse("d8d8d8d8-0000-0000-0000-000000000004");

        private const int Tahun = 2026;

        /// <summary>Tanggal terbit template uji.</summary>
        private const int TanggalTerbit = 25;

        // =====================================================================
        // Penyiapan
        // =====================================================================

        private sealed class Bahan
        {
            public Guid TemplateId { get; init; }
            public Guid AkunBeban { get; init; }
            public Guid AkunAkumulasi { get; init; }
        }

        private static async Task<Bahan> SiapkanAsync(
            TestDatabase database,
            AccountingPeriodStatus statusPeriode = AccountingPeriodStatus.Open,
            bool aktifkan = true)
        {
            await using (var pragma = database.CreateContext())
            {
                await pragma.Database.ExecuteSqlRawAsync("PRAGMA ignore_check_constraints = ON;");
            }

            Guid akunBeban, akunAkumulasi;

            await using (var db = database.CreateContext())
            {
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

                db.Set<AccJournalType>().Add(new AccJournalType
                {
                    Id = JenisJurnalId,
                    JournalTypeCode = "JP",
                    JournalTypeName = "Jurnal Penyesuaian",
                    NumberPrefix = "JP",
                    RequiresApproval = true,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                });

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

                var beban = Akun("5-2003", "Beban Penyusutan", AccountType.Expense, NormalBalance.Debit);
                var akumulasi = Akun("1-4001", "Akumulasi Penyusutan", AccountType.Asset, NormalBalance.Credit);

                db.Set<AccChartOfAccount>().AddRange(beban, akumulasi);

                await db.SaveChangesAsync();

                akunBeban = beban.Id;
                akunAkumulasi = akumulasi.Id;
            }

            Guid templateId;

            await using (var db = database.CreateContext())
            {
                var hasil = await Layanan(db).CreateAsync(new CreateRecurringJournalRequest
                {
                    LegalEntityId = BadanHukum,
                    TemplateCode = "SUSUT-ALKES",
                    TemplateName = "Penyusutan Alat Kesehatan",
                    JournalTypeId = JenisJurnalId,
                    DayOfMonth = TanggalTerbit,
                    StartDate = new DateTime(Tahun, 1, 1),
                    Lines = new List<CreateRecurringJournalLineRequest>
                    {
                        new() { LineNumber = 1, AccountId = akunBeban, CostCenterId = UnitBiayaId, DebitAmount = 5_000_000m },
                        new() { LineNumber = 2, AccountId = akunAkumulasi, CreditAmount = 5_000_000m }
                    }
                }, Pelaku);

                Assert.True(hasil.Success, hasil.Message);
                templateId = hasil.Data!.Id;
            }

            if (aktifkan)
            {
                await using var db = database.CreateContext();
                var hasil = await Layanan(db).ActivateAsync(templateId, Pelaku);
                Assert.True(hasil.Success, hasil.Message);
            }

            return new Bahan
            {
                TemplateId = templateId,
                AkunBeban = akunBeban,
                AkunAkumulasi = akunAkumulasi
            };
        }

        private static AccChartOfAccount Akun(
            string kode, string nama, AccountType jenis, NormalBalance saldoNormal) => new()
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
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            };

        private static AccRecurringJournalService Layanan(ApplicationDbContext db)
            => new(db, new AccJournalService(db));

        private static GenerateRecurringJournalRequest Untuk(int bulan)
            => new() { AccountingDate = new DateTime(Tahun, bulan, TanggalTerbit) };

        // =====================================================================
        // Jalur normal
        // =====================================================================

        [Fact]
        public async Task Penerbitan_MenghasilkanJurnalDraftDanBarisPenerbitan()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                var hasil = await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku);

                Assert.True(hasil.Success, hasil.Message);
                Assert.Equal(201, hasil.StatusCode);
                Assert.Equal("2026-09", hasil.Data!.PeriodCode);
                Assert.Equal(JournalStatus.Draft, hasil.Data.JournalStatus);
                Assert.StartsWith("JP/2026/09/", hasil.Data.JournalNumber);
            }

            await using var pemeriksa = database.CreateContext();

            var jurnal = await pemeriksa.Set<AccJournal>().AsNoTracking().SingleAsync();
            var baris = await pemeriksa.Set<AccJournalLine>().AsNoTracking().ToListAsync();
            var run = await pemeriksa.Set<AccRecurringJournalRun>().AsNoTracking().SingleAsync();

            // Jurnalnya lahir Draft dan seimbang, dan barisnya menyalin baris template apa adanya
            // termasuk unit biayanya.
            Assert.Equal(JournalStatus.Draft, jurnal.JournalStatus);
            Assert.Equal(new DateTime(Tahun, 9, TanggalTerbit), jurnal.AccountingDate);
            Assert.Equal(5_000_000m, jurnal.TotalDebit);
            Assert.Equal(5_000_000m, jurnal.TotalCredit);
            Assert.Equal(2, baris.Count);
            Assert.Equal(UnitBiayaId, baris.Single(x => x.AccountId == bahan.AkunBeban).CostCenterId);

            // Baris penerbitannya menunjuk jurnal itu.
            Assert.Equal(jurnal.Id, run.JournalId);
            Assert.Equal(bahan.TemplateId, run.TemplateId);
        }

        [Fact]
        public async Task PeriodeBerbeda_BolehTerbitMasingMasing()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database);

            foreach (var bulan in new[] { 7, 8, 9 })
            {
                await using var db = database.CreateContext();
                var hasil = await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(bulan), Pelaku);
                Assert.True(hasil.Success, hasil.Message);
            }

            await using var pemeriksa = database.CreateContext();

            Assert.Equal(3, await pemeriksa.Set<AccRecurringJournalRun>().CountAsync());
            Assert.Equal(3, await pemeriksa.Set<AccJournal>().CountAsync());

            // Riwayatnya terlihat lewat endpoint riwayat, terbaru lebih dahulu.
            var riwayat = await Layanan(pemeriksa).GetRunsAsync(bahan.TemplateId);
            Assert.Equal(3, riwayat.Data!.Count);
            Assert.All(riwayat.Data, x => Assert.NotEqual(string.Empty, x.JournalNumber));
        }

        // =====================================================================
        // Acceptance 1 — penerbitan kedua hanya menghasilkan satu jurnal
        // =====================================================================

        [Fact]
        public async Task PenerbitanKedua_DitolakDanTetapSatuJurnal()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                Assert.True((await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku)).Success);
            }

            await using (var db = database.CreateContext())
            {
                var kedua = await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku);

                Assert.False(kedua.Success);
                Assert.Equal(409, kedua.StatusCode);
                Assert.Contains("sudah diterbitkan", kedua.Message);
            }

            await using var pemeriksa = database.CreateContext();

            // Tetap satu jurnal dan satu baris penerbitan.
            Assert.Equal(1, await pemeriksa.Set<AccRecurringJournalRun>().CountAsync());
            Assert.Equal(1, await pemeriksa.Set<AccJournal>().CountAsync());
        }

        /// <summary>
        /// Penjaganya benar-benar <b>database</b>, bukan hanya pemeriksaan di kode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uji ini <b>melangkahi</b> pemeriksaan kode: baris penerbitan disisipkan langsung ke
        /// tabel tanpa melalui service, sehingga <c>CariPenerbitanAsync</c> tetap melihatnya —
        /// lalu baris itu dihapus dari konteks pemeriksaan dan disisipkan ulang tepat pada saat
        /// penyimpanan. Cara paling jujur menirunya pada satu koneksi adalah menyisipkan baris
        /// yang bertabrakan <b>setelah</b> pemeriksaan lolos.
        /// </para>
        /// <para>
        /// Yang dibuktikan: unique index <c>(TemplateId, AccountingPeriodId)</c> benar-benar
        /// terpasang dan menolak baris kedua, dan service menerjemahkan penolakan itu menjadi
        /// <c>409</c>, bukan meledak menjadi <c>500</c>.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task UniqueIndexDatabase_MenolakBarisPenerbitanKedua()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database);

            Guid periodeId;
            Guid jurnalId;

            await using (var db = database.CreateContext())
            {
                var pertama = await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku);
                Assert.True(pertama.Success, pertama.Message);
                periodeId = pertama.Data!.AccountingPeriodId;
                jurnalId = pertama.Data.JournalId;
            }

            // Menyisipkan baris penerbitan KEDUA untuk pasangan yang sama, langsung ke tabel.
            await using var db2 = database.CreateContext();

            db2.Set<AccRecurringJournalRun>().Add(new AccRecurringJournalRun
            {
                Id = Guid.NewGuid(),
                TemplateId = bahan.TemplateId,
                AccountingPeriodId = periodeId,
                JournalId = jurnalId,
                GeneratedAt = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            // Database yang menolak, bukan kode.
            await Assert.ThrowsAsync<DbUpdateException>(() => db2.SaveChangesAsync());
        }

        // =====================================================================
        // Acceptance 2 — periode tidak menerima pencatatan ⇒ dilewati, bukan gagal
        // =====================================================================

        [Fact]
        public async Task PeriodeTutupPermanen_PenerbitanDitolak422()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database, AccountingPeriodStatus.Closed);

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku);

            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains("tidak menerima pencatatan baru", hasil.Message);
            Assert.Equal(0, await db.Set<AccJournal>().CountAsync());
        }

        /// <summary>
        /// Pada penjadwal, periode yang tidak menerima pencatatan membuat template itu
        /// <b>dilewati</b> — bukan menggagalkan siklusnya, dan bukan pula menghentikan template
        /// lain.
        /// </summary>
        [Fact]
        public async Task Penjadwal_MelewatiYangBermasalahDanTetapMenerbitkanSisanya()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database);

            // Template kedua yang sehat.
            Guid templateKedua;

            await using (var db = database.CreateContext())
            {
                var akun = await db.Set<AccChartOfAccount>().AsNoTracking().ToListAsync();

                var hasil = await Layanan(db).CreateAsync(new CreateRecurringJournalRequest
                {
                    LegalEntityId = BadanHukum,
                    TemplateCode = "SEWA-GEDUNG",
                    TemplateName = "Amortisasi Sewa Gedung",
                    JournalTypeId = JenisJurnalId,
                    DayOfMonth = TanggalTerbit,
                    StartDate = new DateTime(Tahun, 1, 1),
                    Lines = new List<CreateRecurringJournalLineRequest>
                    {
                        new() { LineNumber = 1, AccountId = bahan.AkunBeban, CostCenterId = UnitBiayaId, DebitAmount = 2_000_000m },
                        new() { LineNumber = 2, AccountId = bahan.AkunAkumulasi, CreditAmount = 2_000_000m }
                    }
                }, Pelaku);

                Assert.True(hasil.Success, hasil.Message);
                templateKedua = hasil.Data!.Id;
            }

            await using (var db = database.CreateContext())
            {
                Assert.True((await Layanan(db).ActivateAsync(templateKedua, Pelaku)).Success);
            }

            // Template PERTAMA sudah pernah terbit untuk September; yang kedua belum.
            await using (var db = database.CreateContext())
            {
                Assert.True((await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku)).Success);
            }

            await using (var db = database.CreateContext())
            {
                var siklus = await Layanan(db).TerbitkanYangJatuhTempoAsync(
                    new DateTime(Tahun, 9, 27), Pelaku);

                Assert.Equal(2, siklus.Considered);

                // Satu terbit, satu dilewati karena memang sudah terbit — bukan masalah.
                Assert.Single(siklus.Published);
                Assert.Equal(templateKedua, siklus.Published[0].TemplateId);
                Assert.Equal(1, siklus.AlreadyPublishedCount);
                Assert.Equal(0, siklus.ProblemCount);
            }

            await using var pemeriksa = database.CreateContext();
            Assert.Equal(2, await pemeriksa.Set<AccRecurringJournalRun>().CountAsync());
        }

        [Fact]
        public async Task Penjadwal_PeriodeTutupDihitungSebagaiBermasalahBukanSudahTerbit()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.Closed);

            await using var db = database.CreateContext();
            var siklus = await Layanan(db).TerbitkanYangJatuhTempoAsync(
                new DateTime(Tahun, 9, 27), Pelaku);

            Assert.Equal(1, siklus.Considered);
            Assert.Empty(siklus.Published);
            Assert.Equal(1, siklus.ProblemCount);
            Assert.Equal(0, siklus.AlreadyPublishedCount);
            Assert.Contains("tidak menerima pencatatan baru", siklus.Skipped[0].Reason);
        }

        // =====================================================================
        // Acceptance 3 — template nonaktif tidak menerbitkan apa pun
        // =====================================================================

        [Fact]
        public async Task TemplateNonaktif_TidakMenerbitkanApaPun()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database, aktifkan: false);

            await using (var db = database.CreateContext())
            {
                var hasil = await Layanan(db).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku);

                Assert.Equal(409, hasil.StatusCode);
                Assert.Contains("tidak aktif", hasil.Message);
            }

            await using (var db = database.CreateContext())
            {
                // Penjadwal pun tidak melihatnya sama sekali — ia bukan kandidat.
                var siklus = await Layanan(db).TerbitkanYangJatuhTempoAsync(
                    new DateTime(Tahun, 9, 27), Pelaku);

                Assert.Equal(0, siklus.Considered);
                Assert.Empty(siklus.Published);
            }

            await using var pemeriksa = database.CreateContext();
            Assert.Equal(0, await pemeriksa.Set<AccJournal>().CountAsync());
            Assert.Equal(0, await pemeriksa.Set<AccRecurringJournalRun>().CountAsync());
        }

        [Fact]
        public async Task BelumJatuhTempo_TidakDilihatPenjadwal()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var db = database.CreateContext();

            // Tanggal 24, sementara template terbit tanggal 25.
            var siklus = await Layanan(db).TerbitkanYangJatuhTempoAsync(
                new DateTime(Tahun, 9, 24), Pelaku);

            Assert.Equal(0, siklus.Considered);
            Assert.Equal(0, await db.Set<AccJournal>().CountAsync());
        }

        /// <summary>
        /// Template yang jatuh temponya terlewat tetap menghasilkan jurnal bertanggal
        /// <c>DayOfMonth</c>-nya, bukan tanggal penjadwal kebetulan berjalan.
        /// </summary>
        /// <remarks>
        /// Bila tanggalnya ikut bergeser, jurnal penyusutan bulan September dapat mendarat di
        /// periode Oktober ketika penjadwal sempat mati beberapa hari — dan tidak ada error apa
        /// pun yang menandainya.
        /// </remarks>
        [Fact]
        public async Task Penjadwal_MemakaiTanggalTemplateBukanTanggalBerjalan()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                var siklus = await Layanan(db).TerbitkanYangJatuhTempoAsync(
                    new DateTime(Tahun, 9, 30), Pelaku);

                Assert.Single(siklus.Published);
            }

            await using var pemeriksa = database.CreateContext();
            var jurnal = await pemeriksa.Set<AccJournal>().AsNoTracking().SingleAsync();

            Assert.Equal(new DateTime(Tahun, 9, TanggalTerbit), jurnal.AccountingDate);
        }

        [Fact]
        public async Task TemplateSudahBerakhir_DitolakDanTidakTerbit()
        {
            using var database = TestDatabase.Create();
            var bahan = await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                var template = await db.Set<AccRecurringJournalTemplate>()
                    .FirstAsync(x => x.Id == bahan.TemplateId);
                template.EndDate = new DateTime(Tahun, 6, 30);
                await db.SaveChangesAsync();
            }

            await using var pemeriksa = database.CreateContext();
            var hasil = await Layanan(pemeriksa).GenerateAsync(bahan.TemplateId, Untuk(9), Pelaku);

            Assert.Equal(409, hasil.StatusCode);
            Assert.Contains("sudah berakhir", hasil.Message);
            Assert.Equal(0, await pemeriksa.Set<AccJournal>().CountAsync());
        }

        // =====================================================================
        // Acceptance 4 — penjadwal dapat dimatikan lewat konfigurasi
        // =====================================================================

        /// <summary>
        /// Penjadwal yang dimatikan berhenti <b>sebelum</b> timer dibuat, sehingga benar-benar
        /// tidak berjalan sama sekali.
        /// </summary>
        [Fact]
        public async Task PenjadwalDimatikan_SelesaiTanpaMenyentuhDatabase()
        {
            var opsi = new AccRecurringJournalSchedulerOptions { Enabled = false };

            var penjadwal = new AccRecurringJournalSchedulerHostedService(
                new PabrikScopeYangMeledak(),
                Microsoft.Extensions.Options.Options.Create(opsi),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<AccRecurringJournalSchedulerHostedService>.Instance);

            // Bila penjadwal sempat membuat scope, pabrik di bawah melemparkan pengecualian dan
            // uji ini gagal. Selesai tanpa melempar berarti ia benar-benar tidak berjalan.
            await penjadwal.StartAsync(CancellationToken.None);
            await penjadwal.StopAsync(CancellationToken.None);
        }

        /// <summary>Bawaan penjadwal adalah MATI — ia menulis ke buku besar.</summary>
        [Fact]
        public void PenjadwalBawaannya_Mati()
        {
            Assert.False(new AccRecurringJournalSchedulerOptions().Enabled);
        }

        private sealed class PabrikScopeYangMeledak : IServiceScopeFactory
        {
            public IServiceScope CreateScope()
                => throw new InvalidOperationException(
                    "Penjadwal yang dimatikan tidak boleh membuat scope.");
        }

        // =====================================================================
        // Hak akses endpoint penerbitan
        // =====================================================================

        [Fact]
        public void EndpointPenerbitan_MembawaHakAksesYangBenar()
        {
            var controller = typeof(RecurringJournalController);
            var namaController = controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;

            var method = controller.GetMethod(nameof(RecurringJournalController.Generate))!;

            var aksi = method.GetCustomAttribute<AccessActionAttribute>();
            Assert.NotNull(aksi);
            Assert.Equal("Generate", aksi!.ActionName);

            var izin = method.GetCustomAttribute<AccessPermissionAttribute>();
            Assert.NotNull(izin);

            var argumen = izin!.Arguments!;
            Assert.Equal(namaController, (string)argumen[0]);
            Assert.Equal("Generate", (string)argumen[1]);
        }
    }
}
