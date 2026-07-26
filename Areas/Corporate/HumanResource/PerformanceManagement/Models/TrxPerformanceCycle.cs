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
    [Table("TrxPerformanceCycle", Schema = "public")]
    public class TrxPerformanceCycle : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? MasterPerformanceCycleId { get; set; }

        public Guid? PerformanceTemplateId { get; set; }

        public Guid? RatingScaleId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(60)]
        public string CycleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string CycleName { get; set; } = string.Empty;

        public int CycleYear { get; set; }

        public DateTime CycleStartDate { get; set; }

        public DateTime CycleEndDate { get; set; }

        public DateTime? GoalSettingDeadline { get; set; }

        public DateTime? SelfAssessmentDeadline { get; set; }

        public DateTime? ManagerAssessmentDeadline { get; set; }

        public DateTime? CalibrationDeadline { get; set; }

        [Required]
        [MaxLength(50)]
        public string CycleStatus { get; set; } = "Draft";

        public bool AllowEmployeeViewFinalResult { get; set; } = false;

        public bool RequireSelfAssessment { get; set; } = true;

        public bool EnablePeerFeedback { get; set; } = false;

        public int TotalEmployee { get; set; } = 0;

        public int CompletedEmployee { get; set; } = 0;

        public string? CycleConfigurationJson { get; set; }

        public string? PopulationSnapshotJson { get; set; }

        public DateTime? PublishedAt { get; set; }

        public Guid? PublishedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPerformanceCycle? MasterPerformanceCycle { get; set; }
        public MstPerformanceTemplate? PerformanceTemplate { get; set; }
        public MstPerformanceRatingScale? RatingScale { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? PublishedByUser { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }

        public ICollection<TrxEmployeeGoal> EmployeeGoals { get; set; } = new List<TrxEmployeeGoal>();
        public ICollection<TrxEmployeeKpiTarget> KpiTargets { get; set; } = new List<TrxEmployeeKpiTarget>();
        public ICollection<WfpPerformanceReview> PerformanceReviews { get; set; } = new List<WfpPerformanceReview>();
        public ICollection<TrxSelfAssessment> SelfAssessments { get; set; } = new List<TrxSelfAssessment>();
        public ICollection<TrxManagerAssessment> ManagerAssessments { get; set; } = new List<TrxManagerAssessment>();
        public ICollection<TrxCalibrationSession> CalibrationSessions { get; set; } = new List<TrxCalibrationSession>();
    }
}
