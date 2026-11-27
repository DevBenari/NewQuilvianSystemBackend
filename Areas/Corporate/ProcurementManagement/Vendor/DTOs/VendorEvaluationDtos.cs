using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;

// =====================================================================================
// BE-PBJ-VD-008 — DTO penilaian vendor berkala (PBJ-VD-API 1.0 §Vendor Evaluation, STATE bagian 4).
// Satu penilaian = vendor × kategori × periode (PBJ-DEC-059); kriteria disalin saat dibuat (PBJ-DEC-050).
// =====================================================================================

public sealed class VendorEvaluationQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }

    /// <summary>Cari kode atau nama vendor.</summary>
    public string? Search { get; set; }

    public Guid? SupplierId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public int? PeriodYear { get; set; }

    /// <summary><c>SEMESTER</c>, <c>QUARTER</c>, <c>YEAR</c>.</summary>
    [MaxLength(10)] public string? PeriodType { get; set; }

    public int? PeriodNo { get; set; }

    /// <summary><c>DRAFT</c>, <c>FINAL</c>, <c>SUPERSEDED</c>, <c>CANCELLED</c>.</summary>
    [MaxLength(12)] public string? Status { get; set; }

    /// <summary><c>CONTINUE</c>, <c>RESTRICT</c>, <c>BLOCK</c>.</summary>
    [MaxLength(10)] public string? Recommendation { get; set; }

    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class VendorEvaluationSummaryQuery
{
    public Guid? ItemCategoryId { get; set; }
    public int? PeriodYear { get; set; }
    [MaxLength(10)] public string? PeriodType { get; set; }
    public int? PeriodNo { get; set; }
}

/// <summary>
/// Buat draf penilaian. Contoh: <c>{ "supplierId": "…", "itemCategoryId": "…Alkes…", "periodYear": 2027,
/// "periodType": "SEMESTER", "periodNo": 1 }</c>. Server menyalin kriteria aktif kategori itu.
/// </summary>
public sealed class CreateVendorEvaluationRequest
{
    [Required] public Guid SupplierId { get; set; }
    [Required] public Guid ItemCategoryId { get; set; }
    [Range(2000, 2100)] public int PeriodYear { get; set; }

    /// <summary>Bawaan <c>SEMESTER</c> (G-V05).</summary>
    [MaxLength(10)] public string? PeriodType { get; set; }

    /// <summary>Semester 1–2, triwulan 1–4, tahun 1.</summary>
    public int PeriodNo { get; set; }

    [MaxLength(2000)] public string? Notes { get; set; }
}

public sealed class VendorEvaluationScoreInput
{
    [Required] public Guid CriterionId { get; set; }

    /// <summary>1–5; kosong = belum dinilai.</summary>
    public int? Score { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}

/// <summary>Isi nilai, rekomendasi, catatan draf. Contoh nilai: Ketepatan waktu 4, Kualitas 5, Layanan purnajual 3.</summary>
public sealed class UpdateVendorEvaluationRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    public List<VendorEvaluationScoreInput> Scores { get; set; } = new();

    /// <summary><c>CONTINUE</c> (Lanjutkan), <c>RESTRICT</c> (Batasi), <c>BLOCK</c> (Blokir) — hanya saran (PBJ-DEC-048).</summary>
    [MaxLength(10)] public string? Recommendation { get; set; }

    [MaxLength(2000)] public string? Notes { get; set; }
}

public sealed class FinalizeVendorEvaluationRequest
{
    /// <summary>Opsional; bila dikirim harus sama dengan versi terkini (PBJ-VAL-090).</summary>
    public Guid? ExpectedRowVersion { get; set; }
}

public sealed class ReviseVendorEvaluationRequest
{
    /// <summary>Wajib, minimal 10 karakter (PBJ-VAL-104). Contoh: "Nilai ketepatan waktu salah input."</summary>
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class CancelVendorEvaluationRequest
{
    public Guid? ExpectedRowVersion { get; set; }

    /// <summary>Wajib, minimal 10 karakter (PBJ-VAL-104).</summary>
    [MaxLength(1000)] public string? Reason { get; set; }
}

public sealed class VendorEvaluationScoreResponse
{
    public Guid Id { get; set; }
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = string.Empty;
    public decimal WeightPercent { get; set; }
    public int? Score { get; set; }
    public string? Note { get; set; }
}

public sealed class VendorEvaluationListItemResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public int PeriodNo { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public decimal? FinalScore { get; set; }
    public string? Recommendation { get; set; }
    public string? RecommendationLabel { get; set; }
    public bool IsRevision { get; set; }
    public Guid EvaluatorUserId { get; set; }
    public string? EvaluatorName { get; set; }
    public string? FinalizedByName { get; set; }
    public DateTimeOffset? FinalizedAt { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class VendorEvaluationResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public Guid VendorProfileId { get; set; }
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public int PeriodNo { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;

    /// <summary>Rata-rata berbobot dihitung server, dua desimal. Contoh 40/40/20 × 4/5/3 → 4,20.</summary>
    public decimal? FinalScore { get; set; }

    public string? Recommendation { get; set; }
    public string? RecommendationLabel { get; set; }
    public string? Notes { get; set; }

    /// <summary>Penilaian asal bila ini revisi.</summary>
    public Guid? PreviousEvaluationId { get; set; }

    public string? RevisionReason { get; set; }

    /// <summary>Revisi terbaru dari penilaian ini, bila ada.</summary>
    public Guid? LatestRevisionId { get; set; }

    /// <summary>Penilai (pembuat). Pemfinal wajib orang lain (PBJ-DEC-049).</summary>
    public Guid EvaluatorUserId { get; set; }

    public string? EvaluatorName { get; set; }
    public Guid? FinalizedByUserId { get; set; }
    public string? FinalizedByName { get; set; }
    public DateTimeOffset? FinalizedAt { get; set; }
    public Guid RowVersion { get; set; }
    public List<VendorEvaluationScoreResponse> Scores { get; set; } = new();
    public DateTime CreateDateTime { get; set; }
}

public sealed class VendorEvaluationRecommendationCountResponse
{
    public string Recommendation { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class VendorEvaluationSummaryResponse
{
    public int Draft { get; set; }
    public int Final { get; set; }
    public int Superseded { get; set; }
    public int Cancelled { get; set; }

    /// <summary>Sebaran rekomendasi pada penilaian <c>FINAL</c> (sesuai saringan periode/kategori).</summary>
    public List<VendorEvaluationRecommendationCountResponse> Recommendations { get; set; } = new();

    public decimal? AverageFinalScore { get; set; }
}

public sealed class VendorEvaluationPeriodTypeOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int MaxPeriodNo { get; set; }
}

public sealed class VendorEvaluationDefaultFilterResponse
{
    public Guid? SupplierId { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public int? PeriodYear { get; set; }
    public string? PeriodType { get; set; }
    public int? PeriodNo { get; set; }
    public string? Status { get; set; }
    public string? Recommendation { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class VendorEvaluationFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public VendorEvaluationDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<VendorValueOptionResponse> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public List<VendorCategoryOptionResponse> ItemCategoryOptions { get; set; } = new();
    public List<VendorEvaluationPeriodTypeOptionResponse> PeriodTypeOptions { get; set; } = new();
    public List<VendorValueOptionResponse> StatusOptions { get; set; } = new();
    public List<VendorValueOptionResponse> RecommendationOptions { get; set; } = new();
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}
