namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollCompensationQuery
{
    public string? Search { get; set; }

    /// <summary>Karyawan | Dokter.</summary>
    public string? PersonType { get; set; }

    /// <summary>Active | Suspended | OnHold | Terminated (WfpPayroll.PayrollStatus).</summary>
    public string? PayrollStatus { get; set; }

    /// <summary>Siap | Bermasalah — kesiapan data penghasilan untuk dihitung.</summary>
    public string? Readiness { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Ringkasan penghasilan satu penerima payroll (karyawan atau dokter): profil penggajian, penetapan gaji yang berlaku, rekening, dan
/// kelengkapan. Identitas penerima tetap milik HRD (WorkforceProfileId); modul ini tidak membuat identitas ganda. Nominal gaji hanya
/// terisi bila pemanggil memiliki izin ViewAmount (kolom nominal null bila tidak berwenang); rekening dan NPWP selalu disamarkan.
/// </summary>
public class PayrollCompensationListItemResponse
{
    public Guid WorkforceProfileId { get; set; }
    public Guid? PayrollProfileId { get; set; }

    /// <summary>Karyawan | Dokter.</summary>
    public string PersonType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PayrollStatus { get; set; } = string.Empty;
    public bool IsPayrollEligible { get; set; }
    public string? PayrollGroupCode { get; set; }

    public Guid? SalaryAssignmentId { get; set; }
    public DateTime? SalaryEffectiveStartDate { get; set; }
    public DateTime? SalaryEffectiveEndDate { get; set; }
    public bool SalaryApproved { get; set; }
    public DateTime? SalaryApprovedAt { get; set; }

    /// <summary>True bila pemanggil boleh melihat nominal (izin ViewAmount); bila false, kolom nominal di bawah null.</summary>
    public bool AmountsVisible { get; set; }
    public decimal? BaseSalary { get; set; }
    public decimal? TotalAllowance { get; set; }

    /// <summary>Terverifikasi | Belum terverifikasi | Belum ada.</summary>
    public string BankStatus { get; set; } = string.Empty;
    public bool HasTaxProfile { get; set; }
    public bool HasInsuranceProfile { get; set; }
    public bool IsReady { get; set; }
    public List<string> Issues { get; set; } = new();
}

public sealed class PayrollCompensationSalaryHistoryResponse
{
    public Guid Id { get; set; }
    public string? SalaryStructureName { get; set; }
    public string? SalaryGradeName { get; set; }
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Null bila pemanggil tidak memiliki izin ViewAmount.</summary>
    public decimal? BaseSalary { get; set; }
}

public sealed class PayrollCompensationBankResponse
{
    public Guid Id { get; set; }
    public string? BankName { get; set; }

    /// <summary>Nomor rekening disamarkan (empat digit terakhir).</summary>
    public string AccountMasked { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public bool IsPayrollAccount { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

/// <summary>Detail penghasilan satu penerima: ringkasan ditambah riwayat penetapan gaji dan rekening (disamarkan).</summary>
public sealed class PayrollCompensationDetailResponse : PayrollCompensationListItemResponse
{
    public string? TaxStatus { get; set; }
    public bool IsNpwpRegistered { get; set; }

    /// <summary>NPWP disamarkan (lima digit terakhir); nomor lengkap tidak dikirim ke layar.</summary>
    public string? NpwpMasked { get; set; }
    public List<PayrollCompensationSalaryHistoryResponse> SalaryHistory { get; set; } = new();
    public List<PayrollCompensationBankResponse> BankAccounts { get; set; } = new();
}
