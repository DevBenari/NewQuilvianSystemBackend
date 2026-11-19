using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;

// =====================================================================================
// BE-PBJ-VD-002 — DTO tiga master konfigurasi vendor (PBJ-VD-API 1.0, kamus data bagian 2).
// Permintaan ubah status memakai UpdateProcurementMasterStatusRequest; respons hapus memakai
// ProcurementMasterDeleteResponse (keduanya di ProcurementMasterDataDtos.cs).
// =====================================================================================

/// <summary>Satu pilihan kategori pada metadata saringan master vendor.</summary>
public sealed class ProcurementVendorCategoryOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// ---------------------------------------------------------------- jenis dokumen legalitas

public sealed class ProcurementVendorDocumentTypeQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>Saring jenis yang mewajibkan tanggal berlaku. Contoh: <c>true</c> → Izin PBF, Izin Distribusi Alkes.</summary>
    public bool? RequiresExpiryDate { get; set; }

    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class UpsertProcurementVendorDocumentTypeRequest
{
    /// <summary>Kode unik di antara data yang belum dihapus; disimpan huruf besar. Contoh: <c>IZIN_PBF</c>.</summary>
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;

    /// <summary><c>true</c> → dokumen jenis ini wajib diisi tanggal berlaku (PBJ-VAL-101).</summary>
    public bool RequiresExpiryDate { get; set; }

    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProcurementVendorDocumentTypeResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool RequiresExpiryDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Jumlah kategori yang saat ini mewajibkan jenis ini (kewajiban aktif).</summary>
    public int ActiveRequirementCount { get; set; }

    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class ProcurementVendorDocumentTypeOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool RequiresExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProcurementVendorDocumentTypeSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int RequiresExpiryDate { get; set; }
}

public sealed class ProcurementVendorDocumentTypeDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public bool? RequiresExpiryDate { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementVendorDocumentTypeFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public ProcurementVendorDocumentTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}

// ---------------------------------------------------------------- dokumen wajib per kategori

public sealed class ProcurementCategoryDocumentRequirementQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string SortBy { get; set; } = "category";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Isi form kewajiban dokumen. Kewajiban baru lahir nonaktif kecuali <see cref="IsActive"/> dikirim <c>true</c>
/// (PBJ-DEC-061). Contoh: kategori Obat × Izin PBF, nonaktif sampai dokumen vendor Obat terkumpul.
/// </summary>
public sealed class UpsertProcurementCategoryDocumentRequirementRequest
{
    [Required] public Guid ItemCategoryId { get; set; }
    [Required] public Guid DocumentTypeId { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProcurementCategoryDocumentRequirementResponse
{
    public Guid Id { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryCode { get; set; } = string.Empty;
    public string ItemCategoryName { get; set; } = string.Empty;
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool RequiresExpiryDate { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Saat kewajiban pertama kali diaktifkan; kosong = belum pernah berlaku.</summary>
    public DateTimeOffset? ActivatedAt { get; set; }

    public Guid? ActivatedByUserId { get; set; }
    public string? ActivatedByName { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class ProcurementCategoryDocumentRequirementOptionResponse
{
    public Guid Id { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class ProcurementCategoryDocumentRequirementSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }

    /// <summary>Jumlah kategori yang punya sekurang-kurangnya satu kewajiban aktif.</summary>
    public int CategoriesWithActiveRequirement { get; set; }
}

public sealed class ProcurementCategoryDocumentRequirementDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "category";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementCategoryDocumentRequirementFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public ProcurementCategoryDocumentRequirementDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<ProcurementVendorCategoryOptionResponse> ItemCategoryOptions { get; set; } = new();
    public List<ProcurementVendorDocumentTypeOptionResponse> DocumentTypeOptions { get; set; } = new();
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}

// ---------------------------------------------------------------- kriteria penilaian

public sealed class ProcurementEvaluationCriterionQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class UpsertProcurementEvaluationCriterionRequest
{
    [Required] public Guid ItemCategoryId { get; set; }

    /// <summary>Nama kriteria, unik per kategori tanpa beda huruf. Contoh: <c>Ketepatan waktu</c>.</summary>
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;

    /// <summary>Bobot persen, 0 &lt; bobot ≤ 100 (PBJ-VAL-113). Contoh: <c>40</c>.</summary>
    public decimal WeightPercent { get; set; }

    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProcurementEvaluationCriterionResponse
{
    public Guid Id { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryCode { get; set; } = string.Empty;
    public string ItemCategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal WeightPercent { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class ProcurementEvaluationCriterionOptionResponse
{
    public Guid Id { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal WeightPercent { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Total bobot kriteria aktif satu kategori. Penilaian hanya bisa dibuat bila <see cref="IsComplete"/> (PBJ-VAL-108).</summary>
public sealed class ProcurementEvaluationCategoryWeightResponse
{
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryCode { get; set; } = string.Empty;
    public string ItemCategoryName { get; set; } = string.Empty;
    public int ActiveCriterionCount { get; set; }
    public decimal ActiveWeightTotal { get; set; }

    /// <summary><c>true</c> bila total bobot aktif tepat 100.</summary>
    public bool IsComplete { get; set; }
}

public sealed class ProcurementEvaluationCriterionSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }

    /// <summary>Total bobot aktif kategori yang diminta (<c>?itemCategoryId=</c>); kosong bila tidak diminta.</summary>
    public decimal? ActiveWeightTotal { get; set; }

    /// <summary>Total bobot aktif per kategori yang punya kriteria. Contoh: Umum 100 (lengkap), Obat 80 (belum).</summary>
    public List<ProcurementEvaluationCategoryWeightResponse> Categories { get; set; } = new();
}

public sealed class ProcurementEvaluationCriterionDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementEvaluationCriterionFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public ProcurementEvaluationCriterionDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<ProcurementVendorCategoryOptionResponse> ItemCategoryOptions { get; set; } = new();
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
