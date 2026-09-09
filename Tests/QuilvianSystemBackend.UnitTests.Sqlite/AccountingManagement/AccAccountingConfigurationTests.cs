using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-009</c> — endpoint pengaturan akuntansi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang dijaga berkas ini adalah penolakan, bukan keberhasilan. Menunjuk akun laba ditahan
    /// yang keliru <b>tidak menimbulkan error</b> saat tutup tahun — jurnal penutupnya tetap
    /// seimbang, hanya labanya mendarat di akun yang salah, lalu terbawa ke tahun berikutnya
    /// sebagai saldo awal. Satu-satunya kesempatan menangkapnya adalah di sini.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccAccountingConfigurationTests
    {
        private static readonly Guid Pelaku = Guid.Parse("a9a9a9a9-0000-0000-0000-000000000001");
        private static readonly Guid BadanHukum = Guid.Parse("b9b9b9b9-0000-0000-0000-000000000002");
        private static readonly Guid BadanHukumLain = Guid.Parse("c9c9c9c9-0000-0000-0000-000000000003");

        private static async Task SiapkanAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();

            db.Set<MstLegalEntity>().AddRange(
                new MstLegalEntity
                {
                    Id = BadanHukum,
                    LegalEntityCode = "RS-UJI",
                    LegalEntityName = "Rumah Sakit Uji",
                    IsDefault = true,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                },
                new MstLegalEntity
                {
                    Id = BadanHukumLain,
                    LegalEntityCode = "RS-LAIN",
                    LegalEntityName = "Rumah Sakit Lain",
                    IsDefault = false,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                });

            await db.SaveChangesAsync();
        }

        private static AccChartOfAccount Akun(
            string kode,
            string nama,
            AccountType jenis = AccountType.Equity,
            bool menerimaTransaksi = true,
            bool aktif = true,
            Guid? badanHukum = null) => new()
            {
                Id = Guid.NewGuid(),
                LegalEntityId = badanHukum ?? BadanHukum,
                AccountCode = kode,
                AccountName = nama,
                AccountType = jenis,
                NormalBalance = NormalBalance.Credit,
                AccountLevel = 1,
                IsPostable = menerimaTransaksi,
                IsActive = aktif,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            };

        private static async Task<Guid> SimpanAkunAsync(TestDatabase database, AccChartOfAccount akun)
        {
            await using var db = database.CreateContext();
            db.Set<AccChartOfAccount>().Add(akun);
            await db.SaveChangesAsync();
            return akun.Id;
        }

        // =====================================================================
        // Acceptance 1 — akun bukan Equity ditolak 422
        // =====================================================================

        [Theory]
        [InlineData(AccountType.Asset)]
        [InlineData(AccountType.Liability)]
        [InlineData(AccountType.Revenue)]
        [InlineData(AccountType.Expense)]
        public async Task AkunBukanEkuitas_Ditolak422(AccountType jenis)
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var id = await SimpanAkunAsync(database, Akun("1-1001", "Kas Besar", jenis));

            await using var db = database.CreateContext();
            var hasil = await new AccAccountingConfigurationService(db).SetAsync(
                BadanHukum,
                new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = id },
                Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains("Ekuitas", hasil.Message);

            // Nol baris tersimpan.
            Assert.Equal(0, await db.Set<AccAccountingConfiguration>().CountAsync());
        }

        // =====================================================================
        // Acceptance 2 — akun induk ditolak 422
        // =====================================================================

        /// <summary>
        /// Akun induk tidak menerima transaksi (<c>ACC-DEC-022</c>), sehingga jurnal penutup
        /// tahun tidak akan pernah dapat mendarat di sana.
        /// </summary>
        [Fact]
        public async Task AkunIndukYangTidakMenerimaTransaksi_Ditolak422()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var id = await SimpanAkunAsync(database,
                Akun("3-0000", "Ekuitas", AccountType.Equity, menerimaTransaksi: false));

            await using var db = database.CreateContext();
            var hasil = await new AccAccountingConfigurationService(db).SetAsync(
                BadanHukum,
                new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = id },
                Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains("menerima transaksi", hasil.Message);
        }

        // =====================================================================
        // Penolakan tambahan — di luar tulisan kartu, dicatat sebagai delta
        // =====================================================================

        /// <summary>
        /// Akun milik badan hukum lain ditolak.
        /// </summary>
        /// <remarks>
        /// Tanpa penjagaan ini, laba satu badan hukum mendarat di buku besar badan hukum lain —
        /// dan jurnalnya tetap seimbang, sehingga tidak ada yang menandainya (<c>ACC-DEC-037</c>).
        /// </remarks>
        [Fact]
        public async Task AkunMilikBadanHukumLain_Ditolak422()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var id = await SimpanAkunAsync(database,
                Akun("3-2001", "Laba Ditahan", badanHukum: BadanHukumLain));

            await using var db = database.CreateContext();
            var hasil = await new AccAccountingConfigurationService(db).SetAsync(
                BadanHukum,
                new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = id },
                Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains("badan hukum yang sama", hasil.Message);
        }

        [Fact]
        public async Task AkunNonaktif_Ditolak422()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var id = await SimpanAkunAsync(database,
                Akun("3-2001", "Laba Ditahan", aktif: false));

            await using var db = database.CreateContext();
            var hasil = await new AccAccountingConfigurationService(db).SetAsync(
                BadanHukum,
                new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = id },
                Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(422, hasil.StatusCode);
            Assert.Contains("aktif", hasil.Message);
        }

        [Fact]
        public async Task AkunTidakDitemukan_Ditolak422()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var hasil = await new AccAccountingConfigurationService(db).SetAsync(
                BadanHukum,
                new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = Guid.NewGuid() },
                Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(422, hasil.StatusCode);
        }

        // =====================================================================
        // Acceptance 3 — satu badan hukum hanya punya satu pengaturan
        // =====================================================================

        /// <summary>
        /// Menetapkan dua kali menghasilkan <b>satu</b> baris yang diperbarui, bukan dua baris.
        /// </summary>
        [Fact]
        public async Task MenetapkanDuaKali_TetapSatuBaris()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var pertama = await SimpanAkunAsync(database, Akun("3-2001", "Laba Ditahan"));
            var kedua = await SimpanAkunAsync(database, Akun("3-2002", "Laba Ditahan Baru"));

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccAccountingConfigurationService(db).SetAsync(
                    BadanHukum,
                    new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = pertama },
                    Pelaku);

                Assert.True(hasil.Success);
                Assert.True(hasil.Data!.IsConfigured);
            }

            Guid idPengaturan;
            await using (var db = database.CreateContext())
            {
                idPengaturan = (await db.Set<AccAccountingConfiguration>().SingleAsync()).Id;
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccAccountingConfigurationService(db).SetAsync(
                    BadanHukum,
                    new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = kedua },
                    Pelaku);

                Assert.True(hasil.Success);
            }

            await using var pemeriksa = database.CreateContext();
            var seluruh = await pemeriksa.Set<AccAccountingConfiguration>().ToListAsync();

            // Satu baris, dan baris yang SAMA — diperbarui, bukan ditambah.
            Assert.Single(seluruh);
            Assert.Equal(idPengaturan, seluruh[0].Id);
            Assert.Equal(kedua, seluruh[0].RetainedEarningsAccountId);
        }

        // =====================================================================
        // Membaca pengaturan
        // =====================================================================

        /// <summary>
        /// Badan hukum yang belum punya pengaturan menjawab berhasil dengan
        /// <c>IsConfigured = false</c>, bukan gagal.
        /// </summary>
        /// <remarks>
        /// Belum diisi adalah keadaan wajar bagi rumah sakit yang baru memakai modul ini.
        /// Menjadikannya <c>404</c> membuat layar menampilkan kegagalan untuk sesuatu yang normal.
        /// </remarks>
        [Fact]
        public async Task BelumAdaPengaturan_MenjawabBerhasilDenganIsConfiguredSalah()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var hasil = await new AccAccountingConfigurationService(db).GetAsync(BadanHukum);

            Assert.True(hasil.Success);
            Assert.False(hasil.Data!.IsConfigured);
            Assert.Null(hasil.Data.RetainedEarningsAccountId);
        }

        [Fact]
        public async Task SesudahDitetapkan_PengaturanTerbacaLengkap()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var id = await SimpanAkunAsync(database, Akun("3-2001", "Laba Ditahan"));

            await using (var db = database.CreateContext())
            {
                await new AccAccountingConfigurationService(db).SetAsync(
                    BadanHukum,
                    new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = id },
                    Pelaku);
            }

            await using var pemeriksa = database.CreateContext();
            var hasil = await new AccAccountingConfigurationService(pemeriksa).GetAsync(BadanHukum);

            Assert.True(hasil.Data!.IsConfigured);
            Assert.Equal(id, hasil.Data.RetainedEarningsAccountId);
            Assert.Equal("3-2001", hasil.Data.RetainedEarningsAccountCode);
            Assert.Equal("Laba Ditahan", hasil.Data.RetainedEarningsAccountName);
            Assert.Equal(AccountType.Equity, hasil.Data.RetainedEarningsAccountType);
        }

        /// <summary>
        /// Penilaian yang dipakai <c>BE-ACC-P2-010</c> memberi jawaban yang sama dengan endpoint.
        /// </summary>
        [Fact]
        public async Task PenilaianUntukTutupTahun_SamaDenganEndpoint()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using (var kosong = database.CreateContext())
            {
                Assert.Null(await AccAccountingConfigurationService
                    .AmbilAkunLabaDitahanAsync(kosong, BadanHukum));
            }

            var id = await SimpanAkunAsync(database, Akun("3-2001", "Laba Ditahan"));

            await using (var db = database.CreateContext())
            {
                await new AccAccountingConfigurationService(db).SetAsync(
                    BadanHukum,
                    new UpdateAccountingConfigurationRequest { RetainedEarningsAccountId = id },
                    Pelaku);
            }

            await using var pemeriksa = database.CreateContext();
            var akun = await AccAccountingConfigurationService
                .AmbilAkunLabaDitahanAsync(pemeriksa, BadanHukum);

            Assert.NotNull(akun);
            Assert.Equal(id, akun!.Id);
        }

        // =====================================================================
        // Acceptance 4 — kedua endpoint membawa [AccessPermission]
        // =====================================================================

        [Theory]
        [InlineData(nameof(AccountingConfigurationController.Get), "Read")]
        [InlineData(nameof(AccountingConfigurationController.Set), "Update")]
        public void KeduaEndpoint_MembawaHakAksesYangBenar(string namaMethod, string aksiDiharapkan)
        {
            var controller = typeof(AccountingConfigurationController);
            var namaController = controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;

            var method = controller.GetMethod(namaMethod)!;

            Assert.NotNull(method.GetCustomAttribute<AccessActionAttribute>());

            var izin = method.GetCustomAttribute<AccessPermissionAttribute>();
            Assert.NotNull(izin);

            var argumen = izin!.Arguments!;

            // Argumen ke-1 wajib sama persis dengan ControllerName; bila menyimpang hasilnya
            // 403 permanen yang tidak dapat diperbaiki dari layar Akses Role.
            Assert.Equal(namaController, (string)argumen[0]);
            Assert.Equal(aksiDiharapkan, (string)argumen[1]);
        }
    }
}
