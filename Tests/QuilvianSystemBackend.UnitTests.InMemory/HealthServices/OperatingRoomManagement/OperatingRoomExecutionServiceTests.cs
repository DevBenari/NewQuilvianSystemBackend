using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomExecutionServiceTests
{
    [Fact]
    public async Task StartAsync_FromReadyByPrimarySurgeon_MovesToInProgressAndCreatesDraftRecord()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);

        var result = await service.StartAsync(ctx.CaseId, ValidStart("start-1"));

        Assert.Equal(OprCaseStatus.InProgress, result.Status);
        var record = await ctx.Context.OprExecutionRecords.SingleAsync();
        Assert.Equal(OprRecordStatus.Draft, record.Status);
        Assert.Equal(1, await ctx.Context.OprStatusHistories.CountAsync(x => x.Action == "Start"));
    }

    [Fact]
    public async Task StartAsync_FromScheduled_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Scheduled);
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.StartAsync(ctx.CaseId, ValidStart("start-early")));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task StartAsync_WithoutConfirmation_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);
        var request = ValidStart("start-unconfirmed");
        request.ConfirmedProcedure = false;

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.StartAsync(ctx.CaseId, request));

        Assert.Equal("StartNotConfirmed", exception.Code);
        Assert.Equal(OprCaseStatus.Ready, await ctx.CurrentStatusAsync());
    }

    [Fact]
    public async Task StartAsync_ByNurse_IsForbidden()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);
        ctx.ActAs(ctx.ScrubNurseUserId);

        await Assert.ThrowsAsync<OperatingRoomForbiddenException>(() =>
            service.StartAsync(ctx.CaseId, ValidStart("start-nurse")));
    }

    [Fact]
    public async Task StartAsync_SameIdempotencyKey_IsRepeatable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);

        var first = await service.StartAsync(ctx.CaseId, ValidStart("start-twice"));
        var second = await service.StartAsync(ctx.CaseId, ValidStart("start-twice"));

        Assert.Equal(first.Version, second.Version);
        Assert.Equal(1, await ctx.Context.OprExecutionRecords.CountAsync());
    }

    [Fact]
    public async Task SaveRecordAsync_DraftThenFinalize_SetsOutcomeAndLocksRecord()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var draft = await service.SaveRecordAsync(ctx.CaseId, ValidRecord("record-draft"));
        Assert.Equal(OprRecordStatus.Draft, draft.Status);
        Assert.Equal(1, draft.Version);

        var final = await service.SaveRecordAsync(ctx.CaseId,
            ValidRecord("record-final", finalize: true, outcome: OprCaseOutcome.Completed, expectedRecordVersion: 1));

        Assert.Equal(OprRecordStatus.Final, final.Status);
        Assert.Equal(OprCaseOutcome.Completed, final.CaseOutcome);
        Assert.NotNull(final.FinishedAt);
        Assert.NotNull(final.FinalizedAt);
        Assert.Equal(OprCaseStatus.InProgress, final.CaseStatus);
    }

    [Fact]
    public async Task SaveRecordAsync_StoppedEarly_IsRecordedAsOutcomeNotCancellation()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var final = await service.SaveRecordAsync(ctx.CaseId,
            ValidRecord("record-stopped", finalize: true, outcome: OprCaseOutcome.StoppedEarly));

        Assert.Equal(OprCaseOutcome.StoppedEarly, final.CaseOutcome);
        Assert.Equal(OprCaseStatus.InProgress, await ctx.CurrentStatusAsync());
    }

    [Fact]
    public async Task SaveRecordAsync_AfterFinal_RejectsOpr010()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        await service.SaveRecordAsync(ctx.CaseId,
            ValidRecord("record-final", finalize: true, outcome: OprCaseOutcome.Completed));

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.SaveRecordAsync(ctx.CaseId, ValidRecord("record-after-final", expectedRecordVersion: 1)));

        Assert.Equal("OPR010", exception.Code);
    }

    [Fact]
    public async Task SaveRecordAsync_FinalizeWithoutOutcome_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var request = ValidRecord("record-no-outcome", finalize: true);

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.SaveRecordAsync(ctx.CaseId, request));

        Assert.Equal("OutcomeRequired", exception.Code);
    }

    [Fact]
    public async Task SaveRecordAsync_FinalizeWithIncompleteFields_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var request = ValidRecord("record-incomplete", finalize: true, outcome: OprCaseOutcome.Completed);
        request.Findings = "   ";

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.SaveRecordAsync(ctx.CaseId, request));

        Assert.Equal("ExecutionRecordIncomplete", exception.Code);
    }

    [Fact]
    public async Task SaveRecordAsync_StaleRecordVersion_RejectsOpr012()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.SaveRecordAsync(ctx.CaseId, ValidRecord("record-stale", expectedRecordVersion: 7)));

        Assert.Equal("OPR012", exception.Code);
    }

    [Fact]
    public async Task CreateAddendumAsync_OnFinalRecord_AppendsWithoutChangingRecord()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var final = await service.SaveRecordAsync(ctx.CaseId,
            ValidRecord("record-final", finalize: true, outcome: OprCaseOutcome.Completed));

        var addendum = await service.CreateAddendumAsync(ctx.CaseId, new CreateOprExecutionAddendumRequest
        {
            Content = "Perbaikan jumlah perdarahan menjadi 250 ml.",
            Reason = "Salah catat saat operasi berlangsung.",
            IdempotencyKey = "addendum-1"
        });

        Assert.Equal("Perbaikan jumlah perdarahan menjadi 250 ml.", addendum.Content);
        var reloaded = await service.GetRecordAsync(ctx.CaseId);
        Assert.Equal(final.Version, reloaded!.Version);
        Assert.Single(reloaded.Addenda);
    }

    [Fact]
    public async Task CreateAddendumAsync_OnDraftRecord_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.CreateAddendumAsync(ctx.CaseId, new CreateOprExecutionAddendumRequest
            {
                Content = "Isi addendum.", Reason = "Alasan.", IdempotencyKey = "addendum-draft"
            }));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task CancelAsync_FromScheduledWithReason_MovesToCancelledAndReleasesPlan()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Scheduled);
        var service = Build(ctx);

        var result = await service.CancelAsync(ctx.CaseId, new CancelOprCaseRequest
        {
            Reason = "Pasien menolak tindakan.", IdempotencyKey = "cancel-1", ExpectedVersion = 0
        });

        Assert.Equal(OprCaseStatus.Cancelled, result.Status);
        Assert.Empty(result.AvailableActions);
        Assert.Equal(0, await ctx.Context.OprSchedules.CountAsync(x => x.IsCurrent));
        Assert.Equal(0, await ctx.Context.OprTeamMembers.CountAsync(x => x.IsCurrent));
    }

    [Fact]
    public async Task CancelAsync_ByAnesthesiologist_IsAllowed()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);
        ctx.ActAs(ctx.AnesthesiologistUserId);

        var result = await service.CancelAsync(ctx.CaseId, new CancelOprCaseRequest
        {
            Reason = "Kondisi anestesi tidak memungkinkan.", IdempotencyKey = "cancel-anes", ExpectedVersion = 0
        });

        Assert.Equal(OprCaseStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelAsync_ByNurse_IsForbidden()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);
        ctx.ActAs(ctx.CirculatingNurseUserId);

        await Assert.ThrowsAsync<OperatingRoomForbiddenException>(() =>
            service.CancelAsync(ctx.CaseId, new CancelOprCaseRequest
            {
                Reason = "Alasan.", IdempotencyKey = "cancel-nurse", ExpectedVersion = 0
            }));
    }

    [Fact]
    public async Task CancelAsync_AfterStart_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.CancelAsync(ctx.CaseId, new CancelOprCaseRequest
            {
                Reason = "Terlambat dibatalkan.", IdempotencyKey = "cancel-late", ExpectedVersion = 0
            }));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task CancelAsync_WithoutReason_RejectsAsUnprocessable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.Ready);
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            service.CancelAsync(ctx.CaseId, new CancelOprCaseRequest
            {
                Reason = "  ", IdempotencyKey = "cancel-no-reason", ExpectedVersion = 0
            }));

        Assert.Equal("CancelReasonRequired", exception.Code);
    }

    private static OperatingRoomExecutionService Build(OperatingRoomTestContext ctx) =>
        new(ctx.Context, ctx.Accessor, ctx.Logger,
            new OperatingRoomIntegrationService(ctx.Context, ctx.Accessor, ctx.Logger));

    private static StartOprCaseRequest ValidStart(string key) => new()
    {
        ConfirmedPatientIdentity = true, ConfirmedProcedure = true, IdempotencyKey = key, ExpectedVersion = 0
    };

    private static SaveOprExecutionRecordRequest ValidRecord(string key, bool finalize = false,
        OprCaseOutcome? outcome = null, int expectedRecordVersion = 0) => new()
    {
        PreDiagnosis = "Apendisitis akut",
        PostDiagnosis = "Apendisitis perforasi",
        Findings = "Apendiks perforasi dengan cairan purulen terbatas",
        Technique = "Apendektomi terbuka",
        PostPlan = "Antibiotik lanjut, observasi 24 jam",
        BloodLossMl = 120,
        Finalize = finalize,
        Outcome = outcome,
        IdempotencyKey = key,
        ExpectedRecordVersion = expectedRecordVersion
    };
}
