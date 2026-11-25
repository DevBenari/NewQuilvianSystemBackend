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

    /// <summary>Dokumen asal angka. Boleh kosong pada draf, wajib sebelum diajukan.</summary>
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
