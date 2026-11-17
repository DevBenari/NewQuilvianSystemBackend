using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Tabel Rincian Skor per Metrik Klinis OPPE
    /// </summary>
    [Table("HrdOppeStaffEvaluationDetail", Schema = "public")]
    public class HrdOppeStaffEvaluationDetail : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StaffEvaluationId { get; set; }

        [ForeignKey("StaffEvaluationId")]
        public virtual HrdOppeStaffEvaluation? StaffEvaluation { get; set; }

        [Required]
        public Guid MetricCatalogId { get; set; }

        [ForeignKey("MetricCatalogId")]
        public virtual MstOppeMetricCatalog? MetricCatalog { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal TargetValue { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal ActualValue { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal ComputedScore { get; set; }

        public string? EvaluatorNotes { get; set; }

        /// <summary>
        /// Raw data sumber JSON dari SIMRS/KMKP jika berlaku
        /// </summary>
        public string? RawMetricEvidenceJson { get; set; }
    }
}
