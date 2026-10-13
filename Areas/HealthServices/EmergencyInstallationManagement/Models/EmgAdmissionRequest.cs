using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgAdmissionRequest", Schema = "public")]
    public class EmgAdmissionRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid DispositionId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        public EmergencyAdmissionRequestStatus Status { get; set; } = EmergencyAdmissionRequestStatus.Draft;

        public Guid? RequestedServiceUnitId { get; set; }

        public Guid? RequestedPatientClassId { get; set; }

        public Guid? ProposedInpatientDoctorId { get; set; }

        public int EstimatedLengthOfStayDays { get; set; } = 3;

        public bool IsCito { get; set; }

        public bool NeedsIntensiveCare { get; set; }

        public bool NeedsIsolation { get; set; }

        public bool NeedsOxygenSupport { get; set; }

        public bool NeedsSpecialMonitoring { get; set; }

        [MaxLength(2000)]
        public string? ClinicalIndication { get; set; }

        [MaxLength(2000)]
        public string? CarePlanInstruction { get; set; }

        public DateTime? SignatureRequestedAt { get; set; }

        public Guid? SignatureRequestedByUserId { get; set; }

        public DateTime? SignedAt { get; set; }

        public Guid? SignedByUserId { get; set; }

        public Guid? SignedByDoctorId { get; set; }

        [MaxLength(200)]
        public string? SignerNameSnapshot { get; set; }

        [MaxLength(200)]
        public string? SignerPositionSnapshot { get; set; }

        [MaxLength(250)]
        public string? SignatureDeviceInfo { get; set; }

        [MaxLength(64)]
        public string? SignatureIpAddress { get; set; }

        public string? SnapshotJson { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public EmgVisit? EmergencyVisit { get; set; }

        public EmgDisposition? Disposition { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? RequestedServiceUnit { get; set; }

        public MstPatientClass? RequestedPatientClass { get; set; }

        public MstDoctor? ProposedInpatientDoctor { get; set; }

        public MstDoctor? SignedByDoctor { get; set; }

        public ICollection<EmgAdmissionRequestEvent> Events { get; set; }
            = new List<EmgAdmissionRequestEvent>();
    }
}
