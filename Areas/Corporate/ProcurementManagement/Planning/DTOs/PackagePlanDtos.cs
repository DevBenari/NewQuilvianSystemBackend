using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;

// =====================================================================================
// BE-PBJ-011 — DTO konsolidasi kebutuhan dan rencana paket (PBJ-PP-API bagian Consolidation,
// Package Plan). Total, alokasi, dan nilai estimasi hanya dihitung server.
// =====================================================================================

public sealed class PlanConsolidationQuery
{
    [Range(2000, 2100)] public int FiscalYear { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public Guid? ProcurementTypeId { get; set; }
}

/// <summary>Satu unit asal dalam baris konsolidasi; <see cref="LineKey"/> dipakai saat membuat rencana paket.</summary>
public sealed class PlanConsolidationUnit
{
    public Guid LineKey { get; set; }
    public Guid NeedPlanId { get; set; }
    public string? PlanNumber { get; set; }
    public int VersionNo { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}

/// <summary>
/// Gabungan baris rencana berlaku per barang. Contoh: <c>KTL-000045 Kursi Roda Standar</c> → total 30
/// (IGD 3, Rawat Inap 5, Poli 22), dialokasikan 0, sisa 30.
/// </summary>
public sealed class PlanConsolidationRow
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public Guid ProcurementTypeId { get; set; }
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public string ProcurementTypeName { get; set; } = string.Empty;
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal EstimatedValue { get; set; }
    public List<PlanConsolidationUnit> Units { get; set; } = new();
}

public sealed class PackagePlanQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    /// <summary>Cari nomor atau judul rencana paket.</summary>
    public string? Search { get; set; }
    public int? FiscalYear { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class PackagePlanLineRequest
{
    /// <summary><c>LineKey</c> baris rencana berlaku, dari <c>GET /plan-consolidations</c>.</summary>
    [Required] public Guid NeedPlanLineKey { get; set; }
    public decimal AllocatedQuantity { get; set; }
}

public sealed class CreatePackagePlanRequest
{
    [Range(2000, 2100)] public int FiscalYear { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [Required] public Guid ProcurementTypeId { get; set; }
    [MaxLength(100)] public string? IndicativeMethod { get; set; }
    [Range(1, 12)] public int? TargetMonth { get; set; }
    public Guid? PicUserId { get; set; }
    public List<PackagePlanLineRequest> Lines { get; set; } = new();
}

/// <summary>Tahun dan jenis pengadaan tidak dapat diubah; yang diubah kepala lain dan baris alokasi.</summary>
public sealed class UpdatePackagePlanRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(100)] public string? IndicativeMethod { get; set; }
    [Range(1, 12)] public int? TargetMonth { get; set; }
    public Guid? PicUserId { get; set; }
    public List<PackagePlanLineRequest> Lines { get; set; } = new();
}

public sealed class PackagePlanReasonRequest
{
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class PackagePlanLineResponse
{
    public Guid Id { get; set; }
    public Guid NeedPlanLineKey { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;
    public decimal AllocatedQuantity { get; set; }

    /// <summary>Kuantitas baris rencana berlaku saat ini; null bila baris rencana tidak lagi berlaku.</summary>
    public decimal? PlannedQuantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedAmount { get; set; }
}

public sealed class PackagePlanResponse
{
    public Guid Id { get; set; }
    public string? PackagePlanNumber { get; set; }
    public int FiscalYear { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ProcurementTypeId { get; set; }
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public string ProcurementTypeName { get; set; } = string.Empty;
    public string? IndicativeMethod { get; set; }
    public int? TargetMonth { get; set; }
    public decimal EstimatedValue { get; set; }
    public Guid? PicUserId { get; set; }
    public string? PicName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
    public int SubmissionCount { get; set; }
    public Guid? WorkflowInstanceId { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int LineCount { get; set; }
    public int UnitCount { get; set; }
    public Guid RowVersion { get; set; }
    public Guid? CreateBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }

    public bool CanEdit { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanCancel { get; set; }

    /// <summary>Diisi pada detail; kosong pada daftar.</summary>
    public List<PackagePlanLineResponse> Lines { get; set; } = new();
}

public sealed class PackagePlanStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class PackagePlanSummaryResponse
{
    public int Total { get; set; }
    public int PendingApproval { get; set; }
    public decimal ApprovedValue { get; set; }
    public List<PackagePlanStatusCount> ByStatus { get; set; } = new();
}

public sealed class PackagePlanOptionValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class PackagePlanFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public PackagePlanQuery DefaultFilter { get; set; } = new();
    public List<PackagePlanOptionValue> StatusOptions { get; set; } = new();
    public List<PackagePlanOptionValue> SortOptions { get; set; } = new();
    public List<int> FiscalYearOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
