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
    [Table("TrxTrainingBudget", Schema = "public")]
    public class TrxTrainingBudget : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingPlanId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? CostCenterId { get; set; }

        public int FiscalYear { get; set; }

        [Required]
        [MaxLength(40)]
        public string BudgetType { get; set; } = "TrainingPlan";

        [Required]
        [MaxLength(40)]
        public string BudgetStatus { get; set; } = "Draft";

        [Required]
        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal AllocatedAmount { get; set; } = 0m;

        public decimal CommittedAmount { get; set; } = 0m;

        public decimal UsedAmount { get; set; } = 0m;

        public decimal RemainingAmount { get; set; } = 0m;

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(2000)]
        public string? BudgetNote { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingPlan? TrainingPlan { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
