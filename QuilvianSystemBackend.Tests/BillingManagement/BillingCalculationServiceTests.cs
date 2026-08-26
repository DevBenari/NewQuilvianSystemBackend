using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingCalculationServiceTests
{
    [Fact]
    public async Task RecalculateCreatesImmutableVersionsWithTaxProvenance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at);
        db.MstTaxRules.Add(TaxRule("PROC", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var first = await service.RecalculateAsync(invoice.Id, Request(invoice.RowVersion, "Kalkulasi awal"), Guid.NewGuid(), CancellationToken.None);
        var second = await service.RecalculateAsync(invoice.Id, Request(first.InvoiceRowVersion, "Tarif diverifikasi ulang"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(1, first.VersionNo);
        Assert.Equal(2, second.VersionNo);
        Assert.Equal(100_000m, first.GrossAmount);
        Assert.Equal(11_000m, first.TaxAmount);
        Assert.Equal(111_000m, first.PatientAmount);
        Assert.Equal(BillingCalculationContract.Version, first.Breakdown.ContractVersion);
        Assert.Single(first.Breakdown.Taxes);
        Assert.Equal("Kalkulasi awal", (await db.BilCalculationVersions.SingleAsync(x => x.VersionNo == 1)).Reason);
        Assert.Equal(2, await db.BilCalculationVersions.CountAsync());
        Assert.Equal(2, (await db.BilInvoices.FindAsync(invoice.Id))!.CurrentCalculationVersion);
    }

    [Fact]
    public async Task CoverageWaterfallAppliesPrimaryThenExcessThenPatient()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var adapter = new FixedCoverageAdapter(new BillingCoverageDecision(
            "INSURER-CONTRACT-TEST", "APPROVED", "APPROVED", 60_000m, 25_000m, 0, []));
        var service = CreateService(db, adapter);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Coverage dihitung"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(60_000m, result.PrimaryAmount);
        Assert.Equal(40_000m, result.Breakdown.Coverage.ResidualAfterPrimary);
        Assert.Equal(25_000m, result.ExcessAmount);
        Assert.Equal(15_000m, result.Breakdown.Coverage.ResidualAfterExcess);
        Assert.Equal(15_000m, result.PatientAmount);
    }

    [Fact]
    public async Task CoverageCapRejectsAmountAboveEligibleWithoutCreatingVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var adapter = new FixedCoverageAdapter(new BillingCoverageDecision(
            "INSURER-CONTRACT-TEST", "APPROVED", "APPROVED", 80_000m, 30_000m, 0, []));
        var service = CreateService(db, adapter);

        var exception = await Assert.ThrowsAsync<BillingCalculationValidationException>(() =>
            service.RecalculateAsync(invoice.Id, Request(invoice.RowVersion, "Coverage invalid"), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("melebihi biaya", exception.Message);
        Assert.Empty(db.BilCalculationVersions);
    }

    [Fact]
    public async Task RejectedCoverageRemainsUnresolvedAndDoesNotShiftToPatient()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var adapter = new FixedCoverageAdapter(new BillingCoverageDecision(
            "INSURER-CONTRACT-TEST", "REJECTED", "NOT_CONFIGURED", 0, 0, 100_000m, []));
        var service = CreateService(db, adapter);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Claim ditolak"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0, result.PatientAmount);
        Assert.Equal(100_000m, result.UnresolvedCoverageAmount);
    }

    [Fact]
    public async Task AdministrationFeeIsOncePerLocalDayAndRanapAppliesReplacementDifference()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        var firstAt = new DateTimeOffset(2026, 8, 21, 1, 0, 0, TimeSpan.Zero);
        var secondAt = firstAt.AddHours(2);
        var inpatientAt = firstAt.AddHours(4);
        db.MstAdministrationFeePolicies.AddRange(
            AdministrationPolicy("ADM-RAJAL", "RAJAL", 20_000m, 10, firstAt),
            AdministrationPolicy("ADM-RANAP", "RANAP", 50_000m, 100, firstAt));
        var first = await SeedInvoiceAsync(db, patientId, "RAJAL", firstAt);
        var second = await SeedInvoiceAsync(db, patientId, "RAJAL", secondAt);
        var inpatient = await SeedInvoiceAsync(db, patientId, "RANAP", inpatientAt);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var firstResult = await service.RecalculateAsync(first.Id, Request(first.RowVersion, "Kunjungan pertama"), Guid.NewGuid(), CancellationToken.None);
        var secondResult = await service.RecalculateAsync(second.Id, Request(second.RowVersion, "Kunjungan kedua"), Guid.NewGuid(), CancellationToken.None);
        var inpatientResult = await service.RecalculateAsync(inpatient.Id, Request(inpatient.RowVersion, "Transfer rawat inap"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(20_000m, firstResult.AdministrationFeeAmount);
        Assert.Equal(0, secondResult.AdministrationFeeAmount);
        Assert.Equal(30_000m, inpatientResult.AdministrationFeeAmount);
        Assert.True(inpatientResult.Breakdown.AdministrationFee.ReplacesEarlierFee);
        Assert.Equal(50_000m, firstResult.AdministrationFeeAmount + secondResult.AdministrationFeeAmount + inpatientResult.AdministrationFeeAmount);
    }

    // BE-BKC-017 hardening (26 Agustus 2026): CalculateAdministrationFeeAsync mendapat SQL pre-filter
    // pada TrxPatientEncounter.EncounterDate (menggantikan penarikan seluruh riwayat pasien ke memori)
    // - lihat catatan di BillingCalculationService.cs. Test ini secara khusus membuktikan pre-filter
    // itu tetap benar untuk dua encounter yang berada pada businessDate WIB YANG SAMA tapi tanggal
    // kalender UTC-nya BERBEDA (melintasi batas 17:00 UTC), skenario yang akan salah bila pre-filter
    // naif hanya membandingkan tanggal kalender UTC alih-alih rentang WIB yang benar.
    [Fact]
    public async Task AdministrationFeeAcrossUtcMidnightBoundaryIsStillDetectedAsSameBusinessDay()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        // 2026-08-21T18:00:00Z = WIB 2026-08-22 01:00 (awal businessDate WIB 22 Agustus, tanggal kalender UTC-nya 21 Agustus).
        var firstAt = new DateTimeOffset(2026, 8, 21, 18, 0, 0, TimeSpan.Zero);
        // 2026-08-22T10:00:00Z = WIB 2026-08-22 17:00 (masih businessDate WIB yang sama, tanggal kalender UTC-nya 22 Agustus).
        var secondAt = new DateTimeOffset(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);
        Assert.Equal(
            AdministrationFeePolicyService.GetBusinessDate(firstAt),
            AdministrationFeePolicyService.GetBusinessDate(secondAt));
        Assert.NotEqual(firstAt.UtcDateTime.Date, secondAt.UtcDateTime.Date);

        db.MstAdministrationFeePolicies.Add(AdministrationPolicy("ADM-RAJAL-BOUNDARY", "RAJAL", 20_000m, 10, firstAt));
        var first = await SeedInvoiceAsync(db, patientId, "RAJAL", firstAt);
        var second = await SeedInvoiceAsync(db, patientId, "RAJAL", secondAt);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var firstResult = await service.RecalculateAsync(first.Id, Request(first.RowVersion, "Kunjungan pertama"), Guid.NewGuid(), CancellationToken.None);
        var secondResult = await service.RecalculateAsync(second.Id, Request(second.RowVersion, "Kunjungan kedua, businessDate WIB sama"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(20_000m, firstResult.AdministrationFeeAmount);
        Assert.Equal(0, secondResult.AdministrationFeeAmount);
    }

    [Fact]
    public async Task RegistrationCoverageAdapterUsesApprovedGenericPrimaryRule()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-TEST",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-PROC",
            RuleName = "Coverage procedure test",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 80,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Coverage primary"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(80_000m, result.PrimaryAmount);
        Assert.Equal(20_000m, result.PatientAmount);
        Assert.Equal("NOT_CONFIGURED", result.Breakdown.Coverage.ExcessStatus);
        Assert.Single(result.Breakdown.Coverage.AppliedRuleIds);
    }

    [Fact]
    public async Task StaleRowVersionAndClosedInvoiceAreRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        await Assert.ThrowsAsync<BillingCalculationConflictException>(() => service.RecalculateAsync(
            invoice.Id, Request(Guid.NewGuid(), "Versi stale"), Guid.NewGuid(), CancellationToken.None));

        invoice.Status = BillingInvoiceStatuses.Final;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BillingCalculationValidationException>(() => service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Final tidak boleh berubah"), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public void CalculationVersionHasUniqueInvoiceVersionIndex()
    {
        using var db = IsolatedBillingDbContextFactory.Create();
        var entity = db.Model.FindEntityType(typeof(BilCalculationVersion));
        var index = entity!.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(BilCalculationVersion.InvoiceId), nameof(BilCalculationVersion.VersionNo)]));
        Assert.True(index.IsUnique);
    }

    private static BillingCalculationService CreateService(Repositories.ApplicationDbContext db, IBillingCoverageAdapter adapter) =>
        new(
            db,
            adapter,
            new BillingAllocationService(
                db,
                new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor())),
            new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()));

    private static RecalculateInvoiceRequest Request(Guid rowVersion, string reason) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = reason
    };

    private static async Task<BilInvoice> SeedInvoiceAsync(
        Repositories.ApplicationDbContext db,
        Guid patientId,
        string serviceType,
        DateTimeOffset at,
        bool isProcedure = false)
    {
        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = patientId,
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = serviceType == "RANAP" ? EncounterType.Inpatient : EncounterType.Outpatient,
            EncounterStatus = EncounterStatus.Registered,
            EncounterDate = at.UtcDateTime,
            IsActive = true
        };
        var category = new MstBillingItemCategory
        {
            Id = Guid.NewGuid(),
            BillingItemCategoryCode = "PROC",
            BillingItemCategoryName = "Procedure test",
            IsProcedure = isProcedure,
            IsCoveredByInsuranceDefault = true,
            IsActive = true
        };
        var invoice = new BilInvoice
        {
            EncounterId = encounter.Id,
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = serviceType,
            Status = BillingInvoiceStatuses.Open,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = at.UtcDateTime
        };
        invoice.Items.Add(new BilInvoiceItem
        {
            InvoiceId = invoice.Id,
            SourceDomain = isProcedure ? "PROCEDURE" : "SERVICE",
            SourceDetailId = Guid.NewGuid().ToString(),
            SourceVersion = 1,
            SourceContractVersion = "TEST-1",
            SourceStatus = "CONFIRMED",
            SourceOccurredAt = at,
            CategoryId = category.Id,
            Category = category,
            DescriptionSnapshot = "Pelayanan fiktif",
            Quantity = 1,
            UnitPrice = 100_000m,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('A', 64)
        });

        db.TrxPatientEncounters.Add(encounter);
        db.MstBillingItemCategories.Add(category);
        db.BilInvoices.Add(invoice);
        await db.SaveChangesAsync();
        return invoice;
    }

    private static MstTaxRule TaxRule(string category, DateTimeOffset at) => new()
    {
        Code = "TAX-TEST",
        Name = "Tax test",
        TaxableCategory = category,
        Rate = 11,
        RoundingMode = TaxRuleValues.HalfUp,
        AllocationRule = TaxRuleValues.Proportional,
        EffectiveFrom = at.AddDays(-1),
        EffectiveTo = at.AddDays(1),
        IsActive = true
    };

    private static MstAdministrationFeePolicy AdministrationPolicy(
        string code,
        string serviceType,
        decimal amount,
        int priority,
        DateTimeOffset at) => new()
    {
        Code = code,
        Name = code,
        ServiceType = serviceType,
        Amount = amount,
        OncePerPatientLocalDay = true,
        ReplacementPriority = priority,
        Coverable = false,
        Discountable = false,
        EffectiveFrom = at.AddDays(-1),
        EffectiveTo = at.AddDays(1),
        IsActive = true
    };

    private sealed class FixedCoverageAdapter(BillingCoverageDecision decision) : IBillingCoverageAdapter
    {
        public Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken) =>
            Task.FromResult(decision);
    }

    private sealed class SelfPayCoverageAdapter : IBillingCoverageAdapter
    {
        public static readonly SelfPayCoverageAdapter Instance = new();
        public Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken) =>
            Task.FromResult(new BillingCoverageDecision("SELF-PAY-TEST", "SELF_PAY", "NOT_APPLICABLE", 0, 0, 0, []));
    }
}
