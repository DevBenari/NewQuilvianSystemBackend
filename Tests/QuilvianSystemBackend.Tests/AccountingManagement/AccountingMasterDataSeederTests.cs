using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Seeders;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance criteria 3 `BE-ACC-006` — empat jenis jurnal terisi, dengan `JB` dan
    /// `SA` bertanda sistem.
    ///
    /// Seluruh uji di sini memakai basis data SQLite di dalam memori lewat
    /// <see cref="TestDatabase"/>, jadi **tidak** menyentuh basis data pengembangan yang dipakai
    /// bersama satu tim.
    /// </summary>
    public class AccountingMasterDataSeederTests
    {
        private static readonly Guid ActorUserId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        /// <summary>
        /// Acceptance criteria 3 — keempat baris terisi persis seperti
        /// `02-backend-architecture.md` bagian 9.1, kolom demi kolom.
        /// </summary>
        [Fact]
        public async Task Seeder_MengisiEmpatJenisJurnalSesuaiArsitekturBagian91()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            await using ApplicationDbContext db = basisUji.CreateContext();

            AccountingMasterDataSeedResult hasil =
                await AccountingMasterDataSeeder.SeedAsync(db, ActorUserId);

            Assert.Equal(4, hasil.JournalTypeInserted);
            Assert.Equal(0, hasil.JournalTypeSkipped);
            Assert.Null(hasil.JournalTypeSkippedReason);

            List<AccJournalType> jenis = await db.Set<AccJournalType>()
                .OrderBy(x => x.JournalTypeCode)
                .ToListAsync();

            Assert.Equal(4, jenis.Count);
            Assert.Equal(
                new[] { "JB", "JP", "JU", "SA" },
                jenis.Select(x => x.JournalTypeCode));

            (string kode, string nama, string awalan, bool sistem)[] diharapkan =
            {
                ("JU", "Jurnal Umum", "JU", false),
                ("JP", "Jurnal Penyesuaian", "JP", false),
                ("JB", "Jurnal Pembalik", "JB", true),
                ("SA", "Saldo Awal", "SA", true)
            };

            foreach ((string kode, string nama, string awalan, bool sistem) in diharapkan)
            {
                AccJournalType baris = jenis.Single(x => x.JournalTypeCode == kode);

                Assert.Equal(nama, baris.JournalTypeName);
                Assert.Equal(awalan, baris.NumberPrefix);
                Assert.Equal(sistem, baris.IsSystemType);
                Assert.True(baris.IsActive);
                Assert.False(baris.IsDelete);
                Assert.Equal(ActorUserId, baris.CreateBy);
            }
        }

        /// <summary>
        /// Acceptance criteria 3, bagian yang paling mudah tertukar — `JB` dan `SA` bertanda
        /// sistem, `JU` dan `JP` tidak, dan keempatnya tetap menuntut persetujuan.
        ///
        /// Tanda sistem bukan penanda kosmetik: `BE-ACC-008` memakainya untuk menolak
        /// perubahan kode dan awalan nomor. Bila `JU` ikut bertanda sistem, pemilik proses
        /// kehilangan kendali atas jenis jurnal yang justru paling sering disesuaikan.
        /// </summary>
        [Fact]
        public async Task SeluruhJenisJurnal_MenuntutPersetujuanDanHanyaJbSaBertandaSistem()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            await using ApplicationDbContext db = basisUji.CreateContext();

            await AccountingMasterDataSeeder.SeedAsync(db, ActorUserId);

            List<AccJournalType> jenis = await db.Set<AccJournalType>().ToListAsync();

            // ACC-DEC-010 — jurnal manual selalu melewati pemeriksaan orang kedua.
            Assert.All(jenis, x => Assert.True(x.RequiresApproval));

            Assert.Equal(
                new[] { "JB", "SA" },
                jenis.Where(x => x.IsSystemType)
                    .Select(x => x.JournalTypeCode)
                    .OrderBy(kode => kode, StringComparer.Ordinal));
        }

        /// <summary>
        /// Menjalankan seeder dua kali tidak menghasilkan data ganda. Ini yang membuatnya aman
        /// dipanggil berulang, apa pun kelak yang menjadi pemanggilnya.
        /// </summary>
        [Fact]
        public async Task Seeder_DijalankanDuaKali_TidakMenghasilkanDataGanda()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            await using ApplicationDbContext db = basisUji.CreateContext();

            AccountingMasterDataSeedResult pertama =
                await AccountingMasterDataSeeder.SeedAsync(db, ActorUserId);
            AccountingMasterDataSeedResult kedua =
                await AccountingMasterDataSeeder.SeedAsync(db, ActorUserId);

            Assert.Equal(4, pertama.JournalTypeInserted);
            Assert.Equal(0, kedua.JournalTypeInserted);
            Assert.Equal(4, kedua.JournalTypeSkipped);

            Assert.Equal(4, await db.Set<AccJournalType>().CountAsync());
        }

        /// <summary>
        /// Baris yang sudah tersimpan tidak pernah ditimpa, walaupun isinya berbeda dari
        /// definisi seeder.
        ///
        /// Ini janji utama seeder master: admin yang mengganti nama "Jurnal Umum" menjadi
        /// sebutan yang dipakai rumah sakitnya tidak kehilangan perubahannya setiap kali
        /// aplikasi dinyalakan ulang.
        /// </summary>
        [Fact]
        public async Task Seeder_TidakMenimpaBarisYangSudahDisesuaikanAdmin()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            await using ApplicationDbContext db = basisUji.CreateContext();

            db.Set<AccJournalType>().Add(new AccJournalType
            {
                Id = Guid.NewGuid(),
                JournalTypeCode = "JU",
                JournalTypeName = "Jurnal Umum Rumah Sakit",
                NumberPrefix = "JUM",
                RequiresApproval = true,
                IsSystemType = false,
                IsActive = true
            });
            await db.SaveChangesAsync();

            AccountingMasterDataSeedResult hasil =
                await AccountingMasterDataSeeder.SeedAsync(db, ActorUserId);

            // Tiga sisanya tetap ditambahkan; hanya JU yang dilewati.
            Assert.Equal(3, hasil.JournalTypeInserted);
            Assert.Equal(1, hasil.JournalTypeSkipped);

            AccJournalType ju = await db.Set<AccJournalType>()
                .SingleAsync(x => x.JournalTypeCode == "JU");

            Assert.Equal("Jurnal Umum Rumah Sakit", ju.JournalTypeName);
            Assert.Equal("JUM", ju.NumberPrefix);
        }

        /// <summary>
        /// Master yang sudah diisi sumber lain tidak digabung dengan versi seeder.
        ///
        /// Alasannya khas modul ini: `NumberPrefix` menjadi awalan nomor jurnal, sehingga dua
        /// set jenis jurnal berarti dua skema penomoran berjalan bersamaan di atas satu buku
        /// besar. Seeder berhenti dan menyebutkan alasannya, bukan menambah baris.
        /// </summary>
        [Fact]
        public async Task Seeder_BerhentiBilaMasterSudahDiisiSumberLain()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            await using ApplicationDbContext db = basisUji.CreateContext();

            db.Set<AccJournalType>().Add(new AccJournalType
            {
                Id = Guid.NewGuid(),
                JournalTypeCode = "GJ",
                JournalTypeName = "General Journal",
                NumberPrefix = "GJ",
                RequiresApproval = true,
                IsSystemType = false,
                IsActive = true
            });
            await db.SaveChangesAsync();

            AccountingMasterDataSeedResult hasil =
                await AccountingMasterDataSeeder.SeedAsync(db, ActorUserId);

            Assert.Equal(0, hasil.JournalTypeInserted);
            Assert.NotNull(hasil.JournalTypeSkippedReason);

            // Isi tabel tidak berubah sama sekali.
            Assert.Equal("GJ", (await db.Set<AccJournalType>().SingleAsync()).JournalTypeCode);
        }

        /// <summary>
        /// Seeder tidak menyentuh master yang bukan miliknya.
        ///
        /// `AccChartOfAccount` adalah kebijakan akuntansi rumah sakit dan wajib disusun pemilik
        /// proses (`02-backend-architecture.md` bagian 9.3), sedangkan `AccAccountingPeriod`
        /// dibangkitkan lewat `POST /generate` pada `BE-ACC-009` (bagian 9.2). Menebak keduanya
        /// menghasilkan master palsu yang terlanjur dipakai pembukuan, dan test ini menjaga
        /// batas itu tetap sengaja.
        /// </summary>
        [Fact]
        public async Task Seeder_TidakMengisiDaftarAkunMaupunPeriode()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            await using ApplicationDbContext db = basisUji.CreateContext();

            await AccountingMasterDataSeeder.SeedAsync(db, ActorUserId);

            Assert.Empty(db.AccChartOfAccounts);
            Assert.Empty(db.AccAccountingPeriods);
            Assert.Empty(db.AccJournals);
            Assert.Empty(db.AccNumberSeries);
        }
    }
}
