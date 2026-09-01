using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomIntegrationServiceTests
{
    [Fact]
    public async Task GetReconciliationAsync_ReportsPendingDeliveriesAndBlockedDestinations()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await RecordMaterialAsync(ctx, "recon-1");
        var service = Build(ctx);

        var result = await service.GetReconciliationAsync(ctx.CaseId);

        Assert.NotNull(result);
        Assert.Single(result!.Deliveries);
        Assert.Equal(1, result.PendingCount);
        Assert.Equal(0, result.AcceptedCount);
        Assert.Contains(OperatingRoomIntegrationService.InventoryDestination, result.BlockedDestinations);
        Assert.Contains(OperatingRoomIntegrationService.BillingDestination, result.BlockedDestinations);
    }

    [Fact]
    public async Task RecordAttemptAsync_Failed_MarksFailedAndCountsRetry()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await RecordMaterialAsync(ctx, "attempt-fail");
        var service = Build(ctx);
        var delivery = await ctx.Context.OprIntegrationDeliveries.AsNoTracking().SingleAsync();

        var result = await service.RecordAttemptAsync(ctx.CaseId, delivery.Id, new RecordOprDeliveryAttemptRequest
        {
            Accepted = false, ErrorCode = "TIMEOUT", IdempotencyKey = "attempt-1"
        });

        Assert.Equal(OprDeliveryStatus.Failed, result.Status);
        Assert.Equal("TIMEOUT", result.LastErrorCode);
        Assert.Equal(1, result.RetryCount);
        Assert.NotNull(result.LastAttemptAt);
    }

    [Fact]
    public async Task RecordAttemptAsync_Accepted_StoresConsumerReference()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await RecordMaterialAsync(ctx, "attempt-ok");
        var service = Build(ctx);
        var delivery = await ctx.Context.OprIntegrationDeliveries.AsNoTracking().SingleAsync();

        var result = await service.RecordAttemptAsync(ctx.CaseId, delivery.Id, new RecordOprDeliveryAttemptRequest
        {
            Accepted = true, AcceptedReference = "INV-2026-0001", IdempotencyKey = "attempt-accept"
        });

        Assert.Equal(OprDeliveryStatus.Accepted, result.Status);
        Assert.Equal("INV-2026-0001", result.AcceptedReference);
        Assert.Null(result.LastErrorCode);
    }

    [Fact]
    public async Task RecordAttemptAsync_AfterAccepted_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await RecordMaterialAsync(ctx, "attempt-twice");
        var service = Build(ctx);
        var delivery = await ctx.Context.OprIntegrationDeliveries.AsNoTracking().SingleAsync();
        await service.RecordAttemptAsync(ctx.CaseId, delivery.Id, new RecordOprDeliveryAttemptRequest
        {
            Accepted = true, IdempotencyKey = "accept-first"
        });

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.RecordAttemptAsync(ctx.CaseId, delivery.Id, new RecordOprDeliveryAttemptRequest
            {
                Accepted = true, IdempotencyKey = "accept-second"
            }));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task RecordAttemptAsync_SameIdempotencyKey_IsRepeatable()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await RecordMaterialAsync(ctx, "attempt-idem");
        var service = Build(ctx);
        var delivery = await ctx.Context.OprIntegrationDeliveries.AsNoTracking().SingleAsync();
        var request = new RecordOprDeliveryAttemptRequest
        {
            Accepted = false, ErrorCode = "TIMEOUT", IdempotencyKey = "attempt-same"
        };

        var first = await service.RecordAttemptAsync(ctx.CaseId, delivery.Id, request);
        var second = await service.RecordAttemptAsync(ctx.CaseId, delivery.Id, request);

        Assert.Equal(first.RetryCount, second.RetryCount);
        Assert.Equal(1, second.RetryCount);
    }

    [Fact]
    public async Task RetryAsync_FailedDelivery_ReturnsToPendingWithoutDuplicating()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await RecordMaterialAsync(ctx, "retry-flow");
        var service = Build(ctx);
        var delivery = await ctx.Context.OprIntegrationDeliveries.AsNoTracking().SingleAsync();
        await service.RecordAttemptAsync(ctx.CaseId, delivery.Id, new RecordOprDeliveryAttemptRequest
        {
            Accepted = false, ErrorCode = "CONN", IdempotencyKey = "attempt-before-retry"
        });

        var result = await service.RetryAsync(ctx.CaseId, delivery.Id);

        Assert.Equal(OprDeliveryStatus.Pending, result.Status);
        Assert.Equal(1, await ctx.Context.OprIntegrationDeliveries.CountAsync());
    }

    [Fact]
    public async Task RetryAsync_PendingDelivery_RejectsInvalidStateTransition()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        await RecordMaterialAsync(ctx, "retry-pending");
        var service = Build(ctx);
        var delivery = await ctx.Context.OprIntegrationDeliveries.AsNoTracking().SingleAsync();

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            service.RetryAsync(ctx.CaseId, delivery.Id));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task StageChargeDeliveryAsync_SameComponentAndRevision_ProducesSingleDelivery()
    {
        await using var ctx = await OperatingRoomTestContext.CreateAsync(OprCaseStatus.InProgress);
        var service = Build(ctx);
        var now = DateTime.UtcNow;

        await service.StageChargeDeliveryAsync(ctx.CaseId, "procedure", 1, ctx.SurgeonUserId, now);
        await service.StageChargeDeliveryAsync(ctx.CaseId, "procedure", 1, ctx.SurgeonUserId, now);
        await ctx.Context.SaveChangesAsync();

        Assert.Equal(1, await ctx.Context.OprIntegrationDeliveries
            .CountAsync(x => x.Destination == OperatingRoomIntegrationService.BillingDestination));
    }

    private static OperatingRoomIntegrationService Build(OperatingRoomTestContext ctx) =>
        new(ctx.Context, ctx.Accessor, ctx.Logger);

    private static Task RecordMaterialAsync(OperatingRoomTestContext ctx, string key) =>
        new OperatingRoomMaterialService(ctx.Context, ctx.Accessor, ctx.Logger, Build(ctx), OperatingRoomTestContext.StrictRules)
            .RecordAsync(ctx.CaseId, new CreateOprMaterialUsageRequest
            {
                ExternalItemId = Guid.NewGuid(), ItemType = OprMaterialItemType.Consumable, Quantity = 1,
                UnitCode = "PCS", Outcome = OprMaterialOutcome.Used, IdempotencyKey = key
            });
}
