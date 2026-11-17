using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;

// =====================================================================================
// BE-PBJ-010 — DTO pagu pengadaan (PBJ-PP-API bagian Budget).
// Komitmen dan sisa pagu hanya dihitung server: sisa = pagu berlaku − komitmen aktif.
// =====================================================================================

public sealed class BudgetQuery
{
    public int? FiscalYear { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public Guid? FundSourceId { get; set; }
    public string? Status { get; set; }

    /// <summary>Cari nama unit atau kategori.</summary>
    public string? Search { get; set; }

    /// <summary>false (bawaan) = versi yang digantikan (<c>SUPERSEDED</c>) disembunyikan.</summary>
    public bool IncludeHistory { get; set; }

    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class CreateBudgetRequest
{
    [Range(2000, 2100)] public int FiscalYear { get; set; }
    [Required] public Guid DepartmentId { get; set; }
    [Required] public Guid ItemCategoryId { get; set; }
    [Required] public Guid FundSourceId { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Kunci pagu (tahun, unit, kategori, sumber dana) hanya dapat diganti pada versi 1 yang masih draf;
/// pada revisi hanya nilai pagu yang berubah.
/// </summary>
public sealed class UpdateBudgetRequest
{
    /// <summary>Versi data yang terakhir dibaca klien; selisih → 409 PBJ-VAL-090.</summary>
    [Required] public Guid ExpectedRowVersion { get; set; }
    [Range(2000, 2100)] public int FiscalYear { get; set; }
    [Required] public Guid DepartmentId { get; set; }
    [Required] public Guid ItemCategoryId { get; set; }
    [Required] public Guid FundSourceId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class BudgetReasonRequest
{
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class BudgetResponse
{
    public Guid Id { get; set; }
    public int FiscalYear { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public Guid FundSourceId { get; set; }
    public string FundSourceName { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    /// <summary>Σ komitmen aktif pada garis keturunan pagu ini.</summary>
    public decimal CommittedAmount { get; set; }

    /// <summary>Nilai versi ini − komitmen aktif.</summary>
    public decimal RemainingAmount { get; set; }

    public int VersionNo { get; set; }
    public Guid? PreviousVersionId { get; set; }
    public Guid LineageKey { get; set; }
    public bool IsCurrentVersion { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? RevisionReason { get; set; }
    public int SubmissionCount { get; set; }
    public Guid? WorkflowInstanceId { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid RowVersion { get; set; }
    public Guid? CreateBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }

    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanRevise { get; set; }
}

public sealed class BudgetRemainingQuery
{
    [Range(2000, 2100)] public int FiscalYear { get; set; }
    [Required] public Guid DepartmentId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public Guid? FundSourceId { get; set; }
}

/// <summary>Sisa pagu satu kunci. Pagu yang tidak ada dianggap sisa 0 (PBJ-DEC-030).</summary>
public sealed class BudgetRemainingResponse
{
    public int FiscalYear { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public Guid FundSourceId { get; set; }
    public string FundSourceName { get; set; } = string.Empty;
    public Guid? BudgetId { get; set; }
    public bool HasBudget { get; set; }
    public decimal Amount { get; set; }
    public decimal CommittedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
}

public sealed class BudgetStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class BudgetSummaryResponse
{
    public int Total { get; set; }
    public int PendingApproval { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal CommittedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public List<BudgetStatusCount> ByStatus { get; set; } = new();
}

public sealed class BudgetOptionValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class BudgetFilterMetadataResponse
{
    public BudgetQuery DefaultFilter { get; set; } = new();
    public List<BudgetOptionValue> StatusOptions { get; set; } = new();
    public List<BudgetOptionValue> SortOptions { get; set; } = new();
    public List<int> FiscalYearOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
