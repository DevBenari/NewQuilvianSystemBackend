using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Seeders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-374 (kontrak v28, HRD-DEC-224; desain 02-backend-architecture.md 36.7) — definisi workflow
// "Persetujuan Perjalanan Dinas" ditanam idempoten saat startup: satu tahap atasan langsung pemohon,
// aktif, dan tidak pernah menimpa konfigurasi admin.
public sealed class TravelWorkflowDefinitionSeederTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();

    public void Dispose() => _db.Dispose();

    private async Task<(List<MstWorkflowDefinition> Definisi, List<MstWorkflowStep> Tahap)> BacaAsync()
    {
        await using var ctx = _db.CreateContext();
        var definisi = await ctx.Set<MstWorkflowDefinition>().AsNoTracking()
            .Where(x => x.WorkflowCode == BusinessTravelWorkflowCodes.DefinitionCode).ToListAsync();
        var ids = definisi.Select(x => x.Id).ToList();
        var tahap = await ctx.Set<MstWorkflowStep>().AsNoTracking()
            .Where(x => ids.Contains(x.WorkflowDefinitionId)).OrderBy(x => x.StepOrder).ToListAsync();
        return (definisi, tahap);
    }

    private async Task<BusinessTravelWorkflowDefinitionSeeder.SeedResult> TanamAsync()
    {
        await using var ctx = _db.CreateContext();
        return await BusinessTravelWorkflowDefinitionSeeder.SeedAsync(ctx);
    }

    // AC1 — tanam pertama: satu definisi dan satu tahap sesuai desain 36.7.
    [Fact]
    public async Task Tanam_pertama_membuat_satu_definisi_aktif_dengan_tahap_atasan_langsung()
    {
        var hasil = await TanamAsync();

        Assert.Equal(1, hasil.Definition);
        Assert.Equal(1, hasil.Step);

        var (definisi, tahap) = await BacaAsync();
        var d = Assert.Single(definisi);
        Assert.Equal("Persetujuan Perjalanan Dinas", d.WorkflowName);
        Assert.Equal("HRD_TRV_REQUEST", d.RequestType);
        Assert.Equal("HumanResource", d.WorkflowCategory);
        Assert.Equal(1, d.Version);
        Assert.True(d.IsActive);
        Assert.Equal("Active", d.WorkflowStatus);
        Assert.True(d.IsDefault);
        Assert.True(d.AllowRequesterWithdraw);
        Assert.True(d.AllowRequesterCancel);
        Assert.True(d.StopOnRejection);
        Assert.Null(d.LegalEntityId);
        Assert.Null(d.HospitalSiteId);
        Assert.Null(d.OrganizationUnitId);

        var t = Assert.Single(tahap);
        Assert.Equal("HRD-TRV-01", t.StepCode);
        Assert.Equal(1, t.StepOrder);
        Assert.Equal("Approval", t.StepType);
        Assert.Equal("Any", t.ApprovalMode);
        Assert.Equal(1, t.RequiredApprovalCount);
        Assert.Equal("RequesterManager", t.ApproverSourceType);
        Assert.False(t.AllowSelfApproval);
        Assert.True(t.IsRequired);
        Assert.True(t.IsActive);
        Assert.Equal(48, t.EscalationAfterHours);
    }

    // AC1 — definisi yang ditanam memenuhi syarat yang dipakai Workflow V2 saat memilih definisi
    // (WorkflowService.ResolveWorkflowDefinitionAsync): aktif, berstatus "Active", kode cocok, tanpa
    // batas tanggal dan tanpa batas lingkup.
    [Fact]
    public async Task Definisi_tertanam_dapat_dipilih_workflow_untuk_pemohon_mana_pun()
    {
        await TanamAsync();

        await using var ctx = _db.CreateContext();
        var today = DateTime.UtcNow.Date;
        var kode = BusinessTravelWorkflowCodes.DefinitionCode.Trim().ToUpperInvariant();
        var terpilih = await ctx.Set<MstWorkflowDefinition>().AsNoTracking()
            .Where(x =>
                !x.IsDelete && !x.IsCancel && x.IsActive &&
                x.WorkflowStatus == "Active" &&
                x.WorkflowCode.ToUpper() == kode &&
                (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today) &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today) &&
                !x.LegalEntityId.HasValue && !x.HospitalSiteId.HasValue && !x.OrganizationUnitId.HasValue)
            .ToListAsync();

        Assert.Single(terpilih);
        var tahapAktif = await ctx.Set<MstWorkflowStep>().AsNoTracking()
            .CountAsync(x => x.WorkflowDefinitionId == terpilih[0].Id && x.IsActive && !x.IsDelete && !x.IsCancel);
        Assert.Equal(1, tahapAktif);
    }

    // AC1 — tanam kedua tidak menambah apa pun.
    [Fact]
    public async Task Tanam_ulang_tidak_menambah_definisi_maupun_tahap()
    {
        await TanamAsync();
        var kedua = await TanamAsync();

        Assert.Equal(0, kedua.Definition);
        Assert.Equal(0, kedua.Step);
        var (definisi, tahap) = await BacaAsync();
        Assert.Single(definisi);
        Assert.Single(tahap);
    }

    // AC2 — perubahan admin (nonaktif, tahap tambahan, nama tahap, SLA) tidak ditimpa.
    [Fact]
    public async Task Konfigurasi_admin_tidak_ditimpa_saat_tanam_ulang()
    {
        await TanamAsync();
        await using (var ctx = _db.CreateContext())
        {
            var d = await ctx.Set<MstWorkflowDefinition>()
                .SingleAsync(x => x.WorkflowCode == BusinessTravelWorkflowCodes.DefinitionCode);
            d.IsActive = false;
            d.WorkflowStatus = "Inactive";
            d.WorkflowName = "Persetujuan SPPD (diatur admin)";
            var t = await ctx.Set<MstWorkflowStep>().SingleAsync(x => x.WorkflowDefinitionId == d.Id);
            t.StepName = "Kepala Ruangan";
            t.EscalationAfterHours = 24;
            ctx.Set<MstWorkflowStep>().Add(new MstWorkflowStep
            {
                WorkflowDefinitionId = d.Id,
                StepCode = "HRD-TRV-02",
                StepName = "Manajer SDM",
                StepOrder = 2,
                ApproverSourceType = "Position",
                CreateDateTime = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var hasil = await TanamAsync();

        Assert.Equal(0, hasil.Definition);
        Assert.Equal(0, hasil.Step);
        var (definisi, tahap) = await BacaAsync();
        var dd = Assert.Single(definisi);
        Assert.False(dd.IsActive);
        Assert.Equal("Inactive", dd.WorkflowStatus);
        Assert.Equal("Persetujuan SPPD (diatur admin)", dd.WorkflowName);
        Assert.Equal(2, tahap.Count);
        Assert.Equal("Kepala Ruangan", tahap[0].StepName);
        Assert.Equal(24, tahap[0].EscalationAfterHours);
        Assert.Equal("HRD-TRV-02", tahap[1].StepCode);
    }

    // AC2 — definisi yang dihapus admin tidak dihidupkan lagi.
    [Fact]
    public async Task Definisi_yang_dihapus_admin_tidak_ditanam_ulang()
    {
        await TanamAsync();
        await using (var ctx = _db.CreateContext())
        {
            var d = await ctx.Set<MstWorkflowDefinition>()
                .SingleAsync(x => x.WorkflowCode == BusinessTravelWorkflowCodes.DefinitionCode);
            d.IsDelete = true;
            await ctx.SaveChangesAsync();
        }

        var hasil = await TanamAsync();

        Assert.Equal(0, hasil.Definition);
        Assert.Equal(0, hasil.Step);
        var (definisi, _) = await BacaAsync();
        var dd = Assert.Single(definisi);
        Assert.True(dd.IsDelete);
    }

    // AC2 — tahap bawaan yang dihapus admin tidak ditambahkan lagi; tahap yang hilang (definisi
    // ada tanpa tahap bawaan) dilengkapi.
    [Fact]
    public async Task Tahap_dihapus_admin_dibiarkan_dan_tahap_hilang_dilengkapi()
    {
        await TanamAsync();
        await using (var ctx = _db.CreateContext())
        {
            var t = await ctx.Set<MstWorkflowStep>()
                .SingleAsync(x => x.StepCode == BusinessTravelWorkflowCodes.ManagerStepCode);
            t.IsDelete = true;
            await ctx.SaveChangesAsync();
        }

        Assert.Equal(0, (await TanamAsync()).Step);
        var (_, tahap) = await BacaAsync();
        Assert.True(Assert.Single(tahap).IsDelete);

        await using (var ctx = _db.CreateContext())
        {
            ctx.Set<MstWorkflowStep>().RemoveRange(ctx.Set<MstWorkflowStep>());
            await ctx.SaveChangesAsync();
        }

        var lengkapi = await TanamAsync();
        Assert.Equal(0, lengkapi.Definition);
        Assert.Equal(1, lengkapi.Step);
    }

    // AC3 (sebagian) — SeedDefaultData:Enabled = false melewati seeder tanpa galat.
    [Fact]
    public async Task Seeder_dilewati_bila_SeedDefaultData_dimatikan()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SeedDefaultData:Enabled"] = "false" })
            .Build());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddScoped<ApplicationDbContext>(_ => _db.CreateContext());
        await using var provider = services.BuildServiceProvider();

        await BusinessTravelWorkflowDefinitionSeeder.SeedAsync(provider);

        var (definisi, _) = await BacaAsync();
        Assert.Empty(definisi);
    }

    // AC3 (sebagian) — jalur startup bawaan (konfigurasi kosong) menanam definisi.
    [Fact]
    public async Task Seeder_startup_menanam_definisi_dengan_konfigurasi_bawaan()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddScoped<ApplicationDbContext>(_ => _db.CreateContext());
        await using var provider = services.BuildServiceProvider();

        await BusinessTravelWorkflowDefinitionSeeder.SeedAsync(provider);

        var (definisi, tahap) = await BacaAsync();
        Assert.Single(definisi);
        Assert.Single(tahap);
    }
}
