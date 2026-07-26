using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxEmployeeMedicalExamination", Schema = "public")]
    public class TrxEmployeeMedicalExamination : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? HealthRecordId { get; set; }

        [Required]
        [MaxLength(60)]
        public string ExaminationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string ExaminationType { get; set; } = "PeriodicMedicalCheckup";

        public DateTime ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(200)]
        public string? ProviderName { get; set; }
        [MaxLength(300)]
        public string? ProviderLocation { get; set; }

        [Required]
        [MaxLength(40)]
        public string AdministrativeStatus { get; set; } = "Scheduled";

        [MaxLength(40)]
        public string? FitnessResult { get; set; }
        public DateTime? NextExaminationDate { get; set; }

        public bool IsMandatory { get; set; } = false;
        public bool IsClinicalDataRestricted { get; set; } = true;

        [MaxLength(200)]
        public string? ClinicalRecordReference { get; set; }

        [MaxLength(1500)]
        public string? AdministrativeNotes { get; set; }

        public DateTime? ReminderSentAt { get; set; }
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstDepartment? Department { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public WfpHealthRecord? HealthRecord { get; set; }
    }
}
