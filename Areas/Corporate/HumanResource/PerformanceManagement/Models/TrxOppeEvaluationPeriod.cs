using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Tabel Periode Evaluasi OPPE Berkala (Semesteran / Tahunan)
    /// </summary>
    [Table("trx_oppe_evaluation_periods", Schema = "public")]
    public class TrxOppeEvaluationPeriod : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PeriodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PeriodName { get; set; } = string.Empty;

        public int EvaluationYear { get; set; }

        /// <summary>
        /// 1: Semester 1, 2: Semester 2, 0: Full Year
        /// </summary>
        public int Semester { get; set; } = 1;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// DRAFT, OPEN, EVALUATING, FINALIZED, CLOSED
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "OPEN";

        [Column(TypeName = "decimal(5,2)")]
        public decimal QuantitativeWeightPercentage { get; set; } = 60.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal QualitativeWeightPercentage { get; set; } = 40.00m;
    }
}
