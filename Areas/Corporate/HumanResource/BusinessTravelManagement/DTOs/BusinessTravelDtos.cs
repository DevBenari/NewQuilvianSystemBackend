using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs
{
    #region 1. Business Travel Requests DTOs
    public class CreateBusinessTravelRequestDto
    {
        [Required, MaxLength(250)]
        public string TravelTitle { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string TravelPurpose { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ActivityDescription { get; set; }

        [Required, MaxLength(250)]
        public string Origin { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public Guid? TravelTypeId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? DestinationZoneId { get; set; }

        public decimal EstimatedTransportationAmount { get; set; } = 0m;
        public decimal EstimatedAccommodationAmount { get; set; } = 0m;
        public decimal EstimatedAllowanceAmount { get; set; } = 0m;
        public decimal EstimatedOtherAmount { get; set; } = 0m;

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        public bool IsDomestic { get; set; } = true;
        public bool IsInternational { get; set; } = false;
        public bool IsUrgent { get; set; } = false;
    }

    public class UpdateBusinessTravelRequestDto
    {
        [Required, MaxLength(250)]
        public string TravelTitle { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string TravelPurpose { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ActivityDescription { get; set; }

        [Required, MaxLength(250)]
        public string Origin { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Destination { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public decimal EstimatedTransportationAmount { get; set; } = 0m;
        public decimal EstimatedAccommodationAmount { get; set; } = 0m;
        public decimal EstimatedAllowanceAmount { get; set; } = 0m;
        public decimal EstimatedOtherAmount { get; set; } = 0m;
    }

    public class CancelTravelRequestDto
    {
        [Required, MinLength(15, ErrorMessage = "Alasan pembatalan wajib diisi minimal 15 karakter")]
        [MaxLength(2000)]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public class BusinessTravelResponseDto
    {
        public Guid Id { get; set; }
        public string TravelRequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string TravelTitle { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TravelDayCount { get; set; }
        public decimal EstimatedTotalAmount { get; set; }
        public decimal ApprovedBudgetAmount { get; set; }
        public string TravelStatus { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = "IDR";
        public DateTime? CreatedAt { get; set; }
    }

    public class BusinessTravelDetailResponseDto : BusinessTravelResponseDto
    {
        public string TravelPurpose { get; set; } = string.Empty;
        public string? ActivityDescription { get; set; }
        public decimal EstimatedTransportationAmount { get; set; }
        public decimal EstimatedAccommodationAmount { get; set; }
        public decimal EstimatedAllowanceAmount { get; set; }
        public decimal EstimatedOtherAmount { get; set; }
        public bool IsDomestic { get; set; }
        public bool IsInternational { get; set; }
        public bool IsUrgent { get; set; }
        public Guid? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public Guid? DestinationZoneId { get; set; }
        public string? DestinationZoneName { get; set; }
        public string? DocumentPdfUrl { get; set; }
        public string? ApprovalNotes { get; set; }
        public string? CancellationReason { get; set; }
    }
    #endregion

    #region 2. Travel Advances DTOs
    public class CreateTravelAdvanceRequestDto
    {
        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Nominal uang muka harus lebih dari 0")]
        public decimal RequestedAmount { get; set; }

        public Guid? BankAccountId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ApproveAdvanceRequestDto
    {
        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Nominal yang disetujui harus lebih dari 0")]
        public decimal ApprovedAmount { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNote { get; set; }
    }

    public class DisburseAdvancePaymentDto
    {
        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer"; // BankTransfer / CashVoucher

        [Required, MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class TravelAdvanceResponseDto
    {
        public Guid Id { get; set; }
        public string AdvanceRequestNumber { get; set; } = string.Empty;
        public Guid BusinessTravelRequestId { get; set; }
        public string TravelRequestNumber { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string AdvanceStatus { get; set; } = string.Empty;
        public DateOnly RequestDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? BankReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
    #endregion

    #region 3. Travel Settlements DTOs
    public class ActualExpenseItemDto
    {
        public Guid? TravelExpenseCategoryId { get; set; }

        [Required]
        public DateOnly ExpenseDate { get; set; }

        [Required, MaxLength(500)]
        public string ExpenseDescription { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? MerchantName { get; set; }

        [MaxLength(100)]
        public string? ReceiptNumber { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal ClaimedAmount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(500)]
        public string? ReceiptFilePath { get; set; }
    }

    public class MissingReceiptItemDto
    {
        public Guid? TravelExpenseCategoryId { get; set; }

        [Required]
        public DateOnly ExpenseDate { get; set; }

        [Required, MaxLength(500)]
        public string ExpenseDescription { get; set; } = string.Empty;

        [Required, Range(0.01, 150000, ErrorMessage = "Maksimal deklarasi tanpa nota Rp 150.000 per transaksi")]
        public decimal ClaimedAmount { get; set; }

        [Required, MinLength(15, ErrorMessage = "Alasan darurat ketiadaan kuitansi wajib minimal 15 karakter")]
        [MaxLength(1000)]
        public string EmergencyReason { get; set; } = string.Empty;
    }

    public class SubmitTravelSettlementDto
    {
        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public List<ActualExpenseItemDto> ActualExpenses { get; set; } = new List<ActualExpenseItemDto>();
        public List<MissingReceiptItemDto> MissingReceipts { get; set; } = new List<MissingReceiptItemDto>();

        [MaxLength(2000)]
        public string? SettlementNotes { get; set; }
    }

    public class VerifySettlementDto
    {
        [Required, Range(0, double.MaxValue)]
        public decimal ApprovedActualAmount { get; set; }

        [MaxLength(2000)]
        public string? VerificationNotes { get; set; }
    }

    public class CloseSettlementDto
    {
        [Required, MaxLength(100)]
        public string PaymentOrRefundRef { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ReceiptDocUrl { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class TravelSettlementResponseDto
    {
        public Guid Id { get; set; }
        public string SettlementNumber { get; set; } = string.Empty;
        public Guid BusinessTravelRequestId { get; set; }
        public string TravelRequestNumber { get; set; } = string.Empty;
        public decimal AdvancePaidAmount { get; set; }
        public decimal ApprovedExpenseAmount { get; set; }
        public decimal SettlementDifferenceAmount { get; set; }
        public string SettlementType { get; set; } = "Balanced"; // Underpaid, Overpaid, Balanced
        public string SettlementStatus { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public DateOnly SettlementDate { get; set; }
        public DateTime? SettledAt { get; set; }
        public string? SettlementNotes { get; set; }
    }

    public class TravelSettlementDetailResponseDto : TravelSettlementResponseDto
    {
        public decimal EmployeeRefundAmount { get; set; }
        public decimal CompanyPayableAmount { get; set; }
        public string? PaymentOrRefundRef { get; set; }
        public string? ReceiptDocUrl { get; set; }
        public List<ActualExpenseItemDto> Items { get; set; } = new List<ActualExpenseItemDto>();
        public List<MissingReceiptItemDto> MissingReceiptItems { get; set; } = new List<MissingReceiptItemDto>();
    }
    #endregion

    #region 4. Direct Expense Claims DTOs
    public class CreateExpenseClaimItemDto
    {
        public Guid? ExpenseCategoryId { get; set; }

        [Required]
        public DateOnly ExpenseDate { get; set; }

        [Required, MaxLength(500)]
        public string ExpenseDescription { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? MerchantName { get; set; }

        [MaxLength(100)]
        public string? ReceiptNumber { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal ClaimedAmount { get; set; }

        [MaxLength(500)]
        public string? ReceiptFilePath { get; set; }

        public bool IsMissingReceiptDeclared { get; set; } = false;

        [MaxLength(1000)]
        public string? MissingReceiptReason { get; set; }
    }

    public class CreateExpenseClaimDto
    {
        [Required, MaxLength(250)]
        public string ClaimTitle { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ClaimDescription { get; set; }

        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }

        [Required, MaxLength(40)]
        public string ClaimType { get; set; } = "WorkPurchase"; // General, Medical, WorkPurchase, Transportation

        public List<CreateExpenseClaimItemDto> Items { get; set; } = new List<CreateExpenseClaimItemDto>();
    }

    public class VerifyExpenseClaimDto
    {
        [Required, Range(0, double.MaxValue)]
        public decimal VerifiedAmount { get; set; }

        [MaxLength(2000)]
        public string? VerificationNotes { get; set; }
    }

    public class PayExpenseClaimDto
    {
        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        [Required, MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ExpenseClaimResponseDto
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ClaimTitle { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public DateOnly ClaimDate { get; set; }
        public decimal TotalClaimedAmount { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string ClaimStatus { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }

    public class ExpenseClaimDetailResponseDto : ExpenseClaimResponseDto
    {
        public string? ClaimDescription { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public List<CreateExpenseClaimItemDto> Items { get; set; } = new List<CreateExpenseClaimItemDto>();
    }
    #endregion

    #region 5. Travel Rates & Policies DTOs
    public class ConfigureTravelRateDto
    {
        [Required]
        public Guid DestinationZoneId { get; set; }

        public Guid? EmployeeGradeId { get; set; }

        [Required, MaxLength(50)]
        public string AllowanceRateCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string AllowanceRateName { get; set; } = string.Empty;

        [Required, Range(0, double.MaxValue)]
        public decimal RateAmount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class TravelAllowanceRateResponseDto
    {
        public Guid Id { get; set; }
        public Guid DestinationZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public Guid? EmployeeGradeId { get; set; }
        public string? GradeName { get; set; }
        public string AllowanceRateCode { get; set; } = string.Empty;
        public string AllowanceRateName { get; set; } = string.Empty;
        public decimal RateAmount { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public bool IsActive { get; set; }
    }

    public class TravelDestinationZoneResponseDto
    {
        public Guid Id { get; set; }
        public string ZoneCode { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class TravelOverdueReportDto
    {
        public Guid TravelRequestId { get; set; }
        public string TravelRequestNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateOnly EndDate { get; set; }
        public int OverdueDays { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string SettlementStatus { get; set; } = string.Empty;
        public bool IsBlockedFromNewTravel { get; set; }
        public bool PayrollDeductionScheduled { get; set; }
    }
    #endregion
}
