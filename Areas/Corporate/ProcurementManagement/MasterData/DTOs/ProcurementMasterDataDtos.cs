using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;

// =====================================================================================
// BE-PBJ-003 — DTO master Pengadaan (PBJ-PP-API 1.1, PBJ-DEC-037).
// Kategori dan sumber dana berbagi bentuk "referensi" (kode, nama, deskripsi, aktif);
// service dan controller tetap satu per master.
// =====================================================================================

/// <summary>Permintaan ubah status aktif saja (<c>PATCH /{id}/status</c>).</summary>
public sealed class UpdateProcurementMasterStatusRequest
{
    public bool IsActive { get; set; }
}

// ---------------------------------------------------------------- referensi (kategori, sumber dana)

public sealed class ProcurementReferenceQuery
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class UpsertProcurementReferenceRequest
{
    /// <summary>Kode unik di antara data yang belum dihapus; disimpan huruf besar. Contoh: <c>ALKES</c>.</summary>
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProcurementReferenceResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class ProcurementReferenceOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class ProcurementReferenceSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
}

public sealed class ProcurementMasterDeleteResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
}

public sealed class ProcurementReferenceDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementSortOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class ProcurementReferenceFilterMetadataResponse
{
    public ProcurementReferenceDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}

// ---------------------------------------------------------------- jenis pengadaan (hanya baca)

public sealed class ProcurementTypeResponse
{
    public Guid Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;

    /// <summary>true = barang (serah terima lewat GR BLM); false = jasa (serah terima pekerjaan dan BAST).</summary>
    public bool IsGoods { get; set; }

    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// ---------------------------------------------------------------- katalog barang/jasa

public sealed class ProcurementItemQuery
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public string SortBy { get; set; } = "itemName";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class ProcurementItemOptionsQuery
{
    public bool OnlyActive { get; set; } = true;
    public string? Search { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 50;
}

/// <summary>Isi form katalog. Kode <c>KTL-…</c> tidak dikirim; dibuat server.</summary>
public sealed class UpsertProcurementItemRequest
{
    [Required, MaxLength(200)] public string ItemName { get; set; } = string.Empty;
    [Required] public Guid ProcurementTypeId { get; set; }
    [Required] public Guid ItemCategoryId { get; set; }
    [Required] public Guid MeasurementId { get; set; }
    public Guid? DrugId { get; set; }
    [MaxLength(1000)] public string? DefaultSpecification { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProcurementItemResponse
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public Guid ProcurementTypeId { get; set; }
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public string ProcurementTypeName { get; set; } = string.Empty;
    public bool IsGoods { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public Guid MeasurementId { get; set; }
    public string MeasurementName { get; set; } = string.Empty;
    public bool IsDecimalAllowed { get; set; }
    public Guid? DrugId { get; set; }
    public string? DrugName { get; set; }
    public string? DefaultSpecification { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

/// <summary>Pilihan ringan untuk form PR dan rencana.</summary>
public sealed class ProcurementItemOptionResponse
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public Guid ProcurementTypeId { get; set; }
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public bool IsGoods { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public Guid MeasurementId { get; set; }
    public string MeasurementName { get; set; } = string.Empty;
    public bool IsDecimalAllowed { get; set; }
    public string? DefaultSpecification { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProcurementItemSummaryResponse
{
    public int TotalItem { get; set; }
    public int ActiveItem { get; set; }
    public int InactiveItem { get; set; }
    public int GoodsItem { get; set; }
    public int ServiceItem { get; set; }
    public int DrugLinkedItem { get; set; }
}

public sealed class ProcurementItemDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public string SortBy { get; set; } = "itemName";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementItemFilterMetadataResponse
{
    public ProcurementItemDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<ProcurementTypeResponse> ProcurementTypeOptions { get; set; } = new();
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
