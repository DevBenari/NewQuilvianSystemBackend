using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    /// <summary>
    /// Dispensasi: pembebasan terlambat atau pulang cepat untuk satu pegawai pada satu tanggal
    /// dan rentang jam, disetujui atasan langsung (HRD-DEC-200, HRD-ASM-76). Log mesin tidak diubah.
    /// </summary>
    [Table("HrdAttendanceDispensation", Schema = "public")]
    public class HrdAttendanceDispensation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        /// <summary>
        /// LateArrival atau EarlyDeparture.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string DispensationType { get; set; } = string.Empty;

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        /// <summary>
        /// Alasan dispensasi. SENSITIF.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Submitted, Approved, Rejected, atau Cancelled.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Submitted";

        [Required]
        public Guid RequestedByUserId { get; set; }

        public bool IsOnBehalf { get; set; } = false;

        /// <summary>
        /// Ditetapkan server dari WfpManagerAssignment, tidak pernah dari isi permintaan klien.
        /// </summary>
        public Guid? ApproverWorkforceProfileId { get; set; }

        public Guid? DecidedByUserId { get; set; }
        public DateTime? DecidedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkforceProfile? ApproverWorkforceProfile { get; set; }
    }
}
