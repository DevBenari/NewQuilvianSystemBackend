using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxReturnToWorkAssessment", Schema = "public")]
    public class TrxReturnToWorkAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? EmployeeInjuryId { get; set; }
        public Guid? LeaveRequestId { get; set; }
        public Guid? MedicalExaminationId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }

        [Required]
        [MaxLength(60)]
        public string AssessmentNumber { get; set; } = string.Empty;

        public DateTime AssessmentDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string FitnessStatus { get; set; } = "ReviewRequired";

        public bool IsPhasedReturn { get; set; } = false;
        public DateTime? PhasedReturnStartDate { get; set; }
        public DateTime? PhasedReturnEndDate { get; set; }

        [MaxLength(1500)]
        public string? AdministrativeRestrictionSummary { get; set; }

        public bool IsSchedulingAllowed { get; set; } = false;
        public bool IsClinicalDutyAllowed { get; set; } = false;
        public DateTime? ReviewDate { get; set; }

        [MaxLength(200)]
        public string? AssessedByProvider { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [Required]
        [MaxLength(40)]
        public string AssessmentStatus { get; set; } = "Draft";

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public TrxEmployeeInjury? EmployeeInjury { get; set; }
        public WfpLeaveRequest? LeaveRequest { get; set; }
        public TrxEmployeeMedicalExamination? MedicalExamination { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
