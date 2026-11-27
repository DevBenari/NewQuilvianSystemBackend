using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.DTOs;

// =====================================================================================
// BE-PBJ-PS-005 — paket draf (PBJ-PS-API 1.0 §Procurement Package: daftar, buat, baris, detail, riwayat).
//
// Harga terbatas (PBJ-DEC-079): setiap field harga di bawah bertipe nullable dan berisi null bagi pengguna tanpa
// ProcurementPackage : ReadPrice. Penyaringnya ada di ProcurementPackageService, bukan di layar.
// =====================================================================================

/// <summary>
/// Saringan daftar paket (<c>GET /procurement-packages</c>; PBJ-DEC-055). Contoh: tahun 2027, jenis Barang, status
/// <c>DRAFT</c>, dibuat bulan ini.
/// </summary>
public sealed class PackageQuery
{
    /// <summary>Cari di nomor dan judul paket.</summary>
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? ProcurementTypeId { get; set; }
    public int? FiscalYear { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(20)] public string? CustomPeriod { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class PackageOptionValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/// <summary>Pilihan jenis pengadaan pada saringan daftar paket.</summary>
public sealed class PackageProcurementTypeOption
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class PackageFilterMetadataResponse
{
    public List<ProcurementPeriodOptionResponse> CustomPeriods { get; set; } = ProcurementPeriodOptionResponse.Defaults();
    public PackageQuery DefaultFilter { get; set; } = new();
    public List<PackageOptionValue> StatusOptions { get; set; } = new();
    public List<PackageProcurementTypeOption> ProcurementTypeOptions { get; set; } = new();
    public List<int> FiscalYearOptions { get; set; } = new();
    public List<PackageOptionValue> SortOptions { get; set; } = new();
    public List<string> SortDirections { get; set; } = ["asc", "desc"];
    public List<int> PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string ResetButtonLabel { get; set; } = "Atur ulang";
}

public sealed class PackageStatusCount
{
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>Ringkasan paket dalam cakupan pengguna (<c>GET /procurement-packages/summary</c>; delta D-1 laporan).</summary>
public sealed class PackageSummaryResponse
{
    public int Total { get; set; }
    public List<PackageStatusCount> ByStatus { get; set; } = new();
}

/// <summary>
/// Satu baris daftar paket. Contoh: "Kursi Roda 2027", Barang, 2027, <c>DRAFT</c>, 1 barang dari 2 baris PR (IGD, Rawat
/// Inap), HPS Rp 43.500.000 (atau null tanpa izin harga).
/// </summary>
public sealed class PackageListItem
{
    public Guid Id { get; set; }
    public string? PackageNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ProcurementTypeId { get; set; }
    public string ProcurementTypeName { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? MethodName { get; set; }
    public int ItemCount { get; set; }
    public int SourceCount { get; set; }
    public List<string> DepartmentNames { get; set; } = new();

    /// <summary>Jumlah HPS termasuk PPN; null tanpa <c>ReadPrice</c>.</summary>
    public decimal? HpsTotal { get; set; }

    public Guid PicUserId { get; set; }
    public string? PicName { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

/// <summary>
/// Buat paket dari baris PR <c>APPROVED</c> (PBJ-DEC-071). Baris berbarang katalog sama digabung menjadi satu barang paket.
/// Contoh: <c>{ "title": "Kursi Roda 2027", "purchaseRequestLineIds": [idIgd, idRawatInap] }</c>.
/// </summary>
public sealed class CreatePackageRequest
{
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;

    /// <summary>Rujukan rencana paket (PBJ-DEC-082); bukan sumber isi paket.</summary>
    public Guid? PackagePlanId { get; set; }

    [Required, MinLength(1)] public List<Guid> PurchaseRequestLineIds { get; set; } = new();
}

/// <summary>Tambah baris PR ke paket <c>DRAFT</c>/<c>PREPARATION_RETURNED</c>.</summary>
public sealed class AddPackageLinesRequest
{
    [Required, MinLength(1)] public List<Guid> PurchaseRequestLineIds { get; set; } = new();
    [Required] public Guid ExpectedRowVersion { get; set; }
}

/// <summary>Lepas satu baris PR dari paket (<c>DELETE /{id}/sources/{sourceId}?expectedRowVersion=…</c>).</summary>
public sealed class RemovePackageSourceQuery
{
    [Required] public Guid ExpectedRowVersion { get; set; }
}

/// <summary>Satu baris PR asal barang paket, lengkap dengan unit dan sumber dananya (garis pagu, PBJ-DEC-078).</summary>
public sealed class PackageItemSourceResponse
{
    public Guid Id { get; set; }
    public Guid PurchaseRequestLineId { get; set; }
    public int PurchaseRequestLineNo { get; set; }
    public Guid PurchaseRequestId { get; set; }
    public string? RequestNumber { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid FundSourceId { get; set; }
    public string FundSourceName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    /// <summary>Harga perkiraan satuan di PR; null tanpa <c>ReadPrice</c>.</summary>
    public decimal? EstimatedUnitPrice { get; set; }

    public Guid? NeedPlanLineKey { get; set; }
}

/// <summary>
/// Satu barang katalog di paket. Contoh: Kursi Roda Standar, 15 unit (IGD 10 + Rawat Inap 5), HPS satuan Rp 2.900.000.
/// </summary>
public sealed class PackageItemResponse
{
    public Guid Id { get; set; }
    public int LineNo { get; set; }
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public Guid ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; } = string.Empty;
    public string MeasurementName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public decimal Quantity { get; set; }

    /// <summary>Butir kriteria teknis barang, urut <c>CriterionNo</c> (BE-PBJ-PS-007).</summary>
    public List<PackageCriterionResponse> Criteria { get; set; } = new();

    /// <summary>HPS satuan termasuk PPN; null tanpa <c>ReadPrice</c>.</summary>
    public decimal? HpsUnitPrice { get; set; }

    /// <summary>= round(<c>Quantity</c> × <c>HpsUnitPrice</c>, 2); null tanpa <c>ReadPrice</c>.</summary>
    public decimal? HpsAmount { get; set; }

    public List<PackageItemSourceResponse> Sources { get; set; } = new();
}

/// <summary>Detail paket. Nomor <c>PKT-…</c> baru terisi saat persiapan pertama kali diajukan (BE-PBJ-PS-008).</summary>
public sealed class PackageResponse
{
    public Guid Id { get; set; }
    public string? PackageNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ProcurementTypeId { get; set; }
    public string ProcurementTypeCode { get; set; } = string.Empty;
    public string ProcurementTypeName { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public Guid? PackagePlanId { get; set; }
    public string? PackagePlanNumber { get; set; }
    public string? PackagePlanTitle { get; set; }
    public Guid? SplitFromPackageId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public Guid? MethodId { get; set; }
    public string? MethodCode { get; set; }
    public string? MethodName { get; set; }
    public decimal? MethodMinHpsAmount { get; set; }
    public decimal? MethodMaxHpsAmount { get; set; }
    public int? MethodMinimumQuotationCount { get; set; }
    public bool MethodRequiresTechnicalEvaluation { get; set; }
    public bool IsMethodOutOfRange { get; set; }
    public string? MethodDeviationReason { get; set; }

    /// <summary>Jumlah HPS seluruh barang; null tanpa <c>ReadPrice</c>.</summary>
    public decimal? HpsTotal { get; set; }

    /// <summary>HPS yang disetujui saat persiapan (PBJ-DEC-073); null tanpa <c>ReadPrice</c>.</summary>
    public decimal? ApprovedHpsTotal { get; set; }

    /// <summary>true bila field harga pada respons ini terisi (pengguna memegang <c>ReadPrice</c>).</summary>
    public bool CanViewPrice { get; set; }

    public Guid PicUserId { get; set; }
    public string? PicName { get; set; }
    public int PreparationSubmissionCount { get; set; }
    public Guid? PreparationWorkflowInstanceId { get; set; }
    public DateTimeOffset? PreparationSubmittedAt { get; set; }
    public DateTimeOffset? PreparationApprovedAt { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public DateTimeOffset? PurchaseOrderRecordedAt { get; set; }
    public string? CancelReason { get; set; }
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public List<PackageItemResponse> Items { get; set; } = new();

    /// <summary>Checklist dokumen wajib persiapan (salinan saat metode dipilih; BE-PBJ-PS-007).</summary>
    public List<PackageChecklistItemResponse> Checklist { get; set; } = new();

    /// <summary>Evaluator teknis aktif (PBJ-DEC-074; BE-PBJ-PS-007).</summary>
    public List<PackageEvaluatorResponse> Evaluators { get; set; } = new();
}

// =====================================================================================
// BE-PBJ-PS-007 — persiapan paket: ubah persiapan, saran metode, kriteria teknis, evaluator.
// =====================================================================================

/// <summary>Satu butir kriteria teknis. Contoh: 1 "Rangka baja, beban maksimal 100 kg".</summary>
public sealed class PackageCriterionResponse
{
    public Guid Id { get; set; }
    public int CriterionNo { get; set; }
    public string CriterionText { get; set; } = string.Empty;
}

/// <summary>
/// Satu dokumen wajib persiapan. <c>IsFulfilled</c> = ada lampiran paket berlaku berkategori kode ini; <c>null</c> untuk jenis
/// dokumen harga bila pengguna tanpa <c>ReadPrice</c> (keberadaan lampiran harga tidak dibocorkan, BE-PBJ-PS-006).
/// </summary>
public sealed class PackageChecklistItemResponse
{
    public Guid Id { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsPriceSensitive { get; set; }
    public bool? IsFulfilled { get; set; }
}

public sealed class PackageEvaluatorResponse
{
    public Guid Id { get; set; }
    public Guid EvaluatorUserId { get; set; }
    public string? EvaluatorName { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string? AssignedByName { get; set; }
    public DateTime AssignedAt { get; set; }
}

/// <summary>Ubah HPS satuan dan spesifikasi satu barang paket. <c>HpsUnitPrice</c> kosong = tidak diubah.</summary>
public sealed class UpdatePackageItemPreparationRequest
{
    [Required] public Guid PackageItemId { get; set; }
    [MaxLength(2000)] public string? Specification { get; set; }

    /// <summary>Termasuk PPN (PBJ-DEC-084). Hanya pemegang <c>ProcurementPackage : ReadPrice</c> yang boleh mengisinya.</summary>
    [Range(typeof(decimal), "0", "9999999999999999")] public decimal? HpsUnitPrice { get; set; }
}

/// <summary>
/// Ubah persiapan paket <c>DRAFT</c>/<c>PREPARATION_RETURNED</c> (<c>PUT /procurement-packages/{id}</c>). <c>MethodId</c> kosong =
/// metode dikosongkan. Contoh: judul "Kursi Roda 2027", metode Penunjukan Langsung (contoh), kursi roda HPS Rp 2.850.000.
/// </summary>
public sealed class UpdatePackagePreparationRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    public Guid? MethodId { get; set; }

    /// <summary>Wajib bila HPS di luar rentang metode (<c>PBJ-VAL-126</c>).</summary>
    [MaxLength(500)] public string? MethodDeviationReason { get; set; }

    /// <summary>Barang yang diubah; barang yang tidak disebut tidak berubah.</summary>
    [MaxLength(200)] public List<UpdatePackageItemPreparationRequest> Items { get; set; } = new();
}

/// <summary>Satu metode yang rentang HPS-nya mencakup HPS paket.</summary>
public sealed class MethodSuggestionItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? MinHpsAmount { get; set; }
    public decimal? MaxHpsAmount { get; set; }
    public int MinimumQuotationCount { get; set; }
    public bool RequiresTechnicalEvaluation { get; set; }
    public bool IsSample { get; set; }
}

/// <summary>
/// Saran metode dari HPS paket (<c>GET /{id}/method-suggestion</c>; PBJ-DEC-068). Contoh: HPS Rp 42.750.000 → "Penunjukan
/// Langsung (contoh)" (≤ Rp 50.000.000). <c>HpsTotal</c> null tanpa <c>ReadPrice</c>.
/// </summary>
public sealed class MethodSuggestionResponse
{
    public decimal? HpsTotal { get; set; }
    public Guid? CurrentMethodId { get; set; }

    /// <summary>Apakah HPS berada di rentang salinan metode paket; null bila belum ada metode.</summary>
    public bool? IsCurrentMethodInRange { get; set; }

    public List<MethodSuggestionItem> Suggestions { get; set; } = new();
}

public sealed class TechnicalCriterionRequest
{
    [Required, MaxLength(500)] public string CriterionText { get; set; } = string.Empty;
}

/// <summary>Ganti seluruh kriteria teknis satu barang; urutan daftar = nomor butir 1, 2, 3, …</summary>
public sealed class UpdateTechnicalCriteriaRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(50)] public List<TechnicalCriterionRequest> Criteria { get; set; } = new();
}

/// <summary>Ganti seluruh evaluator aktif paket (PBJ-DEC-074). Daftar kosong = tanpa evaluator.</summary>
public sealed class UpdateEvaluatorsRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(20)] public List<Guid> EvaluatorUserIds { get; set; } = new();
}

// =====================================================================================
// BE-PBJ-PS-008 — persetujuan persiapan: ajukan, tarik, buka ulang.
// =====================================================================================

/// <summary>Ajukan persiapan ke <c>Wfl</c>. Header <c>Idempotency-Key</c> membuat tekan-ganda tidak mengajukan dua kali.</summary>
public sealed class SubmitPackagePreparationRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
}

/// <summary>Tarik pengajuan persiapan yang belum diputus (hanya pengaju).</summary>
public sealed class WithdrawPackagePreparationRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string? Reason { get; set; }
}

/// <summary>
/// Buka ulang persiapan paket <c>READY_FOR_RFQ</c>/<c>EVALUATION</c> agar HPS atau metode dapat diubah (PBJ-DEC-073). Alasan
/// wajib (<c>PBJ-VAL-148</c>). Contoh: "HPS perlu disesuaikan dengan daftar harga distributor terbaru".
/// </summary>
public sealed class ReopenPackagePreparationRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string? Reason { get; set; }
}
