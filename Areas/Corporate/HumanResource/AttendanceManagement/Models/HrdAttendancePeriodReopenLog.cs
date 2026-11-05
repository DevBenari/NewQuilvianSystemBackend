using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    /// <summary>
    /// Log pencatatan pembukaan kembali periode presensi darurat oleh HR Manager dengan rollback serah terima payroll (BE-HRD-104, BE-HRD-114, HRD-DEC-108, INT-HR-43).
    /// </summary>
    [Table("HrdAttendancePeriodReopenLog", Schema = "public")]
    public class HrdAttendancePeriodReopenLog : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AttendancePeriodId { get; set; }

        [Required]
        public Guid ReopenedByUserId { get; set; }

        [Required]
        public DateTime ReopenedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(1000)]
        public string ReopenReason { get; set; } = string.Empty;

        public DateTime? PreviousClosedAt { get; set; }
        public Guid? PreviousClosedByUserId { get; set; }

        public bool IsRollbackPayrollExecuted { get; set; } = false;

        [MaxLength(1000)]
        public string? RollbackPayrollNotes { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(AttendancePeriodId))]
        public HrdAttendancePeriod? AttendancePeriod { get; set; }

        [ForeignKey(nameof(ReopenedByUserId))]
        public ApplicationUser? ReopenedByUser { get; set; }

        [ForeignKey(nameof(PreviousClosedByUserId))]
        public ApplicationUser? PreviousClosedByUser { get; set; }
    }
}
