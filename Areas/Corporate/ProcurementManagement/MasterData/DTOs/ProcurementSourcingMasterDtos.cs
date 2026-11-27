using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;

// =====================================================================================
// BE-PBJ-PS-002 — DTO tiga master pemilihan penyedia (PBJ-PS-API 1.0, kamus data pemilihan-penyedia bagian 2).
// Permintaan ubah status memakai UpdateProcurementMasterStatusRequest; respons hapus memakai
// ProcurementMasterDeleteResponse (keduanya di ProcurementMasterDataDtos.cs).
// =====================================================================================

/// <summary>Satu pilihan berlabel (tahap dokumen, cakupan kewajiban) pada metadata saringan.</summary>
public sealed class ProcurementSourcingValueOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

// ---------------------------------------------------------------- metode pengadaan

public sealed class ProcurementMethodQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>Saring metode contoh dari seeder (PBJ-DEC-069). Contoh: <c>true</c> → metode yang wajib diganti SOP.</summary>
    public bool? IsSample { get; set; }

    public bool? RequiresTechnicalEvaluation { get; set; }

    /// <summary><c>hps</c> (bawaan: batas bawah HPS, tanpa batas bawah lebih dulu), <c>code</c>, <c>name</c>, <c>isActive</c>.</summary>
    public string SortBy { get; set; } = "hps";

    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Isi form metode pengadaan. Metode berlaku bila <c>MinHpsAmount &lt; HPS ≤ MaxHpsAmount</c>; batas kosong = tanpa batas
/// di sisi itu. Contoh Penunjukan Langsung: batas bawah kosong, batas atas 50.000.000, satu penawaran.
/// </summary>
public sealed class UpsertProcurementMethodRequest
{
    /// <summary>Huruf besar, angka, garis bawah; unik di antara data yang belum dihapus. Contoh: <c>PENUNJUKAN_LANGSUNG</c>.</summary>
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;

    /// <summary>Batas bawah HPS (eksklusif), termasuk PPN; ≥ 0. Kosong = tanpa batas bawah.</summary>
    public decimal? MinHpsAmount { get; set; }

    /// <summary>Batas atas HPS (inklusif), termasuk PPN; &gt; 0. Kosong = tanpa batas atas.</summary>
    public decimal? MaxHpsAmount { get; set; }

    /// <summary>Jumlah minimum penawaran sah sebelum penetapan diajukan, 1–10.</summary>
    public int MinimumQuotationCount { get; set; } = 1;

    public bool RequiresTechnicalEvaluation { get; set; }

    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProcurementMethodResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? MinHpsAmount { get; set; }
    public decimal? MaxHpsAmount { get; set; }
    public int MinimumQuotationCount { get; set; }
    public bool RequiresTechnicalEvaluation { get; set; }

    /// <summary><c>true</c> = nilai contoh dari seeder, bukan SOP (PBJ-DEC-069). Tidak dapat diubah lewat API.</summary>
    public bool IsSample { get; set; }

    public string? Description { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Jumlah dokumen wajib aktif yang menunjuk metode ini.</summary>
    public int ActiveRequirementCount { get; set; }

    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class ProcurementMethodOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? MinHpsAmount { get; set; }
    public decimal? MaxHpsAmount { get; set; }
    public int MinimumQuotationCount { get; set; }
    public bool RequiresTechnicalEvaluation { get; set; }
    public bool IsSample { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProcurementMethodSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }

    /// <summary>Metode contoh yang masih aktif; seharusnya 0 sebelum produksi (PBJ-DEC-069).</summary>
    public int ActiveSample { get; set; }

    public int RequiresTechnicalEvaluation { get; set; }
}

public sealed class ProcurementMethodDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public bool? IsSample { get; set; }
    public bool? RequiresTechnicalEvaluation { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "hps";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementMethodFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public ProcurementMethodDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}

// ---------------------------------------------------------------- jenis dokumen paket

public sealed class ProcurementPackageDocumentTypeQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }

    /// <summary><c>PREPARATION</c>, <c>QUOTATION</c>, atau <c>NEGOTIATION</c>.</summary>
    [MaxLength(20)] public string? Stage { get; set; }

    /// <summary>Saring jenis yang tertutup harga (PBJ-DEC-079). Contoh: <c>true</c> → Dasar HPS, Surat Penawaran Harga.</summary>
    public bool? IsPriceSensitive { get; set; }

    /// <summary><c>name</c> (bawaan), <c>code</c>, <c>stage</c>, <c>isActive</c>.</summary>
    public string SortBy { get; set; } = "name";

    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class UpsertProcurementPackageDocumentTypeRequest
{
    /// <summary>
    /// Huruf besar, angka, garis bawah; unik di antara data yang belum dihapus. Dipakai sebagai kategori lampiran paket,
    /// sehingga tidak dapat diganti setelah ada lampiran berkategori kode ini. Contoh: <c>KAK_TOR</c>.
    /// </summary>
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;

    /// <summary><c>PREPARATION</c> (bawaan), <c>QUOTATION</c>, atau <c>NEGOTIATION</c>.</summary>
    [Required, MaxLength(20)] public string Stage { get; set; } = "PREPARATION";

    /// <summary><c>true</c> → berkasnya tertutup bagi pengguna tanpa izin harga, termasuk evaluator (PBJ-DEC-079).</summary>
    public bool IsPriceSensitive { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class ProcurementPackageDocumentTypeResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string StageLabel { get; set; } = string.Empty;
    public bool IsPriceSensitive { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Jumlah dokumen wajib aktif yang menunjuk jenis ini.</summary>
    public int ActiveRequirementCount { get; set; }

    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class ProcurementPackageDocumentTypeOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public bool IsPriceSensitive { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProcurementPackageDocumentTypeSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int PriceSensitive { get; set; }
    public int Preparation { get; set; }
    public int Quotation { get; set; }
    public int Negotiation { get; set; }
}

public sealed class ProcurementPackageDocumentTypeDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public string? Stage { get; set; }
    public bool? IsPriceSensitive { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementPackageDocumentTypeFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public ProcurementPackageDocumentTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<ProcurementSourcingValueOptionResponse> StageOptions { get; set; } = new();
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}

// ---------------------------------------------------------------- dokumen wajib paket

public sealed class ProcurementPackageDocumentRequirementQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public Guid? MethodId { get; set; }
    public Guid? ProcurementTypeId { get; set; }

    /// <summary><c>METHOD</c> (kewajiban per metode) atau <c>PROCUREMENT_TYPE</c> (per jenis pengadaan).</summary>
    [MaxLength(20)] public string? Scope { get; set; }

    /// <summary><c>documentType</c> (bawaan), <c>scope</c>, <c>isActive</c>.</summary>
    public string SortBy { get; set; } = "documentType";

    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Isi form dokumen wajib. Tepat satu dari <see cref="MethodId"/> dan <see cref="ProcurementTypeId"/> diisi (400 bila
/// keduanya atau tidak satu pun). Contoh: KAK/TOR × jenis Jasa, atau Dasar HPS × metode Penunjukan Langsung.
/// </summary>
public sealed class UpsertProcurementPackageDocumentRequirementRequest
{
    [Required] public Guid DocumentTypeId { get; set; }
    public Guid? MethodId { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProcurementPackageDocumentRequirementResponse
{
    public Guid Id { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public string DocumentStage { get; set; } = string.Empty;
    public bool IsPriceSensitive { get; set; }

    /// <summary><c>METHOD</c> atau <c>PROCUREMENT_TYPE</c>.</summary>
    public string Scope { get; set; } = string.Empty;

    public Guid? MethodId { get; set; }
    public string? MethodCode { get; set; }
    public string? MethodName { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public string? ProcurementTypeCode { get; set; }
    public string? ProcurementTypeName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public Guid UpdateBy { get; set; }
}

public sealed class ProcurementPackageDocumentRequirementOptionResponse
{
    public Guid Id { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public Guid? MethodId { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ProcurementPackageDocumentRequirementSummaryResponse
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int MethodScope { get; set; }
    public int ProcurementTypeScope { get; set; }
}

public sealed class ProcurementPackageDocumentRequirementDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public Guid? MethodId { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public string? Scope { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "documentType";
    public string SortDirection { get; set; } = "asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ProcurementPackageDocumentRequirementFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public ProcurementPackageDocumentRequirementDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<ProcurementSortOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<ProcurementSourcingValueOptionResponse> ScopeOptions { get; set; } = new();
    public List<ProcurementPackageDocumentTypeOptionResponse> DocumentTypeOptions { get; set; } = new();
    public List<ProcurementMethodOptionResponse> MethodOptions { get; set; } = new();
    public List<ProcurementReferenceOptionResponse> ProcurementTypeOptions { get; set; } = new();
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
