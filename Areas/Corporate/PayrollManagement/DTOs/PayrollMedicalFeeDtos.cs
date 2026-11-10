using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollMedicalFeeQuery
{
    /// <summary>Kode periode Medical Fee, mis. 2026-10.</summary>
    public string? PeriodCode { get; set; }

    /// <summary>Salah satu <see cref="PayrollMedicalFeePathStates"/>.</summary>
    public string? PathState { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Posisi satu hak jasa medis terhadap dua jalur bayar. Tepat satu jalur aktif per sumber; tidak pernah dua.</summary>
public static class PayrollMedicalFeePathStates
{
    /// <summary>Sudah diklaim jalur PAYROLL (masuk penghasilan variabel slip).</summary>
    public const string PayrollClaimed = "PAYROLL_PATH_CLAIMED";

    /// <summary>Sudah diserahkan ke Finance AP (Created/Acknowledged). Perilaku Medical Fee saat ini untuk setiap hasil jasa yang disetujui.</summary>
    public const string ApActive = "AP_PATH_ACTIVE";

    /// <summary>Penyerahan ke Finance AP ditolak (Failed). Pemindahan jalur butuh keputusan pemilik Medical Fee/Finance.</summary>
    public const string ApFailed = "AP_PATH_FAILED";

    /// <summary>Belum punya catatan AP maupun klaim, dan penerimanya penerima payroll: dapat diklaim bila kebijakan jalur PAYROLL aktif.</summary>
    public const string NoPathYet = "NO_PATH_YET";

    /// <summary>Belum punya catatan AP dan penerimanya bukan penerima payroll (mis. dokter nonpegawai): hanya dapat lewat Finance AP.</summary>
    public const string NotPayrollPayee = "NO_PATH_NOT_PAYROLL_PAYEE";
}

public sealed class PayrollMedicalFeeCandidateResponse
{
    public Guid ServiceFeeId { get; set; }
    public string FeeNumber { get; set; } = string.Empty;
    public string MedicalFeePeriodCode { get; set; } = string.Empty;

    /// <summary>Doctor | Workforce.</summary>
    public string PayeeType { get; set; } = string.Empty;
    public string PayeeNumber { get; set; } = string.Empty;
    public string PayeeName { get; set; } = string.Empty;
    public bool IsPayrollPayee { get; set; }
    public string FeeStatus { get; set; } = string.Empty;

    /// <summary>Nilai akhir jasa kotor (sebelum PPh 21/kasbon); perhitungan sharing tetap milik Medical Fee.</summary>
    public decimal FinalAmount { get; set; }

    public string PathState { get; set; } = string.Empty;
    public string? ApHandoffStatus { get; set; }
    public string? ClaimInputNumber { get; set; }
    public string? ClaimRunNumber { get; set; }
    public string? ClaimInputStatus { get; set; }

    /// <summary>True hanya bila kebijakan jalur PAYROLL aktif dan state = NO_PATH_YET.</summary>
    public bool CanClaim { get; set; }
    public string? Explanation { get; set; }
}

public sealed class PayrollMedicalFeeCandidateListResponse
{
    /// <summary>Status kebijakan jalur PAYROLL (NEEDS CONFIRMATION). Bila false, klaim ditolak dan seluruh jasa medis dibayar lewat Finance AP.</summary>
    public bool PayrollPathEnabled { get; set; }
    public string PolicyNote { get; set; } = string.Empty;
    public PagedResult<PayrollMedicalFeeCandidateResponse> Candidates { get; set; } = new();
}

public sealed class PayrollMedicalFeeIntakeRequest
{
    /// <summary>Payroll run terbuka (Draft/CollectingInput/Calculated) tempat jasa medis akan dibayar lewat slip.</summary>
    [Required] public Guid PayrollRunId { get; set; }
    [Required, MinLength(1), MaxLength(200)] public List<Guid> ServiceFeeIds { get; set; } = new();
    [MaxLength(1000)] public string? Notes { get; set; }
}

public sealed class PayrollMedicalFeeIntakeItemResult
{
    public Guid ServiceFeeId { get; set; }
    public string FeeNumber { get; set; } = string.Empty;

    /// <summary>Diklaim | Idempoten | Ditolak.</summary>
    public string Outcome { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? InputNumber { get; set; }
}

public sealed class PayrollMedicalFeeIntakeResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public int ClaimedCount { get; set; }
    public int IdempotentCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal ClaimedAmount { get; set; }
    public List<PayrollMedicalFeeIntakeItemResult> Items { get; set; } = new();

    /// <summary>Catatan perlakuan pajak yang belum disahkan, ditampilkan apa adanya.</summary>
    public string TaxNote { get; set; } = string.Empty;
}
