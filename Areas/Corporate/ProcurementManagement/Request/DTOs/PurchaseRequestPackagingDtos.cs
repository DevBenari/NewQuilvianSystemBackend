using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;

// =====================================================================================
// BE-PBJ-PS-005 — kontrak PBJ-PP-INT 1.2 §2: baris PR yang siap dipaketkan. Dimiliki perencanaan-permintaan,
// dipakai pemilihan-penyedia lewat ProcurementPurchaseRequestService.GetPackageableLinesAsync.
// =====================================================================================

/// <summary>
/// Saringan baris PR siap paket (<c>GET /procurement-packages/packageable-request-lines</c>). Contoh: tahun 2027,
/// jenis Barang Stok, kategori Alat Kesehatan.
/// </summary>
public sealed class PackageableRequestLineQuery
{
    public int? FiscalYear { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public Guid? DepartmentId { get; set; }

    /// <summary>Cari di nomor PR, kode, dan nama barang.</summary>
    public string? Search { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Satu baris PR <c>APPROVED</c> dengan jumlah beli &gt; 0 yang belum masuk paket (PBJ-DEC-071). Contoh: PR-000031 baris 1,
/// IGD, 10 Kursi Roda Standar, harga perkiraan Rp 2.800.000 (termasuk PPN, PBJ-DEC-084).
/// </summary>
public sealed class PackageableRequestLineResponse
{
    public Guid PurchaseRequestLineId { get; set; }
    public int LineNo { get; set; }
    public Guid PurchaseRequestId { get; set; }
    public string? RequestNumber { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public Guid FundSourceId { get; set; }
    public string FundSourceName { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public Guid ProcurementTypeId { get; set; }
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public string ProcurementTypeName { get; set; } = string.Empty;
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public decimal PurchaseQuantity { get; set; }

    /// <summary>Harga perkiraan satuan termasuk PPN; <c>null</c> bagi pengguna tanpa izin harga (PBJ-DEC-079).</summary>
    public decimal? EstimatedUnitPrice { get; set; }

    public Guid? NeedPlanLineKey { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}
