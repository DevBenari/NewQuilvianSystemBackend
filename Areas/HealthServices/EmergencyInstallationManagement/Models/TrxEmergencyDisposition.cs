using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("TrxEmergencyDisposition", Schema = "public")]
    public class TrxEmergencyDisposition : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid DispositionTypeId { get; set; }

        public EmergencyDispositionStatus DispositionStatus { get; set; }
            = EmergencyDispositionStatus.Draft;

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

        public Guid? DecidedByDoctorId { get; set; }

        public Guid? ConfirmedByUserId { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public DateTime? ExecutedAt { get; set; }

        public Guid? DestinationServiceUnitId { get; set; }

        [MaxLength(250)]
        public string? DestinationFacilityName { get; set; }

        [MaxLength(100)]
        public string? ReferralNumber { get; set; }

        [MaxLength(2000)]
        public string? DispositionReason { get; set; }

        [MaxLength(2000)]
        public string? PatientConditionAtDisposition { get; set; }

        [MaxLength(2000)]
        public string? FollowUpInstruction { get; set; }

        [MaxLength(1000)]
        public string? RefusalReason { get; set; }

        public bool IsPatientDeceased { get; set; }

        public DateTime? DeathDateTime { get; set; }

        [MaxLength(250)]
        public string? DeathLocation { get; set; }

        [MaxLength(1000)]
        public string? SuspectedCauseOfDeath { get; set; }

        public bool IsVisumRequested { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmergencyVisit? EmergencyVisit { get; set; }

        public MstEmergencyDispositionType? DispositionType { get; set; }

        public MstDoctor? DecidedByDoctor { get; set; }

        public ApplicationUser? ConfirmedByUser { get; set; }

        public MstServiceUnit? DestinationServiceUnit { get; set; }
    }
}
