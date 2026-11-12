using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;

// =====================================================================================
// BE-PBJ-006 — DTO permintaan pengadaan (PBJ-PP-API 1.1 bagian Purchase Request).
// Angka hasil hitungan (total, pembagian rencana, kebutuhan beli) hanya diisi server.
// =====================================================================================

public sealed class PurchaseRequestQuery
{
    /// <summary>Cari nomor PR.</summary>
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? DepartmentId { get; set; }
    public int? FiscalYear { get; set; }
    public string? UrgencyLevel { get; set; }
    public bool? IsOutOfPlan { get; set; }
    public bool? IsOverBudget { get; set; }

    /// <summary>true = hanya PR yang dibuat pengguna login ("PR saya").</summary>
    public bool? MineOnly { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class PurchaseRequestLineRequest
{
    [Required] public Guid ItemId { get; set; }
    [MaxLength(1000)] public string? Specification { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public Guid? NeedPlanLineKey { get; set; }
    [MaxLength(1000)] public string? OutOfPlanReason { get; set; }
}

/// <summary>Isi draf PR. Nomor PR dan seluruh angka hasil hitungan tidak dikirim klien.</summary>
public class CreatePurchaseRequestRequest
{
    [Required] public Guid DepartmentId { get; set; }
    [Range(2000, 2100)] public int FiscalYear { get; set; }
    [Required] public Guid FundSourceId { get; set; }
    public DateOnly NeededByDate { get; set; }
    [Required, MaxLength(20)] public string UrgencyLevel { get; set; } = "NORMAL";
    [MaxLength(1000)] public string? UrgencyReason { get; set; }
    public DateOnly? UrgencyDeadline { get; set; }
    [MaxLength(2000)] public string? Justification { get; set; }
    public List<PurchaseRequestLineRequest> Lines { get; set; } = new();
}

public sealed class UpdatePurchaseRequestRequest : CreatePurchaseRequestRequest
{
    /// <summary>Versi data yang terakhir dibaca klien; selisih → 409 PBJ-VAL-090.</summary>
    [Required] public Guid ExpectedRowVersion { get; set; }
}

public sealed class PurchaseRequestLineResponse
{
    public Guid Id { get; set; }
    public int LineNo { get; set; }
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal RequestedAmount { get; set; }
    public Guid? NeedPlanLineKey { get; set; }
    public decimal InPlanQuantity { get; set; }
    public decimal OutOfPlanQuantity { get; set; }
    public string? OutOfPlanReason { get; set; }
    public decimal? StockAvailableQuantity { get; set; }
    public decimal? StockFulfilledQuantity { get; set; }
    public string? StockSourceNote { get; set; }
    public DateTimeOffset? StockCheckedAt { get; set; }
    public decimal PurchaseQuantity { get; set; }
    public decimal PurchaseAmount { get; set; }
}

public sealed class PurchaseRequestResponse
{
    public Guid Id { get; set; }
    public string? RequestNumber { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid RequesterUserId { get; set; }
    public string? RequesterName { get; set; }
    public int FiscalYear { get; set; }
    public Guid FundSourceId { get; set; }
    public string FundSourceName { get; set; } = string.Empty;
    public DateOnly NeededByDate { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty;
    public string? UrgencyReason { get; set; }
    public DateOnly? UrgencyDeadline { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsOutOfPlan { get; set; }
    public decimal RequestedTotal { get; set; }
    public decimal PurchaseTotal { get; set; }
    public bool IsOverBudget { get; set; }
    public decimal OverBudgetAmount { get; set; }
    public int SubmissionCount { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? WorkflowInstanceId { get; set; }
    public string? CancelReason { get; set; }
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }

    /// <summary>Aksi yang boleh dilakukan pengguna login saat ini (untuk menyembunyikan tombol).</summary>
    public bool CanEdit { get; set; }

    public List<PurchaseRequestLineResponse> Lines { get; set; } = new();
}

public sealed class PurchaseRequestListItem
{
    public Guid Id { get; set; }
    public string? RequestNumber { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid RequesterUserId { get; set; }
    public string? RequesterName { get; set; }
    public int FiscalYear { get; set; }
    public DateOnly NeededByDate { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsOutOfPlan { get; set; }
    public bool IsOverBudget { get; set; }
    public decimal RequestedTotal { get; set; }
    public decimal PurchaseTotal { get; set; }
    public int LineCount { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class PurchaseRequestStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class PurchaseRequestSummaryResponse
{
    public int Total { get; set; }
    public int Urgent { get; set; }
    public int OutOfPlan { get; set; }
    public int OverBudget { get; set; }
    public List<PurchaseRequestStatusCount> ByStatus { get; set; } = new();
}

public sealed class PurchaseRequestOptionValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class PurchaseRequestFilterMetadataResponse
{
    public PurchaseRequestQuery DefaultFilter { get; set; } = new();
    public List<PurchaseRequestOptionValue> StatusOptions { get; set; } = new();
    public List<PurchaseRequestOptionValue> UrgencyOptions { get; set; } = new();
    public List<PurchaseRequestOptionValue> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
