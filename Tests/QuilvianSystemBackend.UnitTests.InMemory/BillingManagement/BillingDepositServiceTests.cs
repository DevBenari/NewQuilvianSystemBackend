using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingDepositServiceTests
{
    [Fact]
    public async Task FirstNonCashTopUpCreatesOneInpatientAccountAndAppendOnlyMovement()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);

        var result = await service.TopUpAsync(
            encounterId,
            TopUpRequest(paymentMethodId, 8_000_000m),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(BillingSettlementPurposes.DepositTopUp, result.Purpose);
        Assert.Equal(BillingSettlementStatuses.Settled, result.Status);
        Assert.Equal(8_000_000m, result.SuccessfulAmount);
        Assert.Equal(0, result.AllocatedAmount);
        Assert.False(result.IsReplay);
        var deposit = Assert.IsType<DepositResponse>(result.Deposit);
        Assert.StartsWith("DEP-", deposit.AccountNumber);
        Assert.Equal(8_000_000m, deposit.AvailableBalance);
        Assert.Equal(BillingDepositAccountStatuses.Active, deposit.Status);
        var movement = Assert.Single(deposit.Movements);
        Assert.Equal(BillingDepositMovementTypes.TopUp, movement.MovementType);
        Assert.Equal(8_000_000m, movement.BalanceEffect);
        Assert.Equal(8_000_000m, movement.BalanceAfter);
        Assert.Equal(paymentMethodId, movement.PaymentMethodId);
        Assert.Single(db.BilDepositAccounts);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public async Task AdditionalTopUpReusesAccountAndRequiresCurrentVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var first = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 5_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var firstDeposit = Assert.IsType<DepositResponse>(first.Deposit);

        var second = await service.TopUpAsync(
            encounterId,
            TopUpRequest(paymentMethodId, 3_000_000m, firstDeposit.RowVersion),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);
        var secondDeposit = Assert.IsType<DepositResponse>(second.Deposit);

        Assert.Equal(firstDeposit.Id, secondDeposit.Id);
        Assert.Equal(8_000_000m, secondDeposit.AvailableBalance);
        Assert.Equal(2, secondDeposit.Movements.Count);
        Assert.Single(db.BilDepositAccounts);
        Assert.Equal(2, db.BilDepositMovements.Count());
    }

    [Fact]
    public async Task IdenticalIdempotencyReplayDoesNotCreateAnotherMovement()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var request = TopUpRequest(paymentMethodId, 750_000m);
        var key = Guid.NewGuid();

        var first = await service.TopUpAsync(
            encounterId, request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.TopUpAsync(
            encounterId, request, key, Guid.NewGuid(), CancellationToken.None);

        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(first.DepositMovementId, replay.DepositMovementId);
        Assert.Equal(
            750_000m,
            Assert.IsType<DepositResponse>(replay.Deposit).AvailableBalance);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public async Task ReusedIdempotencyKeyWithDifferentPayloadIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var request = TopUpRequest(paymentMethodId, 500_000m);
        var key = Guid.NewGuid();
        await service.TopUpAsync(
            encounterId, request, key, Guid.NewGuid(), CancellationToken.None);
        request.Amount = 600_000m;

        var exception = await Assert.ThrowsAsync<BillingDepositConflictException>(() =>
            service.TopUpAsync(
                encounterId, request, key, Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("isi berbeda", exception.Message);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public async Task StaleVersionRejectsConcurrentTopUpWithoutChangingBalance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var first = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 1_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var firstDeposit = Assert.IsType<DepositResponse>(first.Deposit);
        await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 250_000m, firstDeposit.RowVersion),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingDepositConflictException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(paymentMethodId, 100_000m, firstDeposit.RowVersion),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("Muat ulang", exception.Message);
        Assert.Equal(1_250_000m, db.BilDepositAccounts.Single().AvailableBalance);
        Assert.Equal(2, db.BilDepositMovements.Count());
    }

    [Fact]
    public async Task CashTopUpIsRejectedUntilCashierShiftExists()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var cashMethod = PaymentMethod(isCash: true);
        db.MstPaymentMethods.Add(cashMethod);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(cashMethod.Id, 100_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Buka shift kasir sebelum menerima uang tunai.", exception.Message);
        Assert.Empty(db.BilDepositAccounts);
        Assert.Empty(db.BilDepositMovements);
    }

    [Fact]
    public async Task CashTopUpUsesActiveShiftAndIncreasesSystemCashExactlyOnce()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var cashMethod = PaymentMethod(isCash: true);
        var actorId = Guid.NewGuid();
        var shift = new BilCashierShift
        {
            ShiftNumber = $"CSH-TEST-{Guid.NewGuid():N}",
            CashierId = actorId,
            RegisterId = Guid.NewGuid(),
            OpeningCash = 25_000m,
            Status = CashierShiftStatuses.Open,
            OpenedAt = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid()
        };
        db.MstPaymentMethods.Add(cashMethod);
        db.BilCashierShifts.Add(shift);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var request = TopUpRequest(cashMethod.Id, 100_000m);
        var key = Guid.NewGuid();

        var result = await service.TopUpAsync(
            encounterId, request, key, actorId, CancellationToken.None);
        var replay = await service.TopUpAsync(
            encounterId, request, key, actorId, CancellationToken.None);

        Assert.False(result.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(100_000m, shift.SystemCash);
        var movement = Assert.Single(db.BilDepositMovements);
        Assert.Equal(shift.Id, movement.CashierShiftId);
        Assert.Single(db.BilCashierShiftCommands.Where(
            x => x.CommandType == CashierShiftCommandTypes.CashReceipt));
    }

    [Fact]
    public async Task TenderDependentNonCashMethodIsRejectedAtCurrentBoundary()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var qrisMethod = PaymentMethod();
        qrisMethod.IsQris = true;
        qrisMethod.IsNeedReferenceNumber = true;
        db.MstPaymentMethods.Add(qrisMethod);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(qrisMethod.Id, 100_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("settlement/tender", exception.Message);
        Assert.Empty(db.BilDepositMovements);
    }

    [Fact]
    public async Task DepositIsRejectedForNonInpatientEncounter()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db, EncounterType.Outpatient);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(paymentMethodId, 100_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("rawat inap", exception.Message);
        Assert.Empty(db.BilDepositAccounts);
    }

    [Fact]
    public async Task TopUpDoesNotAllocateOrLockRunningInvoice()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var invoice = new BilInvoice
        {
            EncounterId = encounterId,
            InvoiceNumber = "BIL-TEST-0001",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 0,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        };
        db.BilInvoices.Add(invoice);
        await db.SaveChangesAsync();
        var originalInvoiceVersion = invoice.RowVersion;
        var service = CreateService(db);

        var result = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 8_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var deposit = Assert.IsType<DepositResponse>(result.Deposit);

        Assert.Equal(8_000_000m, deposit.AvailableBalance);
        Assert.Equal(0, result.AllocatedAmount);
        Assert.Null(deposit.Movements.Single().SettlementId);
        Assert.Equal(BillingInvoiceStatuses.Open, invoice.Status);
        Assert.Equal(0, invoice.CurrentCalculationVersion);
        Assert.Equal(originalInvoiceVersion, invoice.RowVersion);
        Assert.Empty(db.BilCalculationVersions);
    }

    [Fact]
    public async Task ReversalAppendsCompensatingMovementAndIsIdempotent()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var topUp = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 900_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var deposit = Assert.IsType<DepositResponse>(topUp.Deposit);
        var original = Assert.Single(deposit.Movements);
        var request = ReversalRequest(deposit.RowVersion);
        var key = Guid.NewGuid();

        var reversed = await service.ReverseTopUpAsync(
            encounterId, original.Id, request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.ReverseTopUpAsync(
            encounterId, original.Id, request, key, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0, reversed.AvailableBalance);
        Assert.Equal(2, reversed.Movements.Count);
        Assert.Equal(2, replay.Movements.Count);
        var storedOriginal = db.BilDepositMovements.Single(x => x.Id == original.Id);
        Assert.Equal(BillingDepositMovementTypes.TopUp, storedOriginal.MovementType);
        Assert.Equal(900_000m, storedOriginal.Amount);
        Assert.False(storedOriginal.IsDelete);
        var compensating = db.BilDepositMovements.Single(x => x.ReversesMovementId == original.Id);
        Assert.Equal(BillingDepositMovementTypes.Reversal, compensating.MovementType);
        Assert.Equal(900_000m, compensating.Amount);
    }

    [Fact]
    public async Task ReversalCannotMakeDepositBalanceNegative()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var topUp = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 500_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var account = db.BilDepositAccounts.Single();
        account.AvailableBalance = 250_000m;
        account.RowVersion = Guid.NewGuid();
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.ReverseTopUpAsync(
                encounterId,
                topUp.DepositMovementId!.Value,
                ReversalRequest(account.RowVersion),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.Equal("Dana deposit atau saldo tagihan tidak mencukupi.", exception.Message);
        Assert.Equal(250_000m, account.AvailableBalance);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public void ControllerContractUsesLockedRoutesAndPermissions()
    {
        var route = typeof(BillingPatientFundsController).GetCustomAttribute<RouteAttribute>();
        Assert.Equal(
            "api/v1/health-services/billing-management/billing/patient-funds",
            route?.Template);
        AssertPermission(nameof(BillingPatientFundsController.GetDeposit), "Read");
        AssertPermission(nameof(BillingPatientFundsController.TopUp), "Create");
    }

    private static async Task<(Guid EncounterId, Guid PaymentMethodId)> SeedAsync(
        Repositories.ApplicationDbContext db,
        EncounterType encounterType = EncounterType.Inpatient)
    {
        var encounter = new TrxPatientEncounter
        {
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = encounterType,
            EncounterStatus = EncounterStatus.Registered,
            IsActive = true
        };
        var paymentMethod = PaymentMethod();
        db.TrxPatientEncounters.Add(encounter);
        db.MstPaymentMethods.Add(paymentMethod);
        await db.SaveChangesAsync();
        return (encounter.Id, paymentMethod.Id);
    }

    private static MstPaymentMethod PaymentMethod(bool isCash = false) => new()
    {
        PaymentMethodCode = $"PM-{Guid.NewGuid():N}"[..20],
        PaymentMethodName = isCash ? "Tunai Uji" : "Non Tunai Manual Uji",
        PaymentMethodType = isCash ? "Cash" : "Transfer",
        IsCash = isCash,
        IsBankTransfer = !isCash,
        IsActive = true,
        IsAvailableForBilling = true
    };

    private static DepositTopUpRequest TopUpRequest(
        Guid paymentMethodId,
        decimal amount,
        Guid? expectedRowVersion = null) => new()
    {
        PaymentMethodId = paymentMethodId,
        Amount = amount,
        ExpectedRowVersion = expectedRowVersion,
        Reason = "Top-up deposit rawat inap untuk pengujian",
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static ReverseDepositMovementRequest ReversalRequest(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = "Koreksi penerimaan top-up untuk pengujian",
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static BillingDepositService CreateService(
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
        var cashierShiftService = new CashierShiftService(db, numberSeries, logger);
        return new BillingDepositService(db, numberSeries, cashierShiftService, logger);
    }

    private static void AssertPermission(string methodName, string action)
    {
        var attribute = typeof(BillingPatientFundsController)
            .GetMethod(methodName)?
            .GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("BillingDeposit", arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
