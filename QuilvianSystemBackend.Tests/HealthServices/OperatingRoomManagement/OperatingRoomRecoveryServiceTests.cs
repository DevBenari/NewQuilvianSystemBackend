using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomRecoveryServiceTests
{
    [Fact]
    public async Task SaveAnesthesiaRecordAsync_ByAnesthesiologist_CreatesDraftThenFinal()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        ctx.ActAs(ctx.AnesthesiologistUserId);

        var draft = await service.SaveAnesthesiaRecordAsync(ctx.CaseId, ValidAnesthesia("anes-draft"));
        Assert.Equal(OprRecordStatus.Draft, draft.Status);
        Assert.Equal(1, draft.Version);

        var final = await service.SaveAnesthesiaRecordAsync(ctx.CaseId,
            ValidAnesthesia("anes-final", finalize: true, expectedRecordVersion: 1));

        Assert.Equal(OprRecordStatus.Final, final.Status);
        Assert.NotNull(final.FinalizedAt);
    }

    [Fact]
    public async Task SaveAnesthesiaRecordAsync_BySurgeon_IsForbidden()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        await Assert.ThrowsAsync<OperatingRoomForbiddenException>(() =>
            service.SaveAnesthesiaRecordAsync(ctx.CaseId, ValidAnesthesia("anes-surgeon")));
    }

    [Fact]
    public async Task SaveAnesthesiaRecordAsync_AfterFinal_RejectsOpr010()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        ctx.ActAs(ctx.AnesthesiologistUserId);
        await service.SaveAnesthesiaRecordAsync(ctx.CaseId, ValidAnesthesia("anes-final", finalize: true));

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.SaveAnesthesiaRecordAsync(ctx.CaseId, ValidAnesthesia("anes-again", expectedRecordVersion: 1)));

        Assert.Equal("OPR010", exception.Code);
    }

    [Fact]
    public async Task SaveAnesthesiaRecordAsync_FinalizeWithMissingField_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        ctx.ActAs(ctx.AnesthesiologistUserId);
        var request = ValidAnesthesia("anes-incomplete", finalize: true);
        request.AirwaySummary = "  ";

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.SaveAnesthesiaRecordAsync(ctx.CaseId, request));

        Assert.Equal("AnesthesiaRecordIncomplete", exception.Code);
    }

    [Fact]
    public async Task SaveRecoveryAsync_MonitoringThenReleased_RecordsDecisionAndReleaser()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        ctx.ActAs(ctx.AnesthesiologistUserId);

        var monitoring = await service.SaveRecoveryAsync(ctx.CaseId, ValidRecovery("rec-1", OprRecoveryStatus.Monitoring));
        Assert.Equal(OprRecoveryStatus.Monitoring, monitoring.Status);
        Assert.Equal(2, monitoring.Observations.Count);

        var released = await service.SaveRecoveryAsync(ctx.CaseId, ValidRecovery("rec-2",
            OprRecoveryStatus.Released, OprRecoveryDecision.Inpatient, expectedRecordVersion: 1));

        Assert.Equal(OprRecoveryStatus.Released, released.Status);
        Assert.Equal(OprRecoveryDecision.Inpatient, released.Decision);
        Assert.Equal(ctx.AnesthesiologistUserId, released.ReleasedBy);
    }

    [Fact]
    public async Task SaveRecoveryAsync_ReleaseWithoutDecision_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        ctx.ActAs(ctx.AnesthesiologistUserId);

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.SaveRecoveryAsync(ctx.CaseId, ValidRecovery("rec-no-decision", OprRecoveryStatus.Released)));

        Assert.Equal("RecoveryDecisionRequired", exception.Code);
    }

    [Fact]
    public async Task SaveRecoveryAsync_AfterRelease_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        ctx.ActAs(ctx.AnesthesiologistUserId);
        await service.SaveRecoveryAsync(ctx.CaseId, ValidRecovery("rec-release",
            OprRecoveryStatus.Released, OprRecoveryDecision.Icu));

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.SaveRecoveryAsync(ctx.CaseId, ValidRecovery("rec-again",
                OprRecoveryStatus.Monitoring, expectedRecordVersion: 1)));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task CreateHandoverAsync_BeforeRecoveryRelease_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-early")));

        Assert.Equal("RecoveryNotReleased", exception.Code);
    }

    [Fact]
    public async Task CreateHandoverAsync_Twice_DoesNotCreateSecondPendingHandover()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await ReleaseRecoveryAsync(ctx, service);

        await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-1"));
        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-2")));

        Assert.Equal("OPR011", exception.Code);
        Assert.Equal(1, await ctx.Context.OprHandovers.CountAsync());
    }

    [Fact]
    public async Task CreateHandoverAsync_RetryWithSameKey_IsRepeatable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await ReleaseRecoveryAsync(ctx, service);

        var first = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-retry"));
        var second = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-retry"));

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await ctx.Context.OprHandovers.CountAsync());
    }

    [Fact]
    public async Task AcceptHandoverAsync_WithFinalRecordAndRelease_CompletesCaseOnce()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await FinalizeExecutionRecordAsync(ctx);
        await ReleaseRecoveryAsync(ctx, service);
        var handover = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-final"));

        ctx.ActAs(ctx.CirculatingNurseUserId);
        var result = await service.AcceptHandoverAsync(ctx.CaseId, handover.Id, new AcceptOprHandoverRequest
        {
            Accept = true, IdempotencyKey = "accept-1"
        });

        Assert.Equal(OprCaseStatus.Completed, result.Status);
        Assert.Empty(result.AvailableActions);
        Assert.Equal(1, await ctx.Context.OprStatusHistories
            .CountAsync(x => x.Action == "CompleteCase" && x.ToStatus == OprCaseStatus.Completed));
    }

    [Fact]
    public async Task AcceptHandoverAsync_WithoutFinalExecutionRecord_KeepsCaseInProgress()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await ReleaseRecoveryAsync(ctx, service);
        var handover = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-nofinal"));

        var result = await service.AcceptHandoverAsync(ctx.CaseId, handover.Id, new AcceptOprHandoverRequest
        {
            Accept = true, IdempotencyKey = "accept-nofinal"
        });

        Assert.Equal(OprCaseStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task AcceptHandoverAsync_Rejected_KeepsCaseInProgressAndAllowsResend()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await FinalizeExecutionRecordAsync(ctx);
        await ReleaseRecoveryAsync(ctx, service);
        var handover = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-reject"));

        var rejected = await service.AcceptHandoverAsync(ctx.CaseId, handover.Id, new AcceptOprHandoverRequest
        {
            Accept = false, RejectionReason = "Tempat tidur belum siap.", IdempotencyKey = "reject-1"
        });
        Assert.Equal(OprCaseStatus.InProgress, rejected.Status);

        var resent = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-resend"));
        Assert.Equal(2, resent.Revision);
    }

    [Fact]
    public async Task AcceptHandoverAsync_RejectWithoutReason_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await ReleaseRecoveryAsync(ctx, service);
        var handover = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-noreason"));

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.AcceptHandoverAsync(ctx.CaseId, handover.Id, new AcceptOprHandoverRequest
            {
                Accept = false, IdempotencyKey = "reject-noreason"
            }));

        Assert.Equal("RejectionReasonRequired", exception.Code);
    }

    [Fact]
    public async Task AcceptHandoverAsync_AlreadyProcessed_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await ReleaseRecoveryAsync(ctx, service);
        var handover = await service.CreateHandoverAsync(ctx.CaseId, ValidHandover(ctx, "handover-once"));
        await service.AcceptHandoverAsync(ctx.CaseId, handover.Id, new AcceptOprHandoverRequest
        {
            Accept = true, IdempotencyKey = "accept-once"
        });

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.AcceptHandoverAsync(ctx.CaseId, handover.Id, new AcceptOprHandoverRequest
            {
                Accept = true, IdempotencyKey = "accept-twice"
            }));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    private static OperatingRoomRecoveryService Build(OperatingRoomTestContext ctx) =>
        new(ctx.Context, ctx.Accessor, ctx.Logger);

    private static async Task ReleaseRecoveryAsync(OperatingRoomTestContext ctx, OperatingRoomRecoveryService service)
    {
        var previous = ctx.Accessor.CurrentUserId;
        ctx.ActAs(ctx.AnesthesiologistUserId);
        await service.SaveRecoveryAsync(ctx.CaseId, ValidRecovery($"release-{Guid.NewGuid():N}",
            OprRecoveryStatus.Released, OprRecoveryDecision.Inpatient));
        ctx.ActAs(previous);
    }

    private static async Task FinalizeExecutionRecordAsync(OperatingRoomTestContext ctx)
    {
        var record = await ctx.Context.OprExecutionRecords.FirstAsync(x => x.OprCaseId == ctx.CaseId);
        record.Status = OprRecordStatus.Final;
        record.FinishedAt = DateTime.UtcNow;
        record.FinalizedAt = DateTime.UtcNow;
        record.FinalizedBy = ctx.SurgeonUserId;
        await ctx.Context.SaveChangesAsync();
        ctx.Context.ChangeTracker.Clear();
    }

    private static SaveOprAnesthesiaRecordRequest ValidAnesthesia(string key, bool finalize = false,
        int expectedRecordVersion = 0) => new()
    {
        AssessmentSummary = "ASA II, tanpa riwayat alergi obat anestesi",
        Technique = "Anestesi umum dengan intubasi",
        MedicationFluidSummary = "Propofol, fentanil, ringer laktat 1000 ml",
        AirwaySummary = "Intubasi endotrakeal nomor 7,5 tanpa kesulitan",
        MonitoringSummary = "Tekanan darah, nadi, saturasi stabil sepanjang tindakan",
        FinalCondition = "Sadar penuh, napas spontan adekuat",
        Finalize = finalize,
        IdempotencyKey = key,
        ExpectedRecordVersion = expectedRecordVersion
    };

    private static SaveOprRecoveryRequest ValidRecovery(string key, OprRecoveryStatus status,
        OprRecoveryDecision? decision = null, int expectedRecordVersion = 0) => new()
    {
        ScoreSystem = "Aldrete",
        ScoreValue = 9,
        Status = status,
        Decision = decision,
        IdempotencyKey = key,
        ExpectedRecordVersion = expectedRecordVersion,
        Observations =
        [
            new OprRecoveryObservationRequest { Code = "BP", Label = "Tekanan darah", Value = "120/80" },
            new OprRecoveryObservationRequest { Code = "SPO2", Label = "Saturasi oksigen", Value = "99%" }
        ]
    };

    private static CreateOprHandoverRequest ValidHandover(OperatingRoomTestContext ctx, string key) => new()
    {
        DestinationUnitId = ctx.DestinationUnitId,
        ConditionSummary = "Pasien sadar, hemodinamik stabil",
        DeviceTherapySummary = "Infus perifer terpasang",
        IdempotencyKey = key
    };
}
