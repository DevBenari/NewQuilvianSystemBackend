using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Permukaan baca aturan keselamatan — <c>BE-RAD-03</c>.
///
/// Yang paling dijaga di sini adalah <c>GET /coverage</c>. Gerbang keselamatan bersifat
/// fail-closed, sehingga alat yang belum punya aturan berlaku akan menolak seluruh
/// pemeriksaannya. Daftar cakupan itulah satu-satunya cara admin mengetahuinya sebelum pasien
/// dipanggil, dan <c>BE-RAD-15</c> menuntutnya kosong sebelum modul dinyatakan siap.
/// </summary>
public sealed class RadSafetyRuleCatalogTests
{
    private static readonly Guid AdminRadiologi = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PenanggungJawabKlinis = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /* ================================================================== *
     * Cakupan alat
     * ================================================================== */

    [Fact]
    public async Task Cakupan_KosongKetikaSeluruhAlatSudahPunyaAturanBerlaku()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "CT", "CT-Scan");

        await SahkanAturanAsync(db, alat, butir);

        var cakupan = await Service(db, PenanggungJawabKlinis).GetCoverageAsync();

        // Daftar kosong adalah jawaban yang benar, bukan tanda tidak ada data.
        Assert.Empty(cakupan);
    }

    [Fact]
    public async Task Cakupan_MemuatAlatYangAturannyaMasihDraf()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "USG", "Ultrasonografi");

        // Draf tidak menyiapkan alat apa pun. Alat ini tetap akan menolak pemeriksaan.
        await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));

        var cakupan = await Service(db, PenanggungJawabKlinis).GetCoverageAsync();

        var baris = Assert.Single(cakupan);
        Assert.Equal(alat, baris.ModalityId);
        Assert.Equal("USG", baris.ModalityCode);

        // Penanda ini membedakan "belum ada yang menyusun" dari "sudah disusun, menunggu sah".
        Assert.Equal(1, baris.DraftOrPendingRuleCount);
    }

    [Fact]
    public async Task Cakupan_MemuatAlatYangBelumDisentuhSamaSekali()
    {
        await using var db = Konteks();
        var alat = await AlatAsync(db, "MRI", "MRI 1.5T");

        var cakupan = await Service(db, AdminRadiologi).GetCoverageAsync();

        var baris = Assert.Single(cakupan);
        Assert.Equal(alat, baris.ModalityId);
        Assert.Equal(0, baris.DraftOrPendingRuleCount);
    }

    [Fact]
    public async Task Cakupan_TidakMemuatAlatYangSudahDipensiunkan()
    {
        // Alat yang tidak dipakai lagi bukan kekurangan yang perlu ditindaklanjuti admin.
        await using var db = Konteks();
        await AlatAsync(db, "RF", "Fluoroskopi", aktif: false);

        var cakupan = await Service(db, AdminRadiologi).GetCoverageAsync();

        Assert.Empty(cakupan);
    }

    [Fact]
    public async Task Cakupan_MemuatKembaliAlatSetelahAturannyaDihentikan()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "CT", "CT-Scan");

        var aturanId = await SahkanAturanAsync(db, alat, butir);
        Assert.Empty(await Service(db, AdminRadiologi).GetCoverageAsync());

        await Service(db, PenanggungJawabKlinis).DeactivateAsync(aturanId);

        var cakupan = await Service(db, AdminRadiologi).GetCoverageAsync();

        Assert.Single(cakupan);
    }

    /* ================================================================== *
     * Daftar berhalaman
     * ================================================================== */

    [Fact]
    public async Task Daftar_MenyaringMenurutKeadaanPengesahan()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "CT", "CT-Scan");

        await SahkanAturanAsync(db, alat, butir);
        await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));

        var service = Service(db, AdminRadiologi);

        var berlaku = await service.GetPagedAsync(
            new RadSafetyRulePagedQuery { RuleStatus = RadSafetyRuleStatus.Active });

        var draf = await service.GetPagedAsync(
            new RadSafetyRulePagedQuery { RuleStatus = RadSafetyRuleStatus.Draft });

        Assert.Equal(1, berlaku.TotalData);
        Assert.Equal(1, draf.TotalData);
        Assert.Equal(nameof(RadSafetyRuleStatus.Active), berlaku.Items.Single().RuleStatus);
    }

    [Fact]
    public async Task Daftar_MencariPadaKodeAlatDanKodeButir()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var ct = await AlatAsync(db, "CT", "CT-Scan");
        var usg = await AlatAsync(db, "USG", "Ultrasonografi");

        await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(ct, butir));
        await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(usg, butir));

        var service = Service(db, AdminRadiologi);

        var perAlat = await service.GetPagedAsync(new RadSafetyRulePagedQuery { Search = "Ultra" });
        var perButir = await service.GetPagedAsync(
            new RadSafetyRulePagedQuery { Search = "PREGNANCY" });

        Assert.Equal(1, perAlat.TotalData);
        Assert.Equal("USG", perAlat.Items.Single().ModalityCode);

        // Butirnya sama untuk keduanya, sehingga pencarian butir menemukan dua-duanya.
        Assert.Equal(2, perButir.TotalData);
    }

    [Fact]
    public async Task Daftar_MenormalkanPermintaanHalamanYangTidakMasukAkal()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "CT", "CT-Scan");

        for (var i = 0; i < 3; i++)
        {
            await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));
        }

        var service = Service(db, AdminRadiologi);

        var nol = await service.GetPagedAsync(
            new RadSafetyRulePagedQuery { PageNumber = 0, PageSize = 0 });

        var kebesaran = await service.GetPagedAsync(
            new RadSafetyRulePagedQuery { PageSize = 500 });

        Assert.Equal(1, nol.PageNumber);
        Assert.Equal(25, nol.PageSize);
        Assert.Equal(100, kebesaran.PageSize);

        // Jumlah halaman dihitung backend, bukan ditebak layar dari panjang Items.
        Assert.Equal(3, nol.TotalData);
        Assert.Equal(1, nol.TotalPage);
    }

    [Fact]
    public async Task Daftar_MembawaNamaAlatDanButirUntukDitampilkan()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "CT", "CT-Scan");

        await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));

        var hasil = await Service(db, AdminRadiologi).GetPagedAsync(new RadSafetyRulePagedQuery());

        var baris = hasil.Items.Single();
        Assert.Equal("CT-Scan", baris.ModalityName);
        Assert.Equal("Skrining kehamilan", baris.RequirementName);
    }

    /* ================================================================== *
     * Rincian satu aturan
     * ================================================================== */

    [Fact]
    public async Task Rincian_MembawaNamaAlatDanButirUntukFormUbah()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "CT", "CT-Scan");

        var draf = await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));

        var hasil = await Service(db, AdminRadiologi).GetByIdAsync(draf.Value!.Id);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal("CT-Scan", hasil.Value!.ModalityName);
        Assert.Equal("Skrining kehamilan", hasil.Value.RequirementName);
        Assert.Equal(nameof(RadSafetyRuleStatus.Draft), hasil.Value.RuleStatus);
    }

    [Fact]
    public async Task Rincian_AturanYangTidakAda_Ditolak404()
    {
        await using var db = Konteks();

        var hasil = await Service(db, AdminRadiologi).GetByIdAsync(Guid.NewGuid());

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.SafetyRuleNotFound, hasil.ErrorCode);
    }

    [Fact]
    public async Task Rincian_AturanYangSudahDihapus_DiperlakukanTidakAda()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var alat = await AlatAsync(db, "CT", "CT-Scan");

        var draf = await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));

        var baris = await db.MstRadModalitySafetyRules.SingleAsync();
        baris.IsDelete = true;
        await db.SaveChangesAsync();

        var hasil = await Service(db, AdminRadiologi).GetByIdAsync(draf.Value!.Id);

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
    }

    /* ================================================================== *
     * Rekap dan metadata
     * ================================================================== */

    [Fact]
    public async Task Rekap_MenghitungPerKeadaanDanAlatYangBelumTercakup()
    {
        await using var db = Konteks();
        var butir = await ButirAsync(db);
        var ct = await AlatAsync(db, "CT", "CT-Scan");
        await AlatAsync(db, "USG", "Ultrasonografi");

        await SahkanAturanAsync(db, ct, butir);
        await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(ct, butir));

        var rekap = await Service(db, AdminRadiologi).GetSummaryAsync();

        Assert.Equal(2, rekap.TotalAturan);
        Assert.Equal(1, rekap.Berlaku);
        Assert.Equal(1, rekap.Draf);
        Assert.Equal(1, rekap.BerlakuDanWajib);

        // USG belum punya aturan berlaku sama sekali.
        Assert.Equal(1, rekap.AlatBelumTercakup);
    }

    [Fact]
    public void Metadata_MenyebutSeluruhPerpindahanStatusYangSah()
    {
        using var db = Konteks();
        var metadata = Service(db, AdminRadiologi).GetFilterMetadata();

        Assert.Equal(4, metadata.RuleStatuses.Count);

        var aksi = metadata.Actions.Select(x => x.Action).ToList();
        Assert.Equal(new[] { "submit", "approve", "reject", "deactivate" }, aksi);

        var sahkan = metadata.Actions.Single(x => x.Action == "approve");
        Assert.Equal(nameof(RadSafetyRuleStatus.PendingApproval), sahkan.FromStatus);
        Assert.Equal(nameof(RadSafetyRuleStatus.Active), sahkan.ToStatus);

        // Penolakan mengembalikan aturan menjadi draf, bukan menghentikannya.
        var tolak = metadata.Actions.Single(x => x.Action == "reject");
        Assert.Equal(nameof(RadSafetyRuleStatus.Draft), tolak.ToStatus);
    }

    [Fact]
    public void Metadata_TidakMenjanjikanPenyaringYangTidakDiproses()
    {
        using var db = Konteks();
        var metadata = Service(db, AdminRadiologi).GetFilterMetadata();

        var disebut = metadata.QueryParameters.Select(x => x.Name).ToList();

        Assert.Contains("search", disebut);
        Assert.Contains("modalityId", disebut);
        Assert.Contains("safetyRequirementId", disebut);
        Assert.Contains("ruleStatus", disebut);
        Assert.Contains("isMandatory", disebut);
        Assert.Contains("pageNumber", disebut);
        Assert.Contains("pageSize", disebut);

        // Daftar ini benar-benar berhalaman, sehingga pilihan jumlah baris boleh dijanjikan.
        Assert.NotEmpty(metadata.PageSizeOptions);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-safety-rule-catalog-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RadSafetyPolicyService Service(ApplicationDbContext db, Guid actorUserId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        return new RadSafetyPolicyService(
            db,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static async Task<Guid> AlatAsync(
        ApplicationDbContext db,
        string kode,
        string nama,
        bool aktif = true)
    {
        var alat = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = kode,
            ModalityName = nama,
            UsesIonisingRadiation = kode != "USG",
            IsActive = aktif,
        };

        db.MstRadModalities.Add(alat);
        await db.SaveChangesAsync();

        return alat.Id;
    }

    private static async Task<Guid> ButirAsync(ApplicationDbContext db)
    {
        var butir = new MstRadSafetyRequirement
        {
            Id = Guid.NewGuid(),
            RequirementCode = "PREGNANCY_SCREENING",
            RequirementName = "Skrining kehamilan",
            IsActive = true,
        };

        db.MstRadSafetyRequirements.Add(butir);
        await db.SaveChangesAsync();

        return butir.Id;
    }

    private static CreateRadSafetyRuleRequest Permintaan(Guid alatId, Guid butirId) => new()
    {
        ModalityId = alatId,
        SafetyRequirementId = butirId,
        IsMandatory = true,
    };

    /// <summary>Aturan yang sudah melewati seluruh siklus pengesahan sampai berlaku.</summary>
    private static async Task<Guid> SahkanAturanAsync(
        ApplicationDbContext db,
        Guid alatId,
        Guid butirId)
    {
        var penyusun = Service(db, AdminRadiologi);
        var draf = await penyusun.CreateDraftAsync(Permintaan(alatId, butirId));
        await penyusun.SubmitAsync(draf.Value!.Id);
        await Service(db, PenanggungJawabKlinis).ApproveAsync(draf.Value.Id);

        return draf.Value.Id;
    }
}
