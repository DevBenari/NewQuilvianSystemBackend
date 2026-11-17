using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Sasaran Kerja Individu Hasil Cascading Target Unit (HRD-DEC-164, PRF-B2, VR-PRF-04)
    /// </summary>
    [Table("HrdPerformanceGoal", Schema = "public")]
    public class HrdPerformanceGoal : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PerformanceCycleId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid KpiMetricCatalogId { get; set; }

        [Required]
        [MaxLength(250)]
        public string GoalDescription { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal WeightPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal TargetValue { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal? ActualValue { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AchievementPercentage { get; set; }

        /// <summary>
        /// Draft, Submitted, Approved_Locked, Cancelled
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Draft";

        public Guid? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public virtual HrdPerformanceCycle? PerformanceCycle { get; set; }
        public virtual MstWorkforceProfile? WorkforceProfile { get; set; }
        public virtual MstKpiMetricCatalog? KpiMetricCatalog { get; set; }
        public virtual MstWorkforceProfile? ApproverProfile { get; set; }

        [NotMapped] public Guid MetricCatalogId { get => KpiMetricCatalogId; set => KpiMetricCatalogId = value; }
        [NotMapped] public string TargetPolarity { get; set; } = "POSITIVE";
        [NotMapped] public Guid? ApprovedByManagerId { get => ApprovedBy; set => ApprovedBy = value; }
        [NotMapped] public bool IsLocked { get => Status == "Approved_Locked"; set => Status = value ? "Approved_Locked" : "Draft"; }

    }
}
