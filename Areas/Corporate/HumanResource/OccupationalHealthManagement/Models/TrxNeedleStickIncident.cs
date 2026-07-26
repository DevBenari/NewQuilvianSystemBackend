using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxNeedleStickIncident", Schema = "public")]
    public class TrxNeedleStickIncident : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OccupationalExposureId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }

        [Required]
        [MaxLength(60)]
        public string IncidentNumber { get; set; } = string.Empty;

        public DateTime IncidentDateTime { get; set; }

        [MaxLength(150)]
        public string? DeviceType { get; set; }
        [MaxLength(250)]
        public string? ProcedurePerformed { get; set; }
        public bool IsHollowBoreNeedle { get; set; } = false;
        public bool WasVisibleBloodPresent { get; set; } = false;
        public bool IsSourceKnown { get; set; } = false;

        [MaxLength(100)]
        public string? SourceRiskCategory { get; set; }
        [MaxLength(150)]
        public string? InjuryBodySite { get; set; }
        [MaxLength(1500)]
        public string? ImmediateTreatment { get; set; }

        public bool PostExposureProphylaxisRecommended { get; set; } = false;
        public bool PostExposureProphylaxisStarted { get; set; } = false;
        public DateTime? FollowUpDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string IncidentStatus { get; set; } = "Reported";

        [MaxLength(4000)]
        public string? ConfidentialClinicalNotes { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxOccupationalExposure? OccupationalExposure { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
    }
}
