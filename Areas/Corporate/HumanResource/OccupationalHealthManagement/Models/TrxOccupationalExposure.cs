using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxOccupationalExposure", Schema = "public")]
    public class TrxOccupationalExposure : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? MedicalExaminationId { get; set; }

        [Required]
        [MaxLength(60)]
        public string ExposureNumber { get; set; } = string.Empty;

        public DateTime ExposureDateTime { get; set; }

        [Required]
        [MaxLength(80)]
        public string ExposureType { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? ExposureLocation { get; set; }
        [MaxLength(100)]
        public string? SourceCategory { get; set; }
        [MaxLength(100)]
        public string? ExposureRoute { get; set; }
        [MaxLength(150)]
        public string? BodySite { get; set; }

        [MaxLength(1000)]
        public string? PersonalProtectiveEquipment { get; set; }
        [MaxLength(1500)]
        public string? ImmediateAction { get; set; }

        [Required]
        [MaxLength(30)]
        public string RiskLevel { get; set; } = "PendingAssessment";

        [Required]
        [MaxLength(40)]
        public string ExposureStatus { get; set; } = "Reported";

        [MaxLength(4000)]
        public string? ClinicalNotesRestricted { get; set; }

        public bool FollowUpRequired { get; set; } = true;
        public DateTime? FollowUpDate { get; set; }
        public bool IsReportableIncident { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstDepartment? Department { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public TrxEmployeeMedicalExamination? MedicalExamination { get; set; }

        public ICollection<TrxNeedleStickIncident> NeedleStickIncidents { get; set; } = new List<TrxNeedleStickIncident>();
        public ICollection<TrxEmployeeHealthSurveillance> HealthSurveillanceRecords { get; set; } = new List<TrxEmployeeHealthSurveillance>();
    }
}
