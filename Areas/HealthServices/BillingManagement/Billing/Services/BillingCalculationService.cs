using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingCalculationService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _dbContext;
    private readonly IBillingCoverageAdapter _coverageAdapter;
    private readonly BillingAllocationService _allocationService;
    private readonly LoggerService _loggerService;

    public BillingCalculationService(
        ApplicationDbContext dbContext,
        IBillingCoverageAdapter coverageAdapter,
        BillingAllocationService allocationService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _coverageAdapter = coverageAdapter;
        _allocationService = allocationService;
        _loggerService = loggerService;
    }

    public async Task<CalculationResponse> RecalculateAsync(
        Guid invoiceId,
        RecalculateInvoiceRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingCalculationValidationException("ExpectedRowVersion wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingCalculationValidationException("Alasan perhitungan ulang wajib diisi.");

        var lockContext = await (
            from invoice in _dbContext.BilInvoices.AsNoTracking()
            join encounter in _dbContext.TrxPatientEncounters.AsNoTracking()
                on invoice.EncounterId equals encounter.Id
            where invoice.Id == invoiceId && !invoice.IsDelete && !encounter.IsDelete && !encounter.IsCancel
            select new { invoice.EncounterId, encounter.PatientId, encounter.EncounterDate })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Invoice Billing atau encounter tidak ditemukan.");
        var lockedEffectiveAt = ToInstant(lockContext.EncounterDate);
        var lockedBusinessDate = AdministrationFeePolicyService.GetBusinessDate(lockedEffectiveAt);

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction is null)
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            }
            if (_dbContext.Database.IsRelational())
            {
                await AcquireLockAsync($"BIL_CALCULATION_{invoiceId:N}", cancellationToken);
                await AcquireLockAsync($"BIL_ENCOUNTER_{lockContext.EncounterId:N}", cancellationToken);
                await AcquireLockAsync(
                    $"BIL_ADMIN_{lockContext.PatientId:N}_{lockedBusinessDate:yyyyMMdd}", cancellationToken);
            }

            var invoice = await _dbContext.BilInvoices
                .Include(x => x.Items).ThenInclude(x => x.Category)
                .Include(x => x.DiscountApplications).ThenInclude(x => x.DiscountPolicy)
                .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");
            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingCalculationValidationException("Hanya invoice OPEN yang dapat dihitung ulang.");
            if (invoice.RowVersion != request.ExpectedRowVersion)
                throw new BillingCalculationConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");

            var encounter = await _dbContext.TrxPatientEncounters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
                ?? throw new KeyNotFoundException("Encounter invoice tidak ditemukan.");
            if (encounter.PatientId != lockContext.PatientId
                || AdministrationFeePolicyService.GetBusinessDate(ToInstant(encounter.EncounterDate)) != lockedBusinessDate)
                throw new BillingCalculationConflictException(
                    "Konteks pasien atau tanggal pelayanan berubah. Muat ulang sebelum melanjutkan.");
            var activeItems = invoice.Items
                .Where(x => !x.IsDelete && x.Status != BillingInvoiceItemStatuses.Voided)
                .OrderBy(x => x.CreateDateTime)
                .ToList();

            var calculatedAt = DateTimeOffset.UtcNow;
            var effectiveAt = ToInstant(encounter.EncounterDate);
            var administrationFee = activeItems.Count == 0
                ? new AdministrationFeeCalculationResponse
                {
                    BusinessDate = AdministrationFeePolicyService.GetBusinessDate(effectiveAt)
                }
                : await CalculateAdministrationFeeAsync(
                    invoice, encounter, effectiveAt, cancellationToken);
            var approvedDiscounts = invoice.DiscountApplications
                .Where(x => !x.IsDelete && x.ApprovalStatus == BillingDiscountApprovalStatuses.Approved)
                .OrderBy(x => x.CreateDateTime)
                .ThenBy(x => x.Id)
                .ToList();
            var itemResult = activeItems.Count == 0
                ? new ItemTaxResult([], [], [])
                : await CalculateItemsAndTaxesAsync(activeItems, approvedDiscounts, cancellationToken);

            var grossAmount = itemResult.Items.Sum(x => x.GrossAmount);
            var itemDiscount = itemResult.Items.Sum(x => x.ItemDiscount);
            var taxAmount = itemResult.Taxes.Sum(x => x.TaxAmount);
            var roundingAmount = 0m;
            var eligibleAmount = grossAmount + administrationFee.AppliedAmount - itemDiscount + taxAmount + roundingAmount;
            if (eligibleAmount < 0)
                throw new BillingCalculationValidationException("Nilai akhir invoice tidak boleh negatif.");

            var components = BuildCoverageComponents(activeItems, itemResult, administrationFee);
            var coverage = await _coverageAdapter.ResolveAsync(
                new BillingCoverageContext(invoice.Id, invoice.EncounterId, calculatedAt, eligibleAmount, components),
                cancellationToken);
            var coverageResult = ApplyCoverageWaterfall(eligibleAmount, components, coverage);
            var discountableItemIds = activeItems.Where(x => !x.Category.IsAdministrationFee)
                .Select(x => x.Id).ToHashSet();
            var patientPromos = ApplyPatientPromos(
                approvedDiscounts.Where(x => x.DiscountType == DiscountPolicyValues.PromoTotal),
                Math.Max(0, coverageResult.PatientAmount - administrationFee.AppliedAmount),
                itemResult.Items.Where(x => discountableItemIds.Contains(x.InvoiceItemId)).Sum(x => x.NetAmount));
            coverageResult.PatientAmount = Money(coverageResult.PatientAmount - patientPromos.TotalAmount);
            var totalDiscount = itemDiscount + patientPromos.TotalAmount;
            var appliedDiscounts = itemResult.Discounts.Concat(patientPromos.Discounts).ToList();
            foreach (var discount in appliedDiscounts)
            {
                var application = approvedDiscounts.Single(x => x.Id == discount.DiscountApplicationId);
                if (application.Amount == discount.AppliedAmount) continue;
                application.Amount = discount.AppliedAmount;
                application.UpdateDateTime = DateTime.UtcNow;
                application.UpdateBy = actorUserId;
            }

            var breakdown = new CalculationBreakdownResponse
            {
                ContractVersion = BillingCalculationContract.Version,
                AdministrationFee = administrationFee,
                Items = itemResult.Items,
                Discounts = appliedDiscounts,
                Taxes = itemResult.Taxes,
                Coverage = coverageResult
            };

            var version = new BilCalculationVersion
            {
                InvoiceId = invoice.Id,
                VersionNo = invoice.CurrentCalculationVersion + 1,
                GrossAmount = grossAmount,
                AdministrationFeeAmount = administrationFee.AppliedAmount,
                ItemDiscount = itemDiscount,
                TotalDiscount = totalDiscount,
                TaxAmount = taxAmount,
                PatientAmount = coverageResult.PatientAmount,
                PrimaryAmount = coverageResult.PrimaryAmount,
                ExcessAmount = coverageResult.ExcessAmount,
                UnresolvedCoverageAmount = coverageResult.UnresolvedAmount,
                RoundingAmount = roundingAmount,
                IsLocked = false,
                CalculatedAt = calculatedAt,
                Reason = request.Reason.Trim(),
                BreakdownSnapshot = JsonSerializer.Serialize(breakdown, SnapshotJsonOptions),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            var previousVersion = invoice.CurrentCalculationVersion;
            var previousTotal = await _dbContext.BilCalculationVersions.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.VersionNo == previousVersion && !x.IsDelete)
                .Select(x => (decimal?)(x.PatientAmount + x.PrimaryAmount + x.ExcessAmount + x.UnresolvedCoverageAmount))
                .FirstOrDefaultAsync(cancellationToken) ?? 0;

            _dbContext.BilCalculationVersions.Add(version);
            invoice.CurrentCalculationVersion = version.VersionNo;
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
            var refundableCredit = await _allocationService.ReconcileCalculationExcessAsync(
                invoice, version, actorUserId, calculatedAt, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await _loggerService.AuditAsync(LogCategory, "BillingInvoice.Recalculate", "Versi kalkulasi invoice dibuat.", new
            {
                InvoiceId = invoice.Id,
                Version = version.VersionNo,
                PreviousTotal = previousTotal,
                CurrentTotal = eligibleAmount - patientPromos.TotalAmount,
                version.PatientAmount,
                version.PrimaryAmount,
                version.ExcessAmount,
                version.UnresolvedCoverageAmount,
                RefundableCredit = refundableCredit,
                UserId = actorUserId,
                ActorUserId = actorUserId,
                Reason = version.Reason
            });

            return MapResponse(version, invoice.RowVersion);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingCalculationConflictException(
                "Versi kalkulasi tidak dapat disimpan karena invoice telah berubah.", exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    internal static CalculationResponse MapResponse(BilCalculationVersion version, Guid invoiceRowVersion) => new()
    {
        Id = version.Id,
        InvoiceId = version.InvoiceId,
        VersionNo = version.VersionNo,
        GrossAmount = version.GrossAmount,
        AdministrationFeeAmount = version.AdministrationFeeAmount,
        ItemDiscount = version.ItemDiscount,
        TotalDiscount = version.TotalDiscount,
        TaxAmount = version.TaxAmount,
        PatientAmount = version.PatientAmount,
        PrimaryAmount = version.PrimaryAmount,
        ExcessAmount = version.ExcessAmount,
        UnresolvedCoverageAmount = version.UnresolvedCoverageAmount,
        RoundingAmount = version.RoundingAmount,
        IsLocked = version.IsLocked,
        CalculatedAt = version.CalculatedAt,
        Reason = version.Reason,
        InvoiceRowVersion = invoiceRowVersion,
        Breakdown = DeserializeBreakdown(version.BreakdownSnapshot)
    };

    private async Task<AdministrationFeeCalculationResponse> CalculateAdministrationFeeAsync(
        BilInvoice invoice,
        TrxPatientEncounter encounter,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken)
    {
        var businessDate = AdministrationFeePolicyService.GetBusinessDate(effectiveAt);
        var policies = await _dbContext.MstAdministrationFeePolicies.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.ServiceType == invoice.ServiceType
                && x.EffectiveFrom <= effectiveAt && (x.EffectiveTo == null || effectiveAt < x.EffectiveTo))
            .OrderByDescending(x => x.ReplacementPriority)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
        if (policies.Count > 1)
            throw new BillingCalculationConflictException(
                "Lebih dari satu policy biaya administrasi aktif pada waktu pelayanan.");

        var policy = policies.SingleOrDefault();
        if (policy is null)
            return new AdministrationFeeCalculationResponse { BusinessDate = businessDate };

        var priorSnapshots = await (
            from priorInvoice in _dbContext.BilInvoices.AsNoTracking()
            join priorEncounter in _dbContext.TrxPatientEncounters.AsNoTracking()
                on priorInvoice.EncounterId equals priorEncounter.Id
            join calculation in _dbContext.BilCalculationVersions.AsNoTracking()
                on new { InvoiceId = priorInvoice.Id, VersionNo = priorInvoice.CurrentCalculationVersion }
                equals new { calculation.InvoiceId, calculation.VersionNo }
            where priorInvoice.Id != invoice.Id && !priorInvoice.IsDelete && !priorEncounter.IsDelete
                && priorEncounter.PatientId == encounter.PatientId && !calculation.IsDelete
            select calculation.BreakdownSnapshot)
            .ToListAsync(cancellationToken);

        var priorFees = priorSnapshots.Select(DeserializeBreakdown)
            .Where(x => x.AdministrationFee.BusinessDate == businessDate)
            .Select(x => x.AdministrationFee)
            .ToList();
        var priorApplied = priorFees.Sum(x => x.AppliedAmount);
        var priorPriority = priorFees.Count == 0 ? int.MinValue : priorFees.Max(x => x.ReplacementPriority);
        var replacesEarlierFee = priorApplied > 0 && policy.ReplacementPriority > priorPriority;
        if (replacesEarlierFee && policy.Amount < priorApplied)
            throw new BillingCalculationValidationException(
                "Policy pengganti menghasilkan biaya lebih kecil; adjustment terpisah diperlukan agar histori tetap utuh.");
        var applied = priorApplied == 0
            ? policy.Amount
            : replacesEarlierFee ? policy.Amount - priorApplied : 0;

        return new AdministrationFeeCalculationResponse
        {
            BusinessDate = businessDate,
            PolicyId = policy.Id,
            PolicyCode = policy.Code,
            PolicyAmount = policy.Amount,
            PriorAppliedAmount = priorApplied,
            AppliedAmount = applied,
            ReplacementPriority = policy.ReplacementPriority,
            Coverable = policy.Coverable,
            ReplacesEarlierFee = replacesEarlierFee
        };
    }

    private async Task<ItemTaxResult> CalculateItemsAndTaxesAsync(
        IReadOnlyList<BilInvoiceItem> activeItems,
        IReadOnlyList<BilDiscountApplication> approvedDiscounts,
        CancellationToken cancellationToken)
    {
        var firstOccurredAt = activeItems.Min(x => x.SourceOccurredAt);
        var lastOccurredAt = activeItems.Max(x => x.SourceOccurredAt);
        var taxRules = await _dbContext.MstTaxRules.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.EffectiveFrom <= lastOccurredAt
                && (x.EffectiveTo == null || firstOccurredAt < x.EffectiveTo))
            .ToListAsync(cancellationToken);

        var items = new List<CalculationItemResponse>();
        var taxes = new List<TaxCalculationResponse>();
        var discounts = new List<DiscountCalculationResponse>();
        foreach (var item in activeItems)
        {
            if (item.Category is null)
                throw new BillingCalculationValidationException("Kategori item invoice tidak ditemukan.");

            var gross = Money(item.Quantity * item.UnitPrice);
            var itemDiscount = 0m;
            var doctorDiscount = 0m;
            foreach (var application in approvedDiscounts.Where(x => x.InvoiceItemId == item.Id))
            {
                if (item.Category.IsAdministrationFee)
                    throw new BillingCalculationValidationException("Biaya administrasi tidak dapat didiskon.");

                decimal basis;
                decimal appliedAmount;
                if (application.DiscountType == DiscountPolicyValues.Doctor)
                {
                    basis = Money(item.DoctorShare);
                    appliedAmount = Money(application.Amount);
                    doctorDiscount += appliedAmount;
                    if (doctorDiscount > item.DoctorShare)
                        throw new BillingCalculationValidationException("Diskon dokter melebihi komponen jasa dokter.");
                }
                else if (application.DiscountType == DiscountPolicyValues.PromoItem)
                {
                    basis = Money(gross - itemDiscount);
                    appliedAmount = BillingDiscountService.CalculatePolicyAmount(application.DiscountPolicy, basis);
                }
                else
                {
                    throw new BillingCalculationValidationException("Target aplikasi diskon item tidak valid.");
                }

                itemDiscount = Money(itemDiscount + appliedAmount);
                if (itemDiscount > gross)
                    throw new BillingCalculationValidationException("Total diskon item melebihi nilai bruto item.");
                discounts.Add(MapDiscountCalculation(application, basis, appliedAmount));
            }
            var matchingRules = taxRules.Where(x =>
                    string.Equals(x.TaxableCategory, item.Category.BillingItemCategoryCode, StringComparison.OrdinalIgnoreCase)
                    && x.EffectiveFrom <= item.SourceOccurredAt
                    && (x.EffectiveTo == null || item.SourceOccurredAt < x.EffectiveTo))
                .ToList();
            if (matchingRules.Count > 1)
                throw new BillingCalculationConflictException(
                    $"Lebih dari satu tax rule aktif untuk kategori {item.Category.BillingItemCategoryCode} pada waktu pelayanan.");
            var rule = matchingRules.SingleOrDefault();
            var tax = 0m;
            if (rule is not null)
            {
                tax = TaxRuleService.CalculateTax(gross, itemDiscount, rule.Rate, rule.RoundingMode, 2);
                taxes.Add(new TaxCalculationResponse
                {
                    InvoiceItemId = item.Id,
                    TaxRuleId = rule.Id,
                    TaxRuleCode = rule.Code,
                    BasisAmount = gross - itemDiscount,
                    Rate = rule.Rate,
                    RoundingMode = rule.RoundingMode,
                    AllocationRule = rule.AllocationRule,
                    UnroundedAmount = (gross - itemDiscount) * rule.Rate / 100m,
                    TaxAmount = tax
                });
            }

            items.Add(new CalculationItemResponse
            {
                InvoiceItemId = item.Id,
                CategoryId = item.CategoryId,
                CategoryCode = item.Category.BillingItemCategoryCode,
                SourceDomain = item.SourceDomain,
                SourceVersion = item.SourceVersion,
                GrossAmount = gross,
                ItemDiscount = itemDiscount,
                TaxAmount = tax,
                NetAmount = gross - itemDiscount + tax,
                Coverable = item.Category.IsCoveredByInsuranceDefault
            });
        }

        return new ItemTaxResult(items, taxes, discounts);
    }

    private static PatientPromoResult ApplyPatientPromos(
        IEnumerable<BilDiscountApplication> applications,
        decimal patientAmount,
        decimal itemNetAmount)
    {
        var remainingDiscountableAmount = Money(Math.Min(patientAmount, itemNetAmount));
        var discounts = new List<DiscountCalculationResponse>();
        foreach (var application in applications)
        {
            var basis = remainingDiscountableAmount;
            var appliedAmount = BillingDiscountService.CalculatePolicyAmount(application.DiscountPolicy, basis);
            remainingDiscountableAmount = Money(remainingDiscountableAmount - appliedAmount);
            discounts.Add(MapDiscountCalculation(application, basis, appliedAmount));
        }

        return new PatientPromoResult(discounts.Sum(x => x.AppliedAmount), discounts);
    }

    private static DiscountCalculationResponse MapDiscountCalculation(
        BilDiscountApplication application,
        decimal basisAmount,
        decimal appliedAmount) => new()
    {
        DiscountApplicationId = application.Id,
        DiscountPolicyId = application.DiscountPolicyId,
        PolicyCode = application.DiscountPolicy.Code,
        DiscountType = application.DiscountType,
        TargetComponent = application.DiscountPolicy.TargetComponent,
        InvoiceItemId = application.InvoiceItemId,
        ValueType = application.DiscountPolicy.ValueType,
        PolicyValue = application.DiscountPolicy.Value,
        PolicyLimit = application.DiscountPolicy.Limit,
        BasisAmount = basisAmount,
        AppliedAmount = appliedAmount
    };

    private static IReadOnlyList<BillingCoverageComponent> BuildCoverageComponents(
        IReadOnlyList<BilInvoiceItem> activeItems,
        ItemTaxResult itemResult,
        AdministrationFeeCalculationResponse administrationFee)
    {
        var taxByItem = itemResult.Taxes.ToDictionary(x => x.InvoiceItemId);
        var resultByItem = itemResult.Items.ToDictionary(x => x.InvoiceItemId);
        var components = new List<BillingCoverageComponent>();

        foreach (var item in activeItems)
        {
            var itemCalculation = resultByItem[item.Id];
            var sourceReference = Guid.TryParse(item.SourceDetailId, out var parsed) ? parsed : (Guid?)null;
            var itemType = CoverageItemType(item);
            components.Add(new BillingCoverageComponent(
                item.Id, "ITEM", itemType, sourceReference, item.Quantity,
                itemCalculation.GrossAmount - itemCalculation.ItemDiscount,
                itemCalculation.Coverable));

            if (itemCalculation.TaxAmount > 0)
            {
                var tax = taxByItem[item.Id];
                var taxCoverable = tax.AllocationRule switch
                {
                    TaxRuleValues.Patient => false,
                    TaxRuleValues.Guarantor => true,
                    _ => itemCalculation.Coverable
                };
                components.Add(new BillingCoverageComponent(
                    tax.TaxRuleId, "TAX", itemType, sourceReference, item.Quantity,
                    itemCalculation.TaxAmount, taxCoverable));
            }
        }

        if (administrationFee.AppliedAmount > 0)
        {
            components.Add(new BillingCoverageComponent(
                administrationFee.PolicyId ?? Guid.Empty,
                "ADMINISTRATION_FEE",
                "ServiceCategory",
                null,
                1,
                administrationFee.AppliedAmount,
                administrationFee.Coverable));
        }

        return components;
    }

    private static CoverageCalculationResponse ApplyCoverageWaterfall(
        decimal eligibleAmount,
        IReadOnlyList<BillingCoverageComponent> components,
        BillingCoverageDecision decision)
    {
        var primaryAmount = Money(decision.PrimaryAmount);
        var excessAmount = Money(decision.ExcessAmount);
        var unresolvedAmount = Money(decision.UnresolvedAmount);
        if (primaryAmount < 0 || excessAmount < 0 || unresolvedAmount < 0)
            throw new BillingCalculationValidationException("Nilai coverage tidak boleh negatif.");

        var coverableAmount = components.Where(x => x.Coverable).Sum(x => x.Amount);
        if (primaryAmount + excessAmount + unresolvedAmount > coverableAmount)
            throw new BillingCalculationValidationException(
                "Total tanggungan penjamin melebihi biaya yang memenuhi syarat.");

        if (primaryAmount > eligibleAmount)
            throw new BillingCalculationValidationException(
                "Total tanggungan penjamin melebihi biaya yang memenuhi syarat.");
        var residualAfterPrimary = eligibleAmount - primaryAmount;
        if (excessAmount > residualAfterPrimary)
            throw new BillingCalculationValidationException(
                "Total tanggungan penjamin melebihi biaya yang memenuhi syarat.");
        var residualAfterExcess = residualAfterPrimary - excessAmount;
        if (unresolvedAmount > residualAfterExcess)
            throw new BillingCalculationValidationException(
                "Nilai coverage yang belum terselesaikan melebihi sisa tagihan.");
        if (decision.PrimaryStatus.Contains("REJECTED", StringComparison.OrdinalIgnoreCase)
            && unresolvedAmount == 0 && coverableAmount > 0)
            throw new BillingCalculationValidationException(
                "Coverage yang ditolak tidak boleh otomatis dipindahkan ke pasien tanpa policy kontrak.");

        return new CoverageCalculationResponse
        {
            ContractVersion = decision.ContractVersion,
            PrimaryStatus = decision.PrimaryStatus,
            ExcessStatus = decision.ExcessStatus,
            EligibleAmount = eligibleAmount,
            PrimaryAmount = primaryAmount,
            ResidualAfterPrimary = residualAfterPrimary,
            ExcessAmount = excessAmount,
            ResidualAfterExcess = residualAfterExcess,
            UnresolvedAmount = unresolvedAmount,
            PatientAmount = residualAfterExcess - unresolvedAmount,
            AppliedRuleIds = decision.AppliedRuleIds
        };
    }

    private static string CoverageItemType(BilInvoiceItem item)
    {
        if (item.Category.IsDrug || item.Category.IsPharmacy
            || item.SourceDomain.Contains("PHARM", StringComparison.OrdinalIgnoreCase))
            return "Drug";
        if (item.Category.IsProcedure || item.SourceDomain.Contains("PROCEDURE", StringComparison.OrdinalIgnoreCase))
            return "Procedure";
        return "ServiceCategory";
    }

    internal static CalculationBreakdownResponse DeserializeBreakdown(string? snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot)) return new CalculationBreakdownResponse();
        try
        {
            return JsonSerializer.Deserialize<CalculationBreakdownResponse>(snapshot, SnapshotJsonOptions)
                ?? new CalculationBreakdownResponse();
        }
        catch (JsonException)
        {
            return new CalculationBreakdownResponse();
        }
    }

    private static DateTimeOffset ToInstant(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return new DateTimeOffset(utc);
    }

    private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private async Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        await _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private sealed record ItemTaxResult(
        IReadOnlyList<CalculationItemResponse> Items,
        IReadOnlyList<TaxCalculationResponse> Taxes,
        IReadOnlyList<DiscountCalculationResponse> Discounts);

    private sealed record PatientPromoResult(
        decimal TotalAmount,
        IReadOnlyList<DiscountCalculationResponse> Discounts);
}

public sealed class BillingCalculationValidationException(string message) : Exception(message);

public sealed class BillingCalculationConflictException : Exception
{
    public BillingCalculationConflictException(string message) : base(message) { }
    public BillingCalculationConflictException(string message, Exception innerException) : base(message, innerException) { }
}
