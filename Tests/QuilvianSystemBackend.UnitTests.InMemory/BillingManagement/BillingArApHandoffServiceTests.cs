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

public sealed class BillingArApHandoffServiceTests
{
    [Fact]
    public async Task NormalSelfPayFinalizationCreatesReadyApHandoffOnlyWithNoArHandoff()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var seeded = await SeedInvoiceAsync(db, patientAmount: 200_000m, doctorShare: 50_000m, doctorId: doctorId);
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 200_000m);
        var finalizationService = CreateFinalizationService(db);

        var result = await finalizationService.FinalizeAsync(
            seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Finalisasi self-pay"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(db.BilArHandoffs.Where(x => x.InvoiceId == seeded.Invoice.Id));
        var ap = await db.BilApHandoffs.SingleAsync(x => x.InvoiceId == seeded.Invoice.Id);
        Assert.Equal(doctorId, ap.DoctorId);
        Assert.Equal(50_000m, ap.Amount);
        Assert.Equal(BillingApReadinessStatuses.Ready, ap.ReadinessStatus);
        Assert.NotNull(ap.ReadyAt);

        var status = await CreateHandoffService(db).GetHandoffStatusAsync(result.Id, CancellationToken.None);
        Assert.Empty(status.ArHandoffs);
        Assert.Single(status.ApHandoffs);
    }

    [Fact]
    public async Task DepartureExceptionCreatesPatientGuarantorArHandoffAndNotReadyAp()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var seeded = await SeedInvoiceAsync(db, patientAmount: 400_000m, doctorShare: 60_000m, doctorId: doctorId);
        var finalizationService = CreateFinalizationService(db);

        var result = await finalizationService.FinalizeAsync(
            seeded.Invoice.Id,
            FinalizeRequest(seeded.Invoice.RowVersion, "Pasien meninggal dunia",
                BillingDepartureReasons.Death, "Ny. Uji", "Istri sah"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var ar = await db.BilArHandoffs.SingleAsync(x => x.InvoiceId == seeded.Invoice.Id);
        Assert.Equal(BillingArDebtorTypes.PatientGuarantor, ar.DebtorType);
        Assert.Equal(400_000m, ar.Amount);
        var invoice = await db.BilInvoices.FindAsync(seeded.Invoice.Id);
        Assert.Equal(invoice!.InvoiceDate, ar.DueDate);

        var ap = await db.BilApHandoffs.SingleAsync(x => x.InvoiceId == seeded.Invoice.Id);
        Assert.Equal(BillingApReadinessStatuses.NotReady, ap.ReadinessStatus);
        Assert.Null(ap.ReadyAt);
        Assert.Equal(result.Id, ar.FinalizationRecordId);
    }

    [Fact]
    public async Task InsuredInvoiceCreatesPayerArHandoffAndKeepsApNotReady()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(
            db, patientAmount: 100_000m, doctorShare: 30_000m, doctorId: Guid.NewGuid(),
            primaryAmount: 250_000m, excessAmount: 50_000m);
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 100_000m);
        var finalizationService = CreateFinalizationService(db);

        await finalizationService.FinalizeAsync(
            seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Finalisasi pasien asuransi"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var ar = await db.BilArHandoffs.SingleAsync(x => x.InvoiceId == seeded.Invoice.Id);
        Assert.Equal(BillingArDebtorTypes.Payer, ar.DebtorType);
        Assert.Equal(300_000m, ar.Amount);
        var ap = await db.BilApHandoffs.SingleAsync(x => x.InvoiceId == seeded.Invoice.Id);
        Assert.Equal(BillingApReadinessStatuses.NotReady, ap.ReadinessStatus);
    }

    [Fact]
    public async Task NoDoctorOnEncounterMeansNoApHandoffCreated()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, patientAmount: 150_000m, doctorShare: 0m, doctorId: null);
        await SeedFullPaymentAsync(db, seeded.Invoice.Id, 150_000m);
        var finalizationService = CreateFinalizationService(db);

        await finalizationService.FinalizeAsync(
            seeded.Invoice.Id, FinalizeRequest(seeded.Invoice.RowVersion, "Tanpa dokter"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(db.BilApHandoffs.Where(x => x.InvoiceId == seeded.Invoice.Id));
    }

    [Fact]
    public async Task PostFinalWriteOffCreatesLinkedHandoffAdjustmentAgainstExistingArHandoffOnce()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, patientAmount: 500_000m, doctorShare: 0m, doctorId: null);
        var finalizationService = CreateFinalizationService(db);
        var finalized = await finalizationService.FinalizeAsync(
            seeded.Invoice.Id,
            FinalizeRequest(seeded.Invoice.RowVersion, "Departure dengan sisa piutang",
                BillingDepartureReasons.Dama, "Ny. Uji", "Wali sah"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var arHandoff = await db.BilArHandoffs.SingleAsync(x => x.InvoiceId == seeded.Invoice.Id);

        var exceptionService = new BillingFinancialExceptionService(db, CreateHandoffService(db), Logger());
        var writeOff = await exceptionService.CreateWriteOffAsync(
            new CreateWriteOffRequest
            {
                InvoiceId = seeded.Invoice.Id,
                Amount = 200_000m,
                ExpectedInvoiceRowVersion = finalized.InvoiceRowVersion,
                Reason = "Sebagian piutang tidak tertagih pasca-final",
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        await exceptionService.ApproveWriteOffAsync(
            writeOff.Id,
            new WriteOffApprovalRequest { ExpectedRowVersion = writeOff.RowVersion, Reason = "Finance menyetujui" },
            Guid.NewGuid(), CancellationToken.None);

        var correction = await db.BilHandoffAdjustments.SingleAsync(x => x.SourceWriteOffCaseId == writeOff.Id);
        Assert.Equal(arHandoff.Id, correction.ArHandoffId);
        Assert.Equal(BillingAdjustmentDirections.Credit, correction.Direction);
        Assert.Equal(200_000m, correction.Amount);

        var status = await CreateHandoffService(db).GetHandoffStatusAsync(finalized.Id, CancellationToken.None);
        Assert.Single(status.Adjustments);
    }

    [Fact]
    public void FinalizationHandoffsEndpointUsesLockedPermission()
    {
        var attribute = typeof(BillingFinalizationsController)
            .GetMethod(nameof(BillingFinalizationsController.Handoffs))?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("BillingFinalization", arguments[0]);
        Assert.Equal("Read", arguments[1]);
    }

    private static async Task<(BilInvoice Invoice, BilCalculationVersion Calculation)> SeedInvoiceAsync(
        Repositories.ApplicationDbContext db,
        decimal patientAmount,
        decimal doctorShare,
        Guid? doctorId,
        decimal primaryAmount = 0,
        decimal excessAmount = 0)
    {
        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            DoctorId = doctorId,
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
            SourceStatus = "PERFORMED",
            SourceOccurredAt = DateTimeOffset.UtcNow,
            CategoryId = category.Id,
            Category = category,
            DescriptionSnapshot = "Item pengujian handoff",
            Quantity = 1,
            UnitPrice = patientAmount + primaryAmount + excessAmount,
            DoctorShare = doctorShare,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('H', 64),
            CreateDateTime = DateTime.UtcNow.AddMinutes(-10)
        };
        var calculation = new BilCalculationVersion
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            VersionNo = 1,
            GrossAmount = patientAmount + primaryAmount + excessAmount,
            PatientAmount = patientAmount,
            PrimaryAmount = primaryAmount,
            ExcessAmount = excessAmount,
            IsLocked = true,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Kalkulasi awal pengujian handoff",
            BreakdownSnapshot = "{}",
            CreateDateTime = DateTime.UtcNow.AddMinutes(-5)
        };
        db.TrxPatientEncounters.Add(encounter);
        db.MstBillingItemCategories.Add(category);
        db.BilInvoices.Add(invoice);
        db.BilInvoiceItems.Add(item);
        db.BilCalculationVersions.Add(calculation);
        await db.SaveChangesAsync();
        return (invoice, calculation);
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

    private static BillingArApHandoffService CreateHandoffService(Repositories.ApplicationDbContext db) =>
        new(db, Logger());

    private static BillingFinalizationService CreateFinalizationService(Repositories.ApplicationDbContext db) =>
        new(db, new ContractBillingChargeSourceAdapter(), CreateHandoffService(db), Logger());

    private static LoggerService Logger() =>
        new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
}
