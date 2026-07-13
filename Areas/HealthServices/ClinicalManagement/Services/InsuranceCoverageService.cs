using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Menentukan tarif rumah sakit, tarif kontrak asuransi, status coverage,
    /// nominal yang ditanggung, nominal pasien, dan kebutuhan approval.
    /// Service ini menjadi satu-satunya tempat kalkulasi coverage untuk resep
    /// maupun tindakan agar frontend tidak menghitung sendiri.
    /// </summary>
    public class InsuranceCoverageService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterInsuranceService _encounterInsuranceService;

        public InsuranceCoverageService(
            ApplicationDbContext dbContext,
            EncounterInsuranceService encounterInsuranceService)
        {
            _dbContext = dbContext;
            _encounterInsuranceService = encounterInsuranceService;
        }

        public async Task<InsuranceCoverageResult> ResolveDrugAsync(
            Guid encounterId,
            Guid drugId,
            decimal quantity = 1,
            DateTime? serviceDate = null,
            CancellationToken cancellationToken = default)
        {
            var context = await _encounterInsuranceService.GetContextAsync(
                encounterId,
                serviceDate,
                cancellationToken);

            if (!context.IsValid)
                return InsuranceCoverageResult.Fail(context.ErrorMessage ?? "Konteks encounter tidak valid.");

            var tariffResolution = await FindDrugTariffAsync(
                drugId,
                context,
                cancellationToken);

            if (tariffResolution.Tariff == null)
            {
                return InsuranceCoverageResult.Fail(
                    "Tarif rumah sakit untuk obat belum dikonfigurasi atau tidak cocok dengan scope encounter.",
                    coverageStatus: "ConfigurationMissing");
            }

            return await ResolveTariffInternalAsync(
                tariffResolution.Tariff,
                context,
                quantity,
                cancellationToken,
                tariffResolution.IsFallback,
                tariffResolution.Warning);
        }

        public async Task<InsuranceCoverageResult> ResolveProcedureAsync(
            Guid encounterId,
            Guid procedureId,
            decimal quantity = 1,
            DateTime? serviceDate = null,
            CancellationToken cancellationToken = default)
        {
            var context = await _encounterInsuranceService.GetContextAsync(
                encounterId,
                serviceDate,
                cancellationToken);

            if (!context.IsValid)
                return InsuranceCoverageResult.Fail(context.ErrorMessage ?? "Konteks encounter tidak valid.");

            var tariff = await FindProcedureTariffAsync(
                procedureId,
                context,
                cancellationToken);

            if (tariff == null)
            {
                return InsuranceCoverageResult.Fail(
                    "Tarif rumah sakit untuk tindakan belum dikonfigurasi.",
                    coverageStatus: "ConfigurationMissing");
            }

            return await ResolveTariffInternalAsync(
                tariff,
                context,
                quantity,
                cancellationToken);
        }

        public async Task<InsuranceCoverageResult> ResolveTariffAsync(
            Guid encounterId,
            Guid tariffId,
            decimal quantity = 1,
            DateTime? serviceDate = null,
            CancellationToken cancellationToken = default)
        {
            var context = await _encounterInsuranceService.GetContextAsync(
                encounterId,
                serviceDate,
                cancellationToken);

            if (!context.IsValid)
                return InsuranceCoverageResult.Fail(context.ErrorMessage ?? "Konteks encounter tidak valid.");

            var tariff = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Include(x => x.Drug)
                    .ThenInclude(x => x!.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .FirstOrDefaultAsync(
                    x => x.Id == tariffId &&
                         !x.IsDelete &&
                         x.IsActive,
                    cancellationToken);

            if (tariff == null)
                return InsuranceCoverageResult.Fail("Tarif rumah sakit tidak ditemukan atau tidak aktif.");

            if (!IsEffective(tariff.EffectiveStartDate, tariff.EffectiveEndDate, context.ServiceDate))
                return InsuranceCoverageResult.Fail("Tarif rumah sakit belum atau tidak lagi berlaku.");

            return await ResolveTariffInternalAsync(
                tariff,
                context,
                quantity,
                cancellationToken);
        }

        private async Task<InsuranceCoverageResult> ResolveTariffInternalAsync(
            MstTariff tariff,
            EncounterInsuranceContext context,
            decimal quantity,
            CancellationToken cancellationToken,
            bool isFallbackTariff = false,
            string? pricingWarning = null)
        {
            quantity = quantity <= 0 ? 1 : quantity;
            var hospitalUnitPrice = Math.Max(0, tariff.NormalPrice);
            var hospitalTotalPrice = RoundMoney(hospitalUnitPrice * quantity);

            if (context.PaymentType == EncounterPaymentType.Cash || !context.HasInsurance)
            {
                return BuildCashResult(
                    tariff,
                    context,
                    quantity,
                    hospitalUnitPrice,
                    hospitalTotalPrice,
                    isFallbackTariff,
                    pricingWarning);
            }

            if (!context.IsInsuranceReady || !context.InsuranceProviderId.HasValue)
            {
                return InsuranceCoverageResult.Fail(
                    "Konteks asuransi encounter belum siap digunakan.");
            }

            var insuranceTariff = await FindInsuranceTariffAsync(
                tariff.Id,
                context,
                cancellationToken);

            // Positive list: tidak ada insurance tariff berarti tidak dicover.
            if (insuranceTariff == null)
            {
                return BuildNotCoveredResult(
                    tariff,
                    context,
                    quantity,
                    hospitalUnitPrice,
                    hospitalTotalPrice,
                    "Item tidak terdapat pada buku tarif asuransi.",
                    isFallbackTariff: isFallbackTariff,
                    pricingWarning: pricingWarning);
            }

            var contractUnitPrice = ResolveContractUnitPrice(
                insuranceTariff,
                hospitalUnitPrice);

            var totalPrice = RoundMoney(contractUnitPrice * quantity);

            var rule = await FindCoverageRuleAsync(
                tariff,
                context,
                cancellationToken);

            var coverageStatus = NormalizeCoverageStatus(rule?.CoverageStatus ?? "Covered");

            if (coverageStatus == "NotCovered")
            {
                return BuildNotCoveredResult(
                    tariff,
                    context,
                    quantity,
                    hospitalUnitPrice,
                    hospitalTotalPrice,
                    rule?.Description ?? "Item dikecualikan oleh aturan coverage.",
                    insuranceTariff,
                    rule,
                    isFallbackTariff,
                    pricingWarning);
            }

            var warnings = new List<string>();
            if (!string.IsNullOrWhiteSpace(pricingWarning))
                warnings.Add(pricingWarning);

            var coveragePercent = Math.Clamp(rule?.CoveragePercent ?? 100m, 0m, 100m);

            decimal coveredQuantity = quantity;
            if (rule?.MaxQuantityPerVisit.HasValue == true &&
                coveredQuantity > rule.MaxQuantityPerVisit.Value)
            {
                coveredQuantity = rule.MaxQuantityPerVisit.Value;
                warnings.Add("Jumlah yang melebihi batas per kunjungan menjadi tanggungan pasien.");
            }

            var eligibleAmount = RoundMoney(contractUnitPrice * coveredQuantity);
            var coveredAmount = RoundMoney(eligibleAmount * coveragePercent / 100m);

            if (rule?.MaxCoverageAmount.HasValue == true)
                coveredAmount = Math.Min(coveredAmount, rule.MaxCoverageAmount.Value);

            if (rule?.MaxAmountPerVisit.HasValue == true)
                coveredAmount = Math.Min(coveredAmount, rule.MaxAmountPerVisit.Value);

            if (context.RemainingLimitAmount.HasValue)
            {
                coveredAmount = Math.Min(coveredAmount, context.RemainingLimitAmount.Value);
                if (context.RemainingLimitAmount.Value < totalPrice)
                    warnings.Add("Coverage dibatasi oleh sisa limit polis pasien.");
            }

            var coPaymentPercent = rule?.CoPaymentPercent
                ?? context.PolicyCoPaymentPercent
                ?? 0m;
            var coPaymentAmount = rule?.CoPaymentAmount
                ?? context.PolicyCoPaymentAmount
                ?? 0m;

            coPaymentPercent = Math.Clamp(coPaymentPercent, 0m, 100m);
            var coPaymentFromPercent = RoundMoney(coveredAmount * coPaymentPercent / 100m);
            var totalCoPayment = RoundMoney(coPaymentFromPercent + Math.Max(0m, coPaymentAmount));

            coveredAmount = Math.Max(0m, RoundMoney(coveredAmount - totalCoPayment));
            var patientPayAmount = Math.Max(0m, RoundMoney(totalPrice - coveredAmount));

            var allowExcessPayment = rule?.IsAllowExcessPaymentByPatient
                ?? context.IsAllowExcessPaymentByPatient;

            if (!allowExcessPayment && patientPayAmount > 0)
                warnings.Add("Provider tidak mengizinkan excess dibayar pasien. Perlu konfirmasi petugas.");

            if (rule?.MaxQuantityPerMonth.HasValue == true ||
                rule?.MaxAmountPerMonth.HasValue == true)
            {
                warnings.Add("Rule mempunyai batas bulanan dan memerlukan pemeriksaan pemakaian kumulatif.");
            }

            var isNeedApproval =
                tariff.IsNeedApproval ||
                insuranceTariff.IsNeedApproval ||
                (tariff.Drug != null && context.IsNeedApprovalForDrug) ||
                (tariff.Procedure != null && context.IsNeedApprovalForProcedure) ||
                (rule?.IsNeedApproval ?? false) ||
                coverageStatus == "NeedApproval";

            var isNeedGuaranteeLetter =
                context.IsNeedGuaranteeLetter ||
                (rule?.IsNeedGuaranteeLetter ?? false);

            if (coverageStatus == "NeedApproval")
                coverageStatus = "NeedApproval";
            else if (coveredAmount <= 0)
                coverageStatus = "NotCovered";
            else if (patientPayAmount > 0 || coveragePercent < 100m)
                coverageStatus = "PartiallyCovered";
            else
                coverageStatus = "Covered";

            return new InsuranceCoverageResult
            {
                IsValid = true,
                TariffId = tariff.Id,
                TariffCode = tariff.TariffCode,
                TariffName = tariff.TariffName,
                InsuranceTariffId = insuranceTariff.Id,
                InsuranceCoverageRuleId = rule?.Id,
                PaymentType = context.PaymentType,
                PaymentTypeName = context.PaymentTypeName,
                PricingSource = "InsuranceTariff",
                IsCoverageApplicable = true,
                IsCovered = coveredAmount > 0,
                CoverageStatus = coverageStatus,
                CoveragePercent = coveragePercent,
                Quantity = quantity,
                HospitalUnitPrice = hospitalUnitPrice,
                ContractUnitPrice = contractUnitPrice,
                UnitPrice = contractUnitPrice,
                TotalPrice = totalPrice,
                CoveredAmount = coveredAmount,
                PatientPayAmount = patientPayAmount,
                CoPaymentAmount = totalCoPayment,
                IsNeedApproval = isNeedApproval,
                IsNeedGuaranteeLetter = isNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = allowExcessPayment,
                InsuranceProviderId = context.InsuranceProviderId,
                InsuranceProviderName = context.InsuranceProviderName,
                BenefitPlanCode = context.BenefitPlanCode,
                BenefitPlanName = context.BenefitPlanName,
                ApprovalInstruction = rule?.ApprovalInstruction,
                BillingInstruction = rule?.BillingInstruction
                    ?? insuranceTariff.BillingInstruction,
                CoverageNote = BuildCoverageNote(rule, warnings),
                IsFallbackTariff = isFallbackTariff,
                PricingWarning = pricingWarning,
                Warnings = warnings
            };
        }

        private async Task<TariffResolution> FindDrugTariffAsync(
            Guid drugId,
            EncounterInsuranceContext context,
            CancellationToken cancellationToken)
        {
            var date = context.ServiceDate;

            var allCandidates = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Include(x => x.Drug)
                    .ThenInclude(x => x!.DrugCategory)
                .Include(x => x.TariffCategory)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.DrugId == drugId &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date))
                .ToListAsync(cancellationToken);

            if (allCandidates.Count == 0)
                return TariffResolution.NotFound();

            var compatible = allCandidates
                .Where(x =>
                    (!x.ServiceUnitId.HasValue || x.ServiceUnitId == context.ServiceUnitId) &&
                    (!x.ClinicId.HasValue || x.ClinicId == context.ClinicId) &&
                    (!x.PatientClassId.HasValue || x.PatientClassId == context.PatientClassId))
                .ToList();

            var exact = SelectBestTariff(compatible);
            if (exact != null)
                return TariffResolution.Exact(exact);

            // Fallback global tetap aman untuk tunai maupun asuransi karena tidak
            // mengikat unit, klinik, atau kelas pasien tertentu.
            var global = SelectBestTariff(allCandidates.Where(x =>
                !x.ServiceUnitId.HasValue &&
                !x.ClinicId.HasValue &&
                !x.PatientClassId.HasValue));

            if (global != null)
            {
                return TariffResolution.Fallback(
                    global,
                    "Tarif global digunakan karena tidak ditemukan tarif dengan scope encounter yang lebih spesifik.");
            }

            // Untuk pasien tunai, data migrasi lama sering membentuk tarif per kelas
            // tetapi encounter lama tidak memiliki PatientClassId. Fallback hanya
            // diizinkan bila seluruh kandidat memiliki harga yang sama agar tidak
            // memilih harga kelas yang keliru.
            if (context.PaymentType == EncounterPaymentType.Cash)
            {
                var distinctPrices = allCandidates
                    .Select(x => RoundMoney(Math.Max(0m, x.NormalPrice)))
                    .Distinct()
                    .ToList();

                if (distinctPrices.Count == 1)
                {
                    var fallback = SelectBestTariff(allCandidates);
                    if (fallback != null)
                    {
                        return TariffResolution.Fallback(
                            fallback,
                            "Tarif fallback digunakan karena scope encounter belum lengkap dan seluruh tarif aktif obat memiliki harga yang sama.");
                    }
                }
            }

            return TariffResolution.NotFound();
        }

        private static MstTariff? SelectBestTariff(IEnumerable<MstTariff> candidates)
        {
            return candidates
                .OrderByDescending(x => x.ClinicId.HasValue)
                .ThenByDescending(x => x.ServiceUnitId.HasValue)
                .ThenByDescending(x => x.PatientClassId.HasValue)
                .ThenByDescending(x => x.EffectiveStartDate)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }

        private async Task<MstTariff?> FindProcedureTariffAsync(
            Guid procedureId,
            EncounterInsuranceContext context,
            CancellationToken cancellationToken)
        {
            var date = context.ServiceDate;

            var candidates = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ProcedureId == procedureId &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date) &&
                    (!x.ServiceUnitId.HasValue || x.ServiceUnitId == context.ServiceUnitId) &&
                    (!x.ClinicId.HasValue || x.ClinicId == context.ClinicId) &&
                    (!x.PatientClassId.HasValue || x.PatientClassId == context.PatientClassId))
                .ToListAsync(cancellationToken);

            return candidates
                .OrderByDescending(x => x.ClinicId.HasValue)
                .ThenByDescending(x => x.ServiceUnitId.HasValue)
                .ThenByDescending(x => x.PatientClassId.HasValue)
                .ThenByDescending(x => x.EffectiveStartDate)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }

        private async Task<MstInsuranceTariff?> FindInsuranceTariffAsync(
            Guid tariffId,
            EncounterInsuranceContext context,
            CancellationToken cancellationToken)
        {
            if (!context.InsuranceProviderId.HasValue)
                return null;

            var date = context.ServiceDate;
            var planCode = NormalizeNullable(context.BenefitPlanCode);

            var candidates = await _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.InsuranceProviderId == context.InsuranceProviderId.Value &&
                    x.TariffId == tariffId &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date) &&
                    (!x.PatientClassId.HasValue || x.PatientClassId == context.PatientClassId) &&
                    (x.BenefitPlanCode == null || x.BenefitPlanCode == "" ||
                     (planCode != null && x.BenefitPlanCode.ToUpper() == planCode)))
                .ToListAsync(cancellationToken);

            return candidates
                .OrderByDescending(x => x.PatientClassId.HasValue)
                .ThenByDescending(x => !string.IsNullOrWhiteSpace(x.BenefitPlanCode))
                .ThenByDescending(x => x.EffectiveStartDate)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }

        private async Task<MstInsuranceCoverageRule?> FindCoverageRuleAsync(
            MstTariff tariff,
            EncounterInsuranceContext context,
            CancellationToken cancellationToken)
        {
            if (!context.InsuranceProviderId.HasValue)
                return null;

            var date = context.ServiceDate;
            var planCode = NormalizeNullable(context.BenefitPlanCode);
            var drugId = tariff.DrugId;
            var drugCategoryId = tariff.Drug?.DrugCategoryId;
            var procedureId = tariff.ProcedureId;
            var tariffCategoryId = tariff.TariffCategoryId;

            var candidates = await _dbContext.Set<MstInsuranceCoverageRule>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.InsuranceProviderId == context.InsuranceProviderId.Value &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date) &&
                    (x.BenefitPlanCode == null || x.BenefitPlanCode == "" ||
                     (planCode != null && x.BenefitPlanCode.ToUpper() == planCode)) &&
                    (
                        (x.ItemType == "Tariff" && x.TariffId == tariff.Id) ||
                        (x.ItemType == "Drug" && drugId.HasValue && x.DrugId == drugId) ||
                        (x.ItemType == "DrugCategory" && drugCategoryId.HasValue && x.DrugCategoryId == drugCategoryId) ||
                        (x.ItemType == "Procedure" && procedureId.HasValue && x.ProcedureId == procedureId) ||
                        (x.ItemType == "ServiceCategory" && x.TariffCategoryId == tariffCategoryId)
                    ))
                .ToListAsync(cancellationToken);

            return candidates
                .OrderByDescending(x => GetRuleSpecificity(x.ItemType))
                .ThenByDescending(x => !string.IsNullOrWhiteSpace(x.BenefitPlanCode))
                .ThenByDescending(x => x.EffectiveStartDate)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }

        private static InsuranceCoverageResult BuildCashResult(
            MstTariff tariff,
            EncounterInsuranceContext context,
            decimal quantity,
            decimal unitPrice,
            decimal totalPrice,
            bool isFallbackTariff = false,
            string? pricingWarning = null)
        {
            return new InsuranceCoverageResult
            {
                IsValid = true,
                TariffId = tariff.Id,
                TariffCode = tariff.TariffCode,
                TariffName = tariff.TariffName,
                PaymentType = context.PaymentType,
                PaymentTypeName = context.PaymentTypeName,
                PricingSource = "HospitalTariff",
                IsCoverageApplicable = false,
                IsCovered = false,
                CoverageStatus = "NotApplicable",
                CoveragePercent = 0,
                Quantity = quantity,
                HospitalUnitPrice = unitPrice,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                CoveredAmount = 0,
                PatientPayAmount = totalPrice,
                IsNeedApproval = tariff.IsNeedApproval,
                IsNeedGuaranteeLetter = false,
                IsAllowExcessPaymentByPatient = true,
                IsFallbackTariff = isFallbackTariff,
                PricingWarning = pricingWarning,
                CoverageNote = pricingWarning,
                Warnings = string.IsNullOrWhiteSpace(pricingWarning)
                    ? new List<string>()
                    : new List<string> { pricingWarning }
            };
        }

        private static InsuranceCoverageResult BuildNotCoveredResult(
            MstTariff tariff,
            EncounterInsuranceContext context,
            decimal quantity,
            decimal hospitalUnitPrice,
            decimal hospitalTotalPrice,
            string note,
            MstInsuranceTariff? insuranceTariff = null,
            MstInsuranceCoverageRule? rule = null,
            bool isFallbackTariff = false,
            string? pricingWarning = null)
        {
            return new InsuranceCoverageResult
            {
                IsValid = true,
                TariffId = tariff.Id,
                TariffCode = tariff.TariffCode,
                TariffName = tariff.TariffName,
                InsuranceTariffId = insuranceTariff?.Id,
                InsuranceCoverageRuleId = rule?.Id,
                PaymentType = context.PaymentType,
                PaymentTypeName = context.PaymentTypeName,
                PricingSource = "HospitalTariff",
                IsCoverageApplicable = true,
                IsCovered = false,
                CoverageStatus = "NotCovered",
                CoveragePercent = 0,
                Quantity = quantity,
                HospitalUnitPrice = hospitalUnitPrice,
                ContractUnitPrice = insuranceTariff?.ContractPrice,
                UnitPrice = hospitalUnitPrice,
                TotalPrice = hospitalTotalPrice,
                CoveredAmount = 0,
                PatientPayAmount = hospitalTotalPrice,
                IsNeedApproval = rule?.IsNeedApproval ?? false,
                IsNeedGuaranteeLetter = rule?.IsNeedGuaranteeLetter
                    ?? context.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = rule?.IsAllowExcessPaymentByPatient
                    ?? context.IsAllowExcessPaymentByPatient,
                InsuranceProviderId = context.InsuranceProviderId,
                InsuranceProviderName = context.InsuranceProviderName,
                BenefitPlanCode = context.BenefitPlanCode,
                BenefitPlanName = context.BenefitPlanName,
                CoverageNote = string.IsNullOrWhiteSpace(pricingWarning)
                    ? note
                    : $"{note} {pricingWarning}",
                ApprovalInstruction = rule?.ApprovalInstruction,
                BillingInstruction = rule?.BillingInstruction,
                IsFallbackTariff = isFallbackTariff,
                PricingWarning = pricingWarning,
                Warnings = string.IsNullOrWhiteSpace(pricingWarning)
                    ? new List<string>()
                    : new List<string> { pricingWarning }
            };
        }

        private static decimal ResolveContractUnitPrice(
            MstInsuranceTariff insuranceTariff,
            decimal hospitalUnitPrice)
        {
            if (insuranceTariff.IsUsingContractPrice)
                return Math.Max(0m, insuranceTariff.ContractPrice);

            if (insuranceTariff.DiscountPercent.HasValue)
            {
                var percent = Math.Clamp(insuranceTariff.DiscountPercent.Value, 0m, 100m);
                return Math.Max(0m, RoundMoney(hospitalUnitPrice * (100m - percent) / 100m));
            }

            if (insuranceTariff.DiscountAmount.HasValue)
                return Math.Max(0m, RoundMoney(hospitalUnitPrice - insuranceTariff.DiscountAmount.Value));

            return Math.Max(0m, insuranceTariff.ContractPrice);
        }

        private static int GetRuleSpecificity(string? itemType)
        {
            return itemType?.Trim().ToLowerInvariant() switch
            {
                "tariff" => 500,
                "drug" => 400,
                "procedure" => 400,
                "drugcategory" => 300,
                "servicecategory" => 200,
                _ => 0
            };
        }

        private static bool IsEffective(
            DateTime? startDate,
            DateTime? endDate,
            DateTime serviceDate)
        {
            return (!startDate.HasValue || startDate.Value.Date <= serviceDate.Date) &&
                   (!endDate.HasValue || endDate.Value.Date >= serviceDate.Date);
        }

        private static string NormalizeCoverageStatus(string? value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "covered" => "Covered",
                "partialcovered" => "PartiallyCovered",
                "partiallycovered" => "PartiallyCovered",
                "notcovered" => "NotCovered",
                "needapproval" => "NeedApproval",
                _ => "Covered"
            };
        }

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static string? BuildCoverageNote(
            MstInsuranceCoverageRule? rule,
            IReadOnlyCollection<string> warnings)
        {
            var values = new List<string>();

            if (!string.IsNullOrWhiteSpace(rule?.Description))
                values.Add(rule.Description.Trim());

            values.AddRange(warnings.Where(x => !string.IsNullOrWhiteSpace(x)));

            return values.Count == 0
                ? null
                : string.Join(" ", values.Distinct());
        }
        private sealed class TariffResolution
        {
            public MstTariff? Tariff { get; private set; }
            public bool IsFallback { get; private set; }
            public string? Warning { get; private set; }

            public static TariffResolution Exact(MstTariff tariff)
            {
                return new TariffResolution
                {
                    Tariff = tariff,
                    IsFallback = false
                };
            }

            public static TariffResolution Fallback(MstTariff tariff, string warning)
            {
                return new TariffResolution
                {
                    Tariff = tariff,
                    IsFallback = true,
                    Warning = warning
                };
            }

            public static TariffResolution NotFound()
            {
                return new TariffResolution();
            }
        }
    }

    public class InsuranceCoverageResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public Guid? TariffId { get; set; }
        public string? TariffCode { get; set; }
        public string? TariffName { get; set; }
        public Guid? InsuranceTariffId { get; set; }
        public Guid? InsuranceCoverageRuleId { get; set; }

        public EncounterPaymentType PaymentType { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public string PricingSource { get; set; } = string.Empty;
        public bool IsFallbackTariff { get; set; }
        public string? PricingWarning { get; set; }

        public bool IsCoverageApplicable { get; set; }
        public bool IsCovered { get; set; }
        public string CoverageStatus { get; set; } = "Unknown";
        public decimal CoveragePercent { get; set; }

        public decimal Quantity { get; set; }
        public decimal HospitalUnitPrice { get; set; }
        public decimal? ContractUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public decimal CoPaymentAmount { get; set; }

        public bool IsNeedApproval { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }

        public Guid? InsuranceProviderId { get; set; }
        public string? InsuranceProviderName { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }

        public string? ApprovalInstruction { get; set; }
        public string? BillingInstruction { get; set; }
        public string? CoverageNote { get; set; }
        public List<string> Warnings { get; set; } = new();

        public static InsuranceCoverageResult Fail(
            string message,
            string coverageStatus = "Invalid")
        {
            return new InsuranceCoverageResult
            {
                IsValid = false,
                ErrorMessage = message,
                CoverageStatus = coverageStatus
            };
        }
    }
}
