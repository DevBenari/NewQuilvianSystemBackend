using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services;
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
    /// Bukti acceptance untuk <c>BE-ACC-P2-007</c> — CRUD template jurnal berulang.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang dijaga berkas ini adalah penolakan saat menyimpan, bukan keberhasilannya.</b>
    /// Template berbeda dari jurnal manual: ia disimpan sekali lalu menerbitkan jurnal
    /// <b>sendiri</b> setiap bulan tanpa ada yang menyusunnya ulang. Template timpang yang lolos
    /// disimpan akan menerbitkan draft timpang setiap bulan, dan masing-masing baru ketahuan
    /// gagal jauh kemudian — saat seseorang mencoba mengajukannya, mungkin berbulan-bulan
    /// setelahnya.
    /// </para>
    /// <para>
    /// Lihat catatan <c>ACC-TD-001</c> pada <c>AccYearEndClosingTests</c> mengenai
    /// <c>PRAGMA ignore_check_constraints</c>: EF menyimpan <c>decimal</c> sebagai TEXT pada
    /// SQLite, sehingga check constraint yang membandingkannya dengan angka mustahil dipenuhi.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccRecurringJournalTemplateTests
    {
        private static readonly Guid Pelaku = Guid.Parse("f1f1f1f1-0000-0000-0000-000000000001");
        private static readonly Guid BadanHukum = Guid.Parse("f2f2f2f2-0000-0000-0000-000000000002");
        private static readonly Guid BadanHukumLain = Guid.Parse("f3f3f3f3-0000-0000-0000-000000000003");
        private static readonly Guid JenisJurnalId = Guid.Parse("f4f4f4f4-0000-0000-0000-000000000004");
        private static readonly Guid UnitBiayaId = Guid.Parse("f5f5f5f5-0000-0000-0000-000000000005");

        // =====================================================================
        // Penyiapan
        // =====================================================================

        private sealed class DaftarAkun
        {
            public AccChartOfAccount BebanSusut { get; init; } = null!;
            public AccChartOfAccount AkumulasiSusut { get; init; } = null!;
            public AccChartOfAccount Kas { get; init; } = null!;
            public AccChartOfAccount Induk { get; init; } = null!;
            public AccChartOfAccount AkunBadanHukumLain { get; init; } = null!;
        }

        private static async Task<DaftarAkun> SiapkanAsync(TestDatabase database)
        {
            await using (var pragma = database.CreateContext())
            {
                await pragma.Database.ExecuteSqlRawAsync("PRAGMA ignore_check_constraints = ON;");
            }

            await using var db = database.CreateContext();

            db.Set<MstLegalEntity>().AddRange(
                Badan(BadanHukum, "RS-UJI", "Rumah Sakit Uji", utama: true),
                Badan(BadanHukumLain, "RS-LAIN", "Rumah Sakit Lain", utama: false));

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

            var daftar = new DaftarAkun
            {
                BebanSusut = Akun("5-2003", "Beban Penyusutan", AccountType.Expense, NormalBalance.Debit),
                AkumulasiSusut = Akun("1-4001", "Akumulasi Penyusutan", AccountType.Asset, NormalBalance.Credit),
                Kas = Akun("1-1001", "Kas Besar", AccountType.Asset, NormalBalance.Debit),
                Induk = Akun("5-2000", "Beban Umum", AccountType.Expense, NormalBalance.Debit, menerimaTransaksi: false),
                AkunBadanHukumLain = Akun("1-1001", "Kas Milik Tetangga", AccountType.Asset, NormalBalance.Debit, badanHukum: BadanHukumLain)
            };

            db.Set<AccChartOfAccount>().AddRange(
                daftar.BebanSusut, daftar.AkumulasiSusut, daftar.Kas,
                daftar.Induk, daftar.AkunBadanHukumLain);

            await db.SaveChangesAsync();

            return daftar;
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
            AccountType jenis,
            NormalBalance saldoNormal,
            bool menerimaTransaksi = true,
            bool aktif = true,
            Guid? badanHukum = null) => new()
            {
                Id = Guid.NewGuid(),
                LegalEntityId = badanHukum ?? BadanHukum,
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

        private static CreateRecurringJournalLineRequest Baris(
            int nomor, Guid akunId, decimal debit, decimal kredit, Guid? unitBiaya = null) => new()
            {
                LineNumber = nomor,
                AccountId = akunId,
                CostCenterId = unitBiaya,
                Description = "Baris uji",
                DebitAmount = debit,
                CreditAmount = kredit
            };

        /// <summary>Template penyusutan yang sah: beban didebit, akumulasi dikredit.</summary>
        private static CreateRecurringJournalRequest Permintaan(
            DaftarAkun akun,
            string kode = "SUSUT-ALKES",
            decimal nominal = 5_000_000m,
            List<CreateRecurringJournalLineRequest>? baris = null) => new()
            {
                LegalEntityId = BadanHukum,
                TemplateCode = kode,
                TemplateName = "Penyusutan Alat Kesehatan",
                JournalTypeId = JenisJurnalId,
                DayOfMonth = 25,
                StartDate = new DateTime(2026, 1, 1),
                Lines = baris ?? new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.BebanSusut.Id, nominal, 0m, UnitBiayaId),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, nominal)
                }
            };

        /// <summary>
        /// Membuat service beserta <c>AccJournalService</c> yang berbagi konteks yang sama.
        /// </summary>
        /// <remarks>
        /// Konteksnya wajib satu dan sama: penerbitan jurnal berulang membuka satu transaction
        /// yang mencakup pembuatan jurnal sekaligus baris penerbitannya, dan dua konteks berbeda
        /// berarti dua transaction yang tidak saling menjaga.
        /// </remarks>
        private static AccRecurringJournalService Layanan(ApplicationDbContext db)
            => new(db, new AccJournalService(db));

        private static async Task<Guid> SimpanTemplateAsync(TestDatabase database, DaftarAkun akun)
        {
            await using var db = database.CreateContext();
            var hasil = await Layanan(db).CreateAsync(Permintaan(akun), Pelaku);
            Assert.True(hasil.Success, hasil.Message);
            return hasil.Data!.Id;
        }

        // =====================================================================
        // Acceptance 1 — template tidak seimbang ditolak 400
        // =====================================================================

        [Fact]
        public async Task TemplateTidakSeimbang_Ditolak400()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.BebanSusut.Id, 5_000_000m, 0m, UnitBiayaId),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, 4_000_000m)
                }),
                Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(400, hasil.StatusCode);
            Assert.Contains("selisih", hasil.Message, StringComparison.OrdinalIgnoreCase);

            // Nol baris tersimpan — bukan template tanpa baris, bukan pula template timpang.
            Assert.Equal(0, await db.Set<AccRecurringJournalTemplate>().CountAsync());
            Assert.Equal(0, await db.Set<AccRecurringJournalTemplateLine>().CountAsync());
        }

        [Fact]
        public async Task BarisKurangDariDua_Ditolak400()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.Kas.Id, 5_000_000m, 0m)
                }),
                Pelaku);

            Assert.Equal(400, hasil.StatusCode);
            Assert.Contains("minimal", hasil.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, await db.Set<AccRecurringJournalTemplate>().CountAsync());
        }

        // =====================================================================
        // Acceptance 2 — baris berakun Expense tanpa cost center ditolak 400
        // =====================================================================

        [Fact]
        public async Task AkunBebanTanpaUnitBiaya_Ditolak400()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    // Sengaja tanpa unit biaya.
                    Baris(1, akun.BebanSusut.Id, 5_000_000m, 0m),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, 5_000_000m)
                }),
                Pelaku);

            Assert.Equal(400, hasil.StatusCode);
            Assert.Contains("unit biaya", hasil.Message, StringComparison.OrdinalIgnoreCase);

            // Nomor barisnya disebut supaya petugas tidak menebak.
            Assert.Contains("Baris ke-1", hasil.Message);
            Assert.Equal(0, await db.Set<AccRecurringJournalTemplate>().CountAsync());
        }

        /// <summary>
        /// Akun <b>bukan</b> beban tidak dipaksa punya unit biaya — kewajibannya diturunkan dari
        /// jenis akun, bukan dipukul rata (<c>ACC-DEC-019</c>).
        /// </summary>
        [Fact]
        public async Task AkunBukanBebanTanpaUnitBiaya_Diterima()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.Kas.Id, 5_000_000m, 0m),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, 5_000_000m)
                }),
                Pelaku);

            Assert.True(hasil.Success, hasil.Message);
        }

        // =====================================================================
        // Acceptance 3 — baris berisi debit DAN kredit sekaligus ditolak 400
        // =====================================================================

        [Theory]
        [InlineData(5_000_000, 5_000_000)]   // keduanya terisi
        [InlineData(0, 0)]                   // keduanya kosong
        public async Task BarisDuaSisiAtauKosong_Ditolak400(int debit, int kredit)
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.Kas.Id, debit, kredit),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, 5_000_000m)
                }),
                Pelaku);

            Assert.Equal(400, hasil.StatusCode);
            Assert.Contains("salah satu saja", hasil.Message);
            Assert.Equal(0, await db.Set<AccRecurringJournalTemplate>().CountAsync());
        }

        [Fact]
        public async Task BarisBernilaiNegatif_Ditolak400DenganPetunjuk()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();

            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.Kas.Id, -5_000_000m, 0m),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, -5_000_000m)
                }),
                Pelaku);

            Assert.Equal(400, hasil.StatusCode);

            // Nilai negatif diperiksa LEBIH DAHULU daripada aturan satu sisi: keduanya menolak,
            // tetapi hanya pesan ini yang memberi tahu cara memperbaikinya.
            Assert.Contains("sisi sebaliknya", hasil.Message);
        }

        // =====================================================================
        // Acceptance 4 — template baru berstatus tidak aktif
        // =====================================================================

        [Fact]
        public async Task TemplateBaru_LahirTidakAktif()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).CreateAsync(Permintaan(akun), Pelaku);

            Assert.True(hasil.Success, hasil.Message);
            Assert.Equal(201, hasil.StatusCode);
            Assert.False(hasil.Data!.IsActive);

            // Dan benar-benar tersimpan tidak aktif, bukan hanya pada respons.
            await using var pemeriksa = database.CreateContext();
            var tersimpan = await pemeriksa.Set<AccRecurringJournalTemplate>().SingleAsync();
            Assert.False(tersimpan.IsActive);
        }

        /// <summary>
        /// Permintaan penambahan <b>tidak punya</b> bidang <c>IsActive</c>, sehingga template
        /// tidak mungkin lahir aktif walaupun pemanggilnya menginginkannya.
        /// </summary>
        /// <remarks>
        /// Ini penjaga bentuk kontrak, bukan penjaga perilaku. Sekali bidang itu ada, template
        /// dapat lahir langsung aktif dan menerbitkan jurnal pada siklus penjadwal berikutnya —
        /// sebelum satu orang pun sempat memeriksa barisnya.
        /// </remarks>
        [Theory]
        [InlineData(typeof(CreateRecurringJournalRequest))]
        [InlineData(typeof(UpdateRecurringJournalRequest))]
        public void PermintaanSimpan_TidakPunyaBidangIsActive(Type tipe)
        {
            Assert.Null(tipe.GetProperty("IsActive"));
        }

        // =====================================================================
        // Pengaktifan dan penonaktifan
        // =====================================================================

        [Fact]
        public async Task Aktifkan_LaluNonaktifkan_BerjalanDanTersimpan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);
            var id = await SimpanTemplateAsync(database, akun);

            await using (var db = database.CreateContext())
            {
                var hasil = await Layanan(db).ActivateAsync(id, Pelaku);
                Assert.True(hasil.Success, hasil.Message);
                Assert.True(hasil.Data!.IsActive);
            }

            await using (var db = database.CreateContext())
            {
                // Mengaktifkan yang sudah aktif ditolak, bukan didiamkan.
                var lagi = await Layanan(db).ActivateAsync(id, Pelaku);
                Assert.Equal(409, lagi.StatusCode);
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await Layanan(db).DeactivateAsync(id, Pelaku);
                Assert.True(hasil.Success, hasil.Message);
                Assert.False(hasil.Data!.IsActive);
            }

            await using var pemeriksa = database.CreateContext();
            Assert.False((await pemeriksa.Set<AccRecurringJournalTemplate>().SingleAsync()).IsActive);
        }

        /// <summary>
        /// Template yang akunnya dinonaktifkan <b>sesudah</b> disimpan ditolak saat diaktifkan.
        /// </summary>
        /// <remarks>
        /// Tanpa pemeriksaan ulang ini, template itu tetap aktif dan gagal menerbitkan setiap
        /// bulan — di dalam penjadwal, tempat tidak seorang pun melihat kegagalannya.
        /// </remarks>
        [Fact]
        public async Task AkunDinonaktifkanSesudahDisimpan_PengaktifanDitolak409()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);
            var id = await SimpanTemplateAsync(database, akun);

            await using (var db = database.CreateContext())
            {
                var target = await db.Set<AccChartOfAccount>().FirstAsync(x => x.Id == akun.BebanSusut.Id);
                target.IsActive = false;
                await db.SaveChangesAsync();
            }

            await using var pemeriksa = database.CreateContext();
            var hasil = await Layanan(pemeriksa).ActivateAsync(id, Pelaku);

            Assert.Equal(409, hasil.StatusCode);
            Assert.Contains("tidak aktif", hasil.Message);
            Assert.False((await pemeriksa.Set<AccRecurringJournalTemplate>().SingleAsync()).IsActive);
        }

        // =====================================================================
        // Penyuntingan — jebakan EF yang dicatat kartu roadmap
        // =====================================================================

        /// <summary>
        /// Mengganti seluruh baris menghasilkan baris yang benar-benar <b>baru</b>, bukan baris
        /// lama yang tertimpa.
        /// </summary>
        /// <remarks>
        /// Inilah jebakan yang disebut kartu roadmap: <c>RemoveRange</c> lalu menambah lewat
        /// navigation yang terlacak membuat EF mencocokkan baris baru dengan baris lama dan
        /// mengirim <c>UPDATE</c> alih-alih <c>INSERT</c>. Yang membuktikan penangkalnya bekerja
        /// adalah <c>Id</c> baris yang berganti — bukan sekadar jumlah barisnya yang benar.
        /// </remarks>
        [Fact]
        public async Task GantiSeluruhBaris_MenghasilkanBarisBaruBukanTimpaan()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);
            var id = await SimpanTemplateAsync(database, akun);

            List<Guid> idBarisLama;

            await using (var db = database.CreateContext())
            {
                idBarisLama = await db.Set<AccRecurringJournalTemplateLine>()
                    .Where(x => x.TemplateId == id).Select(x => x.Id).ToListAsync();
            }

            Assert.Equal(2, idBarisLama.Count);

            await using (var db = database.CreateContext())
            {
                var hasil = await Layanan(db).UpdateAsync(
                    id,
                    new UpdateRecurringJournalRequest
                    {
                        TemplateCode = "SUSUT-ALKES",
                        TemplateName = "Penyusutan Alat Kesehatan (revisi)",
                        JournalTypeId = JenisJurnalId,
                        DayOfMonth = 10,
                        StartDate = new DateTime(2026, 1, 1),
                        Lines = new List<CreateRecurringJournalLineRequest>
                        {
                            // Nomor baris sengaja SAMA — inilah yang memancing EF mencocokkan.
                            Baris(1, akun.BebanSusut.Id, 7_500_000m, 0m, UnitBiayaId),
                            Baris(2, akun.AkumulasiSusut.Id, 0m, 7_500_000m)
                        }
                    },
                    Pelaku);

                Assert.True(hasil.Success, hasil.Message);
                Assert.Equal(7_500_000m, hasil.Data!.TotalDebit);
                Assert.Equal(10, hasil.Data.DayOfMonth);
            }

            await using var pemeriksa = database.CreateContext();

            var barisBaru = await pemeriksa.Set<AccRecurringJournalTemplateLine>()
                .Where(x => x.TemplateId == id && !x.IsDelete)
                .ToListAsync();

            // Tetap dua baris, dan seluruh Id-nya berbeda dari sebelumnya.
            Assert.Equal(2, barisBaru.Count);
            Assert.Empty(barisBaru.Select(x => x.Id).Intersect(idBarisLama));
            Assert.All(barisBaru, x => Assert.Equal(7_500_000m, x.DebitAmount + x.CreditAmount));
        }

        /// <summary>
        /// Penyuntingan yang ditolak <b>tidak</b> menghapus baris yang lama.
        /// </summary>
        /// <remarks>
        /// Penghapusan disimpan lebih dahulu di dalam transaction; bila penambahan gagal
        /// sesudahnya, transaction membatalkan keduanya. Tanpa transaction, template akan
        /// tertinggal tanpa baris sama sekali — dan template tanpa baris adalah template yang
        /// menerbitkan jurnal kosong setiap bulan.
        /// </remarks>
        [Fact]
        public async Task PenyuntinganDitolak_BarisLamaTetapUtuh()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);
            var id = await SimpanTemplateAsync(database, akun);

            await using (var db = database.CreateContext())
            {
                var hasil = await Layanan(db).UpdateAsync(
                    id,
                    new UpdateRecurringJournalRequest
                    {
                        TemplateCode = "SUSUT-ALKES",
                        TemplateName = "Penyusutan Alat Kesehatan",
                        JournalTypeId = JenisJurnalId,
                        DayOfMonth = 25,
                        StartDate = new DateTime(2026, 1, 1),
                        Lines = new List<CreateRecurringJournalLineRequest>
                        {
                            Baris(1, akun.BebanSusut.Id, 9_000_000m, 0m, UnitBiayaId),
                            Baris(2, akun.AkumulasiSusut.Id, 0m, 1_000_000m)
                        }
                    },
                    Pelaku);

                Assert.Equal(400, hasil.StatusCode);
            }

            await using var pemeriksa = database.CreateContext();

            var baris = await pemeriksa.Set<AccRecurringJournalTemplateLine>()
                .Where(x => x.TemplateId == id && !x.IsDelete).ToListAsync();

            Assert.Equal(2, baris.Count);
            Assert.Equal(5_000_000m, baris.Sum(x => x.DebitAmount));
            Assert.Equal(5_000_000m, baris.Sum(x => x.CreditAmount));
        }

        // =====================================================================
        // Penolakan lain yang menutup kesalahan diam
        // =====================================================================

        [Fact]
        public async Task KodeTemplateKembar_Ditolak409()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);
            await SimpanTemplateAsync(database, akun);

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, kode: "susut-alkes"), Pelaku);

            Assert.Equal(409, hasil.StatusCode);
            Assert.Contains("sudah dipakai", hasil.Message);
            Assert.Equal(1, await db.Set<AccRecurringJournalTemplate>().CountAsync());
        }

        [Fact]
        public async Task BarisBerakunInduk_Ditolak409()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.Induk.Id, 5_000_000m, 0m, UnitBiayaId),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, 5_000_000m)
                }),
                Pelaku);

            Assert.Equal(409, hasil.StatusCode);
            Assert.Contains("akun induk", hasil.Message);
        }

        /// <summary>
        /// Baris berakun badan hukum lain ditolak — kesalahan yang <b>tidak menimbulkan error</b>
        /// bila lolos: jurnalnya tetap seimbang, hanya angkanya mendarat di buku besar orang lain
        /// (<c>ACC-DEC-037</c>), dan berulang setiap bulan.
        /// </summary>
        [Fact]
        public async Task BarisBerakunBadanHukumLain_Ditolak409()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).CreateAsync(
                Permintaan(akun, baris: new List<CreateRecurringJournalLineRequest>
                {
                    Baris(1, akun.AkunBadanHukumLain.Id, 5_000_000m, 0m),
                    Baris(2, akun.AkumulasiSusut.Id, 0m, 5_000_000m)
                }),
                Pelaku);

            Assert.Equal(409, hasil.StatusCode);
            Assert.Contains("bukan milik badan hukum", hasil.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(29)]
        [InlineData(31)]
        public async Task TanggalTerbitDiLuar1Sampai28_Ditolak400(int tanggal)
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            var permintaan = Permintaan(akun);
            permintaan.DayOfMonth = tanggal;

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).CreateAsync(permintaan, Pelaku);

            Assert.Equal(400, hasil.StatusCode);
            Assert.Contains("Februari", hasil.Message);
        }

        [Fact]
        public async Task TanggalBerakhirLebihAwalDariMulai_Ditolak400()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);

            var permintaan = Permintaan(akun);
            permintaan.EndDate = new DateTime(2025, 12, 31);

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).CreateAsync(permintaan, Pelaku);

            Assert.Equal(400, hasil.StatusCode);
            Assert.Contains("lebih awal", hasil.Message);
        }

        // =====================================================================
        // Baca
        // =====================================================================

        [Fact]
        public async Task DaftarDanRincian_MembawaRingkasanYangBenar()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);
            var id = await SimpanTemplateAsync(database, akun);

            await using var db = database.CreateContext();
            var layanan = Layanan(db);

            var daftar = await layanan.GetPagedAsync(new RecurringJournalPagedQuery());

            Assert.True(daftar.Success);
            var ringkas = Assert.Single(daftar.Data!.Items);
            Assert.Equal("SUSUT-ALKES", ringkas.TemplateCode);
            Assert.Equal(2, ringkas.LineCount);
            Assert.Equal(5_000_000m, ringkas.TotalAmount);
            Assert.Equal(0, ringkas.RunCount);
            Assert.Equal("JP", ringkas.JournalTypeCode);

            var rinci = await layanan.GetByIdAsync(id);

            Assert.True(rinci.Success);
            Assert.True(rinci.Data!.IsBalanced);
            Assert.Equal(2, rinci.Data.Lines.Count);
            Assert.Equal("5-2003", rinci.Data.Lines[0].AccountCode);
            Assert.Equal("Umum", rinci.Data.Lines[0].CostCenterName);
            Assert.Null(rinci.Data.Lines[1].CostCenterName);
        }

        [Fact]
        public async Task RiwayatPenerbitan_KosongSebelumPernahTerbit()
        {
            using var database = TestDatabase.Create();
            var akun = await SiapkanAsync(database);
            var id = await SimpanTemplateAsync(database, akun);

            await using var db = database.CreateContext();
            var hasil = await Layanan(db).GetRunsAsync(id);

            Assert.True(hasil.Success);
            Assert.Empty(hasil.Data!);
            Assert.Contains("belum pernah", hasil.Message);
        }

        [Fact]
        public async Task TemplateTidakDitemukan_Menjawab404()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var layanan = Layanan(db);

            Assert.Equal(404, (await layanan.GetByIdAsync(Guid.NewGuid())).StatusCode);
            Assert.Equal(404, (await layanan.GetRunsAsync(Guid.NewGuid())).StatusCode);
            Assert.Equal(404, (await layanan.ActivateAsync(Guid.NewGuid(), Pelaku)).StatusCode);
        }

        // =====================================================================
        // Acceptance 5 — tujuh endpoint membawa [AccessPermission]
        // =====================================================================

        [Theory]
        [InlineData(nameof(RecurringJournalController.GetPaged), "Read")]
        [InlineData(nameof(RecurringJournalController.GetById), "Read")]
        [InlineData(nameof(RecurringJournalController.GetRuns), "Read")]
        [InlineData(nameof(RecurringJournalController.Create), "Create")]
        [InlineData(nameof(RecurringJournalController.Update), "Update")]
        [InlineData(nameof(RecurringJournalController.Activate), "Activate")]
        [InlineData(nameof(RecurringJournalController.Deactivate), "Activate")]
        public void TujuhEndpoint_MembawaHakAksesYangBenar(string namaMethod, string aksiDiharapkan)
        {
            var controller = typeof(RecurringJournalController);
            var namaController = controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;

            Assert.Equal("RecurringJournal", namaController);

            var method = controller.GetMethod(namaMethod)!;

            var aksi = method.GetCustomAttribute<AccessActionAttribute>();
            Assert.NotNull(aksi);

            var izin = method.GetCustomAttribute<AccessPermissionAttribute>();
            Assert.NotNull(izin);

            var argumen = izin!.Arguments!;

            // Argumen ke-1 wajib sama persis dengan ControllerName; bila menyimpang hasilnya 403
            // permanen yang tidak dapat diperbaiki dari layar Akses Role.
            Assert.Equal(namaController, (string)argumen[0]);
            Assert.Equal(aksiDiharapkan, (string)argumen[1]);
            Assert.Equal(aksi!.ActionName, (string)argumen[1]);
        }

        /// <summary>
        /// Tujuh endpoint <c>BE-ACC-P2-007</c>, ditambah satu endpoint penerbitan milik
        /// <c>BE-ACC-P2-008</c> — delapan seluruhnya, dan setiap satunya membawa
        /// <c>[AccessPermission]</c>.
        /// </summary>
        /// <remarks>
        /// Angkanya dipatok supaya endpoint yang ditambahkan kelak tanpa metadata hak akses
        /// tertangkap di sini, bukan baru ketahuan sebagai layar yang tidak dapat dicentang di
        /// Akses Role.
        /// </remarks>
        [Fact]
        public void ControllerMemiliki_DelapanEndpointBerhakAkses()
        {
            var endpoint = typeof(RecurringJournalController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<AccessPermissionAttribute>() is not null)
                .ToList();

            Assert.Equal(8, endpoint.Count);

            // Setiap endpoint juga wajib punya [AccessAction], atau kemampuannya tidak akan
            // pernah muncul di layar Akses Role untuk dicentang.
            Assert.All(endpoint, m => Assert.NotNull(m.GetCustomAttribute<AccessActionAttribute>()));
        }
    }
}
