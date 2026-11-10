using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    /// <summary>
    /// Permohonan pemanggilan kembali pegawai cuti saat darurat rumah sakit (BE-HRD-117, HRD-DEC-113).
    /// </summary>
    [Table("TrxLeaveRecallRequest", Schema = "public")]
    public class TrxLeaveRecallRequest : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string RecallNumber { get; set; } = string.Empty;

        [Required]
        public Guid LeaveRequestId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public DateOnly EffectiveRecallDate { get; set; }

        /// <summary>
        /// Jumlah hari cuti yang belum dinikmati dan dipulihkan kembali ke saldo pegawai.
        /// </summary>
        [Column(TypeName = "decimal(4,1)")]
        public decimal RestoredDays { get; set; }

        /// <summary>
        /// Kategori kondisi darurat: DisasterMascualty, EpidemicOutbreak, CriticalSurgeIcu, Other
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EmergencyCategory { get; set; } = string.Empty;

        [Required]
        public string RecallReason { get; set; } = string.Empty;

        /// <summary>
        /// Status permohonan recall: RequestedByHeadUnit, ApprovedByHrManager, Executed, Rejected
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "RequestedByHeadUnit";

        [Required]
        public Guid RequestedByHeadUnitId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid? ApprovedByHrManagerId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(LeaveRequestId))]
        public WfpLeaveRequest? LeaveRequest { get; set; }

        [ForeignKey(nameof(WorkforceProfileId))]
        public MstWorkforceProfile? WorkforceProfile { get; set; }

        [ForeignKey(nameof(RequestedByHeadUnitId))]
        public MstWorkforceProfile? RequestedByHeadUnit { get; set; }

        [ForeignKey(nameof(ApprovedByHrManagerId))]
        public MstWorkforceProfile? ApprovedByHrManager { get; set; }
    }
}
