using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Master pengaturan plafon restitusi benefit tahunan dan batas maksimal pemotongan upah (BE-HRD-161, BE-HRD-162, HRD-DEC-128, HRD-DEC-130).
    /// </summary>
    [Table("HrdRestitutionSetting", Schema = "public")]
    public class HrdRestitutionSetting : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string GradeLevelCode { get; set; } = "GRADE-ALL";

        [Required, MaxLength(100)]
        public string BenefitCategory { get; set; } = "KebijakanUmumPemotongan";
        // 'KebijakanUmumPemotongan', 'KacamataKerja', 'FaskesLuarDarurat', 'DinasCitoMedis'

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualLimitAmount { get; set; } = 0.00m;

        public int MinimumServiceMonthsRequired { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxDeductionPercentageOfGross { get; set; } = 50.00m; // Hard-cap 50% PP 36/2021 jo PP 49/2025

        public bool AllowEmergencyCitoCashier { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
