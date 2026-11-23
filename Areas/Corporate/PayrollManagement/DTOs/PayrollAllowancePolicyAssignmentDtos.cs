using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

/// <summary>
/// Jenis tunjangan menurut cara pembayarannya (PAY-BE-FIX-021). Dipakai Frontend apa adanya; tidak diturunkan ulang di klien.
/// </summary>
public static class WorkforceAllowanceKinds
{
    /// <summary>Tunjangan rutin berbasis kebijakan (Makan, Jabatan, Komunikasi, ...): dapat ditetapkan dan dibayar per periode.</summary>
    public const string RecurringPolicy = "RecurringPolicy";

    /// <summary>Shift, Siaga, Risiko: dibayar hanya bila aktivitas aktual terpenuhi. Pemilihan tidak pernah menjadi perintah bayar.</summary>
    public const string Activity = "Activity";

    /// <summary>Transportasi: dapat ditetapkan lewat alur umum (PAY-BE-FIX-022) dan dibayar mesin hitung bila bersumber kebijakan umum.</summary>
    public const string TransportPolicy = "TransportPolicy";

    /// <summary>Penghasilan insidental (Bonus, Insentif, Honor tugas tambahan): tetap lewat Payroll Run → Penghasilan Variabel.</summary>
    public const string Variable = "Variable";

    /// <summary>Master bermetode input manual per periode.</summary>
    public const string Manual = "Manual";
}

/// <summary>Sumber kebijakan pada daftar kebijakan.</summary>
public static class WorkforceAllowancePolicySources
{
    public const string General = "General";
    public const string TransportLegacy = "TransportLegacy";
}

/// <summary>
/// Satu jenis tunjangan pada dropdown pertama. Tiga konsep sengaja dipisah (bukan satu boolean): <c>Selectable</c> dapat dipilih,
/// <c>Assignable</c> boleh menghasilkan penetapan lewat alur kebijakan, <c>Payable</c> dihitung mesin payroll setelah ditetapkan.
/// </summary>
public sealed class WorkforceAllowanceEligibleTypeResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public bool Selectable { get; set; }
    public bool Assignable { get; set; }
    public bool Payable { get; set; }
    public bool HasPolicyForGrade { get; set; }

    /// <summary>Alasan singkat bila tidak Assignable; null bila Assignable.</summary>
    public string? Reason { get; set; }
}

public sealed class WorkforceAllowanceEligibleTypesResponse
{
    /// <summary>Grade Gaji efektif pegawai (penetapan gaji utama yang berlaku hari ini); null bila belum ada.</summary>
    public Guid? SalaryGradeId { get; set; }
    public string? SalaryGradeName { get; set; }
    public List<WorkforceAllowanceEligibleTypeResponse> Items { get; set; } = new();
}

public sealed class WorkforceAllowancePolicyOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = WorkforceAllowancePolicySources.General;
    public string CalculationMethod { get; set; } = string.Empty;
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public bool Assignable { get; set; }
    public string? Reason { get; set; }
}

public sealed class WorkforceAllowancePolicyOptionsResponse
{
    public Guid AllowanceTypeId { get; set; }
    public Guid? SalaryGradeId { get; set; }
    public string? SalaryGradeName { get; set; }
    public List<WorkforceAllowancePolicyOptionResponse> Items { get; set; } = new();
}

/// <summary>
/// Hasil resolusi backend untuk satu pasangan jenis + kebijakan pada pegawai ini. Semua nilai dihitung ulang server; klien tidak mengirim
/// nominal, tanggal, metode, atau grade. Nominal, persen, dan batas null bagi pemanggil tanpa ViewAmount.
/// </summary>
public sealed class WorkforceAllowancePreviewResponse
{
    public Guid AllowanceTypeId { get; set; }
    public Guid AllowancePolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public Guid SalaryGradeId { get; set; }
    public string? SalaryGradeName { get; set; }
    public string CalculationMethod { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public bool AmountsVisible { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsProrated { get; set; }
    public bool RequiresApproval { get; set; }
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }

    /// <summary>True bila tidak ada satu pun batas akhir dari kebijakan, tarif, jenis tunjangan, maupun penetapan gaji. Eksplisit, bukan asumsi.</summary>
    public bool IsOpenEnded { get; set; }

    /// <summary>Sumber tiap batas tanggal (mis. "Kebijakan", "Tarif Grade", "Jenis Tunjangan", "Penetapan Gaji").</summary>
    public string EffectiveStartSource { get; set; } = string.Empty;
    public string? EffectiveEndSource { get; set; }
}

/// <summary>Hanya identitas. Nominal, metode, grade, dan tanggal selalu diambil ulang oleh backend (klien tidak dipercaya).</summary>
public sealed class CreateWorkforceAllowanceFromPolicyRequest
{
    [Required]
    public Guid AllowanceTypeId { get; set; }

    [Required]
    public Guid AllowancePolicyId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
