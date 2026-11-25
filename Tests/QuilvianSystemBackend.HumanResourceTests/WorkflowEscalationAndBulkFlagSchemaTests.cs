using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-440 (kontrak v29, HRD-DEC-232/233; kamus data 30.1–30.2; desain 02-backend-architecture.md 37.6–37.7) —
// penanda "boleh disetujui massal" pada definisi workflow dan tautan penugasan hasil eskalasi SLA.
public sealed class WorkflowEscalationAndBulkFlagSchemaTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _aktor = Guid.NewGuid();

    public void Dispose() => _db.Dispose();

    private WorkflowDefinitionController Controller()
    {
        var akses = Pelaku.Sebagai(_aktor);
        var c = new WorkflowDefinitionController(_db.CreateContext(), new LoggerService(NullLogger<LoggerService>.Instance, akses));
        c.ControllerContext = new ControllerContext { HttpContext = akses.HttpContext! };
        return c;
    }

    private static CreateWorkflowDefinitionRequest Permintaan(string kode, bool massal) => new()
    {
        WorkflowCode = kode,
        WorkflowName = "Uji massal " + kode,
        RequestType = "OVERTIME_REQUEST",
        WorkflowCategory = "HumanResource",
        Version = 1,
        WorkflowStatus = "Draft",
        AllowBulkApproval = massal,
    };

    private async Task<MstWorkflowDefinition> BacaDefinisiAsync(string kode)
    {
        await using var ctx = _db.CreateContext();
        return await ctx.Set<MstWorkflowDefinition>().AsNoTracking().SingleAsync(x => x.WorkflowCode == kode);
    }

    // AC1 (model) — kolom baru tidak nullable, bawaan false.
    [Fact]
    public void AllowBulkApproval_wajib_dengan_bawaan_false()
    {
        using var ctx = _db.CreateContext();
        var p = ctx.Model.FindEntityType(typeof(MstWorkflowDefinition))!.FindProperty(nameof(MstWorkflowDefinition.AllowBulkApproval));
        Assert.NotNull(p);
        Assert.False(p!.IsNullable);
        Assert.Equal(false, p.GetDefaultValue());
        Assert.False(new MstWorkflowDefinition().AllowBulkApproval);
    }

    // AC1 + AC3 (model) — FK diri sendiri dengan Restrict, ber-index, opsional; konstanta aksi Escalate.
    [Fact]
    public void EscalatedFromAssignmentId_FK_diri_sendiri_Restrict_dan_berindex()
    {
        using var ctx = _db.CreateContext();
        var t = ctx.Model.FindEntityType(typeof(TrxWorkflowApproverAssignment))!;
        var p = t.FindProperty(nameof(TrxWorkflowApproverAssignment.EscalatedFromAssignmentId));
        Assert.NotNull(p);
        Assert.True(p!.IsNullable);

        var fk = Assert.Single(t.GetForeignKeys(), x => x.Properties.Single().Name == nameof(TrxWorkflowApproverAssignment.EscalatedFromAssignmentId));
        Assert.Equal(typeof(TrxWorkflowApproverAssignment), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        Assert.Contains(t.GetIndexes(), x => x.Properties.Count == 1 && x.Properties[0].Name == nameof(TrxWorkflowApproverAssignment.EscalatedFromAssignmentId));

        Assert.Equal("Escalate", WorkflowValueConstants.ActionType.Escalate);
    }

    // AC3 — penugasan eskalasi tersimpan dan dapat membaca penugasan asalnya pada tahap yang sama.
    [Fact]
    public async Task Penugasan_eskalasi_tersimpan_dan_menunjuk_penugasan_asal()
    {
        var instance = Guid.NewGuid();
        var tahap = Guid.NewGuid();
        var asal = new TrxWorkflowApproverAssignment
        {
            WorkflowInstanceId = instance, WorkflowStepInstanceId = tahap, AssignedApproverUserId = Guid.NewGuid(),
            ApproverSourceSnapshot = WorkflowValueConstants.ApproverSource.RequesterManager,
            AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Available, AssignmentOrder = 1, IsRequired = true,
        };
        var eskalasi = new TrxWorkflowApproverAssignment
        {
            WorkflowInstanceId = instance, WorkflowStepInstanceId = tahap, AssignedApproverUserId = Guid.NewGuid(),
            ApproverSourceSnapshot = WorkflowValueConstants.ApproverSource.RequesterManager,
            AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Available, AssignmentOrder = 2, IsRequired = false,
            EscalatedFromAssignmentId = asal.Id,
        };
        await using (var ctx = _db.CreateContext())
        {
            ctx.Set<TrxWorkflowApproverAssignment>().AddRange(asal, eskalasi);
            await ctx.SaveChangesAsync();
        }

        await using var baca = _db.CreateContext();
        var hasil = await baca.Set<TrxWorkflowApproverAssignment>().AsNoTracking()
            .Include(x => x.EscalatedFromAssignment)
            .SingleAsync(x => x.Id == eskalasi.Id);
        Assert.Equal(asal.Id, hasil.EscalatedFromAssignmentId);
        Assert.NotNull(hasil.EscalatedFromAssignment);
        Assert.Equal(hasil.WorkflowStepInstanceId, hasil.EscalatedFromAssignment!.WorkflowStepInstanceId);
        Assert.False(hasil.IsRequired);

        var tanpa = await baca.Set<TrxWorkflowApproverAssignment>().AsNoTracking().SingleAsync(x => x.Id == asal.Id);
        Assert.Null(tanpa.EscalatedFromAssignmentId);
    }

    // AC2 — Master Workflow menyimpan, mengubah, dan mengembalikan allowBulkApproval.
    [Fact]
    public async Task Master_workflow_menyimpan_dan_mengembalikan_allowBulkApproval()
    {
        var buat = await Controller().CreateWorkflowDefinition(Permintaan("UJI_440_MASSAL", true));
        Assert.IsType<OkObjectResult>(buat);
        var d = await BacaDefinisiAsync("UJI_440_MASSAL");
        Assert.True(d.AllowBulkApproval);

        var detail = Assert.IsType<OkObjectResult>(await Controller().GetWorkflowDefinitionById(d.Id));
        var isi = Assert.IsType<ApiResponse<WorkflowDefinitionDetailResponse>>(detail.Value);
        Assert.True(isi.Data!.AllowBulkApproval);

        var ubah = Permintaan("UJI_440_MASSAL", false);
        var hasilUbah = await Controller().UpdateWorkflowDefinition(d.Id, ToUpdate(ubah));
        Assert.IsType<OkObjectResult>(hasilUbah);
        Assert.False((await BacaDefinisiAsync("UJI_440_MASSAL")).AllowBulkApproval);
    }

    // AC1 — definisi yang dibuat tanpa menyebut penanda bernilai false (perilaku definisi lama).
    [Fact]
    public async Task Definisi_tanpa_penanda_bernilai_false()
    {
        var r = Permintaan("UJI_440_BIASA", false);
        Assert.IsType<OkObjectResult>(await Controller().CreateWorkflowDefinition(r));
        Assert.False((await BacaDefinisiAsync("UJI_440_BIASA")).AllowBulkApproval);
    }

    // AC2 — daftar definisi Master Workflow memuat allowBulkApproval per baris.
    [Fact]
    public async Task Daftar_definisi_memuat_allowBulkApproval()
    {
        Assert.IsType<OkObjectResult>(await Controller().CreateWorkflowDefinition(Permintaan("UJI_440_DAFTAR_YA", true)));
        Assert.IsType<OkObjectResult>(await Controller().CreateWorkflowDefinition(Permintaan("UJI_440_DAFTAR_TIDAK", false)));

        var hasil = Assert.IsType<OkObjectResult>(await Controller().GetWorkflowDefinitions(
            null, null, null, null, null, null, null, null, null, null, null, "UJI_440_DAFTAR"));
        var isi = Assert.IsType<ApiResponse<PagedResult<WorkflowDefinitionResponse>>>(hasil.Value);
        Assert.True(isi.Data!.Items.Single(x => x.WorkflowCode == "UJI_440_DAFTAR_YA").AllowBulkApproval);
        Assert.False(isi.Data.Items.Single(x => x.WorkflowCode == "UJI_440_DAFTAR_TIDAK").AllowBulkApproval);
    }

    // AC1 — berkas migration hanya berisi dua kolom, FK, dan index; Down membalikkannya.
    [Fact]
    public void Migration_hanya_menambah_dua_kolom_fk_dan_index_dengan_down_yang_membalik()
    {
        var berkas = Directory.GetFiles(Path.Combine(RepoRoot(), "Migrations"), "*_AddWorkflowBulkFlagAndEscalationLink.cs")
            .Single(x => !x.EndsWith(".Designer.cs", StringComparison.Ordinal));
        var src = File.ReadAllText(berkas);
        var up = Regex.Match(src, @"protected override void Up\(MigrationBuilder migrationBuilder\)\s*\{(?<b>.*?)\n        \}", RegexOptions.Singleline).Groups["b"].Value;
        var down = Regex.Match(src, @"protected override void Down\(MigrationBuilder migrationBuilder\)\s*\{(?<b>.*?)\n        \}", RegexOptions.Singleline).Groups["b"].Value;

        var opsUp = Regex.Matches(up, @"migrationBuilder\.(\w+)").Select(m => m.Groups[1].Value).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "AddColumn", "AddColumn", "AddForeignKey", "CreateIndex" }, opsUp);
        Assert.Matches(@"name: ""AllowBulkApproval""[\s\S]*?table: ""MstWorkflowDefinition""[\s\S]*?defaultValue: false", up);
        Assert.Matches(@"name: ""EscalatedFromAssignmentId""[\s\S]*?table: ""TrxWorkflowApproverAssignment""[\s\S]*?nullable: true", up);
        Assert.Matches(@"principalTable: ""TrxWorkflowApproverAssignment""[\s\S]*?onDelete: ReferentialAction\.Restrict", up);

        var opsDown = Regex.Matches(down, @"migrationBuilder\.(\w+)").Select(m => m.Groups[1].Value).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "DropColumn", "DropColumn", "DropForeignKey", "DropIndex" }, opsDown);
    }

    private static UpdateWorkflowDefinitionRequest ToUpdate(CreateWorkflowDefinitionRequest r) => new()
    {
        WorkflowCode = r.WorkflowCode, WorkflowName = r.WorkflowName, RequestType = r.RequestType, WorkflowCategory = r.WorkflowCategory,
        Version = r.Version, WorkflowStatus = r.WorkflowStatus, AllowBulkApproval = r.AllowBulkApproval,
    };

    private static string RepoRoot()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d != null && !File.Exists(Path.Combine(d.FullName, "QuilvianSystemBackend.csproj"))) d = d.Parent;
        // Cadangan bila proyek uji dijalankan dari salinan di luar repository.
        return d?.FullName ?? Environment.GetEnvironmentVariable("QUILVIAN_BACKEND_ROOT")
            ?? throw new InvalidOperationException("Root repository tidak ditemukan.");
    }
}
