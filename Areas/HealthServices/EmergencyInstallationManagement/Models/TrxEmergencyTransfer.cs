using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("TrxEmergencyTransfer", Schema = "public")]
    public class TrxEmergencyTransfer : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TransferNumber { get; set; } = string.Empty;

        public Guid? FromServiceUnitId { get; set; }

        [Required]
        public Guid ToServiceUnitId { get; set; }

        public Guid? FromRoomId { get; set; }

        public Guid? ToRoomId { get; set; }

        public Guid? FromBedId { get; set; }

        public Guid? ToBedId { get; set; }

        public EmergencyTransferStatus TransferStatus { get; set; }
            = EmergencyTransferStatus.Requested;

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

        public TrxEmergencyVisit? EmergencyVisit { get; set; }

        public MstServiceUnit? FromServiceUnit { get; set; }

        public MstServiceUnit? ToServiceUnit { get; set; }

        public ApplicationUser? RequestedByUser { get; set; }

        public ApplicationUser? AcceptedByUser { get; set; }

        public ApplicationUser? SendingNurseUser { get; set; }

        public ApplicationUser? ReceivingNurseUser { get; set; }
    }
}
