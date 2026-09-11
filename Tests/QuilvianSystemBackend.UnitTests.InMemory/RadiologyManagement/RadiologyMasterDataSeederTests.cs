using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Seeders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Pengisian data master awal Radiologi — <c>BE-RAD-15</c>.
///
/// Satu hal yang dijaga berkas ini lebih penting daripada yang lain: <b>seeder tidak boleh
/// membuat satu pun aturan keselamatan berlaku.</b> Aturan yang menentukan kapan pasien boleh
/// disinari hanya sah setelah disahkan penanggung jawab klinis, dan sebuah program tidak boleh
/// mengambil alih keputusan itu — sekalipun dengan maksud baik supaya modul cepat dapat dipakai.
/// </summary>
public sealed class RadiologyMasterDataSeederTests
{
    private static readonly Guid AdminRadiologi = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PenanggungJawabKlinis = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task MengisiEnamAlatDanEmpatButirKeselamatan()
    {
        await using var db = Konteks();

        var hasil = await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        Assert.Equal(6, hasil.AlatDitambahkan);
        Assert.Equal(4, hasil.ButirDitambahkan);

        var kodeAlat = await db.MstRadModalities.Select(x => x.ModalityCode).ToListAsync();
        Assert.Equal(new[] { "CR", "CT", "MG", "MR", "RF", "US" }, kodeAlat.OrderBy(x => x));

        // Penanda radiasi pengion menentukan butir mana yang masuk akal bagi sebuah alat.
        var mri = await db.MstRadModalities.SingleAsync(x => x.ModalityCode == "MR");
        Assert.False(mri.UsesIonisingRadiation);
        Assert.True(mri.SupportsContrast);
    }

    [Fact]
    public async Task SetiapButirMenyatakanAsalUsulnyaApaAdanya()
    {
        // SourceNote ada supaya tidak ada yang mengira baseline implementasi adalah SOP rumah
        // sakit yang sudah disahkan.
        await using var db = Konteks();

        await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        var butir = await db.MstRadSafetyRequirements.ToListAsync();

        Assert.All(butir, x => Assert.False(string.IsNullOrWhiteSpace(x.SourceNote)));
        Assert.All(butir, x => Assert.Contains("Bukan SOP rumah sakit", x.SourceNote!));
    }

    /* ================================================================== *
     * Inti task ini
     * ================================================================== */

    [Fact]
    public async Task TidakSatuPunAturanLahirBerlaku()
    {
        await using var db = Konteks();

        var hasil = await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        Assert.True(hasil.DrafAturanDitambahkan > 0);

        var aturan = await db.MstRadModalitySafetyRules.ToListAsync();

        Assert.All(aturan, x => Assert.Equal(RadSafetyRuleStatus.Draft, x.RuleStatus));
        Assert.All(aturan, x => Assert.False(x.IsActive));
        Assert.All(aturan, x => Assert.Null(x.ApprovedAt));
        Assert.All(aturan, x => Assert.Equal(1, x.RuleVersion));
    }

    [Fact]
    public async Task GerbangTetapMenolakSeluruhAlatSetelahSeederBerjalan()
    {
        // Inilah yang membuat Definition of Done BE-RAD-15 belum terpenuhi, dan itu disengaja.
        // Selama penanggung jawab klinis belum mengesahkan, modul memang belum dapat dipakai.
        await using var db = Konteks();

        var hasil = await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        Assert.Equal(6, hasil.AlatBelumTercakup);

        var cakupan = await PolicyService(db, PenanggungJawabKlinis).GetCoverageAsync();

        Assert.Equal(6, cakupan.Count);

        // Setiap alat sudah punya draf yang tinggal disahkan — pekerjaannya sudah disiapkan,
        // hanya keputusannya yang belum diambil.
        Assert.All(cakupan, x => Assert.True(x.DraftOrPendingRuleCount > 0));
    }

    [Fact]
    public async Task CakupanMenjadiKosongSetelahSeluruhDrafDisahkan()
    {
        // Membuktikan draf yang disiapkan seeder memang cukup: begitu penanggung jawab klinis
        // mengesahkan satu draf per alat, modul langsung dapat dipakai.
        await using var db = Konteks();
        await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        var satuPerAlat = await db.MstRadModalitySafetyRules
            .AsNoTracking()
            .GroupBy(x => x.ModalityId)
            .Select(g => g.First().Id)
            .ToListAsync();

        foreach (var aturanId in satuPerAlat)
        {
            // Dua orang, bukan satu. Pengesahan sendiri ditolak BE-RAD-02, dan itu berlaku
            // penuh untuk draf yang disiapkan seeder — persis seperti yang diharapkan.
            var diajukan = await PolicyService(db, AdminRadiologi).SubmitAsync(aturanId);
            var disahkan = await PolicyService(db, PenanggungJawabKlinis).ApproveAsync(aturanId);

            Assert.Equal(RadOperationResultKind.Success, diajukan.Kind);
            Assert.Equal(RadOperationResultKind.Success, disahkan.Kind);
        }

        var cakupan = await PolicyService(db, PenanggungJawabKlinis).GetCoverageAsync();

        Assert.Empty(cakupan);
    }

    [Fact]
    public async Task UsgIkutMendapatDrafWalauTidakPunyaButirWajib()
    {
        // Jebakan yang paling mudah terlewat: USG tidak punya satu pun butir wajib, tetapi
        // gerbang fail-closed tetap menuntutnya punya sedikitnya satu aturan berlaku.
        await using var db = Konteks();

        await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        var usg = await db.MstRadModalities.SingleAsync(x => x.ModalityCode == "US");

        var aturanUsg = await db.MstRadModalitySafetyRules
            .Where(x => x.ModalityId == usg.Id)
            .ToListAsync();

        var baris = Assert.Single(aturanUsg);
        Assert.False(baris.IsMandatory);
        Assert.Contains("fail-closed", baris.Note!);
    }

    [Fact]
    public async Task AturanBerkontrasDisusunTidakWajibDanMenyebutSyaratnya()
    {
        // Model aturan saat ini tidak dapat menyatakan "wajib hanya bila memakai kontras".
        // Daripada menebak, baris itu disusun tidak wajib dan syaratnya ditulis apa adanya
        // supaya penanggung jawab klinis yang memutuskan.
        await using var db = Konteks();

        await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        var ct = await db.MstRadModalities.SingleAsync(x => x.ModalityCode == "CT");
        var kontras = await db.MstRadSafetyRequirements
            .SingleAsync(x => x.RequirementCode == "CONTRAST_ALLERGY");

        var aturan = await db.MstRadModalitySafetyRules
            .SingleAsync(x => x.ModalityId == ct.Id && x.SafetyRequirementId == kontras.Id);

        Assert.False(aturan.IsMandatory);
        Assert.Contains("bila pemeriksaannya memakai media kontras", aturan.Note!);
    }

    /* ================================================================== *
     * Tidak menimpa keputusan pengguna
     * ================================================================== */

    [Fact]
    public async Task DijalankanDuaKali_TidakMenambahBarisKembar()
    {
        await using var db = Konteks();

        var pertama = await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);
        var kedua = await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        Assert.True(pertama.AlatDitambahkan > 0);

        Assert.Equal(0, kedua.AlatDitambahkan);
        Assert.Equal(0, kedua.ButirDitambahkan);
        Assert.Equal(0, kedua.DrafAturanDitambahkan);

        Assert.Equal(6, await db.MstRadModalities.CountAsync());
        Assert.Equal(4, await db.MstRadSafetyRequirements.CountAsync());
    }

    [Fact]
    public async Task TidakMenghidupkanKembaliAturanYangSudahDihentikan()
    {
        // Penghentian sebuah aturan adalah keputusan penanggung jawab klinis. Seeder yang
        // mengusulkannya lagi setiap server menyala akan membatalkan keputusan itu diam-diam.
        await using var db = Konteks();
        await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        var mg = await db.MstRadModalities.SingleAsync(x => x.ModalityCode == "MG");
        var aturanId = await db.MstRadModalitySafetyRules
            .Where(x => x.ModalityId == mg.Id)
            .Select(x => x.Id)
            .SingleAsync();

        await PolicyService(db, AdminRadiologi).SubmitAsync(aturanId);
        await PolicyService(db, PenanggungJawabKlinis).ApproveAsync(aturanId);
        await PolicyService(db, PenanggungJawabKlinis).DeactivateAsync(aturanId);

        var ulang = await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        Assert.Equal(0, ulang.DrafAturanDitambahkan);

        var tersimpan = await db.MstRadModalitySafetyRules.AsNoTracking()
            .SingleAsync(x => x.Id == aturanId);

        Assert.Equal(RadSafetyRuleStatus.Inactive, tersimpan.RuleStatus);
    }

    [Fact]
    public async Task TidakMenimpaAlatYangSudahDisuntingPengguna()
    {
        await using var db = Konteks();
        await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        var ct = await db.MstRadModalities.SingleAsync(x => x.ModalityCode == "CT");
        ct.ModalityName = "CT-Scan 128 Slice Ruang 2";
        ct.IsActive = false;
        await db.SaveChangesAsync();

        await RadiologyMasterDataSeeder.SeedAsync(db, NullLogger.Instance);

        var sesudah = await db.MstRadModalities.AsNoTracking()
            .SingleAsync(x => x.ModalityCode == "CT");

        Assert.Equal("CT-Scan 128 Slice Ruang 2", sesudah.ModalityName);
        Assert.False(sesudah.IsActive);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-master-seed-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Service siklus pengesahan atas nama satu pelaku.
    ///
    /// Pelakunya wajib disebut, karena pengesahan sendiri ditolak <c>BE-RAD-02</c>: yang
    /// mengajukan dan yang mengesahkan harus dua orang berbeda. Dibuat ulang pada setiap
    /// pemakaian, sebab <c>HttpContextAccessor</c> memakai <c>AsyncLocal</c> bersama dan
    /// setternya mengosongkan context accessor sebelumnya.
    /// </summary>
    private static RadSafetyPolicyService PolicyService(
        ApplicationDbContext db,
        Guid actorUserId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        return new RadSafetyPolicyService(
            db, accessor, new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }
}
