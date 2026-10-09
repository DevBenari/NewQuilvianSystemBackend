using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;

/// <summary>
/// Parameter pencarian dan penyaringan daftar Ayat Silang.
/// </summary>
public sealed class CrossEntryQuery
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? InsuranceProviderId { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? CrossEntryNumber { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreateDateTime";
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Permintaan pembuatan dokumen Ayat Silang baru.
/// Server menentukan AvailableBalance, AllocatedAmount, dan Status (tidak diterima dari FE).
/// </summary>
public sealed class CreateCrossEntryRequest
{
    [Required(ErrorMessage = "No. Ayat Silang wajib diisi.")]
    [MaxLength(50, ErrorMessage = "No. Ayat Silang maksimal 50 karakter.")]
    public string CrossEntryNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "No. Referensi rekening koran wajib diisi.")]
    [MaxLength(150, ErrorMessage = "No. Referensi maksimal 150 karakter.")]
    public string ReferenceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Perusahaan asuransi wajib dipilih.")]
    public Guid InsuranceProviderId { get; set; }

    [Required(ErrorMessage = "Rekening bank penerima wajib dipilih.")]
    public Guid BankAccountId { get; set; }

    [Required(ErrorMessage = "Nominal pembayaran wajib diisi.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Nominal pembayaran harus lebih besar dari 0.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Tanggal transaksi bank wajib diisi.")]
    public DateOnly TransactionDate { get; set; }

    [MaxLength(1000, ErrorMessage = "Keterangan maksimal 1000 karakter.")]
    public string? Description { get; set; }
}

/// <summary>
/// Baris item daftar ringkas Ayat Silang.
/// AllocatedAmount dan AvailableBalance berasal dari FinReceipt (authoritative single source of truth).
/// </summary>
public sealed class CrossEntryListResponse
{
    public Guid Id { get; set; }
    public string CrossEntryNumber { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public Guid InsuranceProviderId { get; set; }
    public string InsuranceProviderName { get; set; } = string.Empty;
    public Guid BankAccountId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public string? Description { get; set; }
    public Guid RowVersion { get; set; }
}

/// <summary>
/// Ringkasan agregat kartu Ayat Silang (seluruh data sesuai filter).
/// </summary>
public sealed class CrossEntrySummaryResponse
{
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Riwayat mutasi Ayat Silang append-only.
/// Memetakan kolom ke format UI V1 (TransactionDateIn/CreditAmount dan TransactionDateOut/DebitAmount).
/// </summary>
public sealed class CrossEntryTransactionResponse
{
    public Guid Id { get; set; }
    public Guid CrossEntryId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid? ReceiptAllocationId { get; set; }
    public Guid? ReversalOfTransactionId { get; set; }
    public string? Description { get; set; }

    // Proyeksi ke format UI
    public DateTimeOffset? TransactionDateIn { get; set; }
    public decimal CreditAmount { get; set; }
    public DateTimeOffset? TransactionDateOut { get; set; }
    public decimal DebitAmount { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// Informasi metadata berkas dokumen lampiran Ayat Silang.
/// </summary>
public sealed class CrossEntryDocumentResponse
{
    public Guid Id { get; set; }
    public Guid CrossEntryId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? UploadedByName { get; set; }
}

/// <summary>
/// Rincian lengkap Ayat Silang beserta riwayat mutasi dan daftar dokumen pendukung.
/// </summary>
public sealed class CrossEntryDetailResponse
{
    public Guid Id { get; set; }
    public string CrossEntryNumber { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public Guid InsuranceProviderId { get; set; }
    public string InsuranceProviderName { get; set; } = string.Empty;
    public Guid BankAccountId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string? BankAccountNumber { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid RowVersion { get; set; }

    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;

    public List<CrossEntryTransactionResponse> Transactions { get; set; } = new();
    public List<CrossEntryDocumentResponse> Documents { get; set; } = new();
}

/// <summary>
/// Parameter pencarian pilihan Ayat Silang saat digunakan sebagai metode pembayaran di Invoice AR.
/// </summary>
public sealed class CrossEntryOptionQuery
{
    [Required(ErrorMessage = "InsuranceProviderId wajib diisi.")]
    public Guid InsuranceProviderId { get; set; }
    public bool OnlyAvailable { get; set; } = true;
    public string? Search { get; set; }
}

/// <summary>
/// Data pilihan Ayat Silang untuk dropdown/modal pelunasan Invoice AR.
/// </summary>
public sealed class CrossEntryOptionResponse
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string CrossEntryNumber { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableBalance { get; set; }
}

/// <summary>
/// Permintaan unggah berkas dokumen lampiran Ayat Silang.
/// </summary>
public sealed class UploadCrossEntryDocumentRequest
{
    [Required(ErrorMessage = "Nama dokumen wajib diisi.")]
    [MaxLength(200, ErrorMessage = "Nama dokumen maksimal 200 karakter.")]
    public string DocumentName { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Keterangan maksimal 1000 karakter.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Berkas dokumen wajib dilampirkan.")]
    public IFormFile File { get; set; } = null!;
}
