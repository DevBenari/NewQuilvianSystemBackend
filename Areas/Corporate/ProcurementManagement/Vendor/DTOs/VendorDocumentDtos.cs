using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;

// =====================================================================================
// BE-PBJ-VD-004 — DTO dokumen legalitas vendor (PBJ-VD-API 1.0 §Vendor Document, STATE bagian 2).
// Berkas diunggah lewat POST /attachments (documentType PBJ_VENDOR_DOCUMENT, documentId = id dokumen).
// =====================================================================================

public sealed class VendorDocumentQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }

    /// <summary>Cari kode/nama vendor atau nomor dokumen.</summary>
    public string? Search { get; set; }

    public Guid? SupplierId { get; set; }
    public Guid? DocumentTypeId { get; set; }

    /// <summary><c>PENDING</c>, <c>VERIFIED</c>, <c>REJECTED</c>, <c>CANCELLED</c>.</summary>
    [MaxLength(20)] public string? VerificationStatus { get; set; }

    /// <summary><c>VALID</c>, <c>EXPIRING</c>, <c>EXPIRED</c>, <c>REPLACED</c> — hanya dokumen terverifikasi.</summary>
    [MaxLength(20)] public string? ValidityState { get; set; }

    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Catat dokumen baru (status <c>PENDING</c>). Contoh Izin PBF: <c>{ "supplierId": "…", "documentTypeId": "…IZIN_PBF…",
/// "documentNumber": "PBF/123/2026", "issuedDate": "2026-07-01", "expiryDate": "2031-06-30" }</c>.
/// </summary>
public sealed class CreateVendorDocumentRequest
{
    [Required] public Guid SupplierId { get; set; }
    [Required] public Guid DocumentTypeId { get; set; }
    [MaxLength(100)] public string? DocumentNumber { get; set; }
    public DateOnly? IssuedDate { get; set; }

    /// <summary>Wajib bila jenisnya mewajibkan tanggal berlaku; tidak boleh sebelum tanggal terbit (PBJ-VAL-101).</summary>
    public DateOnly? ExpiryDate { get; set; }
}

public sealed class VerifyVendorDocumentRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
}

public sealed class RejectVendorDocumentRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }

    /// <summary>Wajib, minimal 10 karakter (PBJ-VAL-104). Contoh: "Masa berlaku pada berkas berbeda dengan isian."</summary>
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class CancelVendorDocumentRequest
{
    /// <summary>Opsional; bila dikirim, harus sama dengan versi terkini (PBJ-VAL-090).</summary>
    public Guid? ExpectedRowVersion { get; set; }

    /// <summary>Wajib, minimal 10 karakter (PBJ-VAL-104).</summary>
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class VendorDocumentFileResponse
{
    public Guid AttachmentId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int VersionNo { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? UploadedByName { get; set; }
}

public sealed class VendorDocumentListItemResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string VerificationStatusLabel { get; set; } = string.Empty;

    /// <summary>Hanya untuk dokumen terverifikasi: <c>VALID</c>, <c>EXPIRING</c>, <c>EXPIRED</c>, <c>REPLACED</c>.</summary>
    public string? ValidityState { get; set; }

    public string? ValidityStateLabel { get; set; }

    /// <summary>Sisa hari sampai tanggal berlaku (negatif = sudah lewat); kosong bila tanpa tanggal berlaku.</summary>
    public int? DaysUntilExpiry { get; set; }

    public int FileCount { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class VendorDocumentResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public Guid VendorProfileId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool RequiresExpiryDate { get; set; }
    public string? DocumentNumber { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string VerificationStatusLabel { get; set; } = string.Empty;
    public string? ValidityState { get; set; }
    public string? ValidityStateLabel { get; set; }
    public int? DaysUntilExpiry { get; set; }

    /// <summary>Pengunggah (pembuat catatan). Pemeriksa wajib orang lain (PBJ-DEC-046).</summary>
    public Guid UploadedByUserId { get; set; }

    public string? UploadedByName { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>Alasan penolakan atau pembatalan.</summary>
    public string? RejectionReason { get; set; }

    public Guid RowVersion { get; set; }
    public List<VendorDocumentFileResponse> Files { get; set; } = new();
    public DateTime CreateDateTime { get; set; }
}

public sealed class VendorDocumentSummaryCountsResponse
{
    public int Pending { get; set; }
    public int Verified { get; set; }
    public int Rejected { get; set; }

    /// <summary>Dokumen terverifikasi terakhir per vendor × jenis yang sudah kedaluwarsa.</summary>
    public int Expired { get; set; }

    /// <summary>Dokumen terverifikasi terakhir per vendor × jenis yang berakhir dalam batas peringatan.</summary>
    public int ExpiringSoon { get; set; }

    public int ExpiryWarningDays { get; set; }
}

public sealed class VendorDocumentDefaultFilterResponse
{
    public Guid? SupplierId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? VerificationStatus { get; set; }
    public string? ValidityState { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class VendorDocumentTypeFilterOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool RequiresExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public sealed class VendorDocumentFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public VendorDocumentDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<VendorValueOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<VendorDocumentTypeFilterOptionResponse> DocumentTypeOptions { get; set; } = new();
    public List<VendorValueOptionResponse> VerificationStatusOptions { get; set; } = new();
    public List<VendorValueOptionResponse> ValidityStateOptions { get; set; } = new();
    public int ExpiryWarningDays { get; set; }
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
