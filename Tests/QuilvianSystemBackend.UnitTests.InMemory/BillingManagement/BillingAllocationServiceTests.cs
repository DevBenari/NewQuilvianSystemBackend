using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingAllocationServiceTests
{
    [Fact]
    public async Task BilAt007DepositEightMillionAllocatesFiveMillionAndInvoiceStaysOpen()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounter, invoice, account) = await SeedAsync(
            db, 10_000_000m, 8_000_000m, 10_000_000m);
        var service = CreateAllocationService(db);
        var request = Request(invoice, account, 5_000_000m);

        var result = await service.AllocateDepositAsync(
            encounter.Id, request, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(3_000_000m, result.DepositBalance);
        Assert.Equal(5_000_000m, result.InvoiceOutstanding);
        Assert.Equal(5_000_000m, result.Amount);
        Assert.Equal(BillingInvoiceStatuses.Open, invoice.Status);
        Assert.Equal(BillingAllocationTargetTypes.Invoice, result.TargetType);
        Assert.Single(db.BilPaymentAllocations);
        Assert.Single(db.BilDepositMovements.Where(
            x => x.MovementType == BillingDepositMovementTypes.Allocation));
        var settlement = await db.BilSettlements.SingleAsync();
        Assert.Equal(5_000_000m, settlement.SuccessfulAmount);
        Assert.Equal(5_000_000m, settlement.AllocatedAmount);
        Assert.Equal(BillingSettlementStatuses.Settled, settlement.Status);
    }

    [Fact]
    public async Task NewChargeRaisesOutstandingWithoutClosingRunningInvoice()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounter, invoice, account) = await SeedAsync(
            db, 10_000_000m, 8_000_000m, 10_000_000m);
        var service = CreateAllocationService(db);
        await service.AllocateDepositAsync(
            encounter.Id,
            Request(invoice, account, 5_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        invoice.CurrentCalculationVersion = 2;
        invoice.RowVersion = Guid.NewGuid();
        db.BilCalculationVersions.Add(Calculation(invoice.Id, 2, 12_000_000m));
        await db.SaveChangesAsync();
        var second = await service.AllocateDepositAsync(
            encounter.Id,
            Request(invoice, account, 3_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(4_000_000m, second.InvoiceOutstanding);
        Assert.Equal(0, second.DepositBalance);
        Assert.Equal(BillingInvoiceStatuses.Open, invoice.Status);
        Assert.Equal(8_000_000m, await db.BilPaymentAllocations
            .Where(x => !x.ReversesAllocationId.HasValue)
            .SumAsync(x => x.Amount));
    }

    [Fact]
    public async Task BilAt008LowerRecalculationRecognizesRefundableCredit()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounter, invoice, account) = await SeedAsync(
            db, 10_000_000m, 8_000_000m, 5_000_000m);
        var logger = Logger();
        var allocationService = new BillingAllocationService(db, logger);
        await allocationService.AllocateDepositAsync(
            encounter.Id,
            Request(invoice, account, 8_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var calculationService = new BillingCalculationService(
            db, SelfPayCoverageAdapter.Instance, allocationService, logger);

        var result = await calculationService.RecalculateAsync(
            invoice.Id,
            new RecalculateInvoiceRequest
            {
                ExpectedRowVersion = invoice.RowVersion,
                Reason = "Final responsibility lebih rendah"
            },
            Guid.NewGuid(),
            CancellationToken.None);

        var credit = await db.BilRefundableCredits.SingleAsync();
        Assert.Equal(5_000_000m, result.PatientAmount);
        Assert.Equal(3_000_000m, credit.OriginalAmount);
        Assert.Equal(3_000_000m, credit.AvailableAmount);
        Assert.Equal(BillingRefundableCreditSourceTypes.AllocationExcess, credit.SourceType);
        Assert.Equal(BillingRefundableCreditStatuses.Available, credit.Status);
        Assert.Equal(BillingInvoiceStatuses.Open, invoice.Status);
    }

    [Fact]
    public async Task BilAt020StaleVersionLosesWithoutDuplicateAllocation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounter, invoice, account) = await SeedAsync(
            db, 10_000_000m, 8_000_000m, 10_000_000m);
        var service = CreateAllocationService(db);
        var staleRequest = Request(invoice, account, 5_000_000m);
        await service.AllocateDepositAsync(
            encounter.Id, staleRequest, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingAllocationConflictException>(() =>
            service.AllocateDepositAsync(
                encounter.Id,
                new DepositAllocationRequest
                {
                    InvoiceId = staleRequest.InvoiceId,
                    Amount = 1_000_000m,
                    ExpectedDepositRowVersion = staleRequest.ExpectedDepositRowVersion,
                    ExpectedInvoiceRowVersion = staleRequest.ExpectedInvoiceRowVersion,
                    ExpectedCalculationVersion = staleRequest.ExpectedCalculationVersion,
                    Reason = "Request paralel stale",
                    CorrelationId = Guid.NewGuid(),
                    CausationId = Guid.NewGuid()
                },
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Data telah berubah. Muat ulang sebelum melanjutkan.", exception.Message);
        Assert.Single(db.BilPaymentAllocations);
        Assert.Equal(3_000_000m, account.AvailableBalance);
    }

    [Fact]
    public async Task SameIdempotencyKeyReturnsReplayAndDifferentPayloadConflicts()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounter, invoice, account) = await SeedAsync(
            db, 10_000_000m, 8_000_000m, 10_000_000m);
        var service = CreateAllocationService(db);
        var request = Request(invoice, account, 5_000_000m);
        var key = Guid.NewGuid();
        var first = await service.AllocateDepositAsync(
            encounter.Id, request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.AllocateDepositAsync(
            encounter.Id, request, key, Guid.NewGuid(), CancellationToken.None);

        var changed = Request(invoice, account, 1_000_000m);
        var exception = await Assert.ThrowsAsync<BillingAllocationConflictException>(() =>
            service.AllocateDepositAsync(
                encounter.Id, changed, key, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(first.Id, replay.Id);
        Assert.True(replay.IsReplay);
        Assert.Equal(
            "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.",
            exception.Message);
        Assert.Single(db.BilPaymentAllocations);
    }

    [Fact]
    public async Task AllocationCannotExceedDepositOrOutstanding()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounter, invoice, account) = await SeedAsync(
            db, 10_000_000m, 8_000_000m, 10_000_000m);
        var service = CreateAllocationService(db);

        var exception = await Assert.ThrowsAsync<BillingAllocationValidationException>(() =>
            service.AllocateDepositAsync(
                encounter.Id,
                Request(invoice, account, 9_000_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Dana deposit atau saldo tagihan tidak mencukupi.", exception.Message);
        Assert.Empty(db.BilPaymentAllocations);
        Assert.Equal(8_000_000m, account.AvailableBalance);

        await using var secondDb = IsolatedBillingDbContextFactory.Create();
        var (secondEncounter, secondInvoice, secondAccount) = await SeedAsync(
            secondDb, 10_000_000m, 12_000_000m, 10_000_000m);
        var secondService = CreateAllocationService(secondDb);

        var outstandingException = await Assert.ThrowsAsync<BillingAllocationValidationException>(() =>
            secondService.AllocateDepositAsync(
                secondEncounter.Id,
                Request(secondInvoice, secondAccount, 11_000_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(
            "Dana deposit atau saldo tagihan tidak mencukupi.",
            outstandingException.Message);
        Assert.Empty(secondDb.BilPaymentAllocations);
    }

    [Fact]
    public void AllocationEndpointUsesLockedPermission()
    {
        var permission = typeof(BillingPatientFundsController)
            .GetMethod(nameof(BillingPatientFundsController.AllocateDeposit))?
            .GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);
        var arguments = Assert.IsType<object[]>(permission!.Arguments);
        Assert.Equal("BillingDeposit", arguments[0]);
        Assert.Equal("Allocate", arguments[1]);
        var route = typeof(BillingPatientFundsController)
            .GetMethod(nameof(BillingPatientFundsController.AllocateDeposit))?
            .GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPostAttribute>();
        Assert.Equal("deposits/{encounterId:guid}/allocations", route?.Template);
    }

    [Fact]
    public void FinancialRelationsUseRestrictAndRequiredUniqueSource()
    {
        using var db = IsolatedBillingDbContextFactory.Create();
        var allocation = db.Model.FindEntityType(typeof(BilPaymentAllocation));
        var credit = db.Model.FindEntityType(typeof(BilRefundableCredit));

        Assert.NotNull(allocation);
        Assert.All(allocation!.GetForeignKeys(), fk =>
            Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        Assert.Contains(credit!.GetIndexes(), index => index.IsUnique
            && index.Properties.Select(x => x.Name).SequenceEqual(
                [nameof(BilRefundableCredit.SourceType), nameof(BilRefundableCredit.SourceId)]));
    }

    private static async Task<(TrxPatientEncounter Encounter, BilInvoice Invoice, BilDepositAccount Account)> SeedAsync(
        Repositories.ApplicationDbContext db,
        decimal patientAmount,
        decimal depositBalance,
        decimal currentItemAmount)
    {
        var encounter = new TrxPatientEncounter
        {
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = EncounterType.Inpatient,
            EncounterStatus = EncounterStatus.Registered,
            EncounterDate = DateTime.UtcNow,
            IsActive = true
        };
        var category = new MstTariffCategory
        {
            TariffCategoryCode = $"CAT-{Guid.NewGuid():N}",
            TariffCategoryName = "Kategori allocation test",
            IsCoveredByInsuranceDefault = true,
            IsActive = true
        };
        var invoice = new BilInvoice
        {
            EncounterId = encounter.Id,
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            RowVersion = Guid.NewGuid()
        };
        invoice.Items.Add(new BilInvoiceItem
        {
            InvoiceId = invoice.Id,
            SourceDomain = "SERVICE",
            SourceDetailId = Guid.NewGuid().ToString("N"),
            SourceVersion = 1,
            SourceContractVersion = "TEST-1",
            SourceStatus = "CONFIRMED",
            SourceOccurredAt = DateTimeOffset.UtcNow,
            CategoryId = category.Id,
            Category = category,
            DescriptionSnapshot = "Pelayanan fiktif allocation",
            Quantity = 1,
            UnitPrice = currentItemAmount,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('A', 64)
        });
        var account = new BilDepositAccount
        {
            EncounterId = encounter.Id,
            AccountNumber = $"DEP-{Guid.NewGuid():N}",
            AvailableBalance = depositBalance,
            Status = BillingDepositAccountStatuses.Active,
            RowVersion = Guid.NewGuid()
        };

        db.TrxPatientEncounters.Add(encounter);
        db.MstTariffCategories.Add(category);
        db.BilInvoices.Add(invoice);
        db.BilCalculationVersions.Add(Calculation(invoice.Id, 1, patientAmount));
        db.BilDepositAccounts.Add(account);
        await db.SaveChangesAsync();
        return (encounter, invoice, account);
    }

    private static BilCalculationVersion Calculation(
        Guid invoiceId,
        int version,
        decimal patientAmount) => new()
        {
            InvoiceId = invoiceId,
            VersionNo = version,
            GrossAmount = patientAmount,
            PatientAmount = patientAmount,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Seed allocation test",
            BreakdownSnapshot = "{}"
        };

    private static DepositAllocationRequest Request(
        BilInvoice invoice,
        BilDepositAccount account,
        decimal amount) => new()
        {
            InvoiceId = invoice.Id,
            Amount = amount,
            ExpectedDepositRowVersion = account.RowVersion,
            ExpectedInvoiceRowVersion = invoice.RowVersion,
            ExpectedCalculationVersion = invoice.CurrentCalculationVersion,
            Reason = "Progress payment pasien",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

    private static BillingAllocationService CreateAllocationService(
        Repositories.ApplicationDbContext db) => new(db, Logger());

    private static LoggerService Logger() => new(
        NullLogger<LoggerService>.Instance,
        new HttpContextAccessor());

    private sealed class SelfPayCoverageAdapter : IBillingCoverageAdapter
    {
        public static readonly SelfPayCoverageAdapter Instance = new();

        public Task<BillingCoverageDecision> ResolveAsync(
            BillingCoverageContext context,
            CancellationToken cancellationToken) =>
            Task.FromResult(new BillingCoverageDecision(
                "SELF-PAY-TEST", "SELF_PAY", "NOT_APPLICABLE", 0, 0, 0, []));
    }
}
