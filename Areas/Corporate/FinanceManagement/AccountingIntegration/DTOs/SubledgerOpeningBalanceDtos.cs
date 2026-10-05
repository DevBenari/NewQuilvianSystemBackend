using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;

/// <summary>
/// Data representasi satu baris saldo awal cutover subledger (BE-FIN-066, FIN-DES-088).
/// </summary>
public class OpeningBalanceResponse
{
    public Guid Id { get; set; }

    public string BalanceGroup { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateOnly CutoverDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string AccountingReferenceDocument { get; set; } = string.Empty;

    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Nama tampilan penyetuju; null bila belum disetujui atau penggunanya tidak lagi ditemukan.
    /// </summary>
    public string? ApprovedByName { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? LockedAt { get; set; }

    public Guid RowVersion { get; set; }

    public DateTime CreateDateTime { get; set; }

    public DateTime? UpdateDateTime { get; set; }
}

/// <summary>
/// Permintaan pencatatan saldo awal cutover baru (BE-FIN-066, FIN-VAL-180..182, 185).
/// </summary>
public sealed class CreateOpeningBalanceRequest
{
    /// <summary>
    /// Kelompok saldo (KAS-KASIR, KAS-KECIL, PIUTANG, UTANG-SUPPLIER, UTANG-JASA-MEDIS).
    /// </summary>
    [Required(ErrorMessage = "Kelompok saldo wajib diisi.")]
    [MaxLength(30, ErrorMessage = "Kelompok saldo maksimal 30 karakter.")]
    public required string BalanceGroup { get; set; }

    /// <summary>
    /// Nominal saldo awal cutover. Kelompok PIUTANG, UTANG-SUPPLIER, dan UTANG-JASA-MEDIS wajib bernilai 0.00.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tanggal efektif cutover peralihan sistem.
    /// </summary>
    public DateOnly CutoverDate { get; set; }

    /// <summary>
    /// Alasan tertulis penetapan saldo awal.
    /// </summary>
    [Required(ErrorMessage = "Alasan saldo awal wajib diisi.")]
    [MaxLength(500, ErrorMessage = "Alasan maksimal 500 karakter.")]
    public required string Reason { get; set; }

    /// <summary>
    /// Nomor atau nama dokumen rujukan saldo awal Accounting (misal nomor BAP / kertas kerja audit).
    /// </summary>
    [Required(ErrorMessage = "Rujukan dokumen Accounting wajib diisi.")]
    [MaxLength(200, ErrorMessage = "Rujukan dokumen maksimal 200 karakter.")]
    public required string AccountingReferenceDocument { get; set; }
}

/// <summary>
/// Permintaan koreksi saldo awal yang masih berstatus DRAFT (BE-FIN-066, FIN-VAL-181..183, 185).
/// </summary>
public sealed class UpdateOpeningBalanceRequest
{
    /// <summary>
    /// Nominal saldo awal cutover yang dikoreksi.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tanggal efektif cutover peralihan sistem.
    /// </summary>
    public DateOnly CutoverDate { get; set; }

    /// <summary>
    /// Alasan tertulis penetapan saldo awal.
    /// </summary>
    [Required(ErrorMessage = "Alasan saldo awal wajib diisi.")]
    [MaxLength(500, ErrorMessage = "Alasan maksimal 500 karakter.")]
    public required string Reason { get; set; }

    /// <summary>
    /// Nomor atau nama dokumen rujukan saldo awal Accounting.
    /// </summary>
    [Required(ErrorMessage = "Rujukan dokumen Accounting wajib diisi.")]
    [MaxLength(200, ErrorMessage = "Rujukan dokumen maksimal 200 karakter.")]
    public required string AccountingReferenceDocument { get; set; }

    /// <summary>
    /// Concurrency token untuk mencegah konflik pembaruan konkuren.
    /// </summary>
    public Guid RowVersion { get; set; }
}

/// <summary>
/// Permintaan persetujuan saldo awal cutover dari DRAFT ke APPROVED (BE-FIN-066, FIN-STATE-1.6 F.1).
/// </summary>
public sealed class ApproveOpeningBalanceRequest
{
    /// <summary>
    /// Concurrency token baris saldo awal yang akan disetujui.
    /// </summary>
    public Guid RowVersion { get; set; }
}

/// <summary>
/// Permintaan penguncian permanen saldo awal cutover dari APPROVED ke LOCKED (BE-FIN-066, FIN-VAL-184, FIN-DES-088).
/// </summary>
public sealed class LockOpeningBalanceRequest
{
    /// <summary>
    /// Concurrency token baris saldo awal yang akan dikunci.
    /// </summary>
    public Guid RowVersion { get; set; }
}
