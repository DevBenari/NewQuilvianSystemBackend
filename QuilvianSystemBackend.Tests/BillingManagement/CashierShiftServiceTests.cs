using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class CashierShiftServiceTests
{
    [Fact]
    public async Task OpenIsIdempotentAndOnlyOneActiveShiftIsAllowed()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var cashierId = Guid.NewGuid();
        var request = OpenRequest();
        var key = Guid.NewGuid();

        var opened = await service.OpenAsync(
            request, key, cashierId, "Cashier", CancellationToken.None);
        var replay = await service.OpenAsync(
            request, key, cashierId, "Cashier", CancellationToken.None);

        Assert.Equal(CashierShiftStatuses.Open, opened.Status);
        Assert.True(replay.IsReplay);
        Assert.Equal(opened.Id, replay.Id);
        Assert.Single(db.BilCashierShifts);
        Assert.Single(db.BilCashierShiftCommands);

        var exception = await Assert.ThrowsAsync<CashierShiftConflictException>(() =>
            service.OpenAsync(
                OpenRequest(), Guid.NewGuid(), cashierId, "Cashier", CancellationToken.None));
        Assert.Contains("shift aktif", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IdempotencyKeyWithDifferentPayloadIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var cashierId = Guid.NewGuid();
        var request = OpenRequest();
        var key = Guid.NewGuid();
        await service.OpenAsync(
            request, key, cashierId, "Cashier", CancellationToken.None);
        request.OpeningCash++;

        var exception = await Assert.ThrowsAsync<CashierShiftConflictException>(() =>
            service.OpenAsync(
                request, key, cashierId, "Cashier", CancellationToken.None));

        Assert.Equal(
            "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.",
            exception.Message);
        Assert.Single(db.BilCashierShifts);
    }

    [Fact]
    public async Task StaleVersionRejectsCloseWithoutChangingShift()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var cashierId = Guid.NewGuid();
        var opened = await service.OpenAsync(
            OpenRequest(), Guid.NewGuid(), cashierId, "Cashier", CancellationToken.None);

        var exception = await Assert.ThrowsAsync<CashierShiftConflictException>(() =>
            service.CloseAsync(
                opened.Id,
                new CloseShiftRequest
                {
                    PhysicalCash = opened.OpeningCash,
                    ExpectedRowVersion = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                    CausationId = Guid.NewGuid()
                },
                Guid.NewGuid(),
                cashierId,
                "Cashier",
                CancellationToken.None));

        Assert.Equal("Data telah berubah. Muat ulang sebelum melanjutkan.", exception.Message);
        Assert.Equal(CashierShiftStatuses.Open, db.BilCashierShifts.Single().Status);
        Assert.DoesNotContain(db.BilCashierShiftCommands, x =>
            x.CommandType == CashierShiftCommandTypes.Close);
    }

    [Fact]
    public async Task VarianceIsPersistedReviewedAndReopenedWithoutDeletingHistory()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var cashierId = Guid.NewGuid();
        var headCashierId = Guid.NewGuid();
        var opened = await service.OpenAsync(
            OpenRequest(100_000m),
            Guid.NewGuid(),
            cashierId,
            "Cashier",
            CancellationToken.None);
        var shift = db.BilCashierShifts.Single();
        var receiptId = Guid.NewGuid();
        Assert.True(await service.ApplyCashReceiptAsync(
            shift,
            "TENDER",
            receiptId,
            500_000m,
            cashierId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            CancellationToken.None));
        await db.SaveChangesAsync();

        var closed = await service.CloseAsync(
            opened.Id,
            new CloseShiftRequest
            {
                PhysicalCash = 550_000m,
                ExpectedRowVersion = shift.RowVersion,
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(),
            cashierId,
            "Cashier",
            CancellationToken.None);

        Assert.Equal(CashierShiftStatuses.ClosedWithVariance, closed.Status);
        Assert.Equal(500_000m, closed.SystemCash);
        Assert.Equal(600_000m, closed.ExpectedClosingCash);
        Assert.Equal(550_000m, closed.PhysicalCash);
        Assert.Equal(-50_000m, closed.Variance);
        Assert.True(closed.VarianceRequiresReview);

        var reviewed = await service.ReviewVarianceAsync(
            closed.Id,
            new ReviewVarianceRequest
            {
                ExpectedRowVersion = closed.RowVersion,
                Resolution = "Selisih kurang dibebankan ke investigasi kasir.",
                Reason = "Hitung ulang fisik tetap berbeda.",
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(),
            headCashierId,
            "HeadCashier",
            CancellationToken.None);
        var reopened = await service.ReopenAsync(
            closed.Id,
            new ReopenShiftRequest
            {
                ExpectedRowVersion = reviewed.Shift.RowVersion,
                Reason = "Investigasi membutuhkan koreksi penerimaan append-only.",
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(),
            headCashierId,
            "HeadCashier",
            CancellationToken.None);

        Assert.Equal(CashierShiftStatuses.Reviewed, reviewed.Shift.Status);
        Assert.Equal(-50_000m, reviewed.Variance);
        Assert.Equal(CashierShiftStatuses.Reopened, reopened.Status);
        Assert.Null(reopened.ClosedAt);
        var review = Assert.Single(db.BilCashVarianceReviews);
        Assert.Equal(-50_000m, review.Variance);
        Assert.False(review.IsDelete);
        Assert.Contains(db.BilCashierShiftCommands, x =>
            x.CommandType == CashierShiftCommandTypes.Close
            && x.StatusAfter == CashierShiftStatuses.ClosedWithVariance);
        Assert.Contains(db.BilCashierShiftCommands, x =>
            x.CommandType == CashierShiftCommandTypes.Reopen
            && x.ActorUserId == headCashierId
            && x.Reason != null);
    }

    [Fact]
    public async Task HandoverRequiresBothCashiersAndCreatesReceivingShift()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var outgoingId = Guid.NewGuid();
        var incomingId = Guid.NewGuid();
        var opened = await service.OpenAsync(
            OpenRequest(75_000m), Guid.NewGuid(), outgoingId, "Cashier", CancellationToken.None);
        var source = db.BilCashierShifts.Single();
        await service.ApplyCashReceiptAsync(
            source,
            "TENDER",
            Guid.NewGuid(),
            25_000m,
            outgoingId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            CancellationToken.None);
        await db.SaveChangesAsync();

        var initiated = await service.HandoverAsync(
            opened.Id,
            HandoverRequest(incomingId, source.RowVersion),
            Guid.NewGuid(),
            outgoingId,
            "Cashier",
            CancellationToken.None);

        Assert.Equal(CashierShiftStatuses.Open, initiated.Status);
        Assert.Equal(CashierShiftHandoverStatuses.Pending, initiated.PendingHandoverStatus);
        Assert.Equal(incomingId, initiated.ReceivingCashierId);

        var wrongActor = await Assert.ThrowsAsync<CashierShiftForbiddenException>(() =>
            service.HandoverAsync(
                opened.Id,
                HandoverRequest(incomingId, initiated.RowVersion),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Cashier",
                CancellationToken.None));
        Assert.Contains("pemilik shift", wrongActor.Message);

        var received = await service.HandoverAsync(
            opened.Id,
            HandoverRequest(incomingId, initiated.RowVersion),
            Guid.NewGuid(),
            incomingId,
            "Cashier",
            CancellationToken.None);

        Assert.Equal(CashierShiftStatuses.Open, received.Status);
        Assert.Equal(incomingId, received.CashierId);
        Assert.Equal(100_000m, received.OpeningCash);
        Assert.Equal(opened.RegisterId, received.RegisterId);
        Assert.Equal(
            CashierShiftStatuses.HandedOver,
            db.BilCashierShifts.Single(x => x.Id == opened.Id).Status);
        var handover = Assert.Single(db.BilCashierShiftHandovers);
        Assert.Equal(CashierShiftHandoverStatuses.Confirmed, handover.Status);
        Assert.Equal(received.Id, handover.ReceivingShiftId);
    }

    [Fact]
    public void ControllerRoutesAndPermissionsMatchLockedContract()
    {
        var route = typeof(CashierShiftsController).GetCustomAttribute<RouteAttribute>();
        Assert.Equal(
            "api/v1/health-services/billing-management/cashier/shifts",
            route?.Template);
        AssertPermission(nameof(CashierShiftsController.Open), "Create");
        AssertPermission(nameof(CashierShiftsController.Current), "Read");
        AssertPermission(nameof(CashierShiftsController.Handover), "Handover");
        AssertPermission(nameof(CashierShiftsController.Close), "Close");
        AssertPermission(nameof(CashierShiftsController.ReviewVariance), "Review");
        AssertPermission(nameof(CashierShiftsController.Reopen), "Reopen");
    }

    private static OpenShiftRequest OpenRequest(decimal openingCash = 50_000m) => new()
    {
        RegisterId = Guid.NewGuid(),
        OpeningCash = openingCash,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static HandoverShiftRequest HandoverRequest(
        Guid receivingCashierId,
        Guid rowVersion) => new()
        {
            ReceivingCashierId = receivingCashierId,
            ExpectedRowVersion = rowVersion,
            Reason = "Pergantian petugas pada register yang sama.",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

    private static CashierShiftService CreateService(
        Repositories.ApplicationDbContext db)
    {
        var logger = new LoggerService(
            NullLogger<LoggerService>.Instance,
            new HttpContextAccessor());
        var numberSeries = new BillingNumberSeriesService(
            db,
            Options.Create(new BillingInvoiceNumberOptions()),
            Options.Create(new BillingDepositAccountNumberOptions()),
            Options.Create(new BillingCashierShiftNumberOptions()));
        return new CashierShiftService(db, numberSeries, logger);
    }

    private static void AssertPermission(string methodName, string action)
    {
        var attribute = typeof(CashierShiftsController)
            .GetMethod(methodName)?
            .GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("CashierShift", arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
