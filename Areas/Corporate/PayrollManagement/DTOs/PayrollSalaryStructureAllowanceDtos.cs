using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

/// <summary>Baris tunjangan tetap pada satu Struktur Gaji (PAY-BE-16, kontrak PGJ-C1). Nominal bersifat sensitif.</summary>
public class CreateSalaryStructureAllowanceRequest
{
    [Required]
    public Guid AllowanceTypeId { get; set; }

    /// <summary>FixedAmount atau PercentageOfBaseSalary.</summary>
    [Required, MaxLength(30)]
    public string CalculationMethod { get; set; } = "FixedAmount";

    /// <summary>Nominal sebulan penuh; wajib &gt; 0 untuk FixedAmount, diabaikan untuk PercentageOfBaseSalary.</summary>
    public decimal Amount { get; set; }

    /// <summary>Persen gaji pokok; wajib di atas 0 dan paling tinggi 100 untuk PercentageOfBaseSalary.</summary>
    public decimal Percentage { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateSalaryStructureAllowanceRequest : CreateSalaryStructureAllowanceRequest
{
    /// <summary>Token versi dari respons terakhir; kosong = tanpa pemeriksaan (kompatibel klien lama).</summary>
    public string? ExpectedRowVersion { get; set; }
}

public sealed class SalaryStructureAllowanceResponse
{
    public Guid Id { get; set; }
    public Guid SalaryStructureId { get; set; }
    public Guid AllowanceTypeId { get; set; }
    public string AllowanceTypeCode { get; set; } = string.Empty;
    public string AllowanceTypeName { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }

    /// <summary>Batas maksimum dari master Jenis Tunjangan; dipakai mesin hitung.</summary>
    public decimal? MaximumAmount { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsProrated { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}
