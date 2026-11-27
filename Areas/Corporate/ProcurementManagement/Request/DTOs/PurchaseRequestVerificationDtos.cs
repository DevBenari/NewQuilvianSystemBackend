using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;

// =====================================================================================
// BE-PBJ-008 — DTO verifikasi PR oleh Tim Pengadaan (PBJ-PP-API bagian Verification).
// Kebutuhan beli, nilai beli, dan tanda lewat pagu hanya dihitung server.
// =====================================================================================

public sealed class StockCheckQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    /// <summary>Cari nomor PR atau kode/nama barang.</summary>
    public string? Search { get; set; }
    public Guid? DepartmentId { get; set; }

    /// <summary>Status PR; bawaan <c>SUBMITTED</c> (menunggu verifikasi).</summary>
    public string? Status { get; set; }

    /// <summary>true = hanya baris yang sudah dicek; false = hanya yang belum.</summary>
    public bool? IsChecked { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class StockCheckListItem
{
    public Guid PurchaseRequestId { get; set; }
    public string? RequestNumber { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UrgencyLevel { get; set; } = string.Empty;
    public Guid LineId { get; set; }
    public int LineNo { get; set; }
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public decimal? StockAvailableQuantity { get; set; }
    public decimal? StockFulfilledQuantity { get; set; }
    public string? StockSourceNote { get; set; }
    public DateTimeOffset? StockCheckedAt { get; set; }
    public string? StockCheckedByName { get; set; }
    public decimal PurchaseQuantity { get; set; }
    public decimal PurchaseAmount { get; set; }
}

public sealed class StockCheckLineRequest
{
    [Required] public Guid LineId { get; set; }
    public decimal StockAvailableQuantity { get; set; }
    public decimal StockFulfilledQuantity { get; set; }
    [MaxLength(300)] public string? StockSourceNote { get; set; }
}

/// <summary>Hasil cek stok per baris. Baris yang tidak dikirim tidak berubah.</summary>
public sealed class RecordStockCheckRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    public List<StockCheckLineRequest> Lines { get; set; } = new();
}

public sealed class CompleteVerificationRequest
{
    /// <summary>Versi data yang terakhir dibaca verifikator; selisih → 409 PBJ-VAL-090.</summary>
    [Required] public Guid ExpectedRowVersion { get; set; }

    [MaxLength(1000)] public string? Note { get; set; }
}

public sealed class ReturnPurchaseRequestRequest
{
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class RejectPurchaseRequestRequest
{
    [MaxLength(1000)] public string? Reason { get; set; }
}
