using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("TrxManpowerPlanDetail", Schema = "public")]
    public class TrxManpowerPlanDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AnnualManpowerPlanId { get; set; }

        public Guid? PositionHeadcountPlanId { get; set; }
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
        public decimal RequestedAddition { get; set; } = 0m;
        public decimal RequestedReplacement { get; set; } = 0m;
        public decimal PlannedReduction { get; set; } = 0m;
        public decimal ApprovedHeadcount { get; set; } = 0m;

        public decimal? AverageMonthlyCost { get; set; }
        public decimal? EstimatedAnnualCost { get; set; }

        [MaxLength(20)]
        public string PriorityLevel { get; set; } = "Normal";

        [MaxLength(30)]
        public string DetailStatus { get; set; } = "Draft";

        [MaxLength(1000)]
        public string? Justification { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxAnnualManpowerPlan? AnnualManpowerPlan { get; set; }
        public MstPositionHeadcountPlan? PositionHeadcountPlan { get; set; }
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
