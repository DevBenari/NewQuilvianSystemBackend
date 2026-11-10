using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    /// <summary>
    /// Matriks kelayakan paket manfaat berdasarkan grade/level kepegawaian dan jenis ikatan kerja (BE-HRD-176, BE-HRD-178, HRD-DEC-134, FR-BEN-03, FR-BEN-04).
    /// </summary>
    [Table("HrdBenefitGradeEligibility", Schema = "public")]
    public class HrdBenefitGradeEligibility : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BenefitPlanId { get; set; }

        [Required, MaxLength(50)]
        public string GradeLevelCode { get; set; } = string.Empty;
        // 'GRADE-I' (Dokter Spesialis/Direksi), 'GRADE-II' (Perawat/Bidan), 'GRADE-III' (Penunjang Medis), 'GRADE-IV' (Umum/Admin), 'GRADE-ALL'

        [Required, MaxLength(50)]
        public string EmploymentTypeCode { get; set; } = "Permanent";
        // 'Permanent' (PKWTT), 'Contract' (PKWT), 'All'

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpecificAnnualLimit { get; set; }

        [Required, MaxLength(50)]
        public string InpatientRoomClass { get; set; } = "Kelas 1";
        // 'VVIP', 'VIP', 'Kelas 1', 'Kelas 2', 'Kelas 3' (FR-BEN-04)

        public bool IsActive { get; set; } = true;

        public MstBenefitPlan? BenefitPlan { get; set; }
    }
}
