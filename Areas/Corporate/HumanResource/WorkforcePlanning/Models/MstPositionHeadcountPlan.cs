using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("MstPositionHeadcountPlan", Schema = "public")]
    public class MstPositionHeadcountPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PlanCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PlanName { get; set; } = string.Empty;

        public int PlanYear { get; set; }

        [Required]
        public Guid LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? EmployeeGradeId { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? CostCenterId { get; set; }

        public decimal CurrentHeadcount { get; set; } = 0m;
        public decimal BudgetedHeadcount { get; set; } = 0m;
        public decimal TargetHeadcount { get; set; } = 0m;
        public decimal PlannedAddition { get; set; } = 0m;
        public decimal PlannedReplacement { get; set; } = 0m;
        public decimal PlannedReduction { get; set; } = 0m;
        public decimal PlannedVacancy { get; set; } = 0m;

        public decimal? AverageMonthlyCost { get; set; }

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(30)]
        public string PlanStatus { get; set; } = "Draft";
        // Draft, Proposed, Approved, Closed, Cancelled.

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Justification { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstWorkforceType? WorkforceType { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstCostCenter? CostCenter { get; set; }
    }
}
