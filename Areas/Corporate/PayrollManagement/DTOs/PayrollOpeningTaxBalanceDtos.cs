using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

/// <summary>
/// Isian saldo awal PPh historis (PAY-BE-FIX-016). Kunci (pegawai, tahun pajak, sumber) tidak dapat diubah sesudah dibuat; perubahan
/// angka sesudah disahkan memakai revisi (versi baru).
/// </summary>
public sealed class SavePayrollOpeningTaxBalanceRequest
{
    [Required] public Guid WorkforceProfileId { get; set; }

    /// <summary>Tahun pajak (2000 sampai tahun berjalan).</summary>
    public int TaxYear { get; set; }

    /// <summary>Bulan (1..12) yang dicakup. Tanpa duplikat; jumlah bulan diambil dari daftar ini, bukan dari jumlah baris.</summary>
    public List<int> CoveredMonths { get; set; } = new();

    /// <summary>SAME_EMPLOYER_PRE_CUTOVER | PREVIOUS_EMPLOYER. Hanya yang pertama dipakai mesin hitung.</summary>
    [Required, MaxLength(40)] public string EmployerSource { get; set; } = string.Empty;

    public decimal TaxableGross { get; set; }
    public decimal EmployerPremium { get; set; }
    public decimal PensionDeduction { get; set; }
    public decimal TaxWithheld { get; set; }

    /// <summary>Dokumen asal angka (data lama). Opsional; BUKAN syarat simpan, ajukan, maupun setujui. Tidak dikirim = nilai lama dipertahankan.</summary>
    [MaxLength(500)] public string? SourceReference { get; set; }
}

public sealed class PayrollOpeningTaxBalanceDecisionRequest
{
    /// <summary>Catatan keputusan. Wajib pada penolakan.</summary>
    [MaxLength(1000)] public string? Note { get; set; }
}

public sealed class PayrollOpeningTaxBalanceQuery
{
    public Guid? WorkforceProfileId { get; set; }
    public int? TaxYear { get; set; }
    public string? Status { get; set; }
    public string? EmployerSource { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Pencarian opsi tenaga kerja (pegawai dan dokter) untuk form saldo awal PPh (PAY-BE-FIX-035).</summary>
public sealed class PayrollOpeningTaxBalanceWorkforceQuery
{
    /// <summary>Cari nama, nomor/kode pegawai, atau nomor/kode dokter.</summary>
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// Satu opsi tenaga kerja untuk saldo awal PPh. <see cref="WorkforceProfileId"/> adalah identitas pemilik saldo (MstWorkforceProfile.Id),
/// BUKAN MstEmployee.Id maupun MstDoctor.Id. Satu profil muncul sekali walau memiliki relasi ke lebih dari satu tabel. Hanya identitas
/// tampilan; tidak ada data pribadi atau nominal pajak.
/// </summary>
public sealed class PayrollOpeningTaxBalanceWorkforceOptionResponse
{
    public Guid WorkforceProfileId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Employee | Doctor.</summary>
    public string WorkforceType { get; set; } = string.Empty;

    /// <summary>Pegawai | Dokter.</summary>
    public string WorkforceTypeLabel { get; set; } = string.Empty;

    /// <summary>Nomor pegawai (NIP) atau nomor dokter; kosong bila tidak tersedia.</summary>
    public string? Number { get; set; }

    /// <summary>NIP | Nomor Dokter.</summary>
    public string NumberLabel { get; set; } = string.Empty;

    /// <summary>Teks siap tampil: "Nama — Pegawai (NIP ...)" atau "Nama — Dokter (Nomor Dokter ...)".</summary>
    public string Label { get; set; } = string.Empty;
}

/// <summary>Aksi yang sedang boleh dilakukan pemanggil pada saldo awal ini (bantuan tampilan; setiap aksi diperiksa ulang di backend).</summary>
public sealed class PayrollOpeningTaxBalanceActionsResponse
{
    public bool CanUpdate { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanRevise { get; set; }
    public bool CanDelete { get; set; }
}

public sealed class PayrollOpeningTaxBalanceResponse
{
    public Guid Id { get; set; }
    public Guid WorkforceProfileId { get; set; }
    public string WorkforceName { get; set; } = string.Empty;
    public int TaxYear { get; set; }
    public List<int> CoveredMonths { get; set; } = new();
    public string EmployerSource { get; set; } = string.Empty;

    /// <summary>False bila pemanggil tidak punya izin ViewAmount: empat nominal di bawah ini null (bukan nol).</summary>
    public bool AmountsVisible { get; set; }
    public decimal? TaxableGross { get; set; }
    public decimal? EmployerPremium { get; set; }
    public decimal? PensionDeduction { get; set; }
    public decimal? TaxWithheld { get; set; }

    public string? SourceReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? RejectedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public string? DecisionNote { get; set; }

    /// <summary>True hanya bila saldo ini APPROVED dan bersumber SAME_EMPLOYER_PRE_CUTOVER: satu-satunya yang dipakai rekonsiliasi.</summary>
    public bool IsUsedByEngine { get; set; }

    /// <summary>Keterangan bila saldo disimpan tetapi tidak dipakai mesin (mis. pemberi kerja sebelumnya).</summary>
    public string? UsageNote { get; set; }

    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
    public PayrollOpeningTaxBalanceActionsResponse Actions { get; set; } = new();
}

/// <summary>Opsi untuk layar: kosakata status dan sumber, dan apakah penyimpanan saldo awal tersedia di database.</summary>
public sealed class PayrollOpeningTaxBalanceMetadataResponse
{
    /// <summary>False bila tabel saldo awal belum ada di database (DDL belum diterapkan); fitur dinyatakan tidak tersedia, bukan saldo nol.</summary>
    public bool StorageAvailable { get; set; }
    public List<PayrollOpeningTaxBalanceOptionResponse> StatusOptions { get; set; } = new();
    public List<PayrollOpeningTaxBalanceOptionResponse> EmployerSourceOptions { get; set; } = new();
}

public sealed class PayrollOpeningTaxBalanceOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Note { get; set; }
}

// ----------------------------------------------------------------------------------------------------------------------------------
// PAY-BE-FIX-036: impor massal saldo awal PPh. Klien menguraikan berkas CSV menjadi baris teks MENTAH; seluruh penguraian angka/bulan dan
// validasi bisnis dilakukan Backend. Nilai kosong pada kolom wajib tidak pernah dianggap nol.
// ----------------------------------------------------------------------------------------------------------------------------------

/// <summary>Batas operasional impor (keputusan implementasi; dapat disesuaikan pemilik). Satu batch = satu transaksi semua-atau-tidak-sama-sekali.</summary>
public static class OpeningTaxBalanceImportLimits
{
    public const int MaxRows = 1000;
    public const int MaxFileBytes = 2 * 1024 * 1024;
}

public sealed class PayrollOpeningTaxBalanceImportRowRequest
{
    /// <summary>Nomor baris pada berkas asli (1 = baris header) agar galat menunjuk baris yang benar.</summary>
    public int RowNumber { get; set; }

    /// <summary>NIP/kode pegawai atau nomor/kode dokter (atau ProfileCode). Dicocokkan ke WorkforceProfileId; tidak pernah dengan nama.</summary>
    [MaxLength(100)] public string? WorkforceCode { get; set; }

    /// <summary>Pegawai | Dokter | kosong. Dipakai hanya untuk membedakan kode yang ambigu.</summary>
    [MaxLength(30)] public string? WorkforceType { get; set; }

    [MaxLength(10)] public string? TaxYear { get; set; }
    [MaxLength(40)] public string? EmployerSource { get; set; }
    [MaxLength(100)] public string? CoveredMonths { get; set; }
    [MaxLength(40)] public string? TaxableGross { get; set; }
    [MaxLength(40)] public string? EmployerPremium { get; set; }
    [MaxLength(40)] public string? PensionDeduction { get; set; }
    [MaxLength(40)] public string? TaxWithheld { get; set; }
}

public sealed class PayrollOpeningTaxBalanceImportRequest
{
    public List<PayrollOpeningTaxBalanceImportRowRequest>? Rows { get; set; }

    /// <summary>Nama berkas untuk jejak audit (opsional).</summary>
    [MaxLength(200)] public string? FileName { get; set; }
}

public sealed class PayrollOpeningTaxBalanceImportIssue
{
    /// <summary>Kode stabil (mis. WORKFORCE_NOT_FOUND, AMBIGUOUS_WORKFORCE, DUPLICATE_IN_FILE, EXISTING_BALANCE, RUN_FINAL_OVERLAP).</summary>
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class PayrollOpeningTaxBalanceImportRowResult
{
    public int RowNumber { get; set; }
    public string WorkforceCode { get; set; } = string.Empty;
    public Guid? WorkforceProfileId { get; set; }
    public string WorkforceName { get; set; } = string.Empty;
    public string WorkforceType { get; set; } = string.Empty;
    public string WorkforceTypeLabel { get; set; } = string.Empty;
    public int? TaxYear { get; set; }
    public string EmployerSource { get; set; } = string.Empty;
    public List<int> CoveredMonths { get; set; } = new();

    /// <summary>Nominal hasil penguraian (null = kosong atau tidak valid, bukan nol). Hanya untuk pemanggil ber-ViewAmount.</summary>
    public decimal? TaxableGross { get; set; }
    public decimal? EmployerPremium { get; set; }
    public decimal? PensionDeduction { get; set; }
    public decimal? TaxWithheld { get; set; }
    public bool Valid { get; set; }
    public List<PayrollOpeningTaxBalanceImportIssue> Errors { get; set; } = new();

    /// <summary>Id draf yang dibuat; hanya terisi pada commit yang berhasil.</summary>
    public Guid? CreatedId { get; set; }
}

public sealed class PayrollOpeningTaxBalanceImportResult
{
    /// <summary>True hanya bila commit berhasil menyimpan seluruh baris sebagai DRAFT.</summary>
    public bool Committed { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }
    public int ConflictRows { get; set; }

    /// <summary>Jumlah baris siap diimpor: sama dengan ValidRows bila tidak ada baris bermasalah, selain itu 0 (semua atau tidak sama sekali).</summary>
    public int ReadyRows { get; set; }
    public int CreatedCount { get; set; }

    /// <summary>Identitas batch (hanya pada commit berhasil; tercatat pada audit).</summary>
    public Guid? BatchId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<PayrollOpeningTaxBalanceImportRowResult> Rows { get; set; } = new();
}

public sealed class PayrollOpeningTaxBalanceImportTemplateResponse
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Isi CSV (UTF-8). Baris berawalan # adalah petunjuk.</summary>
    public string Content { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public List<string> SourceCodes { get; set; } = new();
    public int MaxRows { get; set; }
    public int MaxFileBytes { get; set; }
}

/// <summary>Commit ditolak seluruhnya. Dibawa ke controller agar hasil per baris ikut di <c>Errors</c> respons 422.</summary>
public sealed class PayrollOpeningTaxBalanceImportException(string message, PayrollOpeningTaxBalanceImportResult result) : Exception(message)
{
    public PayrollOpeningTaxBalanceImportResult Result { get; } = result;
}
