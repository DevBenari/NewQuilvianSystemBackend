using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Mesin tanggungan penjamin perusahaan (BE-BKC-044, MPY-DES-006, CAP-34).
    /// Sejajar dan setempat dengan InsuranceCoverageService di ClinicalManagement.
    /// Bertanggung jawab menghitung porsi tanggungan perusahaan penjamin berdasarkan
    /// aturan MstCompanyGuarantorCoverageRule untuk invoice resmi billing (dipanggil
    /// RegistrationBillingCoverageAdapter) maupun advisory/preview klinis.
    /// </summary>
    public class CompanyGuarantorCoverageService
    {
        private readonly ApplicationDbContext _dbContext;

        public CompanyGuarantorCoverageService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menghitung keputusan coverage penjamin perusahaan untuk komponen tagihan invoice
        /// (RegistrationBillingCoverageAdapter, BIL-CALCULATION-0.9).
        /// </summary>
        public async Task<BillingCoverageDecision> ResolveCoverageAsync(
            BillingCoverageContext context,
            RegPatientEncounterGuarantor paymentSource,
            RegPatientEncounter encounter,
            CancellationToken cancellationToken)
        {
            if (!paymentSource.CompanyGuarantorId.HasValue)
            {
                throw new InvalidOperationException(
                    "CompanyGuarantorId wajib bernilai untuk perhitungan tanggungan perusahaan.");
            }

            var effectiveDate = context.CalculatedAt.UtcDateTime.Date;
            var companyGuarantorId = paymentSource.CompanyGuarantorId.Value;
            var benefitPlanCode = paymentSource.BenefitPlanCodeSnapshot;
            var patientClassId = encounter.PatientClassId;

            string? employeeGrade = null;
            if (paymentSource.PatientCompanyGuarantorId.HasValue)
            {
                employeeGrade = await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking()
                    .Where(x => x.Id == paymentSource.PatientCompanyGuarantorId.Value && !x.IsDelete)
                    .Select(x => x.GradeLevel)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var rules = await _dbContext.MstCompanyGuarantorCoverageRules.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.CompanyGuarantorId == companyGuarantorId
                    && (x.BenefitPlanCode == null || x.BenefitPlanCode == benefitPlanCode)
                    && (x.PatientClassId == null || x.PatientClassId == patientClassId)
                    && (x.EmployeeGrade == null || (employeeGrade != null && x.EmployeeGrade == employeeGrade))
                    && (x.EffectiveStartDate == null || x.EffectiveStartDate <= effectiveDate)
                    && (x.EffectiveEndDate == null || effectiveDate <= x.EffectiveEndDate))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.RuleCode)
                .ToListAsync(cancellationToken);

            return CalculateCoverageDecision(context, rules);
        }

        /// <summary>
        /// Menghitung keputusan coverage untuk kartu penjamin perusahaan kandidat tanpa menyentuh data kunjungan (BE-BKC-046, MPY-DES-005).
        /// </summary>
        public async Task<BillingCoverageDecision> ResolveCandidateCoverageAsync(
            BillingCoverageContext context,
            MstPatientCompanyGuarantor candidateCard,
            RegPatientEncounter encounter,
            CancellationToken cancellationToken)
        {
            var effectiveDate = context.CalculatedAt.UtcDateTime.Date;
            var companyGuarantorId = candidateCard.CompanyGuarantorId;
            var benefitPlanCode = candidateCard.BenefitPlanCode;
            var patientClassId = encounter.PatientClassId;
            var employeeGrade = candidateCard.GradeLevel;

            var rules = await _dbContext.MstCompanyGuarantorCoverageRules.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.CompanyGuarantorId == companyGuarantorId
                    && (x.BenefitPlanCode == null || x.BenefitPlanCode == benefitPlanCode)
                    && (x.PatientClassId == null || x.PatientClassId == patientClassId)
                    && (x.EmployeeGrade == null || (employeeGrade != null && x.EmployeeGrade == employeeGrade))
                    && (x.EffectiveStartDate == null || x.EffectiveStartDate <= effectiveDate)
                    && (x.EffectiveEndDate == null || effectiveDate <= x.EffectiveEndDate))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.RuleCode)
                .ToListAsync(cancellationToken);

            return CalculateCoverageDecision(context, rules);
        }

        private static BillingCoverageDecision CalculateCoverageDecision(
            BillingCoverageContext context,
            List<MstCompanyGuarantorCoverageRule> rules)
        {
            decimal primary = 0;
            decimal unresolved = 0;
            decimal nonBillableResidual = 0;
            var appliedRuleIds = new HashSet<Guid>();
            var appliedPerVisit = new Dictionary<Guid, decimal>();
            var outcomes = new List<BillingCoverageComponentOutcome>();

            foreach (var component in context.Components.Where(x => x.Coverable && x.Amount > 0))
            {
                var rule = rules.FirstOrDefault(x => Matches(x, component));
                if (rule is null)
                {
                    outcomes.Add(new BillingCoverageComponentOutcome(
                        component.ComponentId, component.ComponentType, 0, 0, 0, 0));
                    continue;
                }

                appliedRuleIds.Add(rule.Id);

                if (string.Equals(rule.CoverageStatus, "NotCovered", StringComparison.OrdinalIgnoreCase))
                {
                    var notCoveredNonBillable = !rule.IsAllowExcessPaymentByPatient ? component.Amount : 0;
                    if (notCoveredNonBillable > 0) nonBillableResidual += notCoveredNonBillable;
                    outcomes.Add(new BillingCoverageComponentOutcome(
                        component.ComponentId, component.ComponentType, 0, 0, 0, notCoveredNonBillable));
                    continue;
                }

                var covered = CalculateCoveredAmount(component, rule);
                if (rule.MaxAmountPerVisit.GetValueOrDefault() > 0)
                {
                    var used = appliedPerVisit.GetValueOrDefault(rule.Id);
                    covered = Math.Min(covered, Math.Max(0, rule.MaxAmountPerVisit!.Value - used));
                    appliedPerVisit[rule.Id] = used + covered;
                }

                primary += covered;
                var residual = component.Amount - covered;
                var residualNonBillable = !rule.IsAllowExcessPaymentByPatient ? residual : 0;
                if (residualNonBillable > 0) nonBillableResidual += residualNonBillable;
                outcomes.Add(new BillingCoverageComponentOutcome(
                    component.ComponentId, component.ComponentType, covered, 0, 0, residualNonBillable));
            }

            return new BillingCoverageDecision(
                RegistrationBillingCoverageAdapter.ContractVersion,
                primary > 0 ? "APPROVED" : unresolved > 0 ? "PENDING_OR_UNRESOLVED" : "NO_COVERAGE",
                "NOT_CONFIGURED",
                primary,
                0,
                unresolved,
                appliedRuleIds.Order().ToArray(),
                outcomes,
                0,
                nonBillableResidual,
                [],
                PayerKind: "COMPANY_GUARANTOR");
        }

        public static bool Matches(MstCompanyGuarantorCoverageRule rule, BillingCoverageComponent component)
        {
            return
                (string.Equals(rule.ItemType, "Tariff", StringComparison.OrdinalIgnoreCase)
                    && rule.TariffId.HasValue && rule.TariffId == component.TariffId)
                || (string.Equals(rule.ItemType, "Drug", StringComparison.OrdinalIgnoreCase)
                    && rule.DrugId.HasValue && rule.DrugId == component.DrugId)
                || (string.Equals(rule.ItemType, "DrugCategory", StringComparison.OrdinalIgnoreCase)
                    && rule.DrugCategoryId.HasValue && rule.DrugCategoryId == component.DrugCategoryId)
                || (string.Equals(rule.ItemType, "Procedure", StringComparison.OrdinalIgnoreCase)
                    && rule.ProcedureId.HasValue && rule.ProcedureId == component.ProcedureId)
                || (string.Equals(rule.ItemType, "ServiceCategory", StringComparison.OrdinalIgnoreCase)
                    && rule.TariffCategoryId.HasValue && rule.TariffCategoryId == component.TariffCategoryId);
        }

        public static decimal CalculateCoveredAmount(
            BillingCoverageComponent component,
            MstCompanyGuarantorCoverageRule rule)
        {
            var quantityFactor = 1m;
            if (rule.MaxQuantityPerVisit.GetValueOrDefault() > 0 && component.Quantity > 0)
                quantityFactor = Math.Min(1m, rule.MaxQuantityPerVisit!.Value / component.Quantity);

            var eligible = component.Amount * quantityFactor;
            var covered = eligible * Math.Clamp(rule.CoveragePercent, 0, 100) / 100m;
            if (rule.CoPaymentAmount.HasValue)
                covered -= rule.CoPaymentAmount.Value;
            if (rule.MaxCoverageAmount.GetValueOrDefault() > 0)
                covered = Math.Min(covered, rule.MaxCoverageAmount!.Value);

            return Math.Clamp(decimal.Round(covered, 2, MidpointRounding.AwayFromZero), 0, component.Amount);
        }

        // ============================================================
        // CLINICAL ADVISORY & ITEM RESOLUTION
        // ============================================================

        public async Task<CompanyGuarantorCoverageResult> ResolveTariffAsync(
            Guid encounterId,
            Guid tariffId,
            decimal quantity = 1,
            DateTime? serviceDate = null,
            CancellationToken cancellationToken = default,
            MstPatientCompanyGuarantor? explicitCandidateCard = null)
        {
            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Include(x => x.PaymentSource)
                    .ThenInclude(x => x!.CompanyGuarantor)
                .Include(x => x.PaymentSource)
                    .ThenInclude(x => x!.PatientCompanyGuarantor)
                .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete && x.IsActive, cancellationToken);

            if (encounter == null)
                return CompanyGuarantorCoverageResult.Fail("Encounter tidak ditemukan atau tidak aktif.");

            Guid companyGuarantorId;
            string? benefitPlanCode;
            string? employeeGrade;
            string companyGuarantorName;

            if (explicitCandidateCard != null)
            {
                if (explicitCandidateCard.PatientId != encounter.PatientId)
                    return CompanyGuarantorCoverageResult.Fail("Kartu penjamin perusahaan kandidat bukan milik pasien pada kunjungan ini.");

                companyGuarantorId = explicitCandidateCard.CompanyGuarantorId;
                benefitPlanCode = explicitCandidateCard.BenefitPlanCode;
                employeeGrade = explicitCandidateCard.GradeLevel;
                companyGuarantorName = explicitCandidateCard.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty;
            }
            else
            {
                var paymentSource = encounter.PaymentSource;
                if (paymentSource == null || paymentSource.IsDelete || !paymentSource.IsActive)
                    return CompanyGuarantorCoverageResult.Fail("Sumber pembayaran encounter tidak ditemukan atau tidak aktif.");

                if (paymentSource.PaymentType != EncounterPaymentType.CompanyGuarantor)
                    return CompanyGuarantorCoverageResult.Fail("Tipe pembayaran encounter bukan Penjamin Perusahaan.");

                if (!paymentSource.CompanyGuarantorId.HasValue)
                    return CompanyGuarantorCoverageResult.Fail("Perusahaan penjamin kunjungan ini belum dipilih.");

                companyGuarantorId = paymentSource.CompanyGuarantorId.Value;
                benefitPlanCode = paymentSource.BenefitPlanCodeSnapshot;
                employeeGrade = paymentSource.PatientCompanyGuarantor?.GradeLevel;
                companyGuarantorName = paymentSource.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty;
            }

            var tariff = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Include(x => x.Drug)
                    .ThenInclude(x => x!.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .FirstOrDefaultAsync(x => x.Id == tariffId && !x.IsDelete && x.IsActive, cancellationToken);

            if (tariff == null)
                return CompanyGuarantorCoverageResult.Fail("Tarif rumah sakit tidak ditemukan atau tidak aktif.");

            var effectiveDate = (serviceDate ?? DateTime.UtcNow).Date;
            var unitPrice = Math.Max(0, tariff.NormalPrice);
            quantity = quantity <= 0 ? 1 : quantity;
            var totalPrice = RoundMoney(unitPrice * quantity);

            var rules = await _dbContext.MstCompanyGuarantorCoverageRules.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.CompanyGuarantorId == companyGuarantorId
                    && (x.BenefitPlanCode == null || x.BenefitPlanCode == benefitPlanCode)
                    && (x.PatientClassId == null || x.PatientClassId == encounter.PatientClassId)
                    && (x.EmployeeGrade == null || (employeeGrade != null && x.EmployeeGrade == employeeGrade))
                    && (x.EffectiveStartDate == null || x.EffectiveStartDate <= effectiveDate)
                    && (x.EffectiveEndDate == null || effectiveDate <= x.EffectiveEndDate))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.RuleCode)
                .ToListAsync(cancellationToken);

            var component = new BillingCoverageComponent(
                tariff.Id,
                "ITEM",
                tariff.DrugId.HasValue ? "Drug" : tariff.ProcedureId.HasValue ? "Procedure" : "ServiceCategory",
                tariff.Id,
                tariff.ProcedureId,
                tariff.DrugId,
                tariff.Drug?.DrugCategoryId,
                tariff.TariffCategoryId,
                quantity,
                totalPrice,
                Coverable: true);

            var rule = rules.FirstOrDefault(x => Matches(x, component));
            if (rule == null)
            {
                return new CompanyGuarantorCoverageResult
                {
                    IsValid = true,
                    TariffId = tariff.Id,
                    TariffCode = tariff.TariffCode,
                    TariffName = tariff.TariffName,
                    CompanyGuarantorId = companyGuarantorId,
                    CompanyGuarantorName = companyGuarantorName,
                    PaymentType = EncounterPaymentType.CompanyGuarantor,
                    PaymentTypeName = "Penjamin Perusahaan",
                    PricingSource = "HospitalTariff",
                    IsCoverageApplicable = true,
                    IsCovered = false,
                    CoverageStatus = "NotCovered",
                    CoveragePercent = 0,
                    Quantity = quantity,
                    HospitalUnitPrice = unitPrice,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    CoveredAmount = 0,
                    PatientPayAmount = totalPrice,
                    IsAllowExcessPaymentByPatient = true,
                    BenefitPlanCode = benefitPlanCode,
                    EmployeeGrade = employeeGrade,
                    CoverageNote = "Item tidak memiliki aturan tanggungan yang cocok pada perusahaan penjamin ini."
                };
            }

            var coverageStatus = string.Equals(rule.CoverageStatus, "NotCovered", StringComparison.OrdinalIgnoreCase)
                ? "NotCovered"
                : "Covered";

            decimal coveredAmount = 0;
            if (coverageStatus != "NotCovered")
            {
                coveredAmount = CalculateCoveredAmount(component, rule);
                if (rule.MaxAmountPerVisit.GetValueOrDefault() > 0)
                    coveredAmount = Math.Min(coveredAmount, rule.MaxAmountPerVisit!.Value);
            }

            var patientPayAmount = Math.Max(0, RoundMoney(totalPrice - coveredAmount));

            return new CompanyGuarantorCoverageResult
            {
                IsValid = true,
                TariffId = tariff.Id,
                TariffCode = tariff.TariffCode,
                TariffName = tariff.TariffName,
                CompanyGuarantorId = companyGuarantorId,
                CompanyGuarantorName = companyGuarantorName,
                CompanyGuarantorCoverageRuleId = rule.Id,
                RuleCode = rule.RuleCode,
                PaymentType = EncounterPaymentType.CompanyGuarantor,
                PaymentTypeName = "Penjamin Perusahaan",
                PricingSource = "HospitalTariff",
                IsCoverageApplicable = true,
                IsCovered = coveredAmount > 0,
                CoverageStatus = coveredAmount > 0 ? (patientPayAmount > 0 ? "PartiallyCovered" : "Covered") : "NotCovered",
                CoveragePercent = rule.CoveragePercent,
                Quantity = quantity,
                HospitalUnitPrice = unitPrice,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                CoveredAmount = coveredAmount,
                PatientPayAmount = patientPayAmount,
                CoPaymentAmount = rule.CoPaymentAmount.GetValueOrDefault(),
                IsNeedApproval = rule.IsNeedApproval,
                IsNeedGuaranteeLetter = rule.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = rule.IsAllowExcessPaymentByPatient,
                BenefitPlanCode = benefitPlanCode,
                EmployeeGrade = employeeGrade,
                ApprovalInstruction = rule.ApprovalInstruction,
                BillingInstruction = rule.BillingInstruction,
                CoverageNote = rule.Description
            };
        }

        private static decimal RoundMoney(decimal value) =>
            Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public class CompanyGuarantorCoverageResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public Guid? TariffId { get; set; }
        public string? TariffCode { get; set; }
        public string? TariffName { get; set; }
        public Guid? CompanyGuarantorId { get; set; }
        public string? CompanyGuarantorName { get; set; }
        public Guid? CompanyGuarantorCoverageRuleId { get; set; }
        public string? RuleCode { get; set; }

        public EncounterPaymentType PaymentType { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public string PricingSource { get; set; } = string.Empty;

        public bool IsCoverageApplicable { get; set; }
        public bool IsCovered { get; set; }
        public string CoverageStatus { get; set; } = "Unknown";
        public decimal CoveragePercent { get; set; }

        public decimal Quantity { get; set; }
        public decimal HospitalUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public decimal CoPaymentAmount { get; set; }

        public bool IsNeedApproval { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }

        public string? BenefitPlanCode { get; set; }
        public string? EmployeeGrade { get; set; }
        public string? ApprovalInstruction { get; set; }
        public string? BillingInstruction { get; set; }
        public string? CoverageNote { get; set; }
        public List<string> Warnings { get; set; } = new();

        public static CompanyGuarantorCoverageResult Fail(string message) =>
            new() { IsValid = false, ErrorMessage = message, CoverageStatus = "Invalid" };
    }
}
