using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;

/// <summary>
/// Permintaan unggah bukti pembayaran langsung (BE-FIN-075, FIN-API-1.6 F.3, multipart/form-data).
/// </summary>
public sealed class UploadTransactionProofRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;

    /// <summary>Misalnya BUKTI-TRANSFER, KUITANSI. Daftar tidak tertutup (FinTransactionProofTypes).</summary>
    [Required, MaxLength(30)]
    public string ProofType { get; set; } = string.Empty;
}

/// <summary>Metadata bukti pembayaran (BE-FIN-075, FIN-API-1.6 F.3). Tidak memuat StoredFileName/RelativePath — detail penyimpanan internal.</summary>
public sealed class TransactionProofResponse
{
    public Guid Id { get; set; }
    public string ProofType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}

/// <summary>
/// Kegagalan validasi unggah bukti yang MUST dijawab 400 (FIN-VAL-214..218, 221 — FIN-DES-092 §M.3
/// pemeriksaan 1-6 dan 8).
/// </summary>
public sealed class FinanceTransactionProofValidationException(string message) : Exception(message);

/// <summary>Ukuran berkas melewati FinanceManagement:TransactionProof:MaxFileSizeBytes — MUST dijawab 413 (FIN-VAL-219).</summary>
public sealed class FinanceTransactionProofTooLargeException(string message) : Exception(message);

/// <summary>
/// FinanceManagement:TransactionProof:MaxFileSizeBytes belum dikonfigurasi — MUST dijawab 503,
/// fail-closed, BUKAN dianggap tak terbatas (FIN-VAL-220, FIN-OQ-082).
/// </summary>
public sealed class FinanceTransactionProofNotConfiguredException(string message) : Exception(message);
