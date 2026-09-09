using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Seeders;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-003</c> — jenis jurnal <c>JT</c> masuk ke master, dan
    /// seeder tetap idempoten sesudah baris kelima itu ditambahkan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang dijaga berkas ini bukan sekadar "barisnya ada". <c>NumberPrefix</c> menjadi awalan
    /// nomor jurnal, sehingga seeder yang menggandakan barisnya akan membuat dua skema penomoran
    /// berjalan bersamaan di atas satu buku besar — laporan yang dikelompokkan menurut jenis
    /// jurnal menjadi salah tanpa ada yang menyadarinya. Karena itu pemanggilan kedua wajib
    /// menghasilkan <b>lima</b> baris, bukan sepuluh dan bukan enam.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccountingMasterDataSeederTests
    {
        private static readonly Guid Pelaku = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private static readonly string[] LimaKodeBawaan = ["JB", "JP", "JT", "JU", "SA"];

        // =====================================================================
        // Kriteria 2 — seeder tetap idempoten sesudah JT ditambahkan
        // =====================================================================

        /// <summary>
        /// Pemanggilan pertama pada master kosong mengisi tepat lima baris.
        /// </summary>
        [Fact]
        public async Task PemanggilanPertama_MengisiLimaJenisJurnal()
        {
            using var db = TestDatabase.Create();

            await using var konteks = db.CreateContext();
            var hasil = await AccountingMasterDataSeeder.SeedAsync(konteks, Pelaku);

            Assert.Equal(5, hasil.JournalTypeInserted);
            Assert.Equal(0, hasil.JournalTypeSkipped);
            Assert.Null(hasil.JournalTypeSkippedReason);

            await using var pemeriksa = db.CreateContext();
            var kode = await pemeriksa.Set<AccJournalType>()
                .OrderBy(x => x.JournalTypeCode)
                .Select(x => x.JournalTypeCode)
                .ToListAsync();

            Assert.Equal(LimaKodeBawaan, kode);
        }

        /// <summary>
        /// Dipanggil dua kali tetap menghasilkan lima jenis jurnal, bukan enam dan bukan sepuluh.
        /// Inilah acceptance nomor 2 <c>BE-ACC-P2-003</c>.
        /// </summary>
        [Fact]
        public async Task DipanggilDuaKali_TetapLimaJenisJurnal()
        {
            using var db = TestDatabase.Create();

            await using (var pertama = db.CreateContext())
            {
                await AccountingMasterDataSeeder.SeedAsync(pertama, Pelaku);
            }

            AccountingMasterDataSeedResult hasilKedua;
            await using (var kedua = db.CreateContext())
            {
                hasilKedua = await AccountingMasterDataSeeder.SeedAsync(kedua, Pelaku);
            }

            // Pemanggilan kedua tidak menambah apa pun; kelimanya dilewati.
            Assert.Equal(0, hasilKedua.JournalTypeInserted);
            Assert.Equal(5, hasilKedua.JournalTypeSkipped);

            await using var pemeriksa = db.CreateContext();
            var kode = await pemeriksa.Set<AccJournalType>()
                .OrderBy(x => x.JournalTypeCode)
                .Select(x => x.JournalTypeCode)
                .ToListAsync();

            Assert.Equal(5, kode.Count);
            Assert.Equal(LimaKodeBawaan, kode);
        }

        /// <summary>
        /// Master yang sudah memuat empat baris lama — keadaan sebenarnya di database yang
        /// seeder-nya sudah pernah dijalankan sebelum Phase 2 — hanya bertambah <c>JT</c>.
        /// </summary>
        /// <remarks>
        /// Ini yang membedakan penambahan baris kelima dari pengisian master kosong. Yang keliru
        /// di sini tidak menimbulkan error: keempat baris lama tertimpa diam-diam, dan nomor
        /// jurnal yang sudah terbit berhenti cocok dengan awalan jenisnya.
        /// </remarks>
        [Fact]
        public async Task MasterEmpatBarisLama_HanyaBertambahJt()
        {
            using var db = TestDatabase.Create();

            var idLama = new Dictionary<string, Guid>();

            await using (var penyiap = db.CreateContext())
            {
                foreach (var (kode, nama, sistem) in new[]
                {
                    ("JU", "Jurnal Umum", false),
                    ("JP", "Jurnal Penyesuaian", false),
                    ("JB", "Jurnal Pembalik", true),
                    ("SA", "Saldo Awal", true)
                })
                {
                    var baris = new AccJournalType
                    {
                        Id = Guid.NewGuid(),
                        JournalTypeCode = kode,
                        JournalTypeName = nama,
                        NumberPrefix = kode,
                        RequiresApproval = true,
                        IsSystemType = sistem,
                        IsActive = true,
                        CreateDateTime = DateTime.UtcNow.AddDays(-30),
                        CreateBy = Pelaku
                    };

                    idLama[kode] = baris.Id;
                    penyiap.Set<AccJournalType>().Add(baris);
                }

                await penyiap.SaveChangesAsync();
            }

            await using (var konteks = db.CreateContext())
            {
                var hasil = await AccountingMasterDataSeeder.SeedAsync(konteks, Pelaku);

                Assert.Equal(1, hasil.JournalTypeInserted);
                Assert.Equal(4, hasil.JournalTypeSkipped);
            }

            await using var pemeriksa = db.CreateContext();
            var isi = await pemeriksa.Set<AccJournalType>().ToListAsync();

            Assert.Equal(5, isi.Count);

            // Keempat baris lama tetap baris yang sama, bukan baris baru bernama sama.
            foreach (var (kode, id) in idLama)
            {
                Assert.Equal(id, isi.Single(x => x.JournalTypeCode == kode).Id);
            }
        }

        // =====================================================================
        // Kriteria 3 — JT bernilai RequiresApproval = true
        // =====================================================================

        /// <summary>
        /// <c>JT</c> wajib melewati persetujuan orang kedua, dan wajib berstatus jenis sistem
        /// supaya kode maupun awalan nomornya tidak dapat diubah admin.
        /// </summary>
        /// <remarks>
        /// <c>ACC-DEC-053</c> menetapkan jurnal penutup tahun disusun sistem sebagai
        /// <c>Draft</c>, bukan langsung sah. Bila <c>RequiresApproval</c> bernilai
        /// <c>false</c>, jurnal penutup akan lolos tanpa dilihat siapa pun — dan angkanya
        /// terbawa ke tahun berikutnya sebagai saldo awal.
        /// </remarks>
        [Fact]
        public async Task JenisJurnalJt_MenuntutPersetujuanDanBerjenisSistem()
        {
            using var db = TestDatabase.Create();

            await using (var konteks = db.CreateContext())
            {
                await AccountingMasterDataSeeder.SeedAsync(konteks, Pelaku);
            }

            await using var pemeriksa = db.CreateContext();
            var jt = await pemeriksa.Set<AccJournalType>()
                .SingleAsync(x => x.JournalTypeCode == "JT");

            Assert.Equal("Jurnal Tutup Tahun", jt.JournalTypeName);
            Assert.Equal("JT", jt.NumberPrefix);
            Assert.True(jt.RequiresApproval);
            Assert.True(jt.IsSystemType);
            Assert.True(jt.IsActive);
        }

        // =====================================================================
        // Penjaga kepemilikan — tidak berubah oleh baris kelima
        // =====================================================================

        /// <summary>
        /// Master yang diisi sumber lain tetap tidak disentuh, walaupun daftar seeder kini
        /// memuat lima kode.
        /// </summary>
        [Fact]
        public async Task MasterMilikSumberLain_TidakDisentuh()
        {
            using var db = TestDatabase.Create();

            await using (var penyiap = db.CreateContext())
            {
                penyiap.Set<AccJournalType>().Add(new AccJournalType
                {
                    Id = Guid.NewGuid(),
                    JournalTypeCode = "GJ",
                    JournalTypeName = "General Journal",
                    NumberPrefix = "GJ",
                    RequiresApproval = true,
                    IsSystemType = false,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                });

                await penyiap.SaveChangesAsync();
            }

            await using (var konteks = db.CreateContext())
            {
                var hasil = await AccountingMasterDataSeeder.SeedAsync(konteks, Pelaku);

                Assert.Equal(0, hasil.JournalTypeInserted);
                Assert.NotNull(hasil.JournalTypeSkippedReason);
            }

            await using var pemeriksa = db.CreateContext();
            Assert.Equal(1, await pemeriksa.Set<AccJournalType>().CountAsync());
        }
    }
}
