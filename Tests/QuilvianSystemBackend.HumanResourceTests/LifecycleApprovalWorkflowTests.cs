using System.Data.Common;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// B6 (INT-HR-30; PLANNING_PERSETUJUAN_T15_T18_KONTRAK_DAN_MASA_KERJA.md bagian 8) — dokumen T15–T18 masuk mesin
// persetujuan NYATA (WorkflowService di SQLite): instance dibuat atas nama pengaju dengan langkah Jabatan Kepala Bagian
// SDM; setuju/tolak diteruskan ke service domain dengan penyetuju yang benar; minta revisi → Dikembalikan → ajukan ulang
// instance yang sama; tarik ikut menarik instance; gagal tertutup bila langkah belum diatur; swa-setuju ditolak.
// Service domain (ApplyDecisionAsync dan seterusnya) diganti perekam agar uji fokus pada sambungan.
public sealed class LifecycleApprovalWorkflowTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _jabatanSdm = Guid.NewGuid();
    private readonly Guid _alasanTolak = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User)> _p = new();
    private readonly Guid _kontrakRizky = Guid.NewGuid();

    private readonly IHrdEmploymentContractService _kontrak;
    private readonly PenerapRekam _rekamKontrak;
    private readonly IHrdProbationService _percobaan;
    private readonly PenerapRekam _rekamPercobaan;
    private readonly IHrdInitialContractBatchService _batch;

    private sealed class KunciPostgresKeSqlite : DbCommandInterceptor
    {
        private static void Ubah(DbCommand c)
        {
            if (!c.CommandText.Contains("FOR UPDATE", StringComparison.Ordinal)) return;
            c.CommandText = c.CommandText.Replace("public.\"", "\"", StringComparison.Ordinal).Replace("FOR UPDATE", string.Empty, StringComparison.Ordinal);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r) { Ubah(c); return r; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r, CancellationToken t = default) { Ubah(c); return ValueTask.FromResult(r); }
    }

    public LifecycleApprovalWorkflowTests()
    {
        (_kontrak, _rekamKontrak) = PenerapRekam.Buat<IHrdEmploymentContractService>();
        (_percobaan, _rekamPercobaan) = PenerapRekam.Buat<IHrdProbationService>();
        (_batch, _) = PenerapRekam.Buat<IHrdInitialContractBatchService>();

        using var ctx = _db.CreateContext();
        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var (nama, jabatan) in new[] { ("Yudha", Guid.NewGuid()), ("Hartini", _jabatanSdm), ("Rizky", Guid.NewGuid()) })
        {
            var wf = Guid.NewGuid();
            var user = Guid.NewGuid();
            _p[nama] = (wf, user);
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = wf, ProfileCode = "P-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
            ctx.Users.Add(new ApplicationUser { Id = user, UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = wf, IsActive = true });
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = wf, DepartmentId = Guid.NewGuid(), PositionId = jabatan, AssignmentType = "Primary",
                IsPrimary = true, EffectiveStartDate = mulai, IsActive = true,
            });
        }

        ctx.WfpContractHistories.Add(new WfpContractHistory
        {
            Id = _kontrakRizky, WorkforceProfileId = _p["Rizky"].Wf, ContractNumber = "PKWT-0042", HistoryType = "Initial", ContractStatus = "Active",
            StartDate = new DateTime(2025, 12, 10), EndDate = new DateTime(2026, 12, 9), IsCurrent = true, IsActive = true
        });

        // Satu langkah Jabatan Kepala Bagian SDM untuk T15, T16, T18 (keputusan 3); T17 sengaja tanpa langkah.
        foreach (var (kode, jenis) in new[] { ("T15", "ProbationAssessment"), ("T16", "ContractDecision"), ("T17", "ContractCorrection"), ("T18", "InitialContractBatch") })
        {
            var definisi = Guid.NewGuid();
            ctx.MstWorkflowDefinitions.Add(new MstWorkflowDefinition
            {
                Id = definisi, WorkflowCode = kode, WorkflowName = "Uji " + kode, RequestType = jenis, WorkflowCategory = "OnboardingLifecycle",
                Version = 1, WorkflowStatus = "Active", IsActive = true, AllowRequesterWithdraw = true, AllowRequesterCancel = true
            });
            if (kode == "T17") continue;
            ctx.Set<MstWorkflowStep>().Add(new MstWorkflowStep
            {
                WorkflowDefinitionId = definisi, StepCode = kode + "-SDM", StepName = "Kepala Bagian SDM", StepOrder = 1,
                StepType = WorkflowValueConstants.StepType.Approval, ApprovalMode = WorkflowValueConstants.ApprovalMode.Any, RequiredApprovalCount = 1,
                ApproverSourceType = WorkflowValueConstants.ApproverSource.Position, ApproverPositionId = _jabatanSdm,
                IsRequired = true, AllowSelfApproval = false, IsActive = true
            });
        }

        ctx.Set<MstRejectionReason>().Add(new MstRejectionReason
        {
            Id = _alasanTolak, RequestType = "ContractDecision", ReasonCode = "T16-TOLAK-01", ReasonName = "Tidak sesuai kebijakan",
            RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsCommentRequired = true, AllowResubmit = false, IsActive = true
        });

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private ApplicationDbContext Konteks() => _db.CreateContext(new KunciPostgresKeSqlite());

    private ServiceProvider Layanan(ApplicationDbContext ctx, Guid pelaku)
    {
        var services = new ServiceCollection();
        services.AddSingleton(ctx);
        services.AddSingleton(_kontrak);
        services.AddSingleton(_percobaan);
        services.AddSingleton(_batch);
        services.AddSingleton<IHrdEmploymentLifecycleWorkflowIntegrationService>(sp =>
            new HrdEmploymentLifecycleWorkflowIntegrationService(ctx, sp, NullLogger<HrdEmploymentLifecycleWorkflowIntegrationService>.Instance));
        services.AddSingleton(sp =>
        {
            var handler = new HrdEmploymentLifecycleWorkflowLifecycleHandler(ctx, sp, NullLogger<HrdEmploymentLifecycleWorkflowLifecycleHandler>.Instance);
            return new WorkflowReferenceLifecycleService(
                ctx, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,
                NullLogger<WorkflowReferenceLifecycleService>.Instance, new IWorkflowReferenceLifecycleHandler[] { handler });
        });
        services.AddSingleton(sp =>
        {
            var http = Pelaku.Sebagai(pelaku);
            var userManager = new UserManager<ApplicationUser>(
                new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
                Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!,
                NullLogger<UserManager<ApplicationUser>>.Instance);
            return new WorkflowService(ctx, new HumanResourceContextService(ctx, http, userManager), userManager, http,
                sp.GetRequiredService<WorkflowReferenceLifecycleService>());
        });
        return services.BuildServiceProvider();
    }

    private Guid Usulan(ContractDecisionStatus status = ContractDecisionStatus.Submitted, string pengaju = "Yudha")
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.HrdContractDecisions.Add(new HrdContractDecision
        {
            Id = id, DecisionNumber = "CDA-UJI-" + id.ToString("N")[..6], ContractHistoryId = _kontrakRizky, WorkforceProfileId = _p["Rizky"].Wf,
            DecisionType = ContractDecisionType.Extend, ExtensionMonths = 12, ProposedStartDate = new DateOnly(2026, 12, 10), ProposedEndDate = new DateOnly(2027, 12, 9),
            ReasonText = "Kinerja baik", DecisionStatus = status, SubmittedByUserId = _p[pengaju].User, SubmittedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();
        return id;
    }

    private HrdContractDecision BacaUsulan(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdContractDecisions.AsNoTracking().Single(x => x.Id == id);
    }

    private TrxWorkflowInstance BacaInstance(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == id);
    }

    private async Task<LifecycleWorkflowResult> AjukanT16Async(Guid decisionId, string pengaju = "Yudha")
    {
        await using var ctx = Konteks();
        await using var sp = Layanan(ctx, _p[pengaju].User);
        var r = await sp.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>()
            .StartContractDecisionWorkflowAsync(decisionId, _p[pengaju].User);
        if (r.Success)
        {
            var d = await ctx.HrdContractDecisions.SingleAsync(x => x.Id == decisionId);
            d.WorkflowInstanceId = r.WorkflowInstanceId;
            await ctx.SaveChangesAsync();
        }

        return r;
    }

    private Guid? TugasHartini(Guid workflowId)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowApproverAssignments.AsNoTracking()
            .Where(x => x.WorkflowInstanceId == workflowId && x.AssignedApproverUserId == _p["Hartini"].User)
            .OrderByDescending(x => x.CreateDateTime).Select(x => (Guid?)x.Id).FirstOrDefault();
    }

    private async Task AksiHartiniAsync(Guid workflowId, Func<WorkflowService, Guid, Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>>> aksi)
    {
        await using var ctx = Konteks();
        await using var sp = Layanan(ctx, _p["Hartini"].User);
        var tugas = TugasHartini(workflowId);
        Assert.NotNull(tugas);
        var r = await aksi(sp.GetRequiredService<WorkflowService>(), tugas!.Value);
        Assert.True(r.Success, r.Message);
    }

    private Task SetujuiAsync(Guid wf, string catatan = "Lanjut, kinerja baik.") =>
        AksiHartiniAsync(wf, (w, t) => w.ApproveAsync(wf, t, new WorkflowApproveRequest { Comment = catatan }));

    private Task MintaRevisiAsync(Guid wf, string catatan) =>
        AksiHartiniAsync(wf, (w, t) => w.RequestRevisionAsync(wf, t, new WorkflowRequestRevisionRequest { Comment = catatan }));

    [Fact]
    public async Task Ajukan_T16_membuat_instance_dengan_tugas_untuk_jabatan_Kepala_Bagian_SDM_dan_ringkasan()
    {
        var id = Usulan();

        var r = await AjukanT16Async(id);

        Assert.True(r.Success, r.Message);
        var instance = BacaInstance(r.WorkflowInstanceId!.Value);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.InProgress, instance.WorkflowStatus);
        Assert.Equal(EmploymentLifecycleWorkflowCodes.ContractDecisionReferenceType, instance.ReferenceType);
        Assert.Equal(_p["Yudha"].User, instance.RequestedByUserId);
        Assert.Contains("\"summary\"", instance.RequestContextJson);
        Assert.Contains("Rizky", instance.RequestContextJson);
        Assert.Contains("Perpanjang 12 bulan", instance.RequestContextJson);
        Assert.Contains(_kontrakRizky.ToString(), instance.RequestContextJson);
        Assert.NotNull(TugasHartini(instance.Id));
        Assert.Equal(ContractDecisionStatus.Submitted, BacaUsulan(id).DecisionStatus);
    }

    [Fact]
    public async Task Setujui_T16_menerapkan_keputusan_lewat_service_domain_dengan_penyetuju_dan_catatan()
    {
        var id = Usulan();
        var wf = (await AjukanT16Async(id)).WorkflowInstanceId!.Value;

        await SetujuiAsync(wf, "Lanjut, kinerja baik.");

        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Completed, BacaInstance(wf).WorkflowStatus);
        var panggilan = Assert.Single(_rekamKontrak.Panggilan, x => x.Metode == nameof(IHrdEmploymentContractService.ApplyDecisionAsync));
        Assert.Equal(id, panggilan.Argumen[0]);
        Assert.Equal(_p["Hartini"].User, panggilan.Argumen[1]);
        Assert.Equal(true, panggilan.Argumen[2]);
        Assert.Equal("Lanjut, kinerja baik.", panggilan.Argumen[3]);
    }

    [Fact]
    public async Task Minta_revisi_menjadi_Dikembalikan_lalu_ajukan_ulang_memakai_instance_yang_sama()
    {
        var id = Usulan();
        var wf = (await AjukanT16Async(id)).WorkflowInstanceId!.Value;

        await MintaRevisiAsync(wf, "Lengkapi alasan perpanjangan.");

        var dikembalikan = BacaUsulan(id);
        Assert.Equal(ContractDecisionStatus.Returned, dikembalikan.DecisionStatus);
        Assert.Equal("Lengkapi alasan perpanjangan.", dikembalikan.DecisionNote);
        Assert.Empty(_rekamKontrak.Panggilan);

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _p["Yudha"].User))
        {
            var ulang = await sp.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>().ResubmitAsync(wf, "Sudah dilengkapi.");
            Assert.True(ulang.Success, ulang.Message);
            Assert.Equal(wf, ulang.WorkflowInstanceId);
        }

        Assert.Equal(ContractDecisionStatus.Submitted, BacaUsulan(id).DecisionStatus);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.InProgress, BacaInstance(wf).WorkflowStatus);

        await SetujuiAsync(wf);
        Assert.Single(_rekamKontrak.Panggilan, x => x.Metode == nameof(IHrdEmploymentContractService.ApplyDecisionAsync));
    }

    [Fact]
    public async Task Tolak_final_menerapkan_keputusan_ditolak()
    {
        var id = Usulan();
        var wf = (await AjukanT16Async(id)).WorkflowInstanceId!.Value;

        await AksiHartiniAsync(wf, (w, t) => w.RejectAsync(wf, t, new WorkflowRejectRequest { RejectionReasonId = _alasanTolak, Comment = "Batas PKWT tidak memungkinkan." }));

        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Rejected, BacaInstance(wf).WorkflowStatus);
        var panggilan = Assert.Single(_rekamKontrak.Panggilan, x => x.Metode == nameof(IHrdEmploymentContractService.ApplyDecisionAsync));
        Assert.Equal(false, panggilan.Argumen[2]);
        Assert.Equal(_p["Hartini"].User, panggilan.Argumen[1]);
    }

    [Fact]
    public async Task Tarik_saat_Dikembalikan_menarik_instance_dan_usulan()
    {
        var id = Usulan();
        var wf = (await AjukanT16Async(id)).WorkflowInstanceId!.Value;
        await MintaRevisiAsync(wf, "Periksa tanggal.");

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _p["Yudha"].User))
        {
            var r = await sp.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>().WithdrawAsync(wf, _p["Yudha"].User, "Tidak jadi diajukan.");
            Assert.True(r.Success, r.Message);
        }

        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Withdrawn, BacaInstance(wf).WorkflowStatus);
        Assert.Equal(ContractDecisionStatus.Withdrawn, BacaUsulan(id).DecisionStatus);
        Assert.Null(TugasHartiniTerbuka(wf));
    }

    [Fact]
    public async Task Tarik_oleh_bukan_pengaju_ditolak()
    {
        var id = Usulan();
        var wf = (await AjukanT16Async(id)).WorkflowInstanceId!.Value;

        await using var ctx = Konteks();
        await using var sp = Layanan(ctx, _p["Hartini"].User);
        var r = await sp.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>().WithdrawAsync(wf, _p["Hartini"].User, "Coba tarik.");

        Assert.False(r.Success);
        Assert.Equal(403, r.StatusCode);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.InProgress, BacaInstance(wf).WorkflowStatus);
    }

    [Fact]
    public async Task Definisi_tanpa_langkah_gagal_tertutup_dengan_pesan_untuk_admin()
    {
        Guid koreksi;
        using (var c = _db.CreateContext())
        {
            koreksi = Guid.NewGuid();
            c.HrdContractCorrections.Add(new HrdContractCorrection
            {
                Id = koreksi, CorrectionNumber = "CCA-UJI-01", ContractHistoryId = _kontrakRizky, BeforeContractNumber = "PKWT-0042", AfterContractNumber = "PKWT-0042A",
                BeforeContractTypeId = Guid.NewGuid(), AfterContractTypeId = Guid.NewGuid(), BeforeStartDate = new DateOnly(2025, 12, 10), AfterStartDate = new DateOnly(2025, 12, 10),
                ReasonText = "Salah ketik nomor", EvidenceStoragePath = "x", EvidenceFileName = "scan.pdf", EvidenceContentType = "application/pdf",
                CorrectionStatus = ContractCorrectionStatus.Submitted, SubmittedByUserId = _p["Yudha"].User, SubmittedAt = DateTime.UtcNow
            });
            c.SaveChanges();
        }

        await using var ctx = Konteks();
        await using var sp = Layanan(ctx, _p["Yudha"].User);
        var r = await sp.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>().StartContractCorrectionWorkflowAsync(koreksi, _p["Yudha"].User);

        Assert.False(r.Success);
        Assert.Equal(400, r.StatusCode);
        Assert.Contains("Langkah Workflow T17", r.Message);
        Assert.False(await ctx.TrxWorkflowInstances.AnyAsync(x => x.ReferenceId == koreksi));
    }

    [Fact]
    public async Task Pengaju_pemegang_jabatan_penyetuju_tidak_dapat_menyetujui_sendiri()
    {
        var id = Usulan(pengaju: "Hartini");

        var r = await AjukanT16Async(id, "Hartini");

        Assert.False(r.Success);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Instance_lama_yang_bukan_milik_dokumen_diabaikan()
    {
        var id = Usulan();
        var wf = (await AjukanT16Async(id)).WorkflowInstanceId!.Value;
        using (var c = _db.CreateContext())
        {
            var d = c.HrdContractDecisions.Single(x => x.Id == id);
            d.WorkflowInstanceId = Guid.NewGuid();
            c.SaveChanges();
        }

        await SetujuiAsync(wf);

        Assert.Empty(_rekamKontrak.Panggilan);
        Assert.Equal(ContractDecisionStatus.Submitted, BacaUsulan(id).DecisionStatus);
    }

    [Fact]
    public async Task Setujui_T15_meneruskan_ke_penerapan_penilaian_masa_percobaan()
    {
        var penilaian = Guid.NewGuid();
        using (var c = _db.CreateContext())
        {
            var percobaan = Guid.NewGuid();
            c.HrdProbations.Add(new HrdProbation
            {
                Id = percobaan, WorkforceProfileId = _p["Rizky"].Wf, ContractHistoryId = _kontrakRizky, StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 11, 30),
                ReminderFromDate = new DateOnly(2026, 11, 16), ProbationStatus = ProbationStatus.AwaitingDecision, RowVersion = Guid.NewGuid()
            });
            c.HrdProbationAssessments.Add(new HrdProbationAssessment
            {
                Id = penilaian, AssessmentNumber = "PBA-UJI-01", ProbationId = percobaan, Result = ProbationResult.Pass, ReasonText = "Kompeten",
                AssessmentStatus = ProbationAssessmentStatus.Submitted, SubmittedByUserId = _p["Yudha"].User, SubmittedAt = DateTime.UtcNow, RowVersion = Guid.NewGuid()
            });
            c.SaveChanges();
        }

        Guid wf;
        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _p["Yudha"].User))
        {
            var r = await sp.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>().StartProbationAssessmentWorkflowAsync(penilaian, _p["Yudha"].User);
            Assert.True(r.Success, r.Message);
            wf = r.WorkflowInstanceId!.Value;
            var a = await ctx.HrdProbationAssessments.SingleAsync(x => x.Id == penilaian);
            a.WorkflowInstanceId = wf;
            await ctx.SaveChangesAsync();
        }

        await SetujuiAsync(wf, "Lulus.");

        var panggilan = Assert.Single(_rekamPercobaan.Panggilan, x => x.Metode == nameof(IHrdProbationService.ApplyAssessmentDecisionAsync));
        Assert.Equal(penilaian, panggilan.Argumen[0]);
        Assert.Equal(_p["Hartini"].User, panggilan.Argumen[1]);
        Assert.Equal(true, panggilan.Argumen[2]);
    }

    [Fact]
    public async Task Penerapan_batch_data_awal_sekali_dan_pengaju_tidak_boleh_memutus_sendiri()
    {
        var batch = Guid.NewGuid();
        using (var c = _db.CreateContext())
        {
            c.HrdInitialContractBatches.Add(new HrdInitialContractBatch
            {
                Id = batch, BatchNumber = "ICB-UJI-01", HospitalSiteId = Guid.NewGuid(), BatchStatus = InitialContractBatchStatus.Submitted,
                TotalRows = 2, ValidRows = 2, SubmittedByUserId = _p["Yudha"].User, SubmittedAt = DateTime.UtcNow, RowVersion = Guid.NewGuid()
            });
            foreach (var nomor in new[] { "LAMA-01", "LAMA-02" })
            {
                c.HrdInitialContractBatchRows.Add(new HrdInitialContractBatchRow
                {
                    Id = Guid.NewGuid(), BatchId = batch, WorkforceProfileId = Guid.NewGuid(), ContractTypeId = Guid.NewGuid(), ContractNumber = nomor,
                    StartDate = new DateOnly(2024, 1, 1), IsValid = true
                });
            }

            c.SaveChanges();
        }

        await using var ctx = Konteks();
        var layanan = new HrdInitialContractBatchService(ctx, new HrdNumberSeriesService(ctx), Pengganti.Buat<IHrdEmploymentLifecycleWorkflowIntegrationService>());

        var sendiri = await layanan.ApplyInitialBatchAsync(batch, _p["Yudha"].User, true, null);
        Assert.False(sendiri.Success);

        Assert.True((await layanan.ApplyInitialBatchAsync(batch, _p["Hartini"].User, true, null)).Success);
        Assert.True((await layanan.ApplyInitialBatchAsync(batch, _p["Hartini"].User, true, null)).Success);

        Assert.Equal(2, await ctx.WfpContractHistories.CountAsync(x => x.SourceDocumentId == batch));
        Assert.Equal(InitialContractBatchStatus.Applied, (await ctx.HrdInitialContractBatches.AsNoTracking().SingleAsync(x => x.Id == batch)).BatchStatus);
    }

    private Guid? TugasHartiniTerbuka(Guid workflowId)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowApproverAssignments.AsNoTracking()
            .Where(x => x.WorkflowInstanceId == workflowId && x.AssignedApproverUserId == _p["Hartini"].User &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Cancelled &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Completed &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Approved &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Rejected &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Skipped &&
                        x.IsCurrentAssignment)
            .Select(x => (Guid?)x.Id).FirstOrDefault();
    }
}

// Perekam service domain: mencatat metode dan argumen; Apply*Async mengembalikan sukses.
public class PenerapRekam : DispatchProxy
{
    public List<(string Metode, object?[] Argumen)> Panggilan { get; } = new();

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        Panggilan.Add((targetMethod!.Name, args ?? Array.Empty<object?>()));
        if (targetMethod.ReturnType == typeof(Task<RecruitmentResult<bool>>))
        {
            return Task.FromResult(RecruitmentResult<bool>.Ok(true));
        }

        throw new NotSupportedException($"Perekam tidak menangani {targetMethod.Name}.");
    }

    public static (T Proxy, PenerapRekam Rekam) Buat<T>() where T : class
    {
        var proxy = Create<T, PenerapRekam>();
        return (proxy, (PenerapRekam)(object)proxy);
    }
}
