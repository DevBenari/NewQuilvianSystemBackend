using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Master Katalog Indikator Kinerja & Rubrik Perilaku Core Values BerAKHLAK (HRD-DEC-164, VR-PRF-01..05)
    /// </summary>
    [Table("MstKpiMetricCatalog", Schema = "public")]
    public class MstKpiMetricCatalog : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string MetricCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// QUANTITATIVE_KPI, CORE_VALUES_BEHAVIOR
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ComponentType { get; set; } = "QUANTITATIVE_KPI";

        /// <summary>
        /// FINANCIAL, SERVICE_QUALITY, PATIENT_SAFETY, PEOPLE_CULTURE
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "SERVICE_QUALITY";

        /// <summary>
        /// MEDIS, KEPERAWATAN, PENUNJANG, UMUM, ALL
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TargetProfessionGroup { get; set; } = "ALL";

        /// <summary>
        /// PERSEN, MENIT, RUPIAH, JUMLAH_KASUS, SKALA_1_5
        /// </summary>
        [MaxLength(30)]
        public string? UnitOfMeasure { get; set; }

        /// <summary>
        /// POSITIVE (makin tinggi makin baik), NEGATIVE (makin rendah makin baik)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string TargetPolarity { get; set; } = "POSITIVE";

        [Column(TypeName = "decimal(15,2)")]
        public decimal? DefaultTargetValue { get; set; }

        public string? ScoringRubricJson { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
