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
    [Table("TrxEmployeeKpiTarget", Schema = "public")]
    public class TrxEmployeeKpiTarget : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceCycleId { get; set; }

        public Guid EmployeeGoalId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? KpiCatalogId { get; set; }

        [Required]
        [MaxLength(100)]
        public string KpiCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string KpiName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? KpiDescription { get; set; }

        public decimal Weight { get; set; } = 0m;

        public decimal? TargetValue { get; set; }

        public decimal? ActualValue { get; set; }

        [MaxLength(100)]
        public string? MeasurementUnit { get; set; }

        public decimal AchievementPercentage { get; set; } = 0m;

        [Required]
        [MaxLength(50)]
        public string ScoringMethod { get; set; } = "Percentage";

        public decimal? SelfScore { get; set; }

        public decimal? ManagerScore { get; set; }

        public decimal? FinalScore { get; set; }

        [Required]
        [MaxLength(40)]
        public string TargetStatus { get; set; } = "Active";

        public string? EvidenceJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public TrxEmployeeGoal? EmployeeGoal { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstKpiCatalog? KpiCatalog { get; set; }
    }
}
