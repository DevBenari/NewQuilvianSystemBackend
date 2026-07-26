using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models
{
    [Table("MstPerformanceTemplateDetail", Schema = "public")]
    public class MstPerformanceTemplateDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PerformanceTemplateId { get; set; }

        public Guid? ParentDetailId { get; set; }

        public Guid? KpiCatalogId { get; set; }

        public Guid? CompetencyId { get; set; }

        public Guid? RatingScaleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DetailCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string DetailName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DetailType { get; set; } = "KPI";
        // Section, KPI, Competency, Behavior, Goal, Custom

        [MaxLength(1000)]
        public string? Description { get; set; }

        public decimal Weight { get; set; } = 0m;

        public decimal? TargetValue { get; set; }

        public decimal? MinimumTargetValue { get; set; }

        public decimal? MaximumTargetValue { get; set; }

        [MaxLength(100)]
        public string? MeasurementUnit { get; set; }

        [Required]
        [MaxLength(50)]
        public string ScoreMethod { get; set; } = "RatingScale";
        // RatingScale, PercentageAchievement, Binary, Manual, Formula

        [MaxLength(50)]
        public string? TargetDirection { get; set; }
        // HigherIsBetter, LowerIsBetter, ExactTarget, RangeTarget, Milestone

        [MaxLength(500)]
        public string? EvidenceRequirement { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool AllowEmployeeComment { get; set; } = true;

        public bool AllowReviewerComment { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstPerformanceTemplate? PerformanceTemplate { get; set; }

        public MstPerformanceTemplateDetail? ParentDetail { get; set; }

        public ICollection<MstPerformanceTemplateDetail> ChildDetails { get; set; }
            = new List<MstPerformanceTemplateDetail>();

        public MstKpiCatalog? KpiCatalog { get; set; }

        public MstCompetency? Competency { get; set; }

        public MstPerformanceRatingScale? RatingScale { get; set; }
    }
}
