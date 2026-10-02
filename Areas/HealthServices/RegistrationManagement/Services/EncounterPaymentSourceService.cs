using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Pelaksana kontrak MPY-ENC-PAYER-001 (BE-BKC-045, BE-BKC-047, CAP-33).
    /// Layanan tunggal di RegistrationManagement yang berwenang memperbarui sumber pembayaran
    /// kunjungan (RegPatientEncounterGuarantor) secara in-place di dalam transaksi atomik pemanggil
    /// (billing-kasir), tanpa melanggar filtered unique index maupun menambah baris baru.
    /// </summary>
    public class EncounterPaymentSourceService
    {
        private readonly ApplicationDbContext _dbContext;

        public EncounterPaymentSourceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Mengganti sumber pembayaran kunjungan secara in-place (MPY-ENC-PAYER-001 § 3.2).
        /// MUST NOT membuka atau menutup transaksi sendiri — mengikuti transaksi pemanggil.
        /// </summary>
        public async Task<EncounterPaymentSourceSwitchResult> SwitchPaymentSourceAsync(
            EncounterPaymentSourceSwitchCommand command,
            CancellationToken cancellationToken = default)
        {
            var encounter = await _dbContext.RegPatientEncounters
                .Include(x => x.PaymentSource)
                .FirstOrDefaultAsync(x => x.Id == command.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken);

            if (encounter is null)
            {
                return new EncounterPaymentSourceSwitchResult(
                    Success: false,
                    ErrorCode: "ENCOUNTER_NOT_FOUND",
                    ErrorMessage: "Data kunjungan tidak ditemukan.",
                    StatusCode: 404,
                    OldPaymentType: null,
                    OldPayerName: null,
                    OldCardNumber: null,
                    NewPaymentType: null,
                    NewPayerName: null,
                    NewCardNumber: null);
            }

            var paymentSource = encounter.PaymentSource
                ?? await _dbContext.RegPatientEncounterGuarantors
                    .FirstOrDefaultAsync(x => x.EncounterId == command.EncounterId && x.IsActive && !x.IsDelete, cancellationToken);

            // BIL-VAL-074: Kunjungan tidak memiliki baris sumber pembayaran sama sekali
            if (paymentSource is null)
            {
                return new EncounterPaymentSourceSwitchResult(
                    Success: false,
                    ErrorCode: "PAYMENT_SOURCE_MISSING",
                    ErrorMessage: "Data penjamin kunjungan ini belum lengkap. Hubungi Registrasi sebelum mengubah tagihan.",
                    StatusCode: 422,
                    OldPaymentType: null,
                    OldPayerName: null,
                    OldCardNumber: null,
                    NewPaymentType: null,
                    NewPayerName: null,
                    NewCardNumber: null);
            }

            // BIL-VAL-064: paymentType bernilai di luar CASH/INSURANCE/COMPANY_GUARANTOR
            if (!Enum.IsDefined(typeof(EncounterPaymentType), command.PaymentType))
            {
                return new EncounterPaymentSourceSwitchResult(
                    Success: false,
                    ErrorCode: "INVALID_PAYMENT_TYPE",
                    ErrorMessage: "Jenis pembayaran tidak dikenali.",
                    StatusCode: 400,
                    OldPaymentType: null,
                    OldPayerName: null,
                    OldCardNumber: null,
                    NewPaymentType: null,
                    NewPayerName: null,
                    NewCardNumber: null);
            }

            // BIL-VAL-065: paymentType = CASH tetapi ada kartu asuransi atau kartu perusahaan yang dikirim
            if (command.PaymentType == EncounterPaymentType.Cash)
            {
                if (command.PatientInsuranceId.HasValue || command.PatientCompanyGuarantorId.HasValue)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CASH_WITH_CARD",
                        ErrorMessage: "Pembayaran tunai tidak boleh disertai kartu penjamin.",
                        StatusCode: 400,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }
            }
            // BIL-VAL-066: paymentType = INSURANCE tetapi kartu asuransi tidak dikirim, atau justru kartu perusahaan yang dikirim
            else if (command.PaymentType == EncounterPaymentType.Insurance)
            {
                if (!command.PatientInsuranceId.HasValue || command.PatientCompanyGuarantorId.HasValue)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "INSURANCE_CARD_REQUIRED",
                        ErrorMessage: "Pilih kartu asuransi yang akan dipakai.",
                        StatusCode: 400,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }
            }
            // BIL-VAL-067: paymentType = COMPANY_GUARANTOR tetapi kartu penjamin perusahaan tidak dikirim, atau justru kartu asuransi yang dikirim
            else if (command.PaymentType == EncounterPaymentType.CompanyGuarantor)
            {
                if (!command.PatientCompanyGuarantorId.HasValue || command.PatientInsuranceId.HasValue)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "COMPANY_GUARANTOR_CARD_REQUIRED",
                        ErrorMessage: "Pilih kartu penjamin perusahaan yang akan dipakai.",
                        StatusCode: 400,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }
            }

            // BIL-VAL-073: Payer kandidat sama persis dengan payer yang sedang berlaku
            var isSamePayer = paymentSource.PaymentType == command.PaymentType
                && (command.PaymentType switch
                {
                    EncounterPaymentType.Cash => paymentSource.PaymentMethodId == command.PaymentMethodId,
                    EncounterPaymentType.Insurance => paymentSource.PatientInsuranceId == command.PatientInsuranceId,
                    EncounterPaymentType.CompanyGuarantor => paymentSource.PatientCompanyGuarantorId == command.PatientCompanyGuarantorId,
                    _ => false
                });

            if (isSamePayer)
            {
                return new EncounterPaymentSourceSwitchResult(
                    Success: false,
                    ErrorCode: "SAME_PAYER",
                    ErrorMessage: "Penjamin yang dipilih sama dengan yang sedang dipakai. Tidak ada yang perlu diubah.",
                    StatusCode: 422,
                    OldPaymentType: null,
                    OldPayerName: null,
                    OldCardNumber: null,
                    NewPaymentType: null,
                    NewPayerName: null,
                    NewCardNumber: null);
            }

            var serviceDate = command.ServiceDate.Date;
            var oldPaymentType = paymentSource.PaymentType.ToString();
            var oldPayerName = paymentSource.PaymentSourceNameSnapshot ?? "Tunai";
            var oldCardNumber = paymentSource.CardNumberSnapshot
                ?? paymentSource.PolicyNumberSnapshot
                ?? paymentSource.EmployeeNumberSnapshot;

            string newPaymentType = command.PaymentType.ToString();
            string newPayerName = "Tunai";
            string? newCardNumber = null;

            if (command.PaymentType == EncounterPaymentType.Cash)
            {
                // Bersihkan kolom asuransi dan penjamin perusahaan
                ClearInsuranceSnapshots(paymentSource);
                ClearCompanyGuarantorSnapshots(paymentSource);

                paymentSource.PaymentType = EncounterPaymentType.Cash;
                paymentSource.PaymentMethodId = command.PaymentMethodId;
                paymentSource.PaymentSourceNameSnapshot = "Tunai";
                paymentSource.IsEligible = true;
                paymentSource.IsPolicyActive = false;
                paymentSource.UpdateDateTime = DateTime.UtcNow;
                paymentSource.UpdateBy = command.ActorUserId;

                encounter.PaymentType = EncounterPaymentType.Cash;
                encounter.PaymentMethodId = command.PaymentMethodId;
                encounter.UpdateDateTime = DateTime.UtcNow;
                encounter.UpdateBy = command.ActorUserId;
            }
            else if (command.PaymentType == EncounterPaymentType.Insurance)
            {
                var card = await _dbContext.Set<MstPatientInsurance>()
                    .Include(x => x.InsuranceProvider)
                    .FirstOrDefaultAsync(x => x.Id == command.PatientInsuranceId!.Value, cancellationToken);

                // BIL-VAL-069: Kartu yang dipilih sudah tidak aktif atau sudah ditandai terhapus
                if (card is null || card.IsDelete || !card.IsActive)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_INACTIVE",
                        ErrorMessage: "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // BIL-VAL-068: Kartu yang dipilih bukan milik pasien pada kunjungan ini
                if (card.PatientId != encounter.PatientId)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_WRONG_PATIENT",
                        ErrorMessage: "Kartu penjamin yang dipilih bukan milik pasien ini.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // BIL-VAL-070: Tanggal layanan berada di luar masa berlaku kartu
                if ((card.EffectiveStartDate.HasValue && card.EffectiveStartDate.Value.Date > serviceDate)
                    || (card.EffectiveEndDate.HasValue && card.EffectiveEndDate.Value.Date < serviceDate))
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_EXPIRED",
                        ErrorMessage: "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // BIL-VAL-071: Kartu belum dinyatakan layak dipakai
                if (!card.IsEligible)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_NOT_ELIGIBLE",
                        ErrorMessage: "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                var provider = card.InsuranceProvider;
                // BIL-VAL-072: Perusahaan asuransi pada kartu sudah tidak aktif atau kontraknya sudah berakhir
                if (provider is null || provider.IsDelete || !provider.IsActive
                    || (provider.ContractStartDate.HasValue && provider.ContractStartDate.Value.Date > serviceDate)
                    || (provider.ContractEndDate.HasValue && provider.ContractEndDate.Value.Date < serviceDate))
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "PROVIDER_INACTIVE",
                        ErrorMessage: "Kerja sama dengan perusahaan asuransi ini sudah berakhir pada tanggal pelayanan.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // Bersihkan kolom tunai dan penjamin perusahaan
                ClearCompanyGuarantorSnapshots(paymentSource);
                paymentSource.PaymentMethodId = null;

                // Bangun snapshot baru kartu asuransi
                paymentSource.PaymentType = EncounterPaymentType.Insurance;
                paymentSource.PatientInsuranceId = card.Id;
                paymentSource.InsuranceProviderId = card.InsuranceProviderId;
                paymentSource.PaymentSourceNameSnapshot = provider.InsuranceProviderName;
                paymentSource.PolicyNumberSnapshot = card.PolicyNumber;
                paymentSource.CardNumberSnapshot = card.CardNumber;
                paymentSource.MemberNumberSnapshot = card.MemberNumber;
                paymentSource.PlanNameSnapshot = card.PlanName;
                paymentSource.ClassNameSnapshot = card.ClassName;
                paymentSource.BenefitPlanCodeSnapshot = card.BenefitPlanCode;
                paymentSource.EffectiveStartDateSnapshot = card.EffectiveStartDate;
                paymentSource.EffectiveEndDateSnapshot = card.EffectiveEndDate;
                paymentSource.IsEligible = card.IsEligible;
                paymentSource.IsPolicyActive = true;
                paymentSource.UpdateDateTime = DateTime.UtcNow;
                paymentSource.UpdateBy = command.ActorUserId;

                encounter.PaymentType = EncounterPaymentType.Insurance;
                encounter.PaymentMethodId = null;
                encounter.UpdateDateTime = DateTime.UtcNow;
                encounter.UpdateBy = command.ActorUserId;

                newPayerName = provider.InsuranceProviderName;
                newCardNumber = card.CardNumber ?? card.PolicyNumber;
            }
            else if (command.PaymentType == EncounterPaymentType.CompanyGuarantor)
            {
                var card = await _dbContext.Set<MstPatientCompanyGuarantor>()
                    .Include(x => x.CompanyGuarantor)
                    .FirstOrDefaultAsync(x => x.Id == command.PatientCompanyGuarantorId!.Value, cancellationToken);

                // BIL-VAL-069: Kartu yang dipilih sudah tidak aktif atau sudah ditandai terhapus
                if (card is null || card.IsDelete || !card.IsActive)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_INACTIVE",
                        ErrorMessage: "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // BIL-VAL-068: Kartu yang dipilih bukan milik pasien pada kunjungan ini
                if (card.PatientId != encounter.PatientId)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_WRONG_PATIENT",
                        ErrorMessage: "Kartu penjamin yang dipilih bukan milik pasien ini.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // BIL-VAL-070: Tanggal layanan berada di luar masa berlaku kartu
                if ((card.EffectiveStartDate.HasValue && card.EffectiveStartDate.Value.Date > serviceDate)
                    || (card.EffectiveEndDate.HasValue && card.EffectiveEndDate.Value.Date < serviceDate))
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_EXPIRED",
                        ErrorMessage: "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // BIL-VAL-071: Kartu belum dinyatakan layak dipakai
                if (!card.IsEligible)
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "CARD_NOT_ELIGIBLE",
                        ErrorMessage: "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                var guarantor = card.CompanyGuarantor;
                // BIL-VAL-072: Perusahaan penjamin pada kartu sudah tidak aktif atau kontraknya sudah berakhir
                if (guarantor is null || guarantor.IsDelete || !guarantor.IsActive
                    || (guarantor.ContractStartDate.HasValue && guarantor.ContractStartDate.Value.Date > serviceDate)
                    || (guarantor.ContractEndDate.HasValue && guarantor.ContractEndDate.Value.Date < serviceDate))
                {
                    return new EncounterPaymentSourceSwitchResult(
                        Success: false,
                        ErrorCode: "GUARANTOR_INACTIVE",
                        ErrorMessage: "Kerja sama dengan perusahaan penjamin ini sudah berakhir pada tanggal pelayanan.",
                        StatusCode: 422,
                        OldPaymentType: null,
                        OldPayerName: null,
                        OldCardNumber: null,
                        NewPaymentType: null,
                        NewPayerName: null,
                        NewCardNumber: null);
                }

                // Bersihkan kolom tunai dan asuransi
                ClearInsuranceSnapshots(paymentSource);
                paymentSource.PaymentMethodId = null;

                // Bangun snapshot baru penjamin perusahaan
                paymentSource.PaymentType = EncounterPaymentType.CompanyGuarantor;
                paymentSource.PatientCompanyGuarantorId = card.Id;
                paymentSource.CompanyGuarantorId = card.CompanyGuarantorId;
                paymentSource.PaymentSourceNameSnapshot = guarantor.CompanyGuarantorName;
                paymentSource.CompanyGuarantorCodeSnapshot = guarantor.CompanyGuarantorCode;
                paymentSource.EmployeeNumberSnapshot = card.EmployeeNumber;
                paymentSource.EmployeeNameSnapshot = card.EmployeeName;
                paymentSource.BenefitPlanCodeSnapshot = card.BenefitPlanCode;
                paymentSource.PlanNameSnapshot = card.BenefitPlanName;
                paymentSource.ClassNameSnapshot = card.ClassName;
                paymentSource.EffectiveStartDateSnapshot = card.EffectiveStartDate;
                paymentSource.EffectiveEndDateSnapshot = card.EffectiveEndDate;
                paymentSource.IsEligible = card.IsEligible;
                paymentSource.IsPolicyActive = true;
                paymentSource.UpdateDateTime = DateTime.UtcNow;
                paymentSource.UpdateBy = command.ActorUserId;

                encounter.PaymentType = EncounterPaymentType.CompanyGuarantor;
                encounter.PaymentMethodId = null;
                encounter.UpdateDateTime = DateTime.UtcNow;
                encounter.UpdateBy = command.ActorUserId;

                newPayerName = guarantor.CompanyGuarantorName;
                newCardNumber = card.EmployeeNumber;
            }

            return new EncounterPaymentSourceSwitchResult(
                Success: true,
                ErrorCode: null,
                ErrorMessage: null,
                StatusCode: 200,
                OldPaymentType: oldPaymentType,
                OldPayerName: oldPayerName,
                OldCardNumber: oldCardNumber,
                NewPaymentType: newPaymentType,
                NewPayerName: newPayerName,
                NewCardNumber: newCardNumber);
        }

        private static void ClearInsuranceSnapshots(RegPatientEncounterGuarantor entity)
        {
            entity.PatientInsuranceId = null;
            entity.InsuranceProviderId = null;
            entity.PolicyNumberSnapshot = null;
            entity.CardNumberSnapshot = null;
            entity.MemberNumberSnapshot = null;
            entity.PlanNameSnapshot = null;
            entity.ClassNameSnapshot = null;
            entity.BenefitPlanCodeSnapshot = null;
        }

        private static void ClearCompanyGuarantorSnapshots(RegPatientEncounterGuarantor entity)
        {
            entity.PatientCompanyGuarantorId = null;
            entity.CompanyGuarantorId = null;
            entity.CompanyGuarantorCodeSnapshot = null;
            entity.EmployeeNumberSnapshot = null;
            entity.EmployeeNameSnapshot = null;
        }
    }

    public sealed record EncounterPaymentSourceSwitchCommand(
        Guid EncounterId,
        EncounterPaymentType PaymentType,
        Guid? PaymentMethodId,
        Guid? PatientInsuranceId,
        Guid? PatientCompanyGuarantorId,
        DateTime ServiceDate,
        string Reason,
        Guid ActorUserId);

    public sealed record EncounterPaymentSourceSwitchResult(
        bool Success,
        string? ErrorCode,
        string? ErrorMessage,
        int StatusCode,
        string? OldPaymentType,
        string? OldPayerName,
        string? OldCardNumber,
        string? NewPaymentType,
        string? NewPayerName,
        string? NewCardNumber);
}
