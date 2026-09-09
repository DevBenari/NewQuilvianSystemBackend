using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance nomor 1 <c>BE-ACC-P2-003</c> — bentuk tabel
    /// <c>AccAccountingConfiguration</c> sesuai kamus data bagian 17.
    /// </summary>
    /// <remarks>
    /// Uji ini membaca model EF Core yang sama dengan yang dipakai aplikasi, sehingga kolom,
    /// nullability, index, dan nama tabel yang diperiksa adalah bentuk yang benar-benar akan
    /// dibuat migration <c>BE-ACC-P2-004</c> — bukan salinan yang ditulis ulang di dalam uji.
    /// </remarks>
    public class AccAccountingConfigurationSchemaTests
    {
        private static IEntityType Entity(ApplicationDbContext c) =>
            c.Model.FindEntityType(typeof(AccAccountingConfiguration))!;

        private static IProperty Kolom(ApplicationDbContext c, string nama) =>
            Entity(c).FindProperty(nama)!;

        /// <summary>
        /// Satu badan hukum hanya boleh punya satu pengaturan yang hidup.
        /// </summary>
        /// <remarks>
        /// Tanpa unique index ini, dua pengaturan dapat berdiri untuk badan hukum yang sama dan
        /// menunjuk akun laba ditahan yang berbeda. Tutup tahun kemudian memilih salah satunya
        /// tanpa aturan yang jelas, dan laba tahun berjalan mendarat di akun yang salah — tanpa
        /// error apa pun, karena jurnalnya tetap seimbang.
        /// </remarks>
        [Fact]
        public void SatuBadanHukum_HanyaSatuPengaturanYangHidup()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var index = Entity(context).GetIndexes()
                .Single(x => x.Properties.Count == 1 &&
                             x.Properties[0].Name == "LegalEntityId");

            Assert.True(index.IsUnique);
            Assert.Contains("IsDelete", index.GetFilter());
        }

        /// <summary>
        /// Nama tabel, prefix, kolom wajib, dan bawaan <c>IsActive</c> sesuai kamus data
        /// bagian 17.
        /// </summary>
        [Fact]
        public void BentukTabel_SesuaiKamusDataBagian17()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var entity = Entity(context);

            Assert.Equal("AccAccountingConfiguration", entity.GetTableName());
            Assert.Equal("public", entity.GetSchema());
            Assert.StartsWith("Acc", nameof(AccAccountingConfiguration));

            // Kedua penunjuk wajib terisi — pengaturan tanpa akun laba ditahan tidak berguna.
            Assert.False(Kolom(context, "LegalEntityId").IsNullable);
            Assert.False(Kolom(context, "RetainedEarningsAccountId").IsNullable);

            Assert.Equal(true, Kolom(context, "IsActive").GetDefaultValue());

            // Akun laba ditahan dicari lewat index tersendiri.
            Assert.Contains(entity.GetIndexes(), x =>
                x.Properties.Count == 1 &&
                x.Properties[0].Name == "RetainedEarningsAccountId");
        }

        /// <summary>
        /// Kedua relasi memakai <c>Restrict</c>: akun yang masih dipakai pengaturan tidak boleh
        /// terhapus, dan pengaturan tidak boleh ikut hilang bersama induknya.
        /// </summary>
        [Fact]
        public void KeduaRelasi_MemakaiRestrict()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            foreach (var kolom in new[] { "LegalEntityId", "RetainedEarningsAccountId" })
            {
                var relasi = Entity(context).GetForeignKeys()
                    .Single(x => x.Properties.Count == 1 && x.Properties[0].Name == kolom);

                Assert.Equal(DeleteBehavior.Restrict, relasi.DeleteBehavior);
            }
        }
    }
}
