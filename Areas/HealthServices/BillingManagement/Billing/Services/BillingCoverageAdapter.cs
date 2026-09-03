using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public interface IBillingCoverageAdapter
{
    Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken);
}

public sealed record BillingCoverageContext(
    Guid InvoiceId,
    Guid EncounterId,
    DateTimeOffset CalculatedAt,
    decimal EligibleAmount,
    IReadOnlyList<BillingCoverageComponent> Components);

// TariffId/ProcedureId/DrugId/DrugCategoryId/TariffCategoryId: field terpisah per dimensi rujukan
// rule asuransi (MstInsuranceCoverageRule) - BUKAN satu SourceReferenceId gabungan seperti
// sebelumnya, karena satu item bisa dicocokkan rule di granularitas manapun (tarif spesifik,
// prosedur, kategori obat, atau kategori tarif) sekaligus, dan satu Guid tidak bisa mewakili
// keempatnya. Null untuk komponen yang bukan berasal dari item (ADMINISTRATION_FEE/ROOM_CHARGE/
// TAX non-item) - komponen itu memang tidak punya rujukan tarif/prosedur/obat.
public sealed record BillingCoverageComponent(
    Guid ComponentId,
    string ComponentType,
    string CoverageItemType,
    Guid? TariffId,
    Guid? ProcedureId,
    Guid? DrugId,
    Guid? DrugCategoryId,
    Guid? TariffCategoryId,
    decimal Quantity,
    decimal Amount,
    bool Coverable);

public sealed record BillingCoverageDecision(
    string ContractVersion,
    string PrimaryStatus,
    string ExcessStatus,
    decimal PrimaryAmount,
    decimal ExcessAmount,
    decimal UnresolvedAmount,
    IReadOnlyList<Guid> AppliedRuleIds);

public sealed class RegistrationBillingCoverageAdapter : IBillingCoverageAdapter
{
    public const string ContractVersion = "REGISTRATION-COVERAGE-ADAPTER-1";
    private readonly ApplicationDbContext _dbContext;

    public RegistrationBillingCoverageAdapter(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<BillingCoverageDecision> ResolveAsync(
        BillingCoverageContext context,
        CancellationToken cancellationToken)
    {
        var paymentSource = await _dbContext.TrxPatientEncounterGuarantors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EncounterId == context.EncounterId && x.IsActive && !x.IsDelete, cancellationToken);

        if (paymentSource is null || paymentSource.PaymentType == EncounterPaymentType.Cash)
            return SelfPay();

        var coverableAmount = context.Components.Where(x => x.Coverable).Sum(x => x.Amount);
        if (!paymentSource.IsEligible || !paymentSource.IsPolicyActive || !paymentSource.InsuranceProviderId.HasValue)
            return Unresolved(coverableAmount, "REJECTED");

        var encounter = await _dbContext.TrxPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.EncounterId && !x.IsDelete, cancellationToken);
        if (encounter is null)
            return Unresolved(coverableAmount, "UNRESOLVED");

        var effectiveDate = context.CalculatedAt.UtcDateTime.Date;
        var providerId = paymentSource.InsuranceProviderId.Value;
        var benefitPlanCode = paymentSource.BenefitPlanCodeSnapshot;
        var rules = await _dbContext.MstInsuranceCoverageRules.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.InsuranceProviderId == providerId
                && (x.BenefitPlanCode == null || x.BenefitPlanCode == benefitPlanCode)
                && (x.PatientClassId == null || x.PatientClassId == encounter.PatientClassId)
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= effectiveDate)
                && (x.EffectiveEndDate == null || effectiveDate <= x.EffectiveEndDate))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.RuleCode)
            .ToListAsync(cancellationToken);

        decimal primary = 0;
        decimal unresolved = 0;
        var appliedRuleIds = new HashSet<Guid>();
        var appliedPerVisit = new Dictionary<Guid, decimal>();

        foreach (var component in context.Components.Where(x => x.Coverable && x.Amount > 0))
        {
            var rule = rules.FirstOrDefault(x => Matches(x, component));
            if (rule is null)
            {
                unresolved += component.Amount;
                continue;
            }

            appliedRuleIds.Add(rule.Id);
            // BKC-DEC-062 (amendment BKC-DEC-042): IsNeedApproval/IsNeedGuaranteeLetter TIDAK LAGI
            // menggeser komponen ke unresolved. Sebelumnya, item dengan rule Covered tapi butuh
            // approval/surat jaminan ikut jatuh ke unresolved - padahal approval itu proses
            // administratif terpisah, bukan penolakan coverage. Subtotal Asuransi jadi salah kecil
            // di Menu Pembayaran untuk kasus yang sangat umum (banyak rule asuransi mewajibkan
            // approval/SJP tapi tetap Covered). Scope dipersempit, BUKAN pelepasan gating penuh:
            // CoverageStatus=="NeedApproval" (rule yang secara eksplisit BELUM diputuskan statusnya)
            // dan limit bulanan (butuh pemeriksaan pemakaian kumulatif yang belum tersedia di sini)
            // tetap menggeser ke unresolved seperti sebelumnya.
            if (string.Equals(rule.CoverageStatus, "NeedApproval", StringComparison.OrdinalIgnoreCase)
                || rule.MaxAmountPerMonth.HasValue || rule.MaxQuantityPerMonth.HasValue)
            {
                unresolved += component.Amount;
                continue;
            }

            if (string.Equals(rule.CoverageStatus, "NotCovered", StringComparison.OrdinalIgnoreCase))
            {
                if (!rule.IsAllowExcessPaymentByPatient) unresolved += component.Amount;
                continue;
            }

            var covered = CalculateCoveredAmount(component, rule);
            if (rule.MaxAmountPerVisit.HasValue)
            {
                var used = appliedPerVisit.GetValueOrDefault(rule.Id);
                covered = Math.Min(covered, Math.Max(0, rule.MaxAmountPerVisit.Value - used));
                appliedPerVisit[rule.Id] = used + covered;
            }

            primary += covered;
            var residual = component.Amount - covered;
            if (!rule.IsAllowExcessPaymentByPatient) unresolved += residual;
        }

        return new BillingCoverageDecision(
            ContractVersion,
            primary > 0 ? "APPROVED" : unresolved > 0 ? "PENDING_OR_UNRESOLVED" : "NO_COVERAGE",
            "NOT_CONFIGURED",
            primary,
            0,
            unresolved,
            appliedRuleIds.Order().ToArray());
    }

    private static bool Matches(MstInsuranceCoverageRule rule, BillingCoverageComponent component)
    {
        if (!string.Equals(rule.ItemType, component.CoverageItemType, StringComparison.OrdinalIgnoreCase))
            return false;

        var hasSpecificReference = rule.TariffId.HasValue || rule.DrugId.HasValue || rule.DrugCategoryId.HasValue
            || rule.ProcedureId.HasValue || rule.TariffCategoryId.HasValue;
        if (!hasSpecificReference) return true;

        // Setiap dimensi rule dibandingkan terhadap field component yang sepadan (bukan satu
        // reference gabungan ke lima field rule sekaligus) - satu item bisa match di granularitas
        // manapun yang diisi rule.
        if (rule.TariffId.HasValue && rule.TariffId == component.TariffId) return true;
        if (rule.ProcedureId.HasValue && rule.ProcedureId == component.ProcedureId) return true;
        if (rule.DrugId.HasValue && rule.DrugId == component.DrugId) return true;
        if (rule.DrugCategoryId.HasValue && rule.DrugCategoryId == component.DrugCategoryId) return true;
        if (rule.TariffCategoryId.HasValue && rule.TariffCategoryId == component.TariffCategoryId) return true;

        return false;
    }

    private static decimal CalculateCoveredAmount(BillingCoverageComponent component, MstInsuranceCoverageRule rule)
    {
        var quantityFactor = 1m;
        if (rule.MaxQuantityPerVisit.HasValue && component.Quantity > 0)
            quantityFactor = Math.Min(1m, rule.MaxQuantityPerVisit.Value / component.Quantity);

        var eligible = component.Amount * quantityFactor;
        var covered = eligible * Math.Clamp(rule.CoveragePercent, 0, 100) / 100m;
        if (rule.CoPaymentPercent.HasValue)
            covered -= eligible * Math.Clamp(rule.CoPaymentPercent.Value, 0, 100) / 100m;
        if (rule.CoPaymentAmount.HasValue)
            covered -= rule.CoPaymentAmount.Value;
        if (rule.MaxCoverageAmount.HasValue)
            covered = Math.Min(covered, rule.MaxCoverageAmount.Value);

        return Math.Clamp(decimal.Round(covered, 2, MidpointRounding.AwayFromZero), 0, component.Amount);
    }

    private static BillingCoverageDecision SelfPay() =>
        new(ContractVersion, "SELF_PAY", "NOT_APPLICABLE", 0, 0, 0, []);

    private static BillingCoverageDecision Unresolved(decimal amount, string primaryStatus) =>
        new(ContractVersion, primaryStatus, "NOT_CONFIGURED", 0, 0, amount, []);
}
