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

public sealed record BillingCoverageComponent(
    Guid ComponentId,
    string ComponentType,
    string CoverageItemType,
    Guid? SourceReferenceId,
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
            if (rule.IsNeedApproval || rule.IsNeedGuaranteeLetter
                || string.Equals(rule.CoverageStatus, "NeedApproval", StringComparison.OrdinalIgnoreCase)
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
        if (!component.SourceReferenceId.HasValue) return false;

        var reference = component.SourceReferenceId.Value;
        return rule.TariffId == reference || rule.DrugId == reference || rule.DrugCategoryId == reference
            || rule.ProcedureId == reference || rule.TariffCategoryId == reference;
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
