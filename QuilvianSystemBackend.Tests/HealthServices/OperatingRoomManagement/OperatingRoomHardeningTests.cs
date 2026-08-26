using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

/// <summary>
/// Pengujian pengerasan modul operasi (BE-OPR-011): regresi lifecycle ujung ke ujung,
/// kepatuhan permission terhadap `opr-permission-v1`, dan kelengkapan pendaftaran endpoint.
/// </summary>
public class OperatingRoomHardeningTests
{
    /// <summary>Pasangan resource/action yang persis disahkan `opr-permission-v1`.</summary>
    private static readonly HashSet<string> ContractPermissions =
    [
        "OperatingRoomCase:Read", "OperatingRoomCase:Create", "OperatingRoomCase:Update",
        "OperatingRoomCase:Cancel",
        "OperatingRoomSchedule:Update",
        "OperatingRoomPreparation:Read", "OperatingRoomPreparation:Update",
        "OperatingRoomExecution:Update",
        "OperatingRoomAnesthesia:Update",
        "OperatingRoomMaterial:Update",
        "OperatingRoomHandover:Update",
        "OperatingRoomIntegration:Update"
    ];

    /// <summary>
    /// Permission baca yang <b>belum</b> ada di `opr-permission-v1`. Endpoint GET pembacanya
    /// juga belum tercantum di `opr-api-v1`. Keduanya tambahan implementasi supaya frontend
    /// dapat memuat ulang satu bagian tanpa menarik seluruh workspace kasus, dan sengaja
    /// dibuat lebih ketat daripada `OperatingRoomCase : Read`. Perlu revisi kontrak dan
    /// persetujuan owner sebelum dianggap sah.
    /// </summary>
    private static readonly HashSet<string> PendingContractPermissions =
    [
        "OperatingRoomAnesthesia:Read",
        "OperatingRoomMaterial:Read",
        "OperatingRoomHandover:Read",
        "OperatingRoomIntegration:Read"
    ];

    [Fact]
    public void EveryEndpoint_DeclaresPermissionFromApprovedMatrix()
    {
        var offenders = new List<string>();
        foreach (var (controller, action) in OperatingRoomActions())
        {
            var permission = action.GetCustomAttribute<AccessPermissionAttribute>();
            if (permission == null)
            {
                offenders.Add($"{controller.Name}.{action.Name}: tanpa AccessPermission");
                continue;
            }
            // Nilai resource dan action tersimpan sebagai argumen filter, bukan properti.
            var arguments = permission.Arguments ?? [];
            if (arguments.Length < 2)
            {
                offenders.Add($"{controller.Name}.{action.Name}: AccessPermission tanpa argumen lengkap");
                continue;
            }
            var key = $"{arguments[0]}:{arguments[1]}";
            if (!ContractPermissions.Contains(key) && !PendingContractPermissions.Contains(key))
                offenders.Add($"{controller.Name}.{action.Name}: {key} di luar matrix");
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void PendingPermissions_AreOnlyUsedByAdditiveReadEndpoints()
    {
        // Menjaga agar tambahan di luar kontrak tidak menyebar ke endpoint yang mengubah data
        // dan tetap terlihat sampai kontrak direvisi.
        var pendingUsage = new List<string>();
        foreach (var (controller, action) in OperatingRoomActions())
        {
            var arguments = action.GetCustomAttribute<AccessPermissionAttribute>()?.Arguments;
            if (arguments is not { Length: >= 2 }) continue;
            var key = $"{arguments[0]}:{arguments[1]}";
            if (!PendingContractPermissions.Contains(key)) continue;

            var isReadOnly = action.GetCustomAttributes<HttpMethodAttribute>()
                .All(x => x.HttpMethods.All(m => m == "GET"));
            Assert.True(isReadOnly, $"{controller.Name}.{action.Name} memakai permission di luar kontrak untuk endpoint yang mengubah data.");
            pendingUsage.Add(key);
        }

        // Setiap permission di luar kontrak memang terpakai, sehingga daftarnya tidak
        // menyimpan sisa yang sudah tidak relevan.
        Assert.Equal(PendingContractPermissions.Order(), pendingUsage.Distinct().Order());
    }

    [Fact]
    public void EveryController_RequiresAuthenticationAndDeclaresModule()
    {
        var offenders = new List<string>();
        foreach (var controller in OperatingRoomControllers())
        {
            if (controller.GetCustomAttribute<AuthorizeAttribute>() == null)
                offenders.Add($"{controller.Name}: tanpa [Authorize]");
            var access = controller.GetCustomAttribute<AccessControllerAttribute>();
            if (access == null)
                offenders.Add($"{controller.Name}: tanpa [AccessController]");
            else if (access.ModuleCode != "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT")
                offenders.Add($"{controller.Name}: moduleCode tidak sesuai");
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void MutatingEndpoints_DeclareConflictAndUnprocessableResponses()
    {
        var offenders = new List<string>();
        foreach (var (controller, action) in OperatingRoomActions())
        {
            var isMutating = action.GetCustomAttributes<HttpMethodAttribute>()
                .Any(x => x.HttpMethods.Any(m => m is "POST" or "PUT" or "PATCH"));
            if (!isMutating) continue;
            var codes = action.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Select(x => x.StatusCode).ToHashSet();
            if (!codes.Contains(StatusCodes.Status409Conflict))
                offenders.Add($"{controller.Name}.{action.Name}: tanpa 409");
            if (!codes.Contains(StatusCodes.Status403Forbidden))
                offenders.Add($"{controller.Name}.{action.Name}: tanpa 403");
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void AvailableActions_AreEmptyForTerminalStatuses()
    {
        // Kasus terminal tidak boleh menawarkan tindakan lanjutan apa pun kepada frontend.
        var service = typeof(OperatingRoomSchedulingService).Assembly
            .GetType("QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services." +
                "OperatingRoomCommandSupport")!;
        var method = service.GetMethod("AvailableActions", BindingFlags.Public | BindingFlags.Static)!;

        var completed = (List<string>)method.Invoke(null, [OprCaseStatus.Completed])!;
        var cancelled = (List<string>)method.Invoke(null, [OprCaseStatus.Cancelled])!;
        var ready = (List<string>)method.Invoke(null, [OprCaseStatus.Ready])!;

        Assert.Empty(completed);
        Assert.Empty(cancelled);
        Assert.Contains("Start", ready);
    }

    [Fact]
    public async Task FullLifecycle_FromScheduledToCompleted_ProducesOneTransitionPerStatus()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Scheduled);
        var preparation = new OperatingRoomPreparationService(ctx.Context, ctx.Accessor, ctx.Logger);
        var integration = new OperatingRoomIntegrationService(ctx.Context, ctx.Accessor, ctx.Logger);
        var execution = new OperatingRoomExecutionService(ctx.Context, ctx.Accessor, ctx.Logger, integration);
        var recovery = new OperatingRoomRecoveryService(ctx.Context, ctx.Accessor, ctx.Logger);
        var material = new OperatingRoomMaterialService(ctx.Context, ctx.Accessor, ctx.Logger, integration);

        // Persiapan: checklist selesai lalu tiga sign-off menutup gerbang kesiapan.
        await preparation.SaveChecklistAsync(ctx.CaseId, OprChecklistPhase.SignIn, new SaveOprChecklistRequest
        {
            TemplateVersion = "WHO-SSC-2009",
            Complete = true,
            IdempotencyKey = "life-checklist",
            ExpectedVersion = await ctx.CurrentVersionAsync(),
            Items =
            [
                new OprChecklistItemRequest { Code = "IDENTITY", Label = "Identitas pasien", IsMandatory = true, IsChecked = true },
                new OprChecklistItemRequest { Code = "SITE", Label = "Lokasi tindakan", IsMandatory = true, IsChecked = true }
            ]
        });
        await SignOffAsync(ctx, preparation, OprReadinessRole.PrimarySurgeon, ctx.SurgeonUserId, "life-sign-1");
        await SignOffAsync(ctx, preparation, OprReadinessRole.Anesthesiologist, ctx.AnesthesiologistUserId, "life-sign-2");
        await SignOffAsync(ctx, preparation, OprReadinessRole.Nurse, ctx.ScrubNurseUserId, "life-sign-3");
        Assert.Equal(OprCaseStatus.Ready, await ctx.CurrentStatusAsync());

        // Pelaksanaan: dokter bedah memulai dan memfinalisasi catatan operasi.
        ctx.ActAs(ctx.SurgeonUserId);
        await execution.StartAsync(ctx.CaseId, new StartOprCaseRequest
        {
            ConfirmedPatientIdentity = true, ConfirmedProcedure = true, IdempotencyKey = "life-start",
            ExpectedVersion = await ctx.CurrentVersionAsync()
        });
        Assert.Equal(OprCaseStatus.InProgress, await ctx.CurrentStatusAsync());

        await material.RecordAsync(ctx.CaseId, new CreateOprMaterialUsageRequest
        {
            ExternalItemId = Guid.NewGuid(), ItemType = OprMaterialItemType.Consumable, Quantity = 4,
            UnitCode = "PCS", Outcome = OprMaterialOutcome.Used, IdempotencyKey = "life-material"
        });

        await execution.SaveRecordAsync(ctx.CaseId, new SaveOprExecutionRecordRequest
        {
            PreDiagnosis = "Apendisitis akut", PostDiagnosis = "Apendisitis perforasi",
            Findings = "Apendiks perforasi", Technique = "Apendektomi terbuka",
            PostPlan = "Antibiotik lanjut", Finalize = true, Outcome = OprCaseOutcome.Completed,
            IdempotencyKey = "life-record", ExpectedRecordVersion = 0
        });

        // Anestesi, recovery, lalu serah terima yang diterima unit tujuan.
        ctx.ActAs(ctx.AnesthesiologistUserId);
        await recovery.SaveAnesthesiaRecordAsync(ctx.CaseId, new SaveOprAnesthesiaRecordRequest
        {
            AssessmentSummary = "ASA II", Technique = "Anestesi umum",
            MedicationFluidSummary = "Propofol, RL 1000 ml", AirwaySummary = "Intubasi lancar",
            MonitoringSummary = "Stabil", FinalCondition = "Sadar penuh", Finalize = true,
            IdempotencyKey = "life-anes", ExpectedRecordVersion = 0
        });
        await recovery.SaveRecoveryAsync(ctx.CaseId, new SaveOprRecoveryRequest
        {
            ScoreSystem = "Aldrete", ScoreValue = 9, Status = OprRecoveryStatus.Released,
            Decision = OprRecoveryDecision.Inpatient, IdempotencyKey = "life-recovery",
            ExpectedRecordVersion = 0
        });
        var handover = await recovery.CreateHandoverAsync(ctx.CaseId, new CreateOprHandoverRequest
        {
            DestinationUnitId = ctx.DestinationUnitId, ConditionSummary = "Stabil",
            IdempotencyKey = "life-handover"
        });
        ctx.ActAs(ctx.CirculatingNurseUserId);
        var final = await recovery.AcceptHandoverAsync(ctx.CaseId, handover.Id, new AcceptOprHandoverRequest
        {
            Accept = true, IdempotencyKey = "life-accept"
        });

        Assert.Equal(OprCaseStatus.Completed, final.Status);
        Assert.Equal(OprCaseOutcome.Completed, await ctx.Context.OprCases.AsNoTracking()
            .Where(x => x.Id == ctx.CaseId).Select(x => x.Outcome).FirstAsync());

        // Setiap status hanya boleh dicapai satu kali sepanjang lifecycle.
        var transitions = await ctx.Context.OprStatusHistories.AsNoTracking()
            .Where(x => x.OprCaseId == ctx.CaseId && x.FromStatus != x.ToStatus)
            .Select(x => x.ToStatus).ToListAsync();
        Assert.Equal([OprCaseStatus.Ready, OprCaseStatus.InProgress, OprCaseStatus.Completed],
            transitions.Order().ToList());
        Assert.Equal(transitions.Count, transitions.Distinct().Count());

        // Outbox memuat penyerahan material dan tagihan tanpa duplikat.
        var reconciliation = await integration.GetReconciliationAsync(ctx.CaseId);
        Assert.Equal(2, reconciliation!.Deliveries.Count);
        Assert.Contains(reconciliation.Deliveries,
            x => x.Destination == OperatingRoomIntegrationService.InventoryDestination);
        Assert.Contains(reconciliation.Deliveries,
            x => x.Destination == OperatingRoomIntegrationService.BillingDestination);
        Assert.Equal(reconciliation.Deliveries.Count,
            reconciliation.Deliveries.Select(x => x.IdempotencyKey).Distinct().Count());
    }

    [Fact]
    public async Task CompletedCase_RejectsFurtherLifecycleCommands()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Completed);
        var integration = new OperatingRoomIntegrationService(ctx.Context, ctx.Accessor, ctx.Logger);
        var execution = new OperatingRoomExecutionService(ctx.Context, ctx.Accessor, ctx.Logger, integration);
        var scheduling = new OperatingRoomSchedulingService(ctx.Context, ctx.Accessor, ctx.Logger,
            new OperatingRoomCredentialResolver(ctx.Context),
            Options.Create(new OperatingRoomSchedulingOptions()));

        var cancel = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            execution.CancelAsync(ctx.CaseId, new CancelOprCaseRequest
            {
                Reason = "Terlambat.", IdempotencyKey = "done-cancel", ExpectedVersion = 0
            }));
        var postpone = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            scheduling.PostponeAsync(ctx.CaseId, new PostponeOprCaseRequest
            {
                Reason = "Terlambat.", ConfirmedByDoctorId = ctx.DoctorId,
                IdempotencyKey = "done-postpone", ExpectedVersion = 0
            }));

        Assert.Equal("InvalidStateTransition", cancel.Code);
        Assert.Equal("InvalidStateTransition", postpone.Code);
    }

    private static async Task SignOffAsync(OperatingRoomTestContext ctx,
        OperatingRoomPreparationService preparation, OprReadinessRole role, Guid userId, string key)
    {
        ctx.ActAs(userId);
        await preparation.CreateSignOffAsync(ctx.CaseId, new CreateOprReadinessSignOffRequest
        {
            Role = role, IdempotencyKey = key, ExpectedVersion = await ctx.CurrentVersionAsync()
        });
    }

    private static IEnumerable<Type> OperatingRoomControllers() =>
        typeof(OperatingRoomCaseController).Assembly.GetTypes()
            .Where(x => x.Namespace == typeof(OperatingRoomCaseController).Namespace &&
                typeof(ControllerBase).IsAssignableFrom(x) && !x.IsAbstract);

    private static IEnumerable<(Type Controller, MethodInfo Action)> OperatingRoomActions() =>
        OperatingRoomControllers().SelectMany(controller => controller
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(action => action.GetCustomAttributes<HttpMethodAttribute>().Any())
            .Select(action => (controller, action)));
}
