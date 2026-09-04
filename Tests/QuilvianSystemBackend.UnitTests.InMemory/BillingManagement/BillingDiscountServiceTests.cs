using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingDiscountServiceTests
{
    [Fact]
    public async Task MasterPromoIsEffectiveImmediatelyAndReducesPatientPortion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db);
        var calculationService = CreateCalculationService(db);
        var initial = await calculationService.RecalculateAsync(
            seeded.Invoice.Id, CalculationRequest(seeded.Invoice.RowVersion, "Kalkulasi sebelum promo"),
            Guid.NewGuid(), CancellationToken.None);
        var policy = Policy(DiscountPolicyValues.PromoTotal, DiscountPolicyValues.PatientPortion,
            DiscountPolicyValues.Percentage, 10m);
        db.MstDiscountPolicies.Add(policy);
        await db.SaveChangesAsync();
        var discountService = CreateDiscountService(db);

        var applied = await discountService.ApplyAsync(seeded.Invoice.Id,
            ApplyRequest(policy.Id, initial.InvoiceRowVersion, "Promo master transaksi"),
            Guid.NewGuid(), CancellationToken.None);
        var recalculated = await calculationService.RecalculateAsync(
            seeded.Invoice.Id, CalculationRequest(applied.InvoiceRowVersion, "Promo master diterapkan"),
            Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingDiscountApprovalStatuses.Approved, applied.ApprovalStatus);
        Assert.True(applied.IsEffective);
        Assert.Equal(10_000m, recalculated.TotalDiscount);
        Assert.Equal(90_000m, recalculated.PatientAmount);
        var provenance = Assert.Single(recalculated.Breakdown.Discounts);
        Assert.Equal(policy.Id, provenance.DiscountPolicyId);
        Assert.Equal(100_000m, provenance.BasisAmount);
        Assert.Equal(10_000m, provenance.AppliedAmount);
    }

    [Fact]
    public async Task DoctorDiscountWaitsForCorrectDoctorAndOnlyThenChangesItemNet()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var seeded = await SeedInvoiceAsync(db, doctorId: doctorId, doctorShare: 30_000m);
        var policy = Policy(DiscountPolicyValues.Doctor, DiscountPolicyValues.DoctorShare,
            DiscountPolicyValues.FixedAmount, 1m, 20_000m);
        var requesterId = Guid.NewGuid();
        var correctDoctorUserId = Guid.NewGuid();
        var otherDoctorUserId = Guid.NewGuid();
        db.MstDiscountPolicies.Add(policy);
        db.Users.AddRange(
            User(correctDoctorUserId, doctorId, "doctor.correct"),
            User(otherDoctorUserId, Guid.NewGuid(), "doctor.other"));
        await db.SaveChangesAsync();
        var discountService = CreateDiscountService(db);
        var calculationService = CreateCalculationService(db);

        var applied = await discountService.ApplyAsync(seeded.Invoice.Id,
            ApplyRequest(policy.Id, seeded.Invoice.RowVersion, "Permintaan diskon dokter", seeded.Item.Id, 15_000m),
            requesterId, CancellationToken.None);
        var pendingCalculation = await calculationService.RecalculateAsync(
            seeded.Invoice.Id, CalculationRequest(applied.InvoiceRowVersion, "Diskon masih pending"),
            requesterId, CancellationToken.None);
        var rowVersionBeforeDeniedApproval = pendingCalculation.InvoiceRowVersion;

        await Assert.ThrowsAsync<BillingDiscountForbiddenException>(() =>
            discountService.ApproveDoctorAsync(seeded.Invoice.Id, applied.Id,
                ApproveRequest(rowVersionBeforeDeniedApproval), otherDoctorUserId, CancellationToken.None));
        Assert.Equal(rowVersionBeforeDeniedApproval, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.RowVersion);
        Assert.Equal(BillingDiscountApprovalStatuses.PendingDoctor,
            (await db.BilDiscountApplications.FindAsync(applied.Id))!.ApprovalStatus);

        var approved = await discountService.ApproveDoctorAsync(seeded.Invoice.Id, applied.Id,
            ApproveRequest(rowVersionBeforeDeniedApproval), correctDoctorUserId, CancellationToken.None);
        var approvedCalculation = await calculationService.RecalculateAsync(
            seeded.Invoice.Id, CalculationRequest(approved.InvoiceRowVersion, "Diskon dokter disetujui"),
            correctDoctorUserId, CancellationToken.None);

        Assert.Equal(BillingDiscountApprovalStatuses.PendingDoctor, applied.ApprovalStatus);
        Assert.Equal(0m, pendingCalculation.TotalDiscount);
        Assert.Equal(BillingDiscountApprovalStatuses.Approved, approved.ApprovalStatus);
        Assert.Equal(correctDoctorUserId, approved.ApprovedBy);
        Assert.Equal(100_000m, approvedCalculation.GrossAmount);
        Assert.Equal(15_000m, approvedCalculation.ItemDiscount);
        Assert.Equal(85_000m, approvedCalculation.PatientAmount);
        Assert.Equal(15_000m, Assert.Single(approvedCalculation.Breakdown.Discounts).AppliedAmount);
    }

    [Fact]
    public async Task DoctorCannotMakeAndApproveOwnDiscountRequest()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var seeded = await SeedInvoiceAsync(db, doctorId: doctorId, doctorShare: 25_000m);
        var policy = Policy(DiscountPolicyValues.Doctor, DiscountPolicyValues.DoctorShare,
            DiscountPolicyValues.FixedAmount, 1m, 20_000m);
        db.MstDiscountPolicies.Add(policy);
        db.Users.Add(User(doctorUserId, doctorId, "doctor.self"));
        await db.SaveChangesAsync();
        var service = CreateDiscountService(db);

        var applied = await service.ApplyAsync(seeded.Invoice.Id,
            ApplyRequest(policy.Id, seeded.Invoice.RowVersion, "Pengajuan oleh dokter", seeded.Item.Id, 10_000m),
            doctorUserId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingDiscountForbiddenException>(() =>
            service.ApproveDoctorAsync(seeded.Invoice.Id, applied.Id,
                ApproveRequest(applied.InvoiceRowVersion), doctorUserId, CancellationToken.None));
        Assert.Contains("sendiri", exception.Message);
        Assert.Equal(BillingDiscountApprovalStatuses.PendingDoctor,
            (await db.BilDiscountApplications.FindAsync(applied.Id))!.ApprovalStatus);
    }

    [Fact]
    public async Task DoctorDiscountAbovePolicyLimitIsHeldForFinanceAndHasNoEffect()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var seeded = await SeedInvoiceAsync(db, doctorId: doctorId, doctorShare: 30_000m);
        var policy = Policy(DiscountPolicyValues.Doctor, DiscountPolicyValues.DoctorShare,
            DiscountPolicyValues.FixedAmount, 1m, 10_000m);
        db.MstDiscountPolicies.Add(policy);
        db.Users.Add(User(doctorUserId, doctorId, "doctor.finance-exception"));
        await db.SaveChangesAsync();
        var discountService = CreateDiscountService(db);

        var applied = await discountService.ApplyAsync(seeded.Invoice.Id,
            ApplyRequest(policy.Id, seeded.Invoice.RowVersion, "Melewati limit policy", seeded.Item.Id, 15_000m),
            Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingDiscountApprovalStatuses.PendingFinance, applied.ApprovalStatus);
        Assert.True(applied.RequiresFinanceApproval);
        var exception = await Assert.ThrowsAsync<BillingDiscountValidationException>(() =>
            discountService.ApproveDoctorAsync(seeded.Invoice.Id, applied.Id,
                ApproveRequest(applied.InvoiceRowVersion), doctorUserId, CancellationToken.None));
        Assert.Contains("Finance", exception.Message);

        var calculation = await CreateCalculationService(db).RecalculateAsync(
            seeded.Invoice.Id, CalculationRequest(applied.InvoiceRowVersion, "Exception belum disetujui Finance"),
            Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(0m, calculation.TotalDiscount);
        Assert.Equal(100_000m, calculation.PatientAmount);
    }

    [Fact]
    public async Task DoctorDiscountCannotExceedDoctorShare()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, doctorId: Guid.NewGuid(), doctorShare: 20_000m);
        var policy = Policy(DiscountPolicyValues.Doctor, DiscountPolicyValues.DoctorShare,
            DiscountPolicyValues.FixedAmount, 1m, 30_000m);
        db.MstDiscountPolicies.Add(policy);
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BillingDiscountValidationException>(() =>
            CreateDiscountService(db).ApplyAsync(seeded.Invoice.Id,
                ApplyRequest(policy.Id, seeded.Invoice.RowVersion, "Nominal tidak valid", seeded.Item.Id, 20_000.01m),
                Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("melebihi komponen jasa dokter", exception.Message);
        Assert.Empty(db.BilDiscountApplications);
    }

    [Fact]
    public async Task AdministrationFeeCategoryCannotBeDiscounted()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db, administrationFeeCategory: true);
        var policy = Policy(DiscountPolicyValues.PromoItem, DiscountPolicyValues.InvoiceItem,
            DiscountPolicyValues.Percentage, 10m);
        db.MstDiscountPolicies.Add(policy);
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BillingDiscountValidationException>(() =>
            CreateDiscountService(db).ApplyAsync(seeded.Invoice.Id,
                ApplyRequest(policy.Id, seeded.Invoice.RowVersion, "Promo item", seeded.Item.Id),
                Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Biaya administrasi tidak dapat didiskon.", exception.Message);
        Assert.Empty(db.BilDiscountApplications);
    }

    [Fact]
    public async Task TotalPromoNeverUsesAdministrationFeeAsDiscountBasis()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db);
        db.MstAdministrationFeePolicies.Add(new MstAdministrationFeePolicy
        {
            Code = "ADM-RAJAL-TEST",
            Name = "Biaya administrasi uji",
            ServiceType = "RAJAL",
            Amount = 20_000m,
            OncePerPatientLocalDay = true,
            ReplacementPriority = 10,
            Coverable = true,
            Discountable = false,
            EffectiveFrom = DateTimeOffset.UtcNow.AddHours(-1),
            IsActive = true
        });
        var promo = Policy(DiscountPolicyValues.PromoTotal, DiscountPolicyValues.PatientPortion,
            DiscountPolicyValues.Percentage, 100m);
        db.MstDiscountPolicies.Add(promo);
        await db.SaveChangesAsync();
        var calculationService = CreateCalculationService(db);
        var initial = await calculationService.RecalculateAsync(seeded.Invoice.Id,
            CalculationRequest(seeded.Invoice.RowVersion, "Kalkulasi termasuk admin"), Guid.NewGuid(), CancellationToken.None);

        var applied = await CreateDiscountService(db).ApplyAsync(seeded.Invoice.Id,
            ApplyRequest(promo.Id, initial.InvoiceRowVersion, "Promo maksimum"), Guid.NewGuid(), CancellationToken.None);
        var recalculated = await calculationService.RecalculateAsync(seeded.Invoice.Id,
            CalculationRequest(applied.InvoiceRowVersion, "Promo tanpa admin"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(120_000m, initial.PatientAmount);
        Assert.Equal(100_000m, recalculated.TotalDiscount);
        Assert.Equal(20_000m, recalculated.PatientAmount);
        Assert.Equal(20_000m, recalculated.AdministrationFeeAmount);
    }

    [Fact]
    public async Task StaleRowVersionAndFinalInvoiceAreRejectedWithoutMutation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceAsync(db);
        var policy = Policy(DiscountPolicyValues.PromoItem, DiscountPolicyValues.InvoiceItem,
            DiscountPolicyValues.Percentage, 10m);
        db.MstDiscountPolicies.Add(policy);
        await db.SaveChangesAsync();
        var service = CreateDiscountService(db);

        await Assert.ThrowsAsync<BillingDiscountConflictException>(() => service.ApplyAsync(
            seeded.Invoice.Id, ApplyRequest(policy.Id, Guid.NewGuid(), "Versi stale", seeded.Item.Id),
            Guid.NewGuid(), CancellationToken.None));
        Assert.Empty(db.BilDiscountApplications);

        seeded.Invoice.Status = BillingInvoiceStatuses.Final;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BillingDiscountValidationException>(() => service.ApplyAsync(
            seeded.Invoice.Id, ApplyRequest(policy.Id, seeded.Invoice.RowVersion, "Invoice final", seeded.Item.Id),
            Guid.NewGuid(), CancellationToken.None));
        Assert.Empty(db.BilDiscountApplications);
    }

    [Fact]
    public void DiscountEndpointsUseLockedPermissions()
    {
        AssertPermission(nameof(BillingInvoicesController.ApplyDiscount), "BillingDiscount", "Create");
        AssertPermission(nameof(BillingInvoicesController.ApproveDoctorDiscount), "BillingDoctorDiscount", "Approve");
    }

    private static async Task<(BilInvoice Invoice, BilInvoiceItem Item)> SeedInvoiceAsync(
        Repositories.ApplicationDbContext db,
        Guid? doctorId = null,
        decimal doctorShare = 0,
        bool administrationFeeCategory = false)
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
            BillingItemCategoryCode = administrationFeeCategory ? "ADMIN" : "PROC",
            BillingItemCategoryName = administrationFeeCategory ? "Biaya administrasi" : "Procedure",
            IsAdministrationFee = administrationFeeCategory,
            IsCoveredByInsuranceDefault = true,
            IsActive = true
        };
        var invoice = new BilInvoice
        {
            EncounterId = encounter.Id,
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = "RAJAL",
            Status = BillingInvoiceStatuses.Open,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        };
        var item = new BilInvoiceItem
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            SourceDomain = "PROCEDURE",
            SourceDetailId = Guid.NewGuid().ToString(),
            SourceVersion = 1,
            SourceContractVersion = "TEST-1",
            SourceStatus = "CONFIRMED",
            SourceOccurredAt = DateTimeOffset.UtcNow,
            CategoryId = category.Id,
            Category = category,
            DescriptionSnapshot = "Pelayanan uji diskon",
            Quantity = 1,
            UnitPrice = 100_000m,
            DoctorShare = doctorShare,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('D', 64)
        };
        invoice.Items.Add(item);
        db.TrxPatientEncounters.Add(encounter);
        db.MstBillingItemCategories.Add(category);
        db.BilInvoices.Add(invoice);
        await db.SaveChangesAsync();
        return (invoice, item);
    }

    private static MstDiscountPolicy Policy(
        string type,
        string target,
        string valueType,
        decimal value,
        decimal? limit = null) => new()
    {
        Code = $"DISC-{Guid.NewGuid():N}"[..30],
        Name = "Policy diskon uji",
        DiscountType = type,
        TargetComponent = target,
        ValueType = valueType,
        Value = value,
        Limit = limit,
        RequiresApproval = type == DiscountPolicyValues.Doctor,
        ApproverRole = type == DiscountPolicyValues.Doctor ? DiscountPolicyValues.DoctorApprover : null,
        EffectiveFrom = DateTimeOffset.UtcNow.AddHours(-1),
        IsActive = true
    };

    private static ApplicationUser User(Guid id, Guid doctorId, string userName) => new()
    {
        Id = id,
        UserCode = userName,
        UserName = userName,
        NormalizedUserName = userName.ToUpperInvariant(),
        DisplayName = userName,
        DoctorId = doctorId,
        IsActive = true
    };

    private static ApplyDiscountRequest ApplyRequest(
        Guid policyId,
        Guid rowVersion,
        string reason,
        Guid? itemId = null,
        decimal? amount = null) => new()
    {
        DiscountPolicyId = policyId,
        InvoiceItemId = itemId,
        RequestedAmount = amount,
        ExpectedRowVersion = rowVersion,
        Reason = reason
    };

    private static ApproveDiscountRequest ApproveRequest(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = "Dokter menyetujui pengurangan share"
    };

    private static RecalculateInvoiceRequest CalculationRequest(Guid rowVersion, string reason) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = reason
    };

    private static BillingDiscountService CreateDiscountService(Repositories.ApplicationDbContext db) =>
        new(db, Logger());

    private static BillingCalculationService CreateCalculationService(Repositories.ApplicationDbContext db) =>
        new(
            db,
            new RegistrationBillingCoverageAdapter(db),
            new BillingAllocationService(db, Logger()),
            Logger());

    private static LoggerService Logger() =>
        new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    private static void AssertPermission(string methodName, string controller, string action)
    {
        var attribute = typeof(BillingInvoicesController).GetMethod(methodName)?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal(controller, arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
