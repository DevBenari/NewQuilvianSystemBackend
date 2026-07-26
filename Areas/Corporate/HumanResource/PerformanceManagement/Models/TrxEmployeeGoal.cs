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
    [Table("TrxEmployeeGoal", Schema = "public")]
    public class TrxEmployeeGoal : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceCycleId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? ManagerUserId { get; set; }

        [Required]
        [MaxLength(60)]
        public string GoalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string GoalTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string GoalType { get; set; } = "Individual";

        [MaxLength(3000)]
        public string? Description { get; set; }

        public decimal Weight { get; set; } = 0m;

        public decimal? TargetValue { get; set; }

        [MaxLength(1000)]
        public string? TargetText { get; set; }

        [MaxLength(100)]
        public string? MeasurementUnit { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string GoalStatus { get; set; } = "Draft";

        public decimal ProgressPercentage { get; set; } = 0m;

        public decimal? CurrentValue { get; set; }

        [MaxLength(3000)]
        public string? ProgressNote { get; set; }

        public string? EvidenceJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(2000)]
        public string? ApprovalNote { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public ApplicationUser? ManagerUser { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<TrxEmployeeKpiTarget> KpiTargets { get; set; } = new List<TrxEmployeeKpiTarget>();
        public ICollection<TrxPerformanceCheckIn> CheckIns { get; set; } = new List<TrxPerformanceCheckIn>();
    }
}
