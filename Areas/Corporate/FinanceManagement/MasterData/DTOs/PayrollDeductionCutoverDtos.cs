using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;

/// <summary>
/// Permintaan penetapan periode pertama pemotongan cicilan piutang RS yang ditangani mesin payroll otomatis
/// (BE-FIN-115, FIN-DEC-222, FR-FIN-258, FIN-API-1.11 §S.2).
/// </summary>
public sealed class SetPayrollDeductionCutoverRequest
{
    /// <summary>
    /// Periode pertama yang ditangani mesin payroll otomatis (format: YYYY-MM, contoh: "2027-01").
    /// SENGAJA TIDAK diberi [Required] agar pipeline validasi otomatis [ApiController] tidak memotong
    /// dengan 400 generik sebelum service sempat mengevaluasi dan mengembalikan pesan validasi kontrak yang tepat.
    /// </summary>
    [MaxLength(7)]
    public string AutomaticFromPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Alasan penetapan atau pembaruan periode peralihan (FIN-VAL-286, AC 4).
    /// Wajib diisi; bila kosong atau hanya spasi dijawab 422 Unprocessable Entity.
    /// SENGAJA TIDAK diberi [Required] agar tidak dipotong menjadi 400 oleh pipeline model state MVC.
    /// </summary>
    [MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>
    /// Penanda versi optimistic concurrency yang dibaca klien dari GET /current terakhir (FIN-API-1.11 §S.2).
    /// Nullable pada penetapan pertama saat belum ada baris aktif; bila baris aktif sudah ada,
    /// versi yang tidak cocok dijawab 409 Conflict.
    /// </summary>
    public Guid? ExpectedRowVersion { get; set; }
}

/// <summary>
/// Respons data konfigurasi peralihan mesin payroll (FIN-API-1.11 §S.2).
/// </summary>
public sealed class PayrollDeductionCutoverResponse
{
    public Guid Id { get; set; }

    /// <summary>
    /// Periode pertama yang ditangani mesin payroll otomatis (format: YYYY-MM).
    /// </summary>
    public string AutomaticFromPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Alasan penetapan periode peralihan.
    /// </summary>
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>
    /// Menandakan apakah konfigurasi periode ini aktif.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// ID pengguna yang menetapkan periode ini.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Nama tampilan pengguna yang menetapkan (dibaca saat menyusun respons).
    /// </summary>
    public string? CreatedByName { get; set; }

    /// <summary>
    /// Waktu penetapan konfigurasi periode ini.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Penanda versi optimistic concurrency untuk dikirim kembali saat pembaruan berikutnya.
    /// </summary>
    public Guid RowVersion { get; set; }
}

/// <summary>
/// FIN-VAL-286 / AC 4: Alasan kosong atau aturan bisnis peralihan dilanggar — MUST dijawab 422 Unprocessable Entity.
/// </summary>
public sealed class PayrollDeductionCutoverValidationException(string message) : Exception(message);

/// <summary>
/// Format periode tidak sah (bukan YYYY-MM) — MUST dijawab 400 Bad Request.
/// </summary>
public sealed class PayrollDeductionCutoverBadRequestException(string message) : Exception(message);

/// <summary>
/// ExpectedRowVersion basi atau benturan versi konkurensi — MUST dijawab 409 Conflict.
/// </summary>
public sealed class PayrollDeductionCutoverConflictException(string message) : Exception(message);

