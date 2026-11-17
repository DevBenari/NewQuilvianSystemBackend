using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;

// =====================================================================================
// BE-PBJ-009 — DTO rencana kebutuhan tahunan (PBJ-PP-API bagian Need Plan).
// Total, versi, nomor, dan pemakaian baris hanya diisi server.
// =====================================================================================

public sealed class NeedPlanQuery
{
    /// <summary>Cari nomor rencana.</summary>
    public string? Search { get; set; }
    public Guid? DepartmentId { get; set; }
    public int? FiscalYear { get; set; }
    public string? Status { get; set; }

    /// <summary>false (bawaan) = versi yang digantikan (<c>SUPERSEDED</c>) disembunyikan.</summary>
    public bool IncludeHistory { get; set; }

    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class NeedPlanLineRequest
{
    /// <summary>
    /// Isi dengan <c>lineKey</c> dari respons untuk mengubah baris yang sudah ada; kosongkan untuk
    /// baris baru. Barang pada baris yang sudah ada tidak dapat diganti.
    /// </summary>
    public Guid? LineKey { get; set; }
    [Required] public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    [Range(1, 12)] public int? NeededMonth { get; set; }
    public Guid? FundSourceId { get; set; }
    [MaxLength(1000)] public string? Justification { get; set; }
    [MaxLength(1000)] public string? Specification { get; set; }
}

public sealed class CreateNeedPlanRequest
{
    [Required] public Guid DepartmentId { get; set; }
    [Range(2000, 2100)] public int FiscalYear { get; set; }
    public List<NeedPlanLineRequest> Lines { get; set; } = new();
}

/// <summary>Unit dan tahun tidak dapat diubah setelah rencana dibuat; yang diubah hanya baris.</summary>
public sealed class UpdateNeedPlanRequest
{
    /// <summary>Versi data yang terakhir dibaca klien; selisih → 409 PBJ-VAL-090.</summary>
    [Required] public Guid ExpectedRowVersion { get; set; }
    public List<NeedPlanLineRequest> Lines { get; set; } = new();
}

/// <summary>Isi aksi batal dan buat revisi.</summary>
public sealed class NeedPlanReasonRequest
{
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class NeedPlanLineResponse
{
    public Guid Id { get; set; }
    public Guid LineKey { get; set; }
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedTotal { get; set; }
    public int? NeededMonth { get; set; }
    public Guid? FundSourceId { get; set; }
    public string? FundSourceName { get; set; }
    public string? Justification { get; set; }
    public string? Specification { get; set; }

    /// <summary>"Sudah dipakai PR": Σ kuantitas dalam rencana pada PR berstatus pemakan.</summary>
    public decimal UsedByPurchaseRequest { get; set; }

    /// <summary>"Dialokasikan paket": Σ alokasi rencana paket yang masih hidup.</summary>
    public decimal AllocatedToPackage { get; set; }

    /// <summary>Sisa untuk PR = kuantitas − dipakai PR (tidak kurang dari 0).</summary>
    public decimal RemainingForPurchaseRequest { get; set; }
}

public sealed class NeedPlanResponse
{
    public Guid Id { get; set; }
    public string? PlanNumber { get; set; }
    public int FiscalYear { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int VersionNo { get; set; }
    public Guid? PreviousVersionId { get; set; }
    public bool IsCurrentVersion { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? RevisionReason { get; set; }
    public decimal EstimatedTotal { get; set; }
    public int LineCount { get; set; }
    public int SubmissionCount { get; set; }
    public Guid? WorkflowInstanceId { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid RowVersion { get; set; }
    public Guid? CreateBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }

    /// <summary>Kelayakan status untuk menyembunyikan tombol; izin tetap diperiksa server per endpoint.</summary>
    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanCancel { get; set; }
    public bool CanRevise { get; set; }

    /// <summary>Diisi pada detail; kosong pada daftar.</summary>
    public List<NeedPlanLineResponse> Lines { get; set; } = new();
}

public sealed class NeedPlanStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class NeedPlanSummaryResponse
{
    public int Total { get; set; }
    public int CurrentVersions { get; set; }
    public int PendingApproval { get; set; }
    public List<NeedPlanStatusCount> ByStatus { get; set; } = new();
}

public sealed class NeedPlanOptionValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class NeedPlanFilterMetadataResponse
{
    public NeedPlanQuery DefaultFilter { get; set; } = new();
    public List<NeedPlanOptionValue> StatusOptions { get; set; } = new();
    public List<NeedPlanOptionValue> SortOptions { get; set; } = new();
    public List<int> FiscalYearOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}

/// <summary>Satu baris perbandingan versi, dikunci <c>LineKey</c>.</summary>
public sealed class NeedPlanVersionCompareLine
{
    public Guid LineKey { get; set; }
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;

    /// <summary><c>ADDED</c>, <c>REMOVED</c>, <c>CHANGED</c>, atau <c>UNCHANGED</c>.</summary>
    public string ChangeType { get; set; } = string.Empty;
    public decimal? BeforeQuantity { get; set; }
    public decimal? AfterQuantity { get; set; }
    public decimal? BeforeUnitPrice { get; set; }
    public decimal? AfterUnitPrice { get; set; }
    public decimal? BeforeTotal { get; set; }
    public decimal? AfterTotal { get; set; }
}

/// <summary>"Sebelum" selalu versi bernomor lebih kecil, "sesudah" versi bernomor lebih besar.</summary>
public sealed class NeedPlanVersionCompareResponse
{
    public Guid BeforeId { get; set; }
    public int BeforeVersionNo { get; set; }
    public decimal BeforeTotal { get; set; }
    public Guid AfterId { get; set; }
    public int AfterVersionNo { get; set; }
    public decimal AfterTotal { get; set; }
    public string? RevisionReason { get; set; }
    public List<NeedPlanVersionCompareLine> Lines { get; set; } = new();
}
