using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public sealed class CreateEmergencyPreArrivalReservationRequest
    {
        public Guid? PatientId { get; set; }

        public Guid? PatientMembershipId { get; set; }

        public Guid? MembershipTierId { get; set; }

        [Required(ErrorMessage = "Nama pemanggil/penghubung wajib diisi.")]
        [MaxLength(150)]
        public string CallerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor telepon pemanggil wajib diisi.")]
        [MaxLength(50)]
        public string CallerPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Estimasi waktu tiba wajib diisi.")]
        public DateTime EstimatedArrivalDateTime { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        public bool RequiresAmbulance { get; set; }

        public Guid? AmbulanceArrivalModeId { get; set; }

        [MaxLength(500)]
        public string? AmbulancePickupAddress { get; set; }

        [MaxLength(150)]
        public string? AssignedEscortStaffName { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class UpdateEmergencyPreArrivalReservationRequest
    {
        [Required(ErrorMessage = "Nama pemanggil/penghubung wajib diisi.")]
        [MaxLength(150)]
        public string CallerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor telepon pemanggil wajib diisi.")]
        [MaxLength(50)]
        public string CallerPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Estimasi waktu tiba wajib diisi.")]
        public DateTime EstimatedArrivalDateTime { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        public bool RequiresAmbulance { get; set; }

        public Guid? AmbulanceArrivalModeId { get; set; }

        [MaxLength(500)]
        public string? AmbulancePickupAddress { get; set; }

        [MaxLength(150)]
        public string? AssignedEscortStaffName { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class CancelEmergencyPreArrivalReservationRequest
    {
        [Required(ErrorMessage = "Alasan pembatalan wajib diisi.")]
        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public sealed class CheckInEmergencyPreArrivalReservationRequest
    {
        public DateTime? ArrivalDateTime { get; set; }

        public Guid? ArrivalModeId { get; set; }

        public Guid? CaseTypeId { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(250)]
        public string? ArrivalLocation { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class EmergencyPreArrivalReservationResponse
    {
        public Guid Id { get; set; }

        public string ReservationNumber { get; set; } = string.Empty;

        public Guid? PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid? PatientMembershipId { get; set; }

        public string? MemberNumber { get; set; }

        public Guid? MembershipTierId { get; set; }

        public string? TierCode { get; set; }

        public string? TierName { get; set; }

        public int PriorityLevel { get; set; }

        public string CallerName { get; set; } = string.Empty;

        public string CallerPhoneNumber { get; set; } = string.Empty;

        public DateTime EstimatedArrivalDateTime { get; set; }

        public string? ChiefComplaint { get; set; }

        public bool RequiresAmbulance { get; set; }

        public Guid? AmbulanceArrivalModeId { get; set; }

        public string? AmbulanceArrivalModeName { get; set; }

        public string? AmbulancePickupAddress { get; set; }

        public string? AssignedEscortStaffName { get; set; }

        public EmergencyPreArrivalStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public Guid? ResultEmergencyVisitId { get; set; }

        public string? EmergencyVisitNumber { get; set; }

        public DateTime? ArrivedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancellationReason { get; set; }

        public string? Notes { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid RowVersion { get; set; }
    }
}
