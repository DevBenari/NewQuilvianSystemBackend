using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;

/// <summary>
/// Parameter query berpaging untuk daftar pemetaan akun control subledger (BE-FIN-065, FIN-API-1.5 F.1).
/// </summary>
public sealed class ControlAccountMapPagedQuery
{
    /// <summary>
    /// Saring berdasarkan kelompok saldo (opsional).
    /// </summary>
    public string? BalanceGroup { get; set; }

    /// <summary>
    /// Saring berdasarkan status aktif (opsional).
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Pencarian teks pada ControlAccountCode, SegmentKey, atau Notes (opsional).
    /// </summary>
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// Data representasi satu baris pemetaan akun control subledger (BE-FIN-065).
/// </summary>
public class ControlAccountMapResponse
{
    public Guid Id { get; set; }

    public string BalanceGroup { get; set; } = string.Empty;

    public string? SegmentKey { get; set; }

    public string ControlAccountCode { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreateDateTime { get; set; }

    public DateTime? UpdateDateTime { get; set; }
}

/// <summary>
/// Permintaan pembuatan satu pemetaan akun control subledger (BE-FIN-065, FIN-VAL-172..176, 179).
/// </summary>
public sealed class CreateControlAccountMapRequest
{
    /// <summary>
    /// Kelompok saldo (KAS-KASIR, KAS-KECIL, PIUTANG, UTANG-SUPPLIER, UTANG-JASA-MEDIS).
    /// </summary>
    [Required(ErrorMessage = "Kelompok saldo wajib diisi.")]
    [MaxLength(30, ErrorMessage = "Kelompok saldo maksimal 30 karakter.")]
    public required string BalanceGroup { get; set; }

    /// <summary>
    /// Segmen opsional untuk kelompok PIUTANG (PAYER, PATIENT_GUARANTOR, EMPLOYEE_BENEFIT)
    /// atau UTANG-JASA-MEDIS (DOCTOR, NURSE, OTHER_PRACTITIONER). Kosong/null berarti seluruh kelompok.
    /// </summary>
    [MaxLength(40, ErrorMessage = "Kunci segmen maksimal 40 karakter.")]
    public string? SegmentKey { get; set; }

    /// <summary>
    /// Kode akun control Chart of Accounts (COA) Accounting.
    /// </summary>
    [Required(ErrorMessage = "Kode akun control wajib diisi.")]
    [MaxLength(50, ErrorMessage = "Kode akun maksimal 50 karakter.")]
    public required string ControlAccountCode { get; set; }

    /// <summary>
    /// Catatan operasional opsional.
    /// </summary>
    [MaxLength(300, ErrorMessage = "Catatan maksimal 300 karakter.")]
    public string? Notes { get; set; }
}

/// <summary>
/// Permintaan pengubahan kode akun atau catatan pada pemetaan yang sudah ada (BE-FIN-065, FIN-VAL-175, 179).
/// </summary>
public sealed class UpdateControlAccountMapRequest
{
    /// <summary>
    /// Kode akun control Chart of Accounts (COA) Accounting yang baru.
    /// </summary>
    [Required(ErrorMessage = "Kode akun control wajib diisi.")]
    [MaxLength(50, ErrorMessage = "Kode akun maksimal 50 karakter.")]
    public required string ControlAccountCode { get; set; }

    /// <summary>
    /// Catatan operasional opsional.
    /// </summary>
    [MaxLength(300, ErrorMessage = "Catatan maksimal 300 karakter.")]
    public string? Notes { get; set; }
}

/// <summary>
/// Permintaan penonaktifan pemetaan akun control (BE-FIN-065).
/// </summary>
public sealed class DeactivateControlAccountMapRequest
{
    /// <summary>
    /// Alasan penonaktifan pemetaan.
    /// </summary>
    [MaxLength(300, ErrorMessage = "Alasan penonaktifan maksimal 300 karakter.")]
    public string? Reason { get; set; }
}

/// <summary>
/// Hasil analisis kelengkapan cakupan pemetaan akun control subledger (BE-FIN-065, FIN-DES-080).
/// </summary>
public sealed class ControlAccountCoverageResponse
{
    /// <summary>
    /// Menandakan apakah seluruh 5 kelompok saldo telah terpetakan lengkap tanpa celah.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Daftar item spesifik yang belum memiliki pemetaan aktif.
    /// </summary>
    public List<UnmappedControlAccountItem> UnmappedItems { get; set; } = new();

    /// <summary>
    /// Ringkasan status pemetaan per kelompok saldo.
    /// </summary>
    public List<ControlAccountGroupCoverageSummary> GroupSummaries { get; set; } = new();
}

/// <summary>
/// Rincian kelompok dan segmen yang belum terpetakan.
/// </summary>
public sealed class UnmappedControlAccountItem
{
    public string BalanceGroup { get; set; } = string.Empty;

    public string? SegmentKey { get; set; }

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Ringkasan status pemetaan untuk satu kelompok saldo.
/// </summary>
public sealed class ControlAccountGroupCoverageSummary
{
    public string BalanceGroup { get; set; } = string.Empty;

    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Mode pemetaan yang terdeteksi: "OVERALL", "SEGMENTED", atau "UNMAPPED".
    /// </summary>
    public string MappingMode { get; set; } = string.Empty;

    public bool IsComplete { get; set; }

    public int ActiveMappingsCount { get; set; }

    public List<string> MissingSegments { get; set; } = new();
}
