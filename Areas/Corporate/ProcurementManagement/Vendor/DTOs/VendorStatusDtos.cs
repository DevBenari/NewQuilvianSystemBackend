using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;

// =====================================================================================
// BE-PBJ-VD-007 — DTO usulan blokir/buka blokir vendor (PBJ-VD-API 1.0 §Vendor Status Request, STATE bagian 3).
// Keputusan setuju/tolak/kembalikan dikerjakan penyetuju di kotak masuk Wfl yang sudah ada.
// =====================================================================================

public sealed class VendorStatusRequestQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }

    /// <summary>Cari nomor usulan, kode, atau nama vendor.</summary>
    public string? Search { get; set; }

    public Guid? SupplierId { get; set; }

    /// <summary><c>BLOCK</c> atau <c>UNBLOCK</c>.</summary>
    [MaxLength(10)] public string? RequestType { get; set; }

    /// <summary><c>DRAFT</c>, <c>PENDING_APPROVAL</c>, <c>RETURNED</c>, <c>APPROVED</c>, <c>REJECTED</c>, <c>CANCELLED</c>.</summary>
    [MaxLength(20)] public string? Status { get; set; }

    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Buat usulan. Contoh: <c>{ "supplierId": "…", "requestType": "BLOCK", "reason": "3 NCR kritis dalam 6 bulan",
/// "evidenceEvaluationId": "…penilaian Semester I 2027…" }</c>.
/// </summary>
public sealed class CreateVendorStatusRequestRequest
{
    [Required] public Guid SupplierId { get; set; }
    [Required, MaxLength(10)] public string RequestType { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Reason { get; set; }

    /// <summary>Penilaian final vendor yang sama sebagai bukti (opsional, PBJ-DEC-048).</summary>
    public Guid? EvidenceEvaluationId { get; set; }
}

public sealed class UpdateVendorStatusRequestRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }
    public Guid? EvidenceEvaluationId { get; set; }
}

public sealed class SubmitVendorStatusRequestRequest
{
    /// <summary>Opsional; bila dikirim harus sama dengan versi terkini (PBJ-VAL-090).</summary>
    public Guid? ExpectedRowVersion { get; set; }
}

public sealed class CancelVendorStatusRequestRequest
{
    public Guid? ExpectedRowVersion { get; set; }

    /// <summary>Wajib, minimal 10 karakter (PBJ-VAL-104).</summary>
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class VendorStatusRequestListItemResponse
{
    public Guid Id { get; set; }
    public string? RequestNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string RequestTypeLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public Guid RequesterUserId { get; set; }
    public string? RequesterName { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class VendorStatusRequestResponse
{
    public Guid Id { get; set; }
    public string? RequestNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>Status blokir vendor saat ini di Master Data (baca saja).</summary>
    public bool IsSupplierBlacklisted { get; set; }

    public Guid VendorProfileId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string RequestTypeLabel { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public Guid? EvidenceEvaluationId { get; set; }

    /// <summary>Contoh: "Alat Kesehatan Semester I 2027 — 2,10 (Blokir)".</summary>
    public string? EvidenceEvaluationLabel { get; set; }

    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public Guid? WorkflowInstanceId { get; set; }
    public int SubmissionCount { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }

    /// <summary>Pengusul; tidak boleh menyetujui usulannya sendiri (AllowSelfApproval = false).</summary>
    public Guid RequesterUserId { get; set; }

    public string? RequesterName { get; set; }
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class VendorStatusRequestSummaryResponse
{
    public int Draft { get; set; }
    public int PendingApproval { get; set; }
    public int Returned { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}

public sealed class VendorStatusRequestDefaultFilterResponse
{
    public Guid? SupplierId { get; set; }
    public string? RequestType { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class VendorStatusRequestFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public VendorStatusRequestDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<VendorValueOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<VendorValueOptionResponse> RequestTypeOptions { get; set; } = new();
    public List<VendorValueOptionResponse> StatusOptions { get; set; } = new();
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
