using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models
{
    /// <summary>
    /// Satu jenis tunjangan tetap yang melekat pada satu Struktur Gaji beserta cara menghitungnya
    /// (blueprint payroll-gaji PGJ-C1, PRL-DEC-02). Pengganti angka tunggal TotalAllowance di profil payroll.
    /// Metode berbasis aktivitas (shift, siaga, risiko, transport) tidak dipasang di sini.
    /// </summary>
    [Table("MstSalaryStructureAllowance", Schema = "public")]
    public class MstSalaryStructureAllowance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SalaryStructureId { get; set; }

        [Required]
        public Guid AllowanceTypeId { get; set; }

        /// <summary>FixedAmount atau PercentageOfBaseSalary (lihat SalaryStructureAllowanceCalculationMethods).</summary>
        [Required]
        [MaxLength(30)]
        public string CalculationMethod { get; set; } = SalaryStructureAllowanceCalculationMethods.FixedAmount;

        /// <summary>Nominal sebulan penuh bila metode FixedAmount.</summary>
        public decimal Amount { get; set; } = 0m;

        /// <summary>Persen gaji pokok penetapan bila metode PercentageOfBaseSalary (0 &lt; nilai ≤ 100).</summary>
        public decimal Percentage { get; set; } = 0m;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstSalaryStructure? SalaryStructure { get; set; }
        public MstAllowanceType? AllowanceType { get; set; }
    }

    /// <summary>Cara hitung baris tunjangan struktur (PRL-DEC-13: satu metode utama per baris).</summary>
    public static class SalaryStructureAllowanceCalculationMethods
    {
        public const string FixedAmount = "FixedAmount";
        public const string PercentageOfBaseSalary = "PercentageOfBaseSalary";

        public static readonly IReadOnlyList<string> All = new[] { FixedAmount, PercentageOfBaseSalary };
    }
}
