using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgPreArrivalReservation", Schema = "public")]
    public class EmgPreArrivalReservation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ReservationNumber { get; set; } = string.Empty;

        public Guid? PatientId { get; set; }

        public Guid? PatientMembershipId { get; set; }

        public Guid? MembershipTierId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CallerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CallerPhoneNumber { get; set; } = string.Empty;

        public DateTime EstimatedArrivalDateTime { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        public bool RequiresAmbulance { get; set; }

        public Guid? AmbulanceArrivalModeId { get; set; }

        [MaxLength(500)]
        public string? AmbulancePickupAddress { get; set; }

        [MaxLength(150)]
        public string? AssignedEscortStaffName { get; set; }

        public EmergencyPreArrivalStatus Status { get; set; } = EmergencyPreArrivalStatus.PendingArrival;

        public Guid? ResultEmergencyVisitId { get; set; }

        public DateTime? ArrivedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstPatient? Patient { get; set; }

        public MstPatientMembership? PatientMembership { get; set; }

        public MstMembershipTier? MembershipTier { get; set; }

        public EmgArrivalMode? AmbulanceArrivalMode { get; set; }

        public EmgVisit? ResultEmergencyVisit { get; set; }
    }
}
