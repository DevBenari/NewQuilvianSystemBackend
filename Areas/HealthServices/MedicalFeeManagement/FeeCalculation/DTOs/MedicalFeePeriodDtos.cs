using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs
{
    // =========================================================================
    // Request DTOs
    // =========================================================================

    public class CreateFeePeriodRequest
    {
        [Required(ErrorMessage = "Kode periode (PeriodCode) wajib diisi.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "PeriodCode harus 3-20 karakter (contoh: '2026-10').")]
        public string PeriodCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal awal periode (PeriodStart) wajib diisi.")]
        public DateOnly PeriodStart { get; set; }

        [Required(ErrorMessage = "Tanggal akhir periode (PeriodEnd) wajib diisi.")]
        public DateOnly PeriodEnd { get; set; }
    }

    public class CalculatePeriodRequest
    {
        public Guid? ExpectedRowVersion { get; set; }
    }

    public class ReopenFeePeriodRequest
    {
        public Guid? ExpectedRowVersion { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class VerifyFeePeriodRequest
    {
        public Guid? ExpectedRowVersion { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class ReturnFeePeriodRequest
    {
        public Guid? ExpectedRowVersion { get; set; }

        [Required(ErrorMessage = "Alasan pengembalian periode wajib diisi.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Alasan pengembalian harus 3-500 karakter.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ApproveFeePeriodRequest
    {
        public Guid? ExpectedRowVersion { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class CloseFeePeriodRequest
    {
        public Guid? ExpectedRowVersion { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class FeePeriodPagedQuery
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? Year { get; set; }
        public string? SortBy { get; set; } = "PeriodStart";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    // =========================================================================
    // Response DTOs
    // =========================================================================

    public class FeePeriodHeaderResponse
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public string Status { get; set; } = MdfFeePeriodStatuses.Open;
        public DateTime? CalculatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int PayeeCount { get; set; }
        public decimal GrossTotal { get; set; }
        public int UnresolvedCount { get; set; }
        public Guid RowVersion { get; set; }
    }

    public class FeePeriodDetailResponse
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public string Status { get; set; } = MdfFeePeriodStatuses.Open;
        public DateTime? CalculatedAt { get; set; }
        public Guid? CalculatedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedBy { get; set; }
        public int PayeeCount { get; set; }
        public int DetailCount { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal AdjustmentTotal { get; set; }
        public decimal FinalTotal { get; set; }
        public int UnresolvedCount { get; set; }
        public Dictionary<string, int> UnresolvedByReason { get; set; } = new();
        public Guid RowVersion { get; set; }
    }

    public class FeePeriodSummaryResponse
    {
        public int TotalPeriods { get; set; }
        public int OpenPeriods { get; set; }
        public int CalculatedPeriods { get; set; }
        public int VerifiedPeriods { get; set; }
        public int ApprovedPeriods { get; set; }
        public int ClosedPeriods { get; set; }
    }

    public class CalculationResultResponse
    {
        public Guid PeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string Status { get; set; } = MdfFeePeriodStatuses.Calculated;
        public DateTime CalculatedAt { get; set; }
        public int PayeeCount { get; set; }
        public int DetailCount { get; set; }
        public decimal GrossTotal { get; set; }
        public int UnresolvedCount { get; set; }
        public Dictionary<string, int> UnresolvedByReason { get; set; } = new();
    }

    public class FeePeriodFilterMetadataResponse
    {
        public FeePeriodDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<FeePeriodSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> StatusOptions { get; set; } = new()
        {
            MdfFeePeriodStatuses.Open,
            MdfFeePeriodStatuses.Calculated,
            MdfFeePeriodStatuses.Verified,
            MdfFeePeriodStatuses.Approved,
            MdfFeePeriodStatuses.Closed
        };
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class FeePeriodDefaultFilterResponse
    {
        public string SortBy { get; set; } = "PeriodStart";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class FeePeriodSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
