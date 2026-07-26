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
    [Table("TrxTrainingPlan", Schema = "public")]
    public class TrxTrainingPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? TrainingCatalogId { get; set; }

        public Guid? TrainingCategoryId { get; set; }

        public Guid? MandatoryTrainingRuleId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? CostCenterId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(60)]
        public string TrainingPlanNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string TrainingPlanName { get; set; } = string.Empty;

        public int PlanYear { get; set; }

        [Required]
        [MaxLength(40)]
        public string PlanType { get; set; } = "Annual";

        [Required]
        [MaxLength(40)]
        public string PlanStatus { get; set; } = "Draft";

        public DateTime PlannedStartDate { get; set; }

        public DateTime PlannedEndDate { get; set; }

        [MaxLength(50)]
        public string? DeliveryMode { get; set; }

        [MaxLength(250)]
        public string? ProviderName { get; set; }

        [MaxLength(1000)]
        public string? TargetAudience { get; set; }

        public bool IsMandatory { get; set; } = false;

        public bool IsExternalTraining { get; set; } = false;

        public int PlannedParticipantCount { get; set; } = 0;

        public int ApprovedParticipantCount { get; set; } = 0;

        public decimal EstimatedCost { get; set; } = 0m;

        public decimal ApprovedBudget { get; set; } = 0m;

        [Required]
        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(3000)]
        public string? Objectives { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public string? PlanningSnapshotJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public MstTrainingCatalog? TrainingCatalog { get; set; }
        public MstTrainingCategory? TrainingCategory { get; set; }
        public MstMandatoryTrainingRule? MandatoryTrainingRule { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<TrxTrainingSession> Sessions { get; set; } = new List<TrxTrainingSession>();
        public ICollection<TrxTrainingEnrollmentRequest> EnrollmentRequests { get; set; } = new List<TrxTrainingEnrollmentRequest>();
        public ICollection<TrxTrainingBudget> Budgets { get; set; } = new List<TrxTrainingBudget>();
    }
}
