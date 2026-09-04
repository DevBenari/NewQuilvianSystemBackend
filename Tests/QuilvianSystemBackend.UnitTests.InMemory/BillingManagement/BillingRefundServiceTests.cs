using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingRefundServiceTests
{
    [Fact]
    public async Task FullSuccessSplitsProportionallyAcrossOriginalTendersAndExhaustsCredit()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(db, cashAmount: 600_000m, nonCashAmount: 400_000m, creditAmount: 1_000_000m);
        var makerId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var service = CreateService(db, new AlwaysSucceedsProviderAdapter());

        var created = await service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 500_000m, "Kelebihan bayar rajal"),
            Guid.NewGuid(), makerId, CancellationToken.None);
        var approved = await service.ApproveAsync(
            created.Id, ApprovalRequest(created.RowVersion), approverId, CancellationToken.None);

        Assert.Equal(BillingRefundCaseStatuses.Executed, approved.Status);
        Assert.Equal(500_000m, approved.ExecutedAmount);
        Assert.Equal(2, approved.Lines.Count);
        Assert.All(approved.Lines, x => Assert.Equal(BillingRefundLineStatuses.Succeeded, x.Status));
        var cashLine = Assert.Single(approved.Lines, x => x.OriginalTenderId == seeded.CashTender.Id);
        var nonCashLine = Assert.Single(approved.Lines, x => x.OriginalTenderId == seeded.NonCashTender.Id);
        Assert.Equal(300_000m, cashLine.Amount);
        Assert.Equal(200_000m, nonCashLine.Amount);
        Assert.Equal(500_000m, cashLine.Amount + nonCashLine.Amount);
        var credit = await db.BilRefundableCredits.FindAsync(seeded.Credit.Id);
        Assert.Equal(500_000m, credit!.AvailableAmount);
        Assert.Equal(BillingRefundableCreditStatuses.Available, credit.Status);
    }

    [Fact]
    public async Task IndeterminateProviderLeavesNonCashLinePendingAndCaseIsPartiallyExecuted()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(db, cashAmount: 600_000m, nonCashAmount: 400_000m, creditAmount: 1_000_000m);
        var service = CreateService(db, new DeferredBillingPaymentProviderAdapter());

        var created = await service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 500_000m, "Kelebihan bayar rajal"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var approved = await service.ApproveAsync(
            created.Id, ApprovalRequest(created.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingRefundCaseStatuses.PartiallyExecuted, approved.Status);
        Assert.Equal(300_000m, approved.ExecutedAmount);
        var cashLine = Assert.Single(approved.Lines, x => x.OriginalTenderId == seeded.CashTender.Id);
        var nonCashLine = Assert.Single(approved.Lines, x => x.OriginalTenderId == seeded.NonCashTender.Id);
        Assert.Equal(BillingRefundLineStatuses.Succeeded, cashLine.Status);
        Assert.Equal(BillingRefundLineStatuses.Pending, nonCashLine.Status);
        var credit = await db.BilRefundableCredits.FindAsync(seeded.Credit.Id);
        Assert.Equal(700_000m, credit!.AvailableAmount);
    }

    [Fact]
    public async Task RetryAfterProviderRecoversCompletesRemainingLineAndExecutesCase()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(db, cashAmount: 600_000m, nonCashAmount: 400_000m, creditAmount: 1_000_000m);
        var firstAttempt = await CreateService(db, new DeferredBillingPaymentProviderAdapter())
            .CreateAsync(
                RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 500_000m, "Kelebihan bayar rajal"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var partial = await CreateService(db, new DeferredBillingPaymentProviderAdapter())
            .ApproveAsync(firstAttempt.Id, ApprovalRequest(firstAttempt.RowVersion), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(BillingRefundCaseStatuses.PartiallyExecuted, partial.Status);

        var retried = await CreateService(db, new AlwaysSucceedsProviderAdapter())
            .ApproveAsync(partial.Id, ApprovalRequest(partial.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingRefundCaseStatuses.Executed, retried.Status);
        Assert.Equal(500_000m, retried.ExecutedAmount);
        Assert.All(retried.Lines, x => Assert.Equal(BillingRefundLineStatuses.Succeeded, x.Status));
        var credit = await db.BilRefundableCredits.FindAsync(seeded.Credit.Id);
        Assert.Equal(500_000m, credit!.AvailableAmount);
    }

    [Fact]
    public async Task InpatientInvoiceRejectsNormalRefund()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(
            db, cashAmount: 600_000m, nonCashAmount: 0m, creditAmount: 300_000m, serviceType: "RANAP");
        var service = CreateService(db, new AlwaysSucceedsProviderAdapter());

        var exception = await Assert.ThrowsAsync<BillingRefundValidationException>(() => service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 100_000m, "Coba refund ranap"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("rawat inap", exception.Message);
        Assert.Empty(db.BilRefundCases);
    }

    [Fact]
    public async Task RequesterCannotApproveOwnRefundCase()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(db, cashAmount: 500_000m, nonCashAmount: 0m, creditAmount: 500_000m);
        var makerId = Guid.NewGuid();
        var service = CreateService(db, new AlwaysSucceedsProviderAdapter());

        var created = await service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 200_000m, "Kelebihan bayar OTC"),
            Guid.NewGuid(), makerId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingRefundForbiddenException>(() => service.ApproveAsync(
            created.Id, ApprovalRequest(created.RowVersion), makerId, CancellationToken.None));

        Assert.Contains("sendiri", exception.Message);
        Assert.Equal(BillingRefundCaseStatuses.Submitted,
            (await db.BilRefundCases.FindAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task ApproveWithStaleRowVersionIsRejectedWithoutMutation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(db, cashAmount: 500_000m, nonCashAmount: 0m, creditAmount: 500_000m);
        var service = CreateService(db, new AlwaysSucceedsProviderAdapter());
        var created = await service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 200_000m, "Kelebihan bayar OTC"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingRefundConflictException>(() => service.ApproveAsync(
            created.Id, ApprovalRequest(Guid.NewGuid()), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("berubah", exception.Message);
        Assert.Equal(BillingRefundCaseStatuses.Submitted,
            (await db.BilRefundCases.FindAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task RefundCannotExceedAvailableCredit()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(db, cashAmount: 500_000m, nonCashAmount: 0m, creditAmount: 100_000m);
        var service = CreateService(db, new AlwaysSucceedsProviderAdapter());

        var exception = await Assert.ThrowsAsync<BillingRefundValidationException>(() => service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 100_000.01m, "Melebihi saldo credit"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("melebihi saldo", exception.Message);
        Assert.Empty(db.BilRefundCases);
    }

    [Fact]
    public async Task SecondActiveRefundCaseAgainstSameCreditIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedFundedInvoiceAsync(db, cashAmount: 500_000m, nonCashAmount: 0m, creditAmount: 500_000m);
        var service = CreateService(db, new AlwaysSucceedsProviderAdapter());
        await service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 100_000m, "Refund pertama"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingRefundConflictException>(() => service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 50_000m, "Refund kedua"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("aktif", exception.Message);
        Assert.Single(db.BilRefundCases);
    }

    [Fact]
    public async Task NonRefundableOriginalMethodIsExcludedFromProportionalBase()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        // Cash funded most of the invoice but its method no longer supports refund;
        // only the small QRIS portion remains eligible, which is not enough to cover the request.
        var seeded = await SeedFundedInvoiceAsync(
            db, cashAmount: 500_000m, nonCashAmount: 50_000m, creditAmount: 100_000m,
            cashRefundEligible: false);
        var service = CreateService(db, new AlwaysSucceedsProviderAdapter());

        var exception = await Assert.ThrowsAsync<BillingRefundValidationException>(() => service.CreateAsync(
            RefundRequest(seeded.Invoice.Id, seeded.Credit.Id, 100_000m, "Metode asal tidak mendukung refund"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("tidak cukup mendukung refund", exception.Message);
        Assert.Empty(db.BilRefundCases);
    }

    [Fact]
    public void RefundEndpointsUseLockedPermissions()
    {
        AssertPermission(nameof(BillingFinancialExceptionsController.CreateRefund), "BillingRefund", "Create");
        AssertPermission(nameof(BillingFinancialExceptionsController.ApproveRefund), "BillingRefund", "Approve");
    }

    private static async Task<(BilInvoice Invoice, BilRefundableCredit Credit, BilTender CashTender, BilTender NonCashTender)> SeedFundedInvoiceAsync(
        Repositories.ApplicationDbContext db,
        decimal cashAmount,
        decimal nonCashAmount,
        decimal creditAmount,
        string serviceType = "RAJAL",
        bool cashRefundEligible = true)
    {
        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = serviceType == "RANAP" ? EncounterType.Inpatient : EncounterType.Outpatient,
            EncounterStatus = EncounterStatus.Registered,
            EncounterDate = DateTime.UtcNow,
            IsActive = true
        };
        var invoice = new BilInvoice
        {
            EncounterId = encounter.Id,
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = serviceType,
            Status = BillingInvoiceStatuses.Open,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        };
        var cashMethod = new MstPaymentMethod
        {
            Id = Guid.NewGuid(),
            PaymentMethodCode = "CASH",
            PaymentMethodName = "Tunai",
            IsCash = true,
            IsActive = true,
            IsAvailableForBilling = true,
            IsAvailableForRefund = cashRefundEligible
        };
        var nonCashMethod = new MstPaymentMethod
        {
            Id = Guid.NewGuid(),
            PaymentMethodCode = "QRIS",
            PaymentMethodName = "QRIS",
            IntegrationCode = "QRIS",
            IsCash = false,
            IsActive = true,
            IsAvailableForBilling = true,
            IsAvailableForRefund = true
        };
        var settlement = new BilSettlement
        {
            InvoiceId = invoice.Id,
            Purpose = BillingSettlementPurposes.InvoicePayment,
            RequestedAmount = cashAmount + nonCashAmount,
            SuccessfulAmount = cashAmount + nonCashAmount,
            AllocatedAmount = 0,
            Status = BillingSettlementStatuses.Settled,
            IdempotencyKey = Guid.NewGuid(),
            PayloadHash = new string('S', 64),
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        };
        var cashTender = new BilTender
        {
            SettlementId = settlement.Id,
            Settlement = settlement,
            PaymentMethodId = cashMethod.Id,
            Amount = cashAmount,
            Status = BillingTenderStatuses.Succeeded,
            IdempotencyKey = Guid.NewGuid(),
            PayloadHash = new string('T', 64),
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            AttemptedAt = DateTimeOffset.UtcNow,
            SettledAt = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        };
        settlement.Tenders.Add(cashTender);
        db.TrxPatientEncounters.Add(encounter);
        db.MstPaymentMethods.Add(cashMethod);
        db.BilInvoices.Add(invoice);
        db.BilSettlements.Add(settlement);
        db.BilTenders.Add(cashTender);

        BilTender nonCashTender;
        if (nonCashAmount > 0)
        {
            nonCashTender = new BilTender
            {
                SettlementId = settlement.Id,
                Settlement = settlement,
                PaymentMethodId = nonCashMethod.Id,
                Amount = nonCashAmount,
                Status = BillingTenderStatuses.Succeeded,
                ProviderReference = "PRIOR-PROVIDER-REF",
                IdempotencyKey = Guid.NewGuid(),
                PayloadHash = new string('U', 64),
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid(),
                AttemptedAt = DateTimeOffset.UtcNow,
                SettledAt = DateTimeOffset.UtcNow,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow
            };
            settlement.Tenders.Add(nonCashTender);
            db.MstPaymentMethods.Add(nonCashMethod);
            db.BilTenders.Add(nonCashTender);
        }
        else
        {
            nonCashTender = cashTender;
        }

        var credit = new BilRefundableCredit
        {
            InvoiceId = invoice.Id,
            SourceType = BillingRefundableCreditSourceTypes.Settlement,
            SourceId = settlement.Id,
            OriginalAmount = creditAmount,
            AvailableAmount = creditAmount,
            Status = BillingRefundableCreditStatuses.Available,
            RecognizedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow
        };
        db.BilRefundableCredits.Add(credit);
        await db.SaveChangesAsync();
        return (invoice, credit, cashTender, nonCashTender);
    }

    private static CreateRefundRequest RefundRequest(
        Guid invoiceId, Guid creditId, decimal amount, string reason) => new()
    {
        InvoiceId = invoiceId,
        RefundableCreditId = creditId,
        RequestedAmount = amount,
        Reason = reason,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static RefundApprovalRequest ApprovalRequest(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = "Finance menyetujui refund proporsional"
    };

    private static BillingRefundService CreateService(
        Repositories.ApplicationDbContext db, IBillingPaymentProviderAdapter adapter) =>
        new(db, adapter, Logger());

    private static LoggerService Logger() =>
        new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    private static void AssertPermission(string methodName, string controller, string action)
    {
        var attribute = typeof(BillingFinancialExceptionsController)
            .GetMethod(methodName)?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal(controller, arguments[0]);
        Assert.Equal(action, arguments[1]);
    }

    private sealed class AlwaysSucceedsProviderAdapter : IBillingPaymentProviderAdapter
    {
        public Task<BillingPaymentProviderResult> SubmitAsync(
            BillingPaymentProviderRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new BillingPaymentProviderResult(
                $"evt:{request.TenderId:N}",
                BillingPaymentProviderOutcome.Succeeded,
                $"REF-{request.TenderId:N}"[..12],
                "PROVIDER_OK",
                DateTimeOffset.UtcNow,
                request.CorrelationId,
                request.CausationId));
    }
}
