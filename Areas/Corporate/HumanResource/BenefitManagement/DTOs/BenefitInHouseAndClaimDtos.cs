using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    // =========================================================================
    // 1. DTOS IN-HOUSE HEALTHCARE & SIMRS BALANCING (BE-HRD-186, BE-HRD-187)
    // =========================================================================

    public class InHouseCoverageInquiryQuery
    {
        public string? NationalIdNumber { get; set; }
        public string? EmployeeIdNumber { get; set; }
        public Guid? WorkforceProfileId { get; set; }
    }

    public class InHouseCoverageInquiryDto
    {
        public bool IsEligible { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid? DependentId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientType { get; set; } = "Employee"; // "Employee", "Spouse", "Child"
        public string BenefitPlanName { get; set; } = string.Empty;
        public string InpatientRoomClass { get; set; } = "Kelas 1"; // VVIP, VIP, Kelas 1, Kelas 2
        public decimal HospitalDiscountPercentage { get; set; } = 30.00m; // 30% Jasa medis
        public decimal AnnualQuotaLimit { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool IsNearExhaustion { get; set; } = false; // BEN-W2: > 80% used
        public string? WarningMessage { get; set; }
        public string? IneligibilityReason { get; set; }
    }

    public class DeductInHouseBalanceRequestDto
    {
        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DependentId { get; set; }

        [Required(ErrorMessage = "Nomor registrasi SIMRS wajib diisi.")]
        public string SimrsRegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total tagihan kotor (gross bill) wajib diisi.")]
        [Range(0.01, 1000000000, ErrorMessage = "Nomor tagihan kotor harus lebih dari 0.")]
        public decimal GrossBillAmount { get; set; }

        public decimal DiscountAppliedAmount { get; set; } = 0m;

        [Required(ErrorMessage = "Nominal pemotongan plafon wajib diisi.")]
        [Range(0, 1000000000, ErrorMessage = "Nominal plafon tidak boleh bernilai negatif.")]
        public decimal PlafonDeductedAmount { get; set; }

        public string ServiceType { get; set; } = "Outpatient"; // Outpatient, Inpatient, Emergency, Pharmacy

        public string? Notes { get; set; }
    }

    public class BenefitBalanceDeductionResultDto
    {
        public Guid TransactionId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal GrossBill { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PlafonCoveredAmount { get; set; }
        public decimal OutOfPocketExcessAmount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public bool DelegateExcessToPayroll { get; set; }
        public string Status { get; set; } = "Success";
        public string Message { get; set; } = string.Empty;
    }

    public class BenefitBalanceMovementDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string MovementType { get; set; } = string.Empty; // AnnualAllocation, HospitalCareUsage, ManualAdjustment, YearEndExpiryReset
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset OccurredAt { get; set; }
    }

    public class AdjustBalanceManualRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        [Required(ErrorMessage = "Nominal penyesuaian wajib diisi.")]
        public decimal AdjustmentAmount { get; set; }

        [Required(ErrorMessage = "Alasan penyesuaian saldo wajib diisi secara rinci.")]
        [MinLength(10, ErrorMessage = "Alasan penyesuaian minimal 10 karakter.")]
        public string Reason { get; set; } = string.Empty;

        public string? ReferenceNumber { get; set; }
    }

    // =========================================================================
    // 2. DTOS KLAIM KOORDINASI MANFAAT (COB) & SETTLEMENT (BE-HRD-188, BE-HRD-189)
    // =========================================================================

    public class BenefitClaimListQuery
    {
        public string? ClaimStatus { get; set; }
        public string? ServiceType { get; set; }
        public string? SearchKeyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BenefitClaimSummaryDto
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientType { get; set; } = "Employee";
        public string ServiceType { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string? SepBpjsNumber { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal TotalHospitalBill { get; set; }
        public decimal PrimaryPayerCoveredAmount { get; set; }
        public decimal SecondaryClaimAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string ClaimStatus { get; set; } = "Draft";
        public DateTime? SubmittedAt { get; set; }
    }

    public class CreateEssClaimRequestDto
    {
        public Guid? DependentId { get; set; }

        [Required(ErrorMessage = "Jenis pelayanan wajib diisi ('Inpatient', 'Outpatient', 'Emergency').")]
        public string ServiceType { get; set; } = "Inpatient";

        [Required(ErrorMessage = "Nama rumah sakit tempat perawatan wajib diisi.")]
        [MaxLength(150)]
        public string HospitalName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal pelayanan medis wajib diisi.")]
        public DateTime ServiceDate { get; set; }

        public string? SepBpjsNumber { get; set; } // Wajib jika Inpatient / COB (VR-BEN-16)

        [Required(ErrorMessage = "Total kuitansi tagihan rumah sakit wajib diisi.")]
        [Range(0.01, 1000000000, ErrorMessage = "Total tagihan harus lebih dari 0.")]
        public decimal TotalHospitalBill { get; set; }

        [Range(0, 1000000000)]
        public decimal PrimaryPayerCoveredAmount { get; set; } = 0m;

        [Required(ErrorMessage = "Nominal klaim penjamin sekunder wajib diisi.")]
        [Range(0, 1000000000)]
        public decimal SecondaryClaimAmount { get; set; } = 0m;

        [Range(0, 1000000000)]
        public decimal OutOfPocketExcessAmount { get; set; } = 0m;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class BenefitClaimDetailDto
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public Guid? DependentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = "Self";
        public string BenefitPlanName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public string? SepBpjsNumber { get; set; }
        public decimal TotalHospitalBill { get; set; }
        public decimal PrimaryPayerCoveredAmount { get; set; }
        public decimal SecondaryClaimAmount { get; set; }
        public decimal OutOfPocketExcessAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string ClaimStatus { get; set; } = "Draft";
        public DateTime? SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? SettledAt { get; set; }
        public string? SettlementReference { get; set; }
        public string? RejectionReason { get; set; }
        public string? Notes { get; set; }
        public List<ClaimDocumentDetailDto> Documents { get; set; } = new();
    }

    public class ClaimDocumentUploadRequestDto
    {
        [Required(ErrorMessage = "Jenis dokumen wajib diisi ('BPJS_SEP', 'Hospital_Invoice', 'Receipt_Original', 'Medical_Summary').")]
        public string DocumentType { get; set; } = "Receipt_Original";

        public string? DocumentNumber { get; set; }

        [Required(ErrorMessage = "Path penyimpanan berkas wajib disertakan.")]
        public string FilePath { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public string FileContentType { get; set; } = "application/pdf";
    }

    public class ClaimDocumentDetailDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class VerifyClaimRequestDto
    {
        [Required(ErrorMessage = "Status keputusan verifikasi wajib diisi ('Approved' atau 'Rejected').")]
        public string Status { get; set; } = "Approved";

        public decimal? ApprovedAmount { get; set; }

        public string? RejectionReason { get; set; }

        public string? Notes { get; set; }
    }

    public class SettleClaimRequestDto
    {
        [Required(ErrorMessage = "Nominal santunan yang disetujui cair wajib diisi.")]
        [Range(0, 1000000000)]
        public decimal ApprovedSecondaryAmount { get; set; }

        public decimal OutOfPocketExcessAmount { get; set; } = 0m;

        [Required(ErrorMessage = "Nomor referensi bukti transfer/pelunasan wajib diisi.")]
        public string SettlementReference { get; set; } = string.Empty;

        public bool DelegateExcessToPayroll { get; set; } = false;

        public string? Notes { get; set; }
    }

    public class TerminateBenefitRequestDto
    {
        [Required(ErrorMessage = "Tanggal efektif keluar wajib diisi.")]
        public DateTime EffectiveTerminationDate { get; set; }

        [Required(ErrorMessage = "Alasan pengakhiran benefit wajib diisi.")]
        public string TerminationReason { get; set; } = string.Empty;
    }

    public class ExitClearanceMedicalReceivableDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public bool HasUnpaidMedicalBill { get; set; }
        public decimal UnpaidExcessAmount { get; set; }
        public bool IsClearanceBlocked { get; set; }
        public List<UnpaidBillItemDto> UnpaidBills { get; set; } = new();
    }

    public class UnpaidBillItemDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal ExcessAmount { get; set; }
    }
}
