using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class EmployeeRemainingQuotaDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNik { get; set; } = string.Empty;
        public string GradeLevelCode { get; set; } = string.Empty;
        public string BenefitCategory { get; set; } = string.Empty;
        public decimal AnnualLimitAmount { get; set; }
        public int MinimumServiceMonths { get; set; }
        public int CurrentServiceMonths { get; set; }
        public bool IsEligibleByServiceMonths { get; set; }
        public decimal AccumulatedUsedAmount { get; set; }
        public decimal RemainingQuotaAmount { get; set; }
        public decimal PercentageRemaining { get; set; }
        public bool IsWarningLowQuota { get; set; } // true jika kuota tersisa < 20% (DED-W2)
        public bool AllowEmergencyCitoCashier { get; set; }
    }

    public class SubmitRestitutionClaimRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required, MaxLength(100)]
        public string ClaimCategory { get; set; } = "KacamataKerja";
        // 'KacamataKerja', 'FaskesLuarDarurat', 'DinasCitoMedis'

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Range(1000, 100000000)]
        public decimal ClaimAmount { get; set; }

        [Required]
        public string ReceiptAttachmentUrl { get; set; } = string.Empty;

        public string? MedicalResumeAttachmentUrl { get; set; }

        [Required, MinLength(5)]
        public string ReasonNotes { get; set; } = string.Empty;

        public bool IsCitoEmergency { get; set; } = false;
    }

    public class RestitutionClaimResponseDto
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public string ClaimCategory { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public decimal ClaimAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string ReceiptAttachmentUrl { get; set; } = string.Empty;
        public string? MedicalResumeAttachmentUrl { get; set; }
        public string ClaimStatus { get; set; } = "Draft";
        public string ReasonNotes { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public Guid? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public DateTime? SupervisorApprovedAt { get; set; }
        public Guid? HrReviewerId { get; set; }
        public string? HrReviewerName { get; set; }
        public DateTime? HrReviewedAt { get; set; }
        public Guid? FinalApproverId { get; set; }
        public string? FinalApproverName { get; set; }
        public DateTime? FinalApprovedAt { get; set; }
        public bool IsCitoEmergency { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SupervisorApprovalRequestDto
    {
        public string? Notes { get; set; }
    }

    public class HrReviewRequestDto
    {
        [Range(0, 100000000)]
        public decimal? VerifiedReceiptAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class FinalApprovalRequestDto
    {
        [Range(1000, 100000000)]
        public decimal ApprovedAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class RejectClaimRequestDto
    {
        [Required, MinLength(15, ErrorMessage = "Alasan penolakan klaim wajib memuat minimal 15 karakter audit (VR-DED-22).")]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class RestitutionClaimFilterDto
    {
        public Guid? EmployeeId { get; set; }
        public string? ClaimCategory { get; set; }
        public string? ClaimStatus { get; set; }
        public bool? IsCitoEmergency { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateSalaryCorrectionRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid AffectedPayrollRunId { get; set; } // Slip lampau yang salah potong (PAY-B6, DED-B4)

        public Guid? AffectedPayslipId { get; set; }

        [Range(1000, 100000000)]
        public decimal Amount { get; set; }

        [Required, MinLength(10)]
        public string Justification { get; set; } = string.Empty;

        public Guid? TargetPayrollRunId { get; set; }
    }

    public class PayrollRestitutionResponseDto
    {
        public Guid Id { get; set; }
        public string RestitutionNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public Guid? ClaimId { get; set; }
        public string RestitutionType { get; set; } = "BenefitReimbursement";
        public decimal Amount { get; set; }
        public Guid? AffectedPayrollRunId { get; set; }
        public Guid? AffectedPayslipId { get; set; }
        public Guid? TargetPayrollRunId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Justification { get; set; }
        public bool IsTaxable { get; set; } = false; // Non-pajak PPh 21 (DED-B5, HRD-ASM-33)
        public DateTime CreatedAt { get; set; }
    }

    public class InjectRestitutionToPayrollResultDto
    {
        public Guid PayrollRunId { get; set; }
        public int TotalRestitutionsInjected { get; set; }
        public decimal TotalReimbursementAmount { get; set; }
        public decimal TotalCorrectionAmount { get; set; }
        public decimal TotalNonTaxableInjected { get; set; }
        public List<PayrollRestitutionResponseDto> InjectedItems { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan investigasi keluhan salah potong gaji oleh HR Payroll Officer (BE-HRD-171, DED-B5, HRD-ASM-33, VR-DED-05).
    /// </summary>
    public class SalaryGrievanceInvestigationRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid AffectedPayrollRunId { get; set; }

        public Guid? AffectedPayslipId { get; set; }

        [Range(1000, 100000000)]
        public decimal ClaimedExcessDeductionAmount { get; set; }

        [Range(1000, 100000000)]
        public decimal ApprovedRefundAmount { get; set; }

        [Required, MinLength(15)]
        public string InvestigationFindings { get; set; } = string.Empty;

        public Guid? TargetPayrollRunId { get; set; }
    }

    /// <summary>
    /// Hasil resolusi investigasi keluhan salah potong gaji dengan status terkunci non-pajak PPh 21 (BE-HRD-171, DED-B5).
    /// </summary>
    public class SalaryGrievanceInvestigationResultDto
    {
        public Guid RestitutionId { get; set; }
        public string RestitutionNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public Guid AffectedPayrollRunId { get; set; }
        public Guid? AffectedPayslipId { get; set; }
        public decimal ClaimedExcessDeductionAmount { get; set; }
        public decimal ApprovedRefundAmount { get; set; }
        public bool IsTaxable { get; set; } = false; // Mutlak non-pajak PPh 21 (DED-B5, HRD-ASM-33)
        public string Status { get; set; } = "Approved";
        public string InvestigationFindings { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
