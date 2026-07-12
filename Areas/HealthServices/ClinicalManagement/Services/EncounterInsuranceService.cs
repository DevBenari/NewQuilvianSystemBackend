using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Membaca konteks pembayaran dan polis yang dipilih pada satu encounter.
    /// Service ini tidak menghitung coverage item. Tugasnya hanya menjawab:
    /// encounter ini tunai atau asuransi, provider apa, plan apa, dan apakah
    /// konteks asuransinya siap digunakan.
    /// </summary>
    public class EncounterInsuranceService
    {
        private readonly ApplicationDbContext _dbContext;

        public EncounterInsuranceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<EncounterInsuranceContext> GetContextAsync(
            Guid encounterId,
            DateTime? serviceDate = null,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
                return EncounterInsuranceContext.Fail(encounterId, "EncounterId wajib diisi.");

            var encounter = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Include(x => x.PatientClass)
                .Include(x => x.PaymentSource)
                    .ThenInclude(x => x!.InsuranceProvider)
                .Include(x => x.PaymentSource)
                    .ThenInclude(x => x!.PatientInsurance)
                .FirstOrDefaultAsync(
                    x => x.Id == encounterId &&
                         !x.IsDelete &&
                         x.IsActive,
                    cancellationToken);

            if (encounter == null)
                return EncounterInsuranceContext.Fail(encounterId, "Encounter tidak ditemukan atau tidak aktif.");

            var effectiveDate = (serviceDate ?? DateTime.UtcNow).Date;
            var paymentSource = encounter.PaymentSource;

            if (paymentSource == null || paymentSource.IsDelete || !paymentSource.IsActive)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Sumber pembayaran encounter tidak ditemukan atau tidak aktif.");
            }

            if (encounter.PaymentType == EncounterPaymentType.Cash)
            {
                return new EncounterInsuranceContext
                {
                    IsValid = true,
                    EncounterId = encounter.Id,
                    PatientId = encounter.PatientId,
                    ServiceUnitId = encounter.ServiceUnitId,
                    ClinicId = encounter.ClinicId,
                    PatientClassId = encounter.PatientClassId,
                    PatientClassName = encounter.PatientClass?.PatientClassName,
                    PaymentType = EncounterPaymentType.Cash,
                    PaymentTypeName = "Tunai",
                    HasInsurance = false,
                    PaymentSourceId = paymentSource.Id,
                    PaymentSourceName = paymentSource.PaymentSourceNameSnapshot,
                    ServiceDate = effectiveDate,
                    IsInsuranceReady = false
                };
            }

            if (encounter.PaymentType != EncounterPaymentType.Insurance)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Tipe pembayaran encounter tidak didukung.");
            }

            if (paymentSource.PaymentType != EncounterPaymentType.Insurance)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Tipe pembayaran pada encounter dan payment source tidak konsisten.");
            }

            if (!paymentSource.PatientInsuranceId.HasValue ||
                paymentSource.PatientInsuranceId.Value == Guid.Empty ||
                !paymentSource.InsuranceProviderId.HasValue ||
                paymentSource.InsuranceProviderId.Value == Guid.Empty)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Referensi patient insurance atau insurance provider belum lengkap.");
            }

            var provider = paymentSource.InsuranceProvider;
            var patientInsurance = paymentSource.PatientInsurance;

            if (provider == null || provider.IsDelete || !provider.IsActive)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Provider asuransi tidak ditemukan atau tidak aktif.");
            }

            if (patientInsurance == null ||
                patientInsurance.IsDelete ||
                !patientInsurance.IsActive ||
                patientInsurance.PatientId != encounter.PatientId ||
                patientInsurance.InsuranceProviderId != provider.Id)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Data asuransi pasien tidak ditemukan, tidak aktif, atau tidak sesuai encounter.");
            }

            var policyStart = paymentSource.EffectiveStartDateSnapshot ?? patientInsurance.EffectiveStartDate;
            var policyEnd = paymentSource.EffectiveEndDateSnapshot ?? patientInsurance.EffectiveEndDate;

            if (policyStart.HasValue && policyStart.Value.Date > effectiveDate)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Polis asuransi belum mulai berlaku pada tanggal pelayanan.");
            }

            if (policyEnd.HasValue && policyEnd.Value.Date < effectiveDate)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Polis asuransi sudah berakhir pada tanggal pelayanan.");
            }

            if (provider.ContractStartDate.HasValue && provider.ContractStartDate.Value.Date > effectiveDate)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Kontrak provider asuransi belum mulai berlaku pada tanggal pelayanan.");
            }

            if (provider.ContractEndDate.HasValue && provider.ContractEndDate.Value.Date < effectiveDate)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Kontrak provider asuransi sudah berakhir pada tanggal pelayanan.");
            }

            var eligible = paymentSource.IsEligible && patientInsurance.IsEligible;
            if (!eligible)
            {
                return EncounterInsuranceContext.Fail(
                    encounterId,
                    "Asuransi pasien belum eligible untuk digunakan.");
            }

            return new EncounterInsuranceContext
            {
                IsValid = true,
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                ServiceUnitId = encounter.ServiceUnitId,
                ClinicId = encounter.ClinicId,
                PatientClassId = encounter.PatientClassId,
                PatientClassName = encounter.PatientClass?.PatientClassName
                    ?? paymentSource.ClassNameSnapshot,
                PaymentType = EncounterPaymentType.Insurance,
                PaymentTypeName = "Asuransi",
                HasInsurance = true,
                PaymentSourceId = paymentSource.Id,
                PaymentSourceName = paymentSource.PaymentSourceNameSnapshot
                    ?? provider.InsuranceProviderName,
                PatientInsuranceId = patientInsurance.Id,
                InsuranceProviderId = provider.Id,
                InsuranceProviderName = provider.InsuranceProviderName,
                BenefitPlanCode = paymentSource.BenefitPlanCodeSnapshot
                    ?? patientInsurance.BenefitPlanCode,
                BenefitPlanName = paymentSource.PlanNameSnapshot
                    ?? patientInsurance.PlanName,
                PolicyNumber = paymentSource.PolicyNumberSnapshot
                    ?? patientInsurance.PolicyNumber,
                IsEligible = eligible,
                IsPolicyActive = true,
                IsInsuranceReady = true,
                IsUsingInsuranceTariffBook = provider.IsUsingInsuranceTariffBook,
                IsUsingHospitalTariff = provider.IsUsingHospitalTariff,
                IsNeedGuaranteeLetter = paymentSource.PatientInsurance?.IsNeedGuaranteeLetter
                    ?? provider.IsNeedGuaranteeLetter,
                IsNeedApprovalForDrug = provider.IsNeedApprovalForDrug,
                IsNeedApprovalForProcedure = provider.IsNeedApprovalForProcedure,
                IsAllowExcessPaymentByPatient = patientInsurance.IsAllowExcessPaymentByPatient &&
                    provider.IsAllowExcessPaymentByPatient,
                RemainingLimitAmount = patientInsurance.RemainingLimitAmount,
                PolicyCoPaymentPercent = patientInsurance.CoPaymentPercent,
                PolicyCoPaymentAmount = patientInsurance.CoPaymentAmount,
                ServiceDate = effectiveDate
            };
        }
    }

    public class EncounterInsuranceContext
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public Guid EncounterId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? PatientClassId { get; set; }
        public string? PatientClassName { get; set; }

        public EncounterPaymentType PaymentType { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public bool HasInsurance { get; set; }

        public Guid PaymentSourceId { get; set; }
        public string? PaymentSourceName { get; set; }

        public Guid? PatientInsuranceId { get; set; }
        public Guid? InsuranceProviderId { get; set; }
        public string? InsuranceProviderName { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public string? PolicyNumber { get; set; }

        public bool IsEligible { get; set; }
        public bool IsPolicyActive { get; set; }
        public bool IsInsuranceReady { get; set; }

        public bool IsUsingInsuranceTariffBook { get; set; }
        public bool IsUsingHospitalTariff { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsNeedApprovalForDrug { get; set; }
        public bool IsNeedApprovalForProcedure { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }

        public decimal? RemainingLimitAmount { get; set; }
        public decimal? PolicyCoPaymentPercent { get; set; }
        public decimal? PolicyCoPaymentAmount { get; set; }
        public DateTime ServiceDate { get; set; }

        public static EncounterInsuranceContext Fail(Guid encounterId, string message)
        {
            return new EncounterInsuranceContext
            {
                IsValid = false,
                EncounterId = encounterId,
                ErrorMessage = message
            };
        }
    }
}
