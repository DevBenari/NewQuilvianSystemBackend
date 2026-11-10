using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Master Pustaka Indikator Mutu OPPE (6 Pilar ACGME / KARS)
    /// </summary>
    [Table("mst_oppe_metric_catalogs", Schema = "public")]
    public class MstOppeMetricCatalog : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string MetricCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// 6 Pilar ACGME/KARS: PATIENT_CARE, MEDICAL_KNOWLEDGE, PRACTICE_IMPROVEMENT, INTERPERSONAL_SKILLS, PROFESSIONALISM, SYSTEMS_PRACTICE
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CompetencyDomain { get; set; } = string.Empty;

        /// <summary>
        /// BEDAH, NON_BEDAH, ANESTESI, KEPERAWATAN, ALL
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SpecialtyGroup { get; set; } = "ALL";

        /// <summary>
        /// Sumber data: SIMRS_VOLUME, SIMRS_EMR_COMPLETION, KMKP_INCIDENT, PEER_REVIEW_SMF
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string DataSourceType { get; set; } = "SIMRS_VOLUME";

        [Column(TypeName = "decimal(8,2)")]
        public decimal TargetStandardValue { get; set; } = 85.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MetricWeightPercentage { get; set; } = 10.00m;

        public bool IsActive { get; set; } = true;
    }
}
