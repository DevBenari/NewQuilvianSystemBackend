using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyTransferResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public Guid? FromServiceUnitId { get; set; }
        public Guid ToServiceUnitId { get; set; }
        public Guid? FromRoomId { get; set; }
        public Guid? ToRoomId { get; set; }
        public Guid? FromBedId { get; set; }
        public Guid? ToBedId { get; set; }
        public EmergencyTransferStatus TransferStatus { get; set; }
        public DateTime RequestedAt { get; set; }
        public Guid RequestedByUserId { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public Guid? AcceptedByUserId { get; set; }
        public DateTime? DepartedAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public Guid? SendingNurseUserId { get; set; }
        public Guid? ReceivingNurseUserId { get; set; }
        public string? TransferReason { get; set; }
        public string? HandoverSummary { get; set; }
        public string? RejectionReason { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyTransferRequest
    {
        [Required]
        public Guid EmergencyVisitId { get; set; }

        [MaxLength(50)]
        public string? TransferNumber { get; set; }

        public Guid? FromServiceUnitId { get; set; }

        [Required]
        public Guid ToServiceUnitId { get; set; }

        public Guid? FromRoomId { get; set; }

        public Guid? ToRoomId { get; set; }

        public Guid? FromBedId { get; set; }

        public Guid? ToBedId { get; set; }

        public EmergencyTransferStatus TransferStatus { get; set; } = EmergencyTransferStatus.Requested;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid RequestedByUserId { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public Guid? AcceptedByUserId { get; set; }

        public DateTime? DepartedAt { get; set; }

        public DateTime? ArrivedAt { get; set; }

        public Guid? SendingNurseUserId { get; set; }

        public Guid? ReceivingNurseUserId { get; set; }

        [MaxLength(1000)]
        public string? TransferReason { get; set; }

        [MaxLength(2000)]
        public string? HandoverSummary { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyTransferRequest : CreateEmergencyTransferRequest
    {
    }

    public class UpdateEmergencyTransferTransferStatusRequest
    {
        [Required]
        public EmergencyTransferStatus TransferStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
