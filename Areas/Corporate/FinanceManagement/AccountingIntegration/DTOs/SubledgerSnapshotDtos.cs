using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;

/// <summary>
/// Konstanta default kode akun kontrol untuk integrasi subledger ke Accounting (UTANG-TEKNIS.md §4, ACC-DEC-064).
/// </summary>
public static class SubledgerControlAccountDefaults
{
    public const string CashierCash = "1-1002";      // Kas Kasir
    public const string PettyCash = "1-1003";        // Kas Kecil
    public const string Receivables = "1-2001";      // Piutang Usaha / Pasien & Penjamin
    public const string SupplierPayables = "2-1001"; // Utang Usaha / Supplier
}

/// <summary>
/// Kategori akun kontrol subledger Finance.
/// </summary>
public static class SubledgerAccountCategories
{
    public const string CashierCash = "KAS-KASIR";
    public const string PettyCash = "KAS-KECIL";
    public const string Receivables = "PIUTANG";
    public const string SupplierPayables = "UTANG-SUPPLIER";
}

/// <summary>
/// Permintaan pembuatan / regenerasi snapshot saldo subledger untuk satu periode akuntansi (BE-FIN-049).
/// </summary>
public sealed class GenerateSubledgerSnapshotsRequest
{
    /// <summary>
    /// Periode akuntansi berbentuk YYYY-MM (wajib, contoh: "2026-09").
    /// </summary>
    [Required]
    [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$", ErrorMessage = "Kode periode harus berbentuk YYYY-MM, contoh: 2026-09.")]
    public required string AccountingPeriodCode { get; set; }

    /// <summary>
    /// Override opsional kode akun kontrol Kas Kasir (default: "1-1002").
    /// </summary>
    [MaxLength(50)]
    public string? CashierControlAccountCode { get; set; }

    /// <summary>
    /// Override opsional kode akun kontrol Kas Kecil (default: "1-1003").
    /// </summary>
    [MaxLength(50)]
    public string? PettyCashControlAccountCode { get; set; }

    /// <summary>
    /// Override opsional kode akun kontrol Piutang (default: "1-2001").
    /// </summary>
    [MaxLength(50)]
    public string? ReceivableControlAccountCode { get; set; }

    /// <summary>
    /// Override opsional kode akun kontrol Utang Supplier (default: "2-1001").
    /// </summary>
    [MaxLength(50)]
    public string? PayableControlAccountCode { get; set; }

    /// <summary>
    /// Catatan operasional opsional saat penutupan saldo subledger.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Butir snapshot saldo subledger untuk satu akun kontrol.
/// </summary>
public sealed class SubledgerAccountSnapshotItemResponse
{
    public string AccountCategory { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string ControlAccountCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string EventNumber { get; set; } = string.Empty;
    public Guid OutboxEventId { get; set; }
    public string SourceTransactionId { get; set; } = string.Empty;
    public string SourceVersion { get; set; } = string.Empty;
    public DateOnly AccountingDate { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTimeOffset EventOccurredAt { get; set; }
}

/// <summary>
/// Respons eksekusi kalkulasi snapshot subledger 4 akun kontrol.
/// </summary>
public sealed class GenerateSubledgerSnapshotsResponse
{
    public string AccountingPeriodCode { get; set; } = string.Empty;
    public DateOnly AccountingDate { get; set; }
    public int TotalAccounts { get; set; }
    public decimal TotalBalance { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
    public List<SubledgerAccountSnapshotItemResponse> Items { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Respons query daftar snapshot subledger per periode.
/// </summary>
public sealed class SubledgerPeriodSnapshotsResponse
{
    public string AccountingPeriodCode { get; set; } = string.Empty;
    public DateOnly AccountingDate { get; set; }
    public bool IsComplete { get; set; }
    public decimal TotalBalance { get; set; }
    public List<SubledgerAccountSnapshotItemResponse> Items { get; set; } = new();
}
