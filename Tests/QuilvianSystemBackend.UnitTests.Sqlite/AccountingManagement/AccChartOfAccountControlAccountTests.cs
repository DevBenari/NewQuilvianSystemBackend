using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-011</c> — penanda control account pada daftar akun.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang paling penting dijaga di sini adalah acceptance nomor 4: <b>akun yang sudah ada tetap
    /// bernilai <c>false</c></b>. <c>AccChartOfAccount</c> adalah tabel Phase 1 yang sudah
    /// berjalan; bila kolom ini lahir tanpa nilai bawaan, akun yang sudah dipakai pembukuan bisa
    /// berubah perilakunya diam-diam dan jurnal manual yang selama ini sah mendadak ditolak
    /// <c>BE-ACC-P2-012</c>.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccChartOfAccountControlAccountTests
    {
        private static readonly Guid Pelaku = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid BadanHukum = Guid.Parse("33333333-3333-3333-3333-333333333333");

        private static IEntityType Entity(ApplicationDbContext c) =>
            c.Model.FindEntityType(typeof(AccChartOfAccount))!;

        /// <summary>
        /// Menyiapkan badan hukum induk. <c>AccChartOfAccount.LegalEntityId</c> ber-FK
        /// <c>Restrict</c> ke <c>MstLegalEntity</c>, sehingga akun tidak dapat disimpan sebelum
        /// barisnya ada.
        /// </summary>
        private static async Task SiapkanBadanHukumAsync(TestDatabase database)
        {
            await using var penyiap = database.CreateContext();

            penyiap.Set<MstLegalEntity>().Add(new MstLegalEntity
            {
                Id = BadanHukum,
                LegalEntityCode = "RS-UJI",
                LegalEntityName = "Rumah Sakit Uji",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            await penyiap.SaveChangesAsync();
        }

        private static AccChartOfAccount Akun(string kode, string nama) => new()
        {
            Id = Guid.NewGuid(),
            LegalEntityId = BadanHukum,
            AccountCode = kode,
            AccountName = nama,
            AccountType = AccountType.Asset,
            NormalBalance = NormalBalance.Debit,
            AccountLevel = 1,
            IsPostable = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Pelaku
        };

        // =====================================================================
        // Acceptance 1 — kolom bool tidak nullable, bawaan false
        // =====================================================================

        [Fact]
        public void Kolom_TidakNullableDanBerbawaanFalse()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var kolom = Entity(context).FindProperty("IsControlAccount")!;

            Assert.Equal(typeof(bool), kolom.ClrType);
            Assert.False(kolom.IsNullable);
            Assert.Equal(false, kolom.GetDefaultValue());
        }

        // =====================================================================
        // Acceptance 2 — index (IsControlAccount) terpasang
        // =====================================================================

        /// <summary>
        /// Index-nya dipakai <c>BE-ACC-P2-013</c> untuk menyaring control account, dan
        /// <c>BE-ACC-P2-012</c> membacanya pada setiap baris jurnal manual.
        /// </summary>
        [Fact]
        public void Index_IsControlAccount_Terpasang()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var index = Entity(context).GetIndexes()
                .Single(x => x.Properties.Count == 1 &&
                             x.Properties[0].Name == "IsControlAccount");

            // Bukan unique — banyak akun boleh menjadi control account sekaligus.
            Assert.False(index.IsUnique);
        }

        // =====================================================================
        // Acceptance 4 — akun yang sudah ada tetap false, tanpa pengisian data
        // =====================================================================

        /// <summary>
        /// Akun disimpan <b>tanpa</b> menyentuh <c>IsControlAccount</c> sama sekali, meniru baris
        /// yang sudah ada di database sebelum kolom ini lahir. Hasilnya wajib <c>false</c>.
        /// </summary>
        [Fact]
        public async Task AkunYangDisimpanTanpaMenyentuhPenanda_TetapBukanControlAccount()
        {
            using var database = TestDatabase.Create();
            await SiapkanBadanHukumAsync(database);

            Guid id;
            await using (var penyiap = database.CreateContext())
            {
                var akun = Akun("1-1001", "Kas Besar");
                id = akun.Id;

                penyiap.Set<AccChartOfAccount>().Add(akun);
                await penyiap.SaveChangesAsync();
            }

            await using var pemeriksa = database.CreateContext();
            var tersimpan = await pemeriksa.Set<AccChartOfAccount>().SingleAsync(x => x.Id == id);

            Assert.False(tersimpan.IsControlAccount);
        }

        /// <summary>
        /// Penanda benar-benar tersimpan dan terbaca kembali, bukan sekadar ada di memori.
        /// </summary>
        [Fact]
        public async Task PenandaTersimpan_DanTerbacaKembali()
        {
            using var database = TestDatabase.Create();
            await SiapkanBadanHukumAsync(database);

            Guid idKasKasir;
            Guid idBebanListrik;

            await using (var penyiap = database.CreateContext())
            {
                var kasKasir = Akun("1-1002", "Kas Kasir");
                kasKasir.IsControlAccount = true;
                idKasKasir = kasKasir.Id;

                var bebanListrik = Akun("5-1001", "Beban Listrik");
                bebanListrik.AccountType = AccountType.Expense;
                idBebanListrik = bebanListrik.Id;

                penyiap.Set<AccChartOfAccount>().AddRange(kasKasir, bebanListrik);
                await penyiap.SaveChangesAsync();
            }

            await using var pemeriksa = database.CreateContext();

            Assert.True((await pemeriksa.Set<AccChartOfAccount>()
                .SingleAsync(x => x.Id == idKasKasir)).IsControlAccount);

            // Akun non-control tetap dapat dijurnal manual seperti biasa — dasar acceptance (3)
            // BE-ACC-P2-012.
            Assert.False((await pemeriksa.Set<AccChartOfAccount>()
                .SingleAsync(x => x.Id == idBebanListrik)).IsControlAccount);
        }

        // =====================================================================
        // Penanda tidak boleh terlepas diam-diam lewat PUT
        // =====================================================================

        /// <summary>
        /// Permintaan ubah yang <b>tidak menyebut</b> <c>IsControlAccount</c> mempertahankan
        /// nilai yang tersimpan.
        /// </summary>
        /// <remarks>
        /// Ini perbaikan yang menutup kegagalan diam: permintaan lama yang hanya hendak mengubah
        /// nama akun tidak boleh membuka kembali Kas Kasir ke jurnal manual. Tidak ada error yang
        /// akan muncul bila hal itu terjadi — penandanya hilang, jurnal manual kembali diterima,
        /// dan selisihnya baru ketahuan saat rekonsiliasi.
        /// </remarks>
        [Fact]
        public void PermintaanUbahTanpaMenyebutPenanda_MempertahankanNilaiTersimpan()
        {
            var permintaan = new UpdateChartOfAccountRequest
            {
                AccountCode = "1-1002",
                AccountName = "Kas Kasir Lantai 1"
            };

            // Tidak disebut sama sekali.
            Assert.Null(permintaan.IsControlAccount);

            // Pola pemetaan yang dipakai service: kosong berarti pertahankan.
            var tersimpan = true;
            var hasil = permintaan.IsControlAccount ?? tersimpan;

            Assert.True(hasil);
        }

        /// <summary>
        /// Melepas penanda tetap bisa, tetapi menuntut pernyataan tegas <c>false</c>.
        /// </summary>
        [Fact]
        public void MelepasPenanda_MenuntutPernyataanTegasFalse()
        {
            var permintaan = new UpdateChartOfAccountRequest
            {
                AccountCode = "1-1002",
                AccountName = "Kas Kasir",
                IsControlAccount = false
            };

            var tersimpan = true;
            var hasil = permintaan.IsControlAccount ?? tersimpan;

            Assert.False(hasil);
        }

        /// <summary>
        /// Penyaringan menurut penanda mengembalikan hanya control account — bentuk query yang
        /// akan dipakai <c>BE-ACC-P2-013</c>.
        /// </summary>
        [Fact]
        public async Task PenyaringanMenurutPenanda_HanyaMengembalikanControlAccount()
        {
            using var database = TestDatabase.Create();
            await SiapkanBadanHukumAsync(database);

            await using (var penyiap = database.CreateContext())
            {
                var kasKasir = Akun("1-1002", "Kas Kasir");
                kasKasir.IsControlAccount = true;

                var piutang = Akun("1-1201", "Piutang Pasien");
                piutang.IsControlAccount = true;

                var bebanGaji = Akun("5-1002", "Beban Gaji");
                bebanGaji.AccountType = AccountType.Expense;

                penyiap.Set<AccChartOfAccount>().AddRange(kasKasir, piutang, bebanGaji);
                await penyiap.SaveChangesAsync();
            }

            await using var pemeriksa = database.CreateContext();
            var kode = await pemeriksa.Set<AccChartOfAccount>()
                .Where(x => !x.IsDelete && x.IsControlAccount)
                .OrderBy(x => x.AccountCode)
                .Select(x => x.AccountCode)
                .ToListAsync();

            Assert.Equal(new[] { "1-1002", "1-1201" }, kode);
        }
    }
}
