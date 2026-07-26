using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxCandidateHiring", Schema = "public")]
    public class TrxCandidateHiring : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string HiringNumber { get; set; } = string.Empty;

        [Required]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid CandidateApplicationId { get; set; }

        [Required]
        public Guid JobOfferId { get; set; }

        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }

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
        public Guid? EmploymentStatusId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? WorkerSourceId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public DateTime HireDate { get; set; }
        public DateTime? OnboardingStartDate { get; set; }
        public DateTime? EmploymentStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        [MaxLength(30)]
        public string HiringStatus { get; set; } = "Prepared";
        // Prepared, ProfileCreated, EmployeeCreated, OnboardingStarted, Completed, Cancelled.

        public Guid? ProcessedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(1500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxCandidate? Candidate { get; set; }
        public TrxCandidateApplication? CandidateApplication { get; set; }
        public TrxJobOffer? JobOffer { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstWorkforceType? WorkforceType { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstEmploymentStatus? EmploymentStatus { get; set; }
        public MstContractType? ContractType { get; set; }
        public MstWorkerSource? WorkerSource { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public ApplicationUser? ProcessedByUser { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }
    }
}
