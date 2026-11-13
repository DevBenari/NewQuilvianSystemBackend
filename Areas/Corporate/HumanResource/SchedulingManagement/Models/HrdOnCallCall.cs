using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    /// <summary>
    /// Satu catatan pemanggilan siaga. Mengubah baris ini tidak mengubah status penugasan siaga
    /// (HRD-DEC-198, INT-HR-39). Jawaban dicatat petugas unit pemanggil (HRD-DEC-204).
    /// </summary>
    [Table("HrdOnCallCall", Schema = "public")]
    public class HrdOnCallCall : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OnCallAssignmentId { get; set; }

        [Required]
        public Guid CalledWorkforceProfileId { get; set; }

        /// <summary>
        /// 0 = siaga utama; 1 dan seterusnya = urutan cadangan.
        /// </summary>
        public int BackupSequence { get; set; } = 0;

        [Required]
        public Guid CallingOrganizationUnitId { get; set; }

        /// <summary>
        /// Alasan panggilan. SENSITIF: dilarang memuat nama atau nomor rekam medis pasien.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string CallReason { get; set; } = string.Empty;

        /// <summary>
        /// Sent, Accepted, Declined, Expired, atau Completed.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string CallStatus { get; set; } = "Sent";

        public DateTime CalledAt { get; set; }

        [Required]
        public Guid RecordedByUserId { get; set; }

        public DateTime? AnsweredAt { get; set; }
        public Guid? AnswerRecordedByUserId { get; set; }

        /// <summary>
        /// Alasan menolak. SENSITIF.
        /// </summary>
        [MaxLength(500)]
        public string? DeclineReason { get; set; }

        public DateTime? ExpiredAt { get; set; }
        public Guid? ExpiredByUserId { get; set; }

        public DateTime? ArrivedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }

        public int? ResponseMinutes { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public TrxOnCallAssignment? OnCallAssignment { get; set; }
        public MstWorkforceProfile? CalledWorkforceProfile { get; set; }
        public MstOrganizationUnit? CallingOrganizationUnit { get; set; }
    }
}
