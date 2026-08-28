using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingSettlementServiceTests
{
    [Fact]
    public async Task BilAt005SuccessfulTenderSurvivesAnotherTenderFailure()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, firstMethod, secondMethod) = await SeedInvoiceAsync(db, 1_000_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Succeeded, "PROVIDER-SUCCESS-0001", "PAID");
        provider.EnqueueResult(BillingPaymentProviderOutcome.Failed, null, "DECLINED");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 1_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var first = await service.AddTenderAsync(
            settlement.Id!.Value,
            TenderRequest(firstMethod.Id, 300_000m, settlement.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var afterFirst = await service.GetAsync(settlement.Id.Value, CancellationToken.None);
        var second = await service.AddTenderAsync(
            settlement.Id.Value,
            TenderRequest(secondMethod.Id, 700_000m, afterFirst.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var result = await service.GetAsync(settlement.Id.Value, CancellationToken.None);

        Assert.Equal(BillingTenderStatuses.Succeeded, first.Status);
        Assert.Equal(BillingTenderStatuses.Failed, second.Status);
        Assert.Equal(BillingSettlementStatuses.PartiallySettled, result.Status);
        Assert.Equal(300_000m, result.SuccessfulAmount);
        Assert.Equal(300_000m, result.AllocatedAmount);
        Assert.Equal(700_000m, result.OutstandingAmount);
        Assert.Equal(700_000m, result.CollectibleAmount);
        Assert.Equal(0, result.PendingAmount);
        Assert.Equal(2, result.Tenders.Count);
        Assert.Single(db.BilPaymentAllocations);
    }

    [Fact]
    public async Task BilAt006TimeoutStaysPendingAndSameRetryDoesNotDuplicateCharge()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 700_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueTimeout();
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 700_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var request = TenderRequest(
            paymentMethod.Id, 700_000m, settlement.RowVersion!.Value);
        var key = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<BillingSettlementProviderPendingException>(() =>
            service.AddTenderAsync(
                settlement.Id!.Value, request, key, Guid.NewGuid(), CancellationToken.None));
        var replay = await service.AddTenderAsync(
            settlement.Id!.Value, request, key, Guid.NewGuid(), CancellationToken.None);
        var current = await service.GetAsync(settlement.Id.Value, CancellationToken.None);

        Assert.Equal(StatusCodes.Status504GatewayTimeout, exception.StatusCode);
        Assert.Equal(BillingTenderStatuses.Pending, exception.Tender.Status);
        Assert.Equal(BillingTenderStatuses.Pending, replay.Status);
        Assert.True(replay.IsReplay);
        Assert.Equal(1, provider.CallCount);
        Assert.Single(db.BilTenders);
        Assert.Equal(700_000m, current.PendingAmount);
        Assert.Equal(700_000m, current.OutstandingAmount);
        Assert.Equal(0, current.CollectibleAmount);
    }

    [Fact]
    public async Task PendingTenderReservesAmountAgainstAnotherIdempotencyKey()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, firstMethod, secondMethod) = await SeedInvoiceAsync(db, 500_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Pending, null, "PROCESSING");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 500_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        await service.AddTenderAsync(
            settlement.Id!.Value,
            TenderRequest(firstMethod.Id, 500_000m, settlement.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var current = await service.GetAsync(settlement.Id.Value, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingSettlementValidationException>(() =>
            service.AddTenderAsync(
                settlement.Id.Value,
                TenderRequest(secondMethod.Id, 1m, current.RowVersion!.Value),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(
            "Total metode pembayaran melebihi saldo yang harus dibayar.",
            exception.Message);
        Assert.Single(db.BilTenders);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task BilAt017LateNonCashSuccessUpdatesOriginalPendingTenderOnly()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 250_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Pending, null, "PROCESSING");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 250_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var pending = await service.AddTenderAsync(
            settlement.Id!.Value,
            TenderRequest(paymentMethod.Id, 250_000m, settlement.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var stored = await db.BilTenders.SingleAsync(x => x.Id == pending.Id);
        var callback = ProviderResult(
            stored,
            "late-success-1",
            BillingPaymentProviderOutcome.Succeeded,
            "QRIS-LATE-12345678",
            "PAID",
            DateTimeOffset.UtcNow.AddMinutes(5));

        var succeeded = await service.ReconcileTenderAsync(
            stored.Id, callback, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.ReconcileTenderAsync(
            stored.Id, callback, Guid.NewGuid(), CancellationToken.None);
        var result = await service.GetAsync(settlement.Id.Value, CancellationToken.None);

        Assert.Equal(stored.Id, succeeded.Id);
        Assert.Equal(BillingTenderStatuses.Succeeded, succeeded.Status);
        Assert.Null(succeeded.CashierShiftId);
        Assert.Equal("****5678", succeeded.ProviderReferenceMasked);
        Assert.True(replay.IsReplay);
        Assert.Equal(BillingSettlementStatuses.Settled, result.Status);
        Assert.Equal(250_000m, result.SuccessfulAmount);
        Assert.Equal(250_000m, result.AllocatedAmount);
        Assert.Single(db.BilTenders);
        Assert.Single(db.BilPaymentAllocations);
    }

    [Fact]
    public async Task CallbackReplayWithDifferentPayloadIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 100_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(
            BillingPaymentProviderOutcome.Pending, null, "PROCESSING", "same-event");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var pending = await service.AddTenderAsync(
            settlement.Id!.Value,
            TenderRequest(paymentMethod.Id, 100_000m, settlement.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var stored = await db.BilTenders.SingleAsync(x => x.Id == pending.Id);
        var conflicting = ProviderResult(
            stored,
            "same-event",
            BillingPaymentProviderOutcome.Succeeded,
            "DIFFERENT-REFERENCE",
            "PAID",
            DateTimeOffset.UtcNow);

        var exception = await Assert.ThrowsAsync<BillingSettlementConflictException>(() =>
            service.ReconcileTenderAsync(
                stored.Id, conflicting, Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("isi berbeda", exception.Message);
        Assert.Equal(BillingTenderStatuses.Pending, stored.Status);
    }

    [Fact]
    public async Task FinalFailedTenderCannotBeChangedByLateSuccess()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 100_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Failed, null, "DECLINED");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var failed = await service.AddTenderAsync(
            settlement.Id!.Value,
            TenderRequest(paymentMethod.Id, 100_000m, settlement.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var stored = await db.BilTenders.SingleAsync(x => x.Id == failed.Id);

        var ignored = await service.ReconcileTenderAsync(
            stored.Id,
            ProviderResult(
                stored,
                "late-after-final-failure",
                BillingPaymentProviderOutcome.Succeeded,
                "LATE-REF",
                "PAID",
                DateTimeOffset.UtcNow.AddMinutes(1)),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(ignored.IsReplay);
        Assert.Equal(BillingTenderStatuses.Failed, ignored.Status);
        Assert.Equal(BillingSettlementStatuses.Failed,
            (await service.GetAsync(settlement.Id.Value, CancellationToken.None)).Status);
    }

    [Fact]
    public async Task TenderRequiresCurrentSettlementVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 100_000m);
        var provider = new SequenceProviderAdapter();
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingSettlementConflictException>(() =>
            service.AddTenderAsync(
                settlement.Id!.Value,
                TenderRequest(paymentMethod.Id, 100_000m, Guid.NewGuid()),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Data telah berubah. Muat ulang sebelum melanjutkan.", exception.Message);
        Assert.Empty(db.BilTenders);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task ReusedTenderIdempotencyKeyWithDifferentPayloadIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 100_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Pending, null, "PROCESSING");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var request = TenderRequest(
            paymentMethod.Id, 100_000m, settlement.RowVersion!.Value);
        var key = Guid.NewGuid();
        await service.AddTenderAsync(
            settlement.Id!.Value, request, key, Guid.NewGuid(), CancellationToken.None);
        request.Amount = 99_000m;

        var exception = await Assert.ThrowsAsync<BillingSettlementConflictException>(() =>
            service.AddTenderAsync(
                settlement.Id.Value, request, key, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(
            "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.",
            exception.Message);
        Assert.Single(db.BilTenders);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task CashTenderIsRejectedUntilCashierShiftTaskExists()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, _, _) = await SeedInvoiceAsync(db, 100_000m);
        var cash = PaymentMethod("CASH", isCash: true);
        db.MstPaymentMethods.Add(cash);
        await db.SaveChangesAsync();
        var provider = new SequenceProviderAdapter();
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingSettlementValidationException>(() =>
            service.AddTenderAsync(
                settlement.Id!.Value,
                TenderRequest(cash.Id, 100_000m, settlement.RowVersion!.Value),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Buka shift kasir sebelum menerima uang tunai.", exception.Message);
        Assert.Empty(db.BilTenders);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task CashTenderUsesActiveShiftAndIncreasesSystemCashExactlyOnce()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, _, _) = await SeedInvoiceAsync(db, 100_000m);
        var cash = PaymentMethod("CASH", isCash: true);
        db.MstPaymentMethods.Add(cash);
        var actorId = Guid.NewGuid();
        var shift = ActiveShift(actorId, 50_000m);
        db.BilCashierShifts.Add(shift);
        await db.SaveChangesAsync();
        var provider = new SequenceProviderAdapter();
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), actorId, CancellationToken.None);
        var request = TenderRequest(cash.Id, 100_000m, settlement.RowVersion!.Value);
        var key = Guid.NewGuid();

        var result = await service.AddTenderAsync(
            settlement.Id!.Value, request, key, actorId, CancellationToken.None);
        var replay = await service.AddTenderAsync(
            settlement.Id.Value, request, key, actorId, CancellationToken.None);

        Assert.Equal(BillingTenderStatuses.Succeeded, result.Status);
        Assert.Equal(shift.Id, result.CashierShiftId);
        Assert.True(replay.IsReplay);
        Assert.Equal(100_000m, shift.SystemCash);
        Assert.Equal(0, provider.CallCount);
        Assert.Single(db.BilCashierShiftCommands.Where(
            x => x.CommandType == CashierShiftCommandTypes.CashReceipt));
    }

    [Fact]
    public async Task LateQrisSuccessAfterShiftCloseDoesNotChangePhysicalShiftCash()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 100_000m);
        paymentMethod.IsQris = true;
        var actorId = Guid.NewGuid();
        var cashierService = CreateCashierService(db);
        var opened = await cashierService.OpenAsync(
            new OpenShiftRequest
            {
                RegisterId = Guid.NewGuid(),
                OpeningCash = 75_000m,
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(), actorId, "Cashier", CancellationToken.None);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Pending, null, "PROCESSING");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), actorId, CancellationToken.None);
        var pending = await service.AddTenderAsync(
            settlement.Id!.Value,
            TenderRequest(paymentMethod.Id, 100_000m, settlement.RowVersion!.Value),
            Guid.NewGuid(), actorId, CancellationToken.None);
        var currentShift = db.BilCashierShifts.Single(x => x.Id == opened.Id);
        var closed = await cashierService.CloseAsync(
            opened.Id,
            new CloseShiftRequest
            {
                PhysicalCash = 75_000m,
                ExpectedRowVersion = currentShift.RowVersion,
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(), actorId, "Cashier", CancellationToken.None);
        var tender = db.BilTenders.Single(x => x.Id == pending.Id);

        var succeeded = await service.ReconcileTenderAsync(
            tender.Id,
            ProviderResult(
                tender,
                $"late-qris-{Guid.NewGuid():N}",
                BillingPaymentProviderOutcome.Succeeded,
                "QRIS-LATE-001",
                "PAID",
                DateTimeOffset.UtcNow.AddMinutes(1)),
            actorId,
            CancellationToken.None);

        Assert.Equal(BillingTenderStatuses.Succeeded, succeeded.Status);
        Assert.Null(succeeded.CashierShiftId);
        Assert.Equal(CashierShiftStatuses.Closed, closed.Status);
        Assert.Equal(0, currentShift.SystemCash);
        Assert.Equal(75_000m, currentShift.PhysicalCash);
        Assert.Equal(0, currentShift.Variance);
    }

    [Fact]
    public async Task SuccessfulDepositTenderCreatesExactlyOneAppendOnlyMovement()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var account = new BilDepositAccount
        {
            EncounterId = Guid.NewGuid(),
            AccountNumber = "DEP-TEST-SETTLEMENT",
            AvailableBalance = 50_000m,
            Status = BillingDepositAccountStatuses.Active,
            RowVersion = Guid.NewGuid()
        };
        var paymentMethod = PaymentMethod("QRIS");
        paymentMethod.IsQris = true;
        db.BilDepositAccounts.Add(account);
        db.MstPaymentMethods.Add(paymentMethod);
        await db.SaveChangesAsync();
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(
            BillingPaymentProviderOutcome.Succeeded, "DEPOSIT-PROVIDER-1234", "PAID");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            DepositSettlementRequest(account.Id, 200_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var request = TenderRequest(
            paymentMethod.Id, 200_000m, settlement.RowVersion!.Value);
        var key = Guid.NewGuid();

        var result = await service.AddTenderAsync(
            settlement.Id!.Value, request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.AddTenderAsync(
            settlement.Id.Value, request, key, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingTenderStatuses.Succeeded, result.Status);
        Assert.True(replay.IsReplay);
        Assert.Equal(250_000m, account.AvailableBalance);
        var movement = Assert.Single(db.BilDepositMovements);
        Assert.Equal(settlement.Id, movement.SettlementId);
        Assert.Equal(BillingDepositMovementTypes.TopUp, movement.MovementType);
        Assert.Equal(200_000m, movement.Amount);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task SettlementCreateIsIdempotentAndRejectsPayloadMismatch()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, _, _) = await SeedInvoiceAsync(db, 100_000m);
        var service = CreateService(db, new SequenceProviderAdapter());
        var request = InvoiceSettlementRequest(invoice.Id, 100_000m);
        var key = Guid.NewGuid();

        var first = await service.CreateAsync(
            request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.CreateAsync(
            request, key, Guid.NewGuid(), CancellationToken.None);
        request.RequestedAmount = 90_000m;
        var exception = await Assert.ThrowsAsync<BillingSettlementConflictException>(() =>
            service.CreateAsync(
                request, key, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(first.Id, replay.Id);
        Assert.True(replay.IsReplay);
        Assert.Contains("isi berbeda", exception.Message);
        Assert.Single(db.BilSettlements);
    }

    [Fact]
    public async Task SettlementNoteAndTenderCashierReferenceNoteRoundTripIndependentlyOfProviderReference()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 100_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Pending, null, "PROCESSING");
        var service = CreateService(db, provider);
        var createRequest = InvoiceSettlementRequest(invoice.Id, 100_000m);
        createRequest.Note = "Pasien hanya membawa uang pas, sisanya menyusul.";
        var settlement = await service.CreateAsync(
            createRequest, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var tenderRequest = TenderRequest(paymentMethod.Id, 100_000m, settlement.RowVersion!.Value);
        tenderRequest.CashierReferenceNote = "TRF-MANUAL-00123";

        var tender = await service.AddTenderAsync(
            settlement.Id!.Value, tenderRequest, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var result = await service.GetAsync(settlement.Id.Value, CancellationToken.None);

        Assert.Equal("Pasien hanya membawa uang pas, sisanya menyusul.", result.Note);
        Assert.Equal("TRF-MANUAL-00123", tender.CashierReferenceNote);
        Assert.Null(tender.ProviderReferenceMasked);
    }

    // BKC-DEC-057: nomor Kwitansi dialokasikan SEKALI setiap tender baru dibuat - satu nomor per
    // pembayaran, bukan per invoice - dan tetap dialokasikan terlepas dari hasil akhir tender
    // (SUCCEEDED maupun FAILED), karena kasir sudah menerima pembayaran (atau percobaan
    // pembayaran) itu pada saat tender dibuat.
    [Fact]
    public async Task KwitansiNumber_AllocatedForEveryTenderRegardlessOfOutcome()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, firstMethod, secondMethod) = await SeedInvoiceAsync(db, 1_000_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueResult(BillingPaymentProviderOutcome.Succeeded, "PROVIDER-SUCCESS-KWS-1", "PAID");
        provider.EnqueueResult(BillingPaymentProviderOutcome.Failed, null, "DECLINED");
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 1_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var first = await service.AddTenderAsync(
            settlement.Id!.Value,
            TenderRequest(firstMethod.Id, 300_000m, settlement.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var afterFirst = await service.GetAsync(settlement.Id.Value, CancellationToken.None);
        var second = await service.AddTenderAsync(
            settlement.Id.Value,
            TenderRequest(secondMethod.Id, 700_000m, afterFirst.RowVersion!.Value),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingTenderStatuses.Succeeded, first.Status);
        Assert.Equal(BillingTenderStatuses.Failed, second.Status);
        Assert.False(string.IsNullOrWhiteSpace(first.KwitansiNumber));
        Assert.False(string.IsNullOrWhiteSpace(second.KwitansiNumber));
        Assert.StartsWith("KWS-", first.KwitansiNumber);
        Assert.StartsWith("KWS-", second.KwitansiNumber);
        Assert.NotEqual(first.KwitansiNumber, second.KwitansiNumber);
    }

    [Fact]
    public async Task KwitansiNumber_ReplaySameIdempotencyKeyReturnsSameNumberNotANewOne()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, _, _) = await SeedInvoiceAsync(db, 100_000m);
        var cash = PaymentMethod("CASH", isCash: true);
        db.MstPaymentMethods.Add(cash);
        var actorId = Guid.NewGuid();
        var shift = ActiveShift(actorId, 50_000m);
        db.BilCashierShifts.Add(shift);
        await db.SaveChangesAsync();
        var provider = new SequenceProviderAdapter();
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), actorId, CancellationToken.None);
        var request = TenderRequest(cash.Id, 100_000m, settlement.RowVersion!.Value);
        var key = Guid.NewGuid();

        var result = await service.AddTenderAsync(
            settlement.Id!.Value, request, key, actorId, CancellationToken.None);
        var replay = await service.AddTenderAsync(
            settlement.Id.Value, request, key, actorId, CancellationToken.None);
        var reprinted = await service.GetAsync(settlement.Id.Value, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.KwitansiNumber));
        Assert.True(replay.IsReplay);
        Assert.Equal(result.KwitansiNumber, replay.KwitansiNumber);
        Assert.Equal(result.KwitansiNumber, reprinted.Tenders.Single().KwitansiNumber);
        Assert.Single(db.BilNumberSeries.Where(x => x.SequenceKey == "BILLING_KWITANSI"));
    }

    [Fact]
    public void SettlementModelHasLockedUniquenessAndConcurrencyConfiguration()
    {
        using var db = IsolatedBillingDbContextFactory.Create();
        var settlement = db.Model.FindEntityType(typeof(BilSettlement));
        var tender = db.Model.FindEntityType(typeof(BilTender));

        Assert.True(settlement!.FindProperty(nameof(BilSettlement.RowVersion))!.IsConcurrencyToken);
        Assert.True(tender!.FindProperty(nameof(BilTender.RowVersion))!.IsConcurrencyToken);
        Assert.True(settlement.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(BilSettlement.IdempotencyKey)])).IsUnique);
        Assert.True(tender.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(BilTender.ProviderReference)])).IsUnique);
    }

    [Fact]
    public void ControllerUsesLockedPatientFundsRoutesAndBillingPaymentPermissions()
    {
        var route = typeof(BillingSettlementsController).GetCustomAttribute<RouteAttribute>();
        Assert.Equal(
            "api/v1/health-services/billing-management/billing/patient-funds",
            route?.Template);
        AssertPermission(nameof(BillingSettlementsController.CreateSettlement), "Create");
        AssertPermission(nameof(BillingSettlementsController.AddTender), "Create");
        AssertPermission(nameof(BillingSettlementsController.GetSettlement), "Read");
    }

    [Fact]
    public async Task ControllerReturnsGatewayTimeoutWithPendingTenderData()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (invoice, paymentMethod, _) = await SeedInvoiceAsync(db, 100_000m);
        var provider = new SequenceProviderAdapter();
        provider.EnqueueTimeout();
        var service = CreateService(db, provider);
        var settlement = await service.CreateAsync(
            InvoiceSettlementRequest(invoice.Id, 100_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var controller = new BillingSettlementsController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.AddTender(
            settlement.Id!.Value,
            Guid.NewGuid(),
            TenderRequest(paymentMethod.Id, 100_000m, settlement.RowVersion!.Value),
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, objectResult.StatusCode);
        var body = Assert.IsType<ApiResponse<TenderResponse>>(objectResult.Value);
        Assert.False(body.Success);
        Assert.Equal(BillingTenderStatuses.Pending, body.Data!.Status);
    }

    private static async Task<(BilInvoice Invoice, MstPaymentMethod First, MstPaymentMethod Second)>
        SeedInvoiceAsync(Repositories.ApplicationDbContext db, decimal patientAmount)
    {
        var invoice = new BilInvoice
        {
            EncounterId = Guid.NewGuid(),
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = "RAJAL",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            RowVersion = Guid.NewGuid()
        };
        var calculation = new BilCalculationVersion
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            VersionNo = 1,
            GrossAmount = patientAmount,
            PatientAmount = patientAmount,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Calculation for settlement test",
            BreakdownSnapshot = "{}"
        };
        var first = PaymentMethod("QRIS-A");
        first.IsQris = true;
        var second = PaymentMethod("CARD-B");
        second.IsCardPayment = true;
        invoice.CalculationVersions.Add(calculation);
        db.BilInvoices.Add(invoice);
        db.BilCalculationVersions.Add(calculation);
        db.MstPaymentMethods.AddRange(first, second);
        await db.SaveChangesAsync();
        return (invoice, first, second);
    }

    private static MstPaymentMethod PaymentMethod(string code, bool isCash = false) => new()
    {
        PaymentMethodCode = code,
        PaymentMethodName = $"Payment {code}",
        PaymentMethodType = isCash ? "Cash" : "QRIS",
        IsCash = isCash,
        IsActive = true,
        IsAvailableForBilling = true,
        IntegrationCode = isCash ? null : $"INT-{code}"
    };

    private static CreateSettlementRequest InvoiceSettlementRequest(
        Guid invoiceId,
        decimal amount) => new()
    {
        InvoiceId = invoiceId,
        Purpose = BillingSettlementPurposes.InvoicePayment,
        RequestedAmount = amount,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static CreateSettlementRequest DepositSettlementRequest(
        Guid depositAccountId,
        decimal amount) => new()
    {
        DepositAccountId = depositAccountId,
        Purpose = BillingSettlementPurposes.DepositTopUp,
        RequestedAmount = amount,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static CreateTenderRequest TenderRequest(
        Guid paymentMethodId,
        decimal amount,
        Guid rowVersion) => new()
    {
        PaymentMethodId = paymentMethodId,
        Amount = amount,
        ExpectedRowVersion = rowVersion,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static BillingPaymentProviderResult ProviderResult(
        BilTender tender,
        string eventId,
        BillingPaymentProviderOutcome outcome,
        string? providerReference,
        string? statusCode,
        DateTimeOffset occurredAt) => new(
            eventId,
            outcome,
            providerReference,
            statusCode,
            occurredAt,
            tender.CorrelationId,
            tender.CausationId);

    private static BillingSettlementService CreateService(
        Repositories.ApplicationDbContext db,
        IBillingPaymentProviderAdapter provider)
    {
        var logger = new LoggerService(
            NullLogger<LoggerService>.Instance,
            new HttpContextAccessor());
        var numberSeries = new BillingNumberSeriesService(
            db,
            Options.Create(new BillingInvoiceNumberOptions()),
            Options.Create(new BillingDepositAccountNumberOptions()),
            Options.Create(new BillingCashierShiftNumberOptions()));
        var cashierShiftService = new CashierShiftService(db, numberSeries, logger);
        return new BillingSettlementService(
            db,
            provider,
            new BillingAllocationService(db, logger),
            cashierShiftService,
            numberSeries,
            logger);
    }

    private static CashierShiftService CreateCashierService(
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

    private static BilCashierShift ActiveShift(Guid cashierId, decimal openingCash) => new()
    {
        ShiftNumber = $"CSH-TEST-{Guid.NewGuid():N}",
        CashierId = cashierId,
        RegisterId = Guid.NewGuid(),
        OpeningCash = openingCash,
        SystemCash = 0,
        PhysicalCash = 0,
        Variance = 0,
        Status = CashierShiftStatuses.Open,
        OpenedAt = DateTimeOffset.UtcNow,
        RowVersion = Guid.NewGuid()
    };

    private static void AssertPermission(string methodName, string action)
    {
        var attribute = typeof(BillingSettlementsController)
            .GetMethod(methodName)?
            .GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("BillingPayment", arguments[0]);
        Assert.Equal(action, arguments[1]);
    }

    private sealed class SequenceProviderAdapter : IBillingPaymentProviderAdapter
    {
        private readonly Queue<Func<BillingPaymentProviderRequest, BillingPaymentProviderResult>> _steps = new();

        public int CallCount { get; private set; }

        public void EnqueueResult(
            BillingPaymentProviderOutcome outcome,
            string? providerReference,
            string? statusCode,
            string? eventId = null) =>
            _steps.Enqueue(request => new BillingPaymentProviderResult(
                eventId ?? $"event-{Guid.NewGuid():N}",
                outcome,
                providerReference,
                statusCode,
                DateTimeOffset.UtcNow,
                request.CorrelationId,
                request.CausationId));

        public void EnqueueTimeout() =>
            _steps.Enqueue(_ => throw new BillingPaymentProviderTimeoutException(
                "Provider melewati batas waktu."));

        public Task<BillingPaymentProviderResult> SubmitAsync(
            BillingPaymentProviderRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            if (_steps.Count == 0)
                throw new InvalidOperationException("Provider test step belum dikonfigurasi.");
            return Task.FromResult(_steps.Dequeue()(request));
        }
    }
}
