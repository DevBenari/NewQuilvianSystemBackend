using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxEmployeeInjury", Schema = "public")]
    public class TrxEmployeeInjury : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? MedicalExaminationId { get; set; }
        public Guid? FitnessToWorkId { get; set; }

        [Required]
        [MaxLength(60)]
        public string InjuryNumber { get; set; } = string.Empty;

        public DateTime InjuryDateTime { get; set; }

        [Required]
        [MaxLength(100)]
        public string InjuryType { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? BodyPart { get; set; }
        [Required]
        [MaxLength(30)]
        public string SeverityLevel { get; set; } = "Minor";

        [MaxLength(3000)]
        public string? InjuryDescription { get; set; }
        [MaxLength(1500)]
        public string? ImmediateAction { get; set; }

        public bool IsLostTimeInjury { get; set; } = false;
        public int LostWorkDays { get; set; } = 0;
        public bool IsReportableIncident { get; set; } = false;

        [MaxLength(100)]
        public string? InsuranceClaimNumber { get; set; }
        [MaxLength(100)]
        public string? WorkCompensationClaimNumber { get; set; }

        public DateTime? ReturnToWorkDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string InjuryStatus { get; set; } = "Reported";

        [MaxLength(4000)]
        public string? ConfidentialClinicalNotes { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstDepartment? Department { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public TrxEmployeeMedicalExamination? MedicalExamination { get; set; }
        public TrxEmployeeFitnessToWork? FitnessToWork { get; set; }

        public ICollection<TrxReturnToWorkAssessment> ReturnToWorkAssessments { get; set; } = new List<TrxReturnToWorkAssessment>();
    }
}
