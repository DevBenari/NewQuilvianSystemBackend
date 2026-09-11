using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Bentuk tabel hasil bacaan — <c>BE-RAD-07</c>, terhadap <c>RAD-ERD-REP-001</c> dan
/// <c>RAD-ERD-DICT-001</c> bagian 1 dan 2.
///
/// Uji ini membaca <b>model Entity Framework</b>, bukan database. Yang dijaga adalah bentuk
/// yang menentukan keselamatan riwayat klinis, dan ketiganya mudah hilang tanpa disadari
/// ketika seseorang menyunting configuration di kemudian hari:
///
/// <list type="number">
/// <item>Satu study paling banyak satu bacaan — dijaga index unik, bukan hanya service.</item>
/// <item>Nomor versi tidak kembar dalam satu bacaan — tanpa itu riwayat koreksi bercabang.</item>
/// <item>Tidak ada kunci asing dari induk ke versi berlaku — relasinya akan melingkar.</item>
/// </list>
/// </summary>
public sealed class RadReportModelContractTests
{
    /* ================================================================== *
     * Index yang menjaga riwayat
     * ================================================================== */

    [Fact]
    public void SatuStudyPalingBanyakSatuBacaan_DijagaIndexUnik()
    {
        var index = IndexOf<RadReport>(nameof(RadReport.RadStudyId));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique, "Index RadStudyId wajib unik.");

        // Difilter pada baris yang belum dihapus, supaya bacaan yang sudah ditandai terhapus
        // tidak menghalangi bacaan pengganti.
        Assert.Equal("\"IsDelete\" = false", index.GetFilter());
    }

    [Fact]
    public void NomorBacaanTidakBolehKembar()
    {
        var index = IndexOf<RadReport>(nameof(RadReport.ReportNumber));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
        Assert.Equal("\"IsDelete\" = false", index.GetFilter());
    }

    [Fact]
    public void NomorVersiTidakBolehKembarDalamSatuBacaan()
    {
        // Tanpa penjaga ini, dua amandemen yang berjalan hampir bersamaan dapat sama-sama
        // menjadi "versi 2", dan riwayat klinisnya bercabang tanpa ada yang tahu mana yang
        // berlaku.
        var index = IndexOf<RadReportVersion>(
            nameof(RadReportVersion.RadReportId),
            nameof(RadReportVersion.VersionNumber));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
    }

    [Fact]
    public void RantaiKoreksiDapatDitelusuri()
    {
        Assert.NotNull(IndexOf<RadReportVersion>(nameof(RadReportVersion.PreviousVersionId)));
        Assert.NotNull(IndexOf<RadReport>(nameof(RadReport.EncounterId)));
        Assert.NotNull(IndexOf<RadReport>(nameof(RadReport.ReportStatus)));
    }

    /* ================================================================== *
     * Jebakan yang disebut ERD secara khusus
     * ================================================================== */

    [Fact]
    public void TidakAdaKunciAsingDariIndukKeVersiBerlaku()
    {
        // RAD-ERD-REP-001 menyebut ini eksplisit sebagai jebakan: kunci asing dari RadReport
        // ke versi berlaku membuat relasinya melingkar, dan penyisipan versi pertama mustahil
        // dilakukan dalam satu transaksi tanpa kolom sementara yang kosong.
        var report = EntityOf<RadReport>();

        var fkKeVersi = report.GetForeignKeys()
            .Where(x => x.PrincipalEntityType.ClrType == typeof(RadReportVersion))
            .ToList();

        Assert.Empty(fkKeVersi);

        // Yang disimpan hanya nomornya.
        Assert.NotNull(report.FindProperty(nameof(RadReport.CurrentVersionNumber)));
    }

    [Fact]
    public void EncounterIdTidakPunyaKunciAsing()
    {
        // Pemilik kunjungan adalah Registration Management. Kolom ini salinan untuk pencarian,
        // sebagaimana ditetapkan RAD-ERD-DICT-001 bagian 1.
        var fk = EntityOf<RadReport>()
            .GetForeignKeys()
            .Where(x => x.Properties.Any(p => p.Name == nameof(RadReport.EncounterId)))
            .ToList();

        Assert.Empty(fk);
    }

    [Fact]
    public void SeluruhRelasiMemakaiRestrict()
    {
        // Menghapus study, pesanan, atau satu mata rantai versi akan memutus riwayat yang
        // justru wajib utuh.
        foreach (var tipe in new[] { typeof(RadReport), typeof(RadReportVersion) })
        {
            var entity = Model().FindEntityType(tipe)!;

            Assert.All(
                entity.GetForeignKeys(),
                fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        }
    }

    /* ================================================================== *
     * Kolom sesuai kamus data
     * ================================================================== */

    [Fact]
    public void KesimpulanWajibDiisi_TemuanDanSaranBoleh()
    {
        // Kesimpulan adalah yang benar-benar dibaca dokter pengirim. Temuan dan saran boleh
        // kosong; kesimpulan tidak.
        Assert.False(PropertyOf<RadReportVersion>(nameof(RadReportVersion.Impression)).IsNullable);
        Assert.True(PropertyOf<RadReportVersion>(nameof(RadReportVersion.Findings)).IsNullable);
        Assert.True(PropertyOf<RadReportVersion>(nameof(RadReportVersion.Recommendation)).IsNullable);
    }

    [Theory]
    [InlineData(nameof(RadReportVersion.Findings), 8000)]
    [InlineData(nameof(RadReportVersion.Impression), 4000)]
    [InlineData(nameof(RadReportVersion.Recommendation), 2000)]
    [InlineData(nameof(RadReportVersion.AmendmentReason), 1000)]
    public void PanjangKolomIsiSesuaiKamusData(string nama, int panjang)
    {
        Assert.Equal(panjang, PropertyOf<RadReportVersion>(nama).GetMaxLength());
    }

    [Fact]
    public void NomorBacaanPanjangnyaSesuaiKamusData()
    {
        Assert.Equal(64, PropertyOf<RadReport>(nameof(RadReport.ReportNumber)).GetMaxLength());
        Assert.False(PropertyOf<RadReport>(nameof(RadReport.ReportNumber)).IsNullable);
    }

    [Fact]
    public void SeluruhEnumDisimpanSebagaiAngka()
    {
        // Disimpan sebagai int, bukan teks — RAD-ERD-DICT-001. Nama status boleh berganti
        // tanpa migrasi data.
        Assert.Equal(
            typeof(int),
            PropertyOf<RadReport>(nameof(RadReport.ReportStatus)).GetProviderClrType());

        Assert.Equal(
            typeof(int),
            PropertyOf<RadReportVersion>(nameof(RadReportVersion.VersionStatus))
                .GetProviderClrType());

        Assert.Equal(
            typeof(int),
            PropertyOf<RadReportVersion>(nameof(RadReportVersion.AuthorRoleSnapshot))
                .GetProviderClrType());
    }

    [Fact]
    public void KeduaTabelTerpetakanKeSchemaPublic()
    {
        Assert.Equal("RadReport", EntityOf<RadReport>().GetTableName());
        Assert.Equal("RadReportVersion", EntityOf<RadReportVersion>().GetTableName());
        Assert.Equal("public", EntityOf<RadReport>().GetSchema());
        Assert.Equal("public", EntityOf<RadReportVersion>().GetSchema());
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static IModel Model()
    {
        using var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"rad-report-model-{Guid.NewGuid():N}")
                .Options);

        return db.Model;
    }

    private static IEntityType EntityOf<T>() => Model().FindEntityType(typeof(T))!;

    private static IProperty PropertyOf<T>(string nama) => EntityOf<T>().FindProperty(nama)!;

    private static IIndex? IndexOf<T>(params string[] properties)
        => EntityOf<T>()
            .GetIndexes()
            .FirstOrDefault(x =>
                x.Properties.Select(p => p.Name).SequenceEqual(properties));
}
