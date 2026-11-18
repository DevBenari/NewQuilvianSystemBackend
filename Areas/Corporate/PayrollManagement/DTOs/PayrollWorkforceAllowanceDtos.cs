using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

/// <summary>Sumber tunjangan pada layar Tunjangan Pegawai (<see cref="WorkforceAllowanceItemResponse.SourceType"/>).</summary>
public static class WorkforceAllowanceSourceTypes
{
    /// <summary>Baris tunjangan pada Struktur Gaji pegawai (MstSalaryStructureAllowance); berlaku untuk semua pemilik struktur.</summary>
    public const string SalaryStructure = "SalaryStructure";

    /// <summary>Penetapan khusus pegawai berbasis Master Tunjangan (PrlWorkforceAllowance).</summary>
    public const string Individual = "Individual";

    /// <summary>Dihitung kebijakan (transportasi); dikelola di fiturnya sendiri, bukan di layar ini.</summary>
    public const string Policy = "Policy";
}

/// <summary>Tujuan pengelolaan baris yang tidak dapat diubah dari layar Tunjangan Pegawai.</summary>
public static class WorkforceAllowanceManagementTargets
{
    public const string SalaryStructure = "SalaryStructure";
    public const string TransportAllowance = "TransportAllowance";
}

public sealed class WorkforceAllowanceQuery
{
    /// <summary>SalaryStructure | Individual | Policy; kosong berarti semua sumber.</summary>
    public string? Source { get; set; }

    /// <summary>Status baris: Draft | PendingApproval | Approved | Rejected | Inactive | Active; kosong berarti semua.</summary>
    public string? Status { get; set; }

    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// Penetapan tunjangan khusus satu pegawai. Kategori, metode hitung, mata uang, IsTaxable, IsProrated, RequiresApproval, dan batas
/// maksimum TIDAK dikirim klien: semuanya dibaca server dari Master Tunjangan. Nominal dan persen sensitif; tidak pernah ditulis ke log.
/// </summary>
public class CreateWorkforceAllowanceRequest
{
    [Required]
    public Guid AllowanceTypeId { get; set; }

    /// <summary>Nominal sebulan penuh (jenis berbasis nominal tetap). Kosong memakai DefaultAmount master. Wajib &gt; 0 dan tidak melebihi batas maksimum master.</summary>
    public decimal? Amount { get; set; }

    /// <summary>Persen gaji pokok (jenis berbasis persentase). Kosong memakai DefaultPercentage master. Wajib di atas 0 dan paling tinggi 100.</summary>
    public decimal? Percentage { get; set; }

    [Required]
    public DateTime EffectiveStartDate { get; set; }

    public DateTime? EffectiveEndDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public sealed class UpdateWorkforceAllowanceRequest : CreateWorkforceAllowanceRequest
{
    /// <summary>Token versi dari respons terakhir; kosong = tanpa pemeriksaan (kompatibel klien lama).</summary>
    public string? ExpectedRowVersion { get; set; }
}

/// <summary>Aksi perpindahan status (submit, approve, reject, deactivate). Alasan wajib untuk reject dan deactivate.</summary>
public sealed class WorkforceAllowanceActionRequest
{
    public string? ExpectedRowVersion { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>Kemampuan pemanggil (dari Akses Role) yang dipakai membentuk AvailableActions dan menyembunyikan nominal.</summary>
public sealed record WorkforceAllowanceCaller(
    bool CanViewAmount,
    bool CanUpdate,
    bool CanSubmit,
    bool CanApprove,
    bool CanReject,
    bool CanDeactivate);

/// <summary>
/// Satu baris tunjangan pegawai dari salah satu sumber. Nominal, persen, dan batas maksimum null bila pemanggil tidak berwenang
/// ViewAmount atau nilainya memang tidak tersedia (tidak pernah diganti nol).
/// </summary>
public sealed class WorkforceAllowanceItemResponse
{
    /// <summary>Id baris pada sumbernya (penetapan, baris struktur, atau tunjangan transportasi): identitas untuk audit.</summary>
    public Guid Id { get; set; }

    /// <summary>SalaryStructure | Individual | Policy (<see cref="WorkforceAllowanceSourceTypes"/>).</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>Dari Struktur Gaji | Khusus Pegawai | Berbasis Kebijakan.</summary>
    public string SourceLabel { get; set; } = string.Empty;

    public Guid? AllowanceTypeId { get; set; }
    public string AllowanceTypeCode { get; set; } = string.Empty;
    public string AllowanceTypeName { get; set; } = string.Empty;
    public string? AllowanceCategory { get; set; }

    /// <summary>FixedAmount | PercentageOfBaseSalary | PolicyBased.</summary>
    public string CalculationMethod { get; set; } = string.Empty;

    /// <summary>Draft | PendingApproval | Approved | Rejected | Inactive untuk penetapan khusus; Active | Inactive untuk struktur dan kebijakan.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Berlaku dan dibayarkan HARI INI: Approved/Active, aktif, dan masa berlakunya mencakup hari ini.</summary>
    public bool IsEffectiveNow { get; set; }

    public string CurrencyCode { get; set; } = "IDR";
    public bool IsTaxable { get; set; }
    public bool IsProrated { get; set; }
    public bool RequiresApproval { get; set; }

    public bool AmountsVisible { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? MaximumAmount { get; set; }

    /// <summary>
    /// Perkiraan sebulan penuh (bukan hasil payroll): nominal tetap, atau gaji pokok penetapan yang disetujui x persen (dibatasi
    /// maksimum). Null bila tidak tersedia, mis. berbasis kebijakan atau belum ada gaji pokok yang disetujui.
    /// </summary>
    public decimal? EstimatedMonthlyAmount { get; set; }

    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? Notes { get; set; }

    /// <summary>Sudah pernah masuk komponen payroll run; isinya tidak lagi boleh diubah dari layar ini.</summary>
    public bool UsedInPayroll { get; set; }

    /// <summary>Isinya boleh diubah dari layar ini (hanya penetapan khusus pegawai yang belum dipakai payroll).</summary>
    public bool IsEditable { get; set; }

    /// <summary>Alasan baris tidak dapat diubah dari layar ini; null bila dapat diubah.</summary>
    public string? LockReason { get; set; }

    /// <summary>Bantuan tampilan, bukan pengaman: Update | Submit | Approve | Reject | Deactivate. Setiap endpoint tetap memeriksa ulang di Backend.</summary>
    public List<string> AvailableActions { get; set; } = new();

    /// <summary>SalaryStructure | TransportAllowance (<see cref="WorkforceAllowanceManagementTargets"/>); null untuk penetapan khusus.</summary>
    public string? ManagementTarget { get; set; }

    /// <summary>Id struktur gaji bila ManagementTarget = SalaryStructure.</summary>
    public Guid? ManagementReferenceId { get; set; }

    public string RowVersion { get; set; } = string.Empty;
}

public sealed class WorkforceAllowanceSummaryResponse
{
    public Guid WorkforceProfileId { get; set; }
    public bool AmountsVisible { get; set; }

    /// <summary>Jumlah perkiraan sebulan penuh seluruh tunjangan yang berlaku hari ini; null bila tidak berwenang ViewAmount.</summary>
    public decimal? TotalEstimatedMonthlyAmount { get; set; }
    public string CurrencyCode { get; set; } = "IDR";

    /// <summary>Tunjangan yang berlaku hari ini dari semua sumber.</summary>
    public int ActiveCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int SalaryStructureCount { get; set; }
    public int IndividualCount { get; set; }
    public int PolicyCount { get; set; }

    public bool HasSalaryAssignment { get; set; }
    public bool HasTransportAllowance { get; set; }

    /// <summary>Informasi yang selalu relevan di layar, mis. tunjangan variabel per periode dimasukkan di Payroll Run.</summary>
    public List<string> Notes { get; set; } = new();
}
