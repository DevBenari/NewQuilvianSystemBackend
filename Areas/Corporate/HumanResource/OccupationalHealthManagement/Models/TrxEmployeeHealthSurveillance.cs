using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxEmployeeHealthSurveillance", Schema = "public")]
    public class TrxEmployeeHealthSurveillance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? OccupationalExposureId { get; set; }
        public Guid? MedicalExaminationId { get; set; }

        [Required]
        [MaxLength(60)]
        public string SurveillanceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string SurveillanceProgram { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RiskFactor { get; set; }

        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? NextSurveillanceDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string SurveillanceStatus { get; set; } = "Scheduled";

        [MaxLength(1000)]
        public string? AdministrativeOutcome { get; set; }

        [MaxLength(4000)]
        public string? ClinicalFindingsRestricted { get; set; }

        public DateTime? ReminderSentAt { get; set; }
        public bool IsCompliant { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public TrxOccupationalExposure? OccupationalExposure { get; set; }
        public TrxEmployeeMedicalExamination? MedicalExamination { get; set; }
    }
}
