using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    [Table("WfpPerformanceReviewDetail", Schema = "public")]
    public class WfpPerformanceReviewDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceReviewId { get; set; }

        public Guid? KpiCatalogId { get; set; }

        public Guid? PerformanceTemplateDetailId { get; set; }

        [Required]
        [MaxLength(40)]
        public string DetailType { get; set; } = "KPI";

        [MaxLength(150)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? IndicatorCode { get; set; }

        [Required]
        [MaxLength(250)]
        public string IndicatorName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public decimal Weight { get; set; } = 0m;

        public decimal? TargetValue { get; set; }

        public decimal? ActualValue { get; set; }

        public decimal? SelfScore { get; set; }

        public decimal? ManagerScore { get; set; }

        public decimal? FinalScore { get; set; }

        public decimal? Score { get; set; }

        [MaxLength(100)]
        public string? Rating { get; set; }

        [MaxLength(1000)]
        public string? EvidencePath { get; set; }

        [MaxLength(3000)]
        public string? Comments { get; set; }

        public int Sequence { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public WfpPerformanceReview? PerformanceReview { get; set; }
        public MstKpiCatalog? KpiCatalog { get; set; }
        public MstPerformanceTemplateDetail? PerformanceTemplateDetail { get; set; }
    }
}
