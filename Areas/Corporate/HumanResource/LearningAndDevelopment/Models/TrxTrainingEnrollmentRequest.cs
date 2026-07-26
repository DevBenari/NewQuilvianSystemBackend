using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    [Table("TrxTrainingEnrollmentRequest", Schema = "public")]
    public class TrxTrainingEnrollmentRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingPlanId { get; set; }

        public Guid? TrainingSessionId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? ManagerUserId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(60)]
        public string RequestNumber { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string EnrollmentType { get; set; } = "Internal";

        [Required]
        [MaxLength(50)]
        public string RequestStatus { get; set; } = "Draft";

        [MaxLength(2000)]
        public string? DevelopmentNeed { get; set; }

        [MaxLength(2000)]
        public string? RequestReason { get; set; }

        [MaxLength(250)]
        public string? ExternalProviderName { get; set; }

        [MaxLength(250)]
        public string? ExternalTrainingName { get; set; }

        public DateTime? ExternalStartDate { get; set; }

        public DateTime? ExternalEndDate { get; set; }

        public decimal RequestedCost { get; set; } = 0m;

        [Required]
        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(1000)]
        public string? AttachmentPath { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ManagerActionAt { get; set; }

        public Guid? ManagerActionByUserId { get; set; }

        [MaxLength(2000)]
        public string? ManagerActionNote { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingPlan? TrainingPlan { get; set; }
        public TrxTrainingSession? TrainingSession { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public ApplicationUser? ManagerUser { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ManagerActionByUser { get; set; }
    }
}
