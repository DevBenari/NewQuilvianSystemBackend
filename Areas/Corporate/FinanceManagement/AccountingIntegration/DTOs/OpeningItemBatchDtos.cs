using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;

/// <summary>BE-FIN-080, FIN-VAL-224: keduanya wajib — nilai di luar enum yang sah ditolak 400 di controller, bukan lewat atribut validasi otomatis.</summary>
public sealed class OpeningItemBatchTemplateQuery
{
    public string? ItemKind { get; set; }
    public string? Format { get; set; }
}

/// <summary>BE-FIN-081: format (CSV/XLSX) ditentukan dari tipe media/ekstensi berkas, BUKAN ruas terpisah yang diisi pengguna (FIN-DES-093) — karena itu ruas ini sengaja tidak ada.</summary>
public sealed class UploadOpeningItemBatchRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;

    // Sengaja TANPA [Required]: nilai di luar RECEIVABLE/SUPPLIER_PAYABLE MUST dijawab pesan dan
    // kode 400 persis milik service, bukan pesan generik validasi model otomatis [ApiController]
    // (pola sama dengan ProofId pada BE-FIN-077/078).
    public string? ItemKind { get; set; }
}

/// <summary>
/// Mengunggah ulang berkas pada batch DRAFT (state-transition-matrix.md F.2). Jenis item tidak dikirim: ia tetap
/// milik batch. Format tetap tidak ditanyakan (FIN-DES-093); `ExpectedRowVersion` wajib supaya unggah ulang tidak
/// menimpa batch yang sudah diubah orang lain.
/// </summary>
public sealed class ReuploadOpeningItemBatchRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required] public Guid ExpectedRowVersion { get; set; }
}

/// <summary>BE-FIN-081, FIN-API-1.6 F.2: ringkasan batch, tanpa rincian baris (dipakai respons POST /).</summary>
public class OpeningItemBatchResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string ItemKind { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly CutoverDate { get; set; }
    public int TotalItemCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal DeclaredAccountingOpeningAmount { get; set; }
    public string AccountingReferenceDocument { get; set; } = string.Empty;
    public string UploadedFileName { get; set; } = string.Empty;
    public string SourceFormat { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
    public Guid RowVersion { get; set; }
}

/// <summary>BE-FIN-081: hasil validasi satu baris. Sensitif — Identifier dapat memuat nama debitur atau kode supplier (R14.6).</summary>
public sealed class OpeningItemBatchRowValidationResult
{
    public int RowNumber { get; set; }
    public string? Identifier { get; set; }
    public bool IsValid { get; set; }
    public decimal? OutstandingAmount { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
}

/// <summary>BE-FIN-081, FIN-API-1.6 F.2: respons POST /{id}/validate — batch beserta hasil validasi per baris.</summary>
public sealed class OpeningItemBatchDetailResponse : OpeningItemBatchResponse
{
    public List<OpeningItemBatchRowValidationResult> Rows { get; set; } = new();
}

/// <summary>BE-FIN-082, FIN-API-1.6 F.2: daftar batch, bersaring jenis dan status.</summary>
public sealed class OpeningItemBatchPagedQuery
{
    public string? ItemKind { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>BE-FIN-082, state-transition-matrix.md F.2: boleh dipanggil dari DRAFT atau VALIDATED, status batch tidak berubah.</summary>
public sealed class DeclareAccountingOpeningRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }

    // Sengaja TANPA [Range]: nol/negatif MUST dijawab 422 ("nominal ... wajib diisi") milik
    // service, bukan 400 generik validasi model otomatis.
    public decimal DeclaredAccountingOpeningAmount { get; set; }

    [MaxLength(200)]
    public string? AccountingReferenceDocument { get; set; }
}

/// <summary>BE-FIN-082: menyetujui batch VALIDATED — melahirkan item piutang/utang beserta mutasi pembukanya dalam satu transaksi.</summary>
public sealed class ApproveOpeningItemBatchRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
}

/// <summary>BE-FIN-082, state-transition-matrix.md F.2: boleh dipanggil dari DRAFT atau VALIDATED, alasan wajib.</summary>
public sealed class RejectOpeningItemBatchRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }

    // Sengaja TANPA [Required]: kosong MUST dijawab 422 ("alasan ... wajib diisi") milik service.
    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
