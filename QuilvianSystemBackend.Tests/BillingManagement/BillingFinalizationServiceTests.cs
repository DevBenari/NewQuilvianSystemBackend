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

public sealed class BillingFinalizationServiceTests
{
    [Fact]
    public async Task NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 200_000m, "PERFORMED");
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 200_000m);
        var service = CreateService(db);

        var result = await service.FinalizeAsync(
            seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Finalisasi normal"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingInvoiceStatuses.Final, result.InvoiceStatus);
        Assert.False(result.IsDepartureException);
        Assert.Equal(0m, result.OutstandingAtFinalization);
        var invoice = await db.BilInvoices.FindAsync(seeded.Invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Final, invoice!.Status);
        Assert.NotNull(invoice.InvoiceDate);
    }

    [Fact]
    public async Task FinalizationBlockedWhenOutstandingRemainsWithoutDepartureException()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 200_000m, "PERFORMED");
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinalizationBlockedException>(() =>
            service.FinalizeAsync(
                seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Belum lunas"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(200_000m, exception.Checklist.Outstanding);
        Assert.Contains(exception.Checklist.BlockingReasons, x => x.Contains("belum lunas"));
        Assert.Equal(BillingInvoiceStatuses.Open, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);
    }

    [Fact]
    public async Task FinalizationBlockedWhenActiveOrderNotYetComplete()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 200_000m, "CONFIRMED");
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 200_000m);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinalizationBlockedException>(() =>
            service.FinalizeAsync(
                seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Order belum selesai"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.False(exception.Checklist.AllOrdersComplete);
        Assert.Contains(exception.Checklist.BlockingReasons, x => x.Contains("Semua order"));
    }

    [Fact]
    public async Task FinalizationBlockedWhenCalculationIsStale()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 200_000m, "PERFORMED");
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 200_000m);
        var category = await db.MstBillingItemCategories.FirstAsync();
        db.BilInvoiceItems.Add(new BilInvoiceItem
        {
            InvoiceId = seeded.Invoice.Id,
            SourceDomain = "PROCEDURE",
            SourceDetailId = Guid.NewGuid().ToString(),
            SourceVersion = 1,
            SourceContractVersion = "BIL-INTEGRATION-0.4",
            SourceStatus = "CONFIRMED",
            SourceOccurredAt = DateTimeOffset.UtcNow,
            CategoryId = category.Id,
            DescriptionSnapshot = "Item susulan setelah kalkulasi",
            Quantity = 1,
            UnitPrice = 50_000m,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('N', 64),
            CreateDateTime = seeded.Calculation.CalculatedAt.UtcDateTime.AddMinutes(5)
        });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinalizationBlockedException>(() =>
            service.FinalizeAsync(
                seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Kalkulasi basi"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.False(exception.Checklist.CalculationCurrent);
        Assert.Contains(exception.Checklist.BlockingReasons, x => x.Contains("Tagihan berubah"));
    }

    [Fact]
    public async Task DepartureExceptionAllowsFinalizationWithOutstandingAndRecordsDebtor()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 500_000m, "PERFORMED");
        var service = CreateService(db);

        var result = await service.FinalizeAsync(
            seeded.Invoice.Id,
            FinalizeRequest(seeded.Invoice.RowVersion, "Pasien meninggal dunia",
                BillingDepartureReasons.Death, "Keluarga inti - Ny. Uji", "Istri sah"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsDepartureException);
        Assert.Equal(BillingDepartureReasons.Death, result.DepartureReason);
        Assert.Equal(500_000m, result.OutstandingAtFinalization);
        Assert.Equal(BillingInvoiceStatuses.Final, result.InvoiceStatus);
        var record = await db.BilFinalizationRecords.SingleAsync(x => x.InvoiceId == seeded.Invoice.Id);
        Assert.Equal("Istri sah", record.DebtorRelationship);
    }

    [Fact]
    public async Task DepartureExceptionWithoutDebtorEvidenceIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 300_000m, "PERFORMED");
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinalizationValidationException>(() =>
            service.FinalizeAsync(
                seeded.Invoice.Id,
                FinalizeRequest(seeded.Invoice.RowVersion, "DAMA tanpa debtor",
                    BillingDepartureReasons.Dama, null, null),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("menanggung sisa tagihan", exception.Message);
        Assert.Empty(db.BilFinalizationRecords);
    }

    [Fact]
    public async Task InvalidDepartureReasonCodeIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 300_000m, "PERFORMED");
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinalizationValidationException>(() =>
            service.FinalizeAsync(
                seeded.Invoice.Id,
                FinalizeRequest(seeded.Invoice.RowVersion, "Kode tidak valid",
                    "RUNAWAY", "Seseorang", "Tidak jelas"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("DEATH, EMERGENCY_TRANSFER, atau DAMA", exception.Message);
    }

    [Fact]
    public async Task FinalizeWithStaleRowVersionIsRejectedWithoutMutation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 200_000m, "PERFORMED");
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 200_000m);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinalizationConflictException>(() =>
            service.FinalizeAsync(
                seeded.Invoice.Id, FinalizeRequest(Guid.NewGuid(), "RowVersion basi"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("berubah", exception.Message);
        Assert.Equal(BillingInvoiceStatuses.Open, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);
        Assert.Empty(db.BilFinalizationRecords);
    }

    [Fact]
    public async Task CannotFinalizeInvoiceThatIsNotOpen()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 200_000m, "PERFORMED");
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 200_000m);
        var service = CreateService(db);
        var first = await service.FinalizeAsync(
            seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Finalisasi pertama"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingFinalizationConflictException>(() =>
            service.FinalizeAsync(
                seeded.Invoice.Id, FinalizeRequest(first.InvoiceRowVersion, "Coba finalisasi kedua"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("OPEN", exception.Message);
        Assert.Single(db.BilFinalizationRecords);
    }

    [Fact]
    public async Task PreviewReflectsWriteOffCoveredOutstandingAsReady()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, 150_000m, "PERFORMED");
        var exceptionService = new BillingFinancialExceptionService(db, new BillingArApHandoffService(db, Logger()), Logger());
        var writeOff = await exceptionService.CreateWriteOffAsync(
            new CreateWriteOffRequest
            {
                InvoiceId = seeded.Invoice.Id,
                Amount = 150_000m,
                ExpectedInvoiceRowVersion = seeded.Invoice.RowVersion,
                Reason = "Piutang kecil tidak tertagih",
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        await exceptionService.ApproveWriteOffAsync(
            writeOff.Id,
            new WriteOffApprovalRequest { ExpectedRowVersion = writeOff.RowVersion, Reason = "Finance menyetujui" },
            Guid.NewGuid(), CancellationToken.None);
        var finalizationService = CreateService(db);

        var preview = await finalizationService.PreviewAsync(seeded.Invoice.Id, CancellationToken.None);

        Assert.Equal(0m, preview.Outstanding);
        Assert.True(preview.AllOrdersComplete);
        Assert.True(preview.CalculationCurrent);
        Assert.True(preview.IsReadyForNormalFinalization);
        Assert.Empty(preview.BlockingReasons);
    }

    [Fact]
    public void FinalizationEndpointsUseLockedPermissions()
    {
        AssertPermission(nameof(BillingFinalizationsController.Preview), "BillingFinalization", "Read");
        AssertPermission(nameof(BillingFinalizationsController.Finalize), "BillingFinalization", "Create");
    }

    private static async Task<(BilInvoice Invoice, BilCalculationVersion Calculation, BilInvoiceItem Item)> SeedInvoiceAsync(
        Repositories.ApplicationDbContext db,
        decimal patientAmount,
        string itemSourceStatus)
    {
        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = EncounterType.Outpatient,
            EncounterStatus = EncounterStatus.Registered,
            EncounterDate = DateTime.UtcNow,
            IsActive = true
        };
        var category = new MstBillingItemCategory
        {
            Id = Guid.NewGuid(),
            BillingItemCategoryCode = "PROC",
            BillingItemCategoryName = "Procedure",
            IsAdministrationFee = false,
            IsCoveredByInsuranceDefault = true,
            IsActive = true
        };
        var invoice = new BilInvoice
        {
            EncounterId = encounter.Id,
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = "RAJAL",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow.AddMinutes(-10)
        };
        var item = new BilInvoiceItem
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            SourceDomain = "PROCEDURE",
            SourceDetailId = Guid.NewGuid().ToString(),
            SourceVersion = 1,
            SourceContractVersion = "BIL-INTEGRATION-0.4",
            SourceStatus = itemSourceStatus,
            SourceOccurredAt = DateTimeOffset.UtcNow,
            CategoryId = category.Id,
            Category = category,
            DescriptionSnapshot = "Item pengujian finalisasi",
            Quantity = 1,
            UnitPrice = patientAmount,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('I', 64),
            CreateDateTime = DateTime.UtcNow.AddMinutes(-10)
        };
        var calculation = new BilCalculationVersion
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            VersionNo = 1,
            GrossAmount = patientAmount,
            PatientAmount = patientAmount,
            IsLocked = true,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Kalkulasi awal pengujian finalisasi",
            BreakdownSnapshot = "{}",
            CreateDateTime = DateTime.UtcNow.AddMinutes(-5)
        };
        db.TrxPatientEncounters.Add(encounter);
        db.MstBillingItemCategories.Add(category);
        db.BilInvoices.Add(invoice);
        db.BilInvoiceItems.Add(item);
        db.BilCalculationVersions.Add(calculation);
        await db.SaveChangesAsync();
        return (invoice, calculation, item);
    }

    private static async Task SeedFullPaymentAsync(
        Repositories.ApplicationDbContext db, Guid invoiceId, decimal amount)
    {
        db.BilPaymentAllocations.Add(new BilPaymentAllocation
        {
            SettlementId = Guid.NewGuid(),
            TargetType = BillingAllocationTargetTypes.Invoice,
            TargetId = invoiceId,
            Amount = amount,
            CalculationVersion = 1,
            AllocatedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static FinalizeInvoiceRequest FinalizeRequest(
        Guid rowVersion,
        string reason,
        string? departureReason = null,
        string? debtorIdentity = null,
        string? debtorRelationship = null) => new()
    {
        ExpectedRowVersion = rowVersion,
        DepartureReason = departureReason,
        DebtorIdentity = debtorIdentity,
        DebtorRelationship = debtorRelationship,
        Reason = reason,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static BillingFinalizationService CreateService(Repositories.ApplicationDbContext db) =>
        new(db, new ContractBillingChargeSourceAdapter(), new BillingArApHandoffService(db, Logger()), Logger());

    private static LoggerService Logger() =>
        new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    private static void AssertPermission(string methodName, string controller, string action)
    {
        var attribute = typeof(BillingFinalizationsController)
            .GetMethod(methodName)?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal(controller, arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
